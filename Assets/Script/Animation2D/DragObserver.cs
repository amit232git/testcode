//#define WINDOWS

using UnityEngine;
using System.Collections;

[System.Serializable]
public enum FingerState
{
    Disabled = -1,          // drag observer wylaczony
    Idle = 0,               // czekamy na toucha
    FingerDown,             // mamy toucha, nie wiadomo czy podwojny czy dlugi czy dragowanie / przesuwanie / zoom
    Ended,                  // finger podniesiony, czekamy na double click, jak sie nie doczekamy to wysylamy click
    FingerMove,             // ruszamy klikiem
}

#if WINDOWS
public struct iPhoneTouch
{
    public int fingerId;
}
#endif

[AddComponentMenu( "Animation2D/DragObserver" )]
public class DragObserver : MonoBehaviour, IDragObserverHitTest
{
    public static float DefaultDoubleClickDuration;     // po tym czasie zdarzenie bedzie traktowane jako klik
    public static float DefaultLongClickDuration;       //                        - || -                  dlugi klik
    public static float DefaultMinimumDistanceToMove;   // minimalna odleglosc wyslania on move ( mozna zmieniac na TouchDown, zeby bylo per object
    public static int   MaxTouchCount           = 8;    // maksymalna ilosc touchy i multi touchy obslugiwana przez DragObserver
    public static int   MouseTouchIndex         = MaxTouchCount - 1;

    public class DragObserverClientComparer : IComparer
    {
        #region IComparer Members

        public int Compare( object x, object y )
        {
            ObserverCollider objx = x as ObserverCollider;
            ObserverCollider objy = y as ObserverCollider;

            return objx.Trans.position.z.CompareTo( objy.Trans.position.z );
        }

        #endregion
    }

    public class RaycastHitComparer : IComparer
    {
        #region IComparer Members

        public int Compare( object x, object y )
        {
            RaycastHit objx = ( RaycastHit ) x;
            RaycastHit objy = ( RaycastHit ) y;

            return objx.point.z.CompareTo( objy.point.z );
        }

        #endregion
    }

    //////////////////////////////////////////////////////////////////////////
    /// Drag Observer Singleton
    //////////////////////////////////////////////////////////////////////////

    private static DragObserver instance = null;

    public static DragObserver Instance
    {
        get
        {
            if ( instance == null )
            {
                instance = FindObjectOfType( typeof( DragObserver ) ) as DragObserver;
                if ( instance == null )
                {
                    Debug.LogError( "[DragObserver] Could not locate an DragObserver object. You have to have exactly one DragObserver in the scene." );

                    return null;
                }
            }

            return instance;
        }
    }
    
    // public variables
    public LayerMask            LayerMask;              // maska obiektow ktore aktualnie obsluguje drag observer ( warunek konieczny jakiejkolwiek interakcji )
    public Camera               Cam;                    // camera do zmiany
    public bool                 EmulateMouse;           // emulacja myszy wlaczona
    public bool                 UsePhysics              = false;
    public bool                 UseObserverColliders    = true;
    public float                RayDepth                = 300F;

    public IDragObserverHitTest HitTestDelegate;
    public MultiTouchDispatchDelegate DispatchDelegate;

    #region public events and dispatch

    public event ObserverColliderTouchEventHandler      NCTouchUp;
    public event ObserverColliderTouchEventHandler      NCTouchDown;

    public event ObserverColliderTouchEventHandler      NCBeginMove;
    public event ObserverColliderTouchEventHandler      NCMoving;
    public event ObserverColliderTouchEventHandler      NCEndMove;

    public void OnNCTouchUp( ObserverTouch touch )
    {
        if ( NCTouchUp != null )
        {
            NCTouchUp( touch );
        }
    }

    public void OnNCTouchDown( ObserverTouch touch )
    {
        if ( NCTouchDown != null )
        {
            NCTouchDown( touch );
        }
    }

    public void OnNCBeginMove( ObserverTouch touch )
    {
        if ( NCBeginMove != null )
        {
            NCBeginMove( touch );
        }
    }

    public void OnNCMoving( ObserverTouch touch )
    {
        if ( NCMoving != null )
        {
            NCMoving( touch );
        }
    }

    public void OnNCEndMove( ObserverTouch touch )
    {
        if ( NCEndMove != null )
        {
            NCEndMove( touch );
        }
    }

    #endregion

    public ObserverTouch      []        Touches;
    public ObserverMultiTouch []        MultiTouches;

    private ArrayList                   observerClients;

    public ArrayList                    ObserverClients
    {
        get { return observerClients; }
    }
 
    private DragObserverClientComparer  comparer;
    private RaycastHitComparer          raycastHitComparer;
    private RaycastHit                  lastHit;
    private bool                        physicsHit;

    private Touch                 virtualMouseTouch;
    private int                         mouseButton;

    public int                          MouseButton
    {
        get { return mouseButton; }
    }
    private ObserverTouch               mouseTouch;



    void Awake( )
    {
        observerClients         = new ArrayList( );
        Touches                 = new ObserverTouch[ MaxTouchCount ];
        MultiTouches            = new ObserverMultiTouch [ MaxTouchCount ];
        for ( int i = 0; i < MaxTouchCount; ++i )
        {
            Touches [ i ]       = new ObserverTouch( i );
            MultiTouches [ i ]  = new ObserverMultiTouch( i );
        }
        comparer                = new DragObserverClientComparer( );
        HitTestDelegate         = this;        
        raycastHitComparer      = new RaycastHitComparer( );
        physicsHit              = false;
        mouseTouch              = Touches [ MouseTouchIndex ];
        virtualMouseTouch       = new Touch( );
        EmulateMouse            = EmulateMouse && Application.isEditor ;        
    }

    #region Observed objects manipulation

    public void AddClient( ObserverCollider client )
    {
        if ( !observerClients.Contains( client ) )
        {
            observerClients.Add( client );
        }
    }

    public void RemoveClient( ObserverCollider client )
    {
        observerClients.Remove( client );
    }

    #endregion    

    public ObserverCollider DefaultHitTest( ref Vector3 pos )
    {        
        ObserverCollider dragObserverClient = null;
        ObserverCollider physicsClient      = null;        
        int currentMask = LayerMask.value;

        if ( UsePhysics )
        {
            RaycastHit[] hits = null;
            Ray  screenRay = Cam.ScreenPointToRay( pos );

            if ( ( hits = Physics.RaycastAll( screenRay.origin, screenRay.direction, RayDepth, LayerMask.value ) ) != null )
            {
                System.Array.Sort( hits, raycastHitComparer );

                for ( int i = hits.Length - 1; i >= 0; --i )
                {
                    ObserverCollider client = hits [ i ].collider.GetComponent( typeof( ObserverCollider ) ) as ObserverCollider;
                    //Debug.Log( hits [ i ].collider.gameObject.name + " : " + ( client == null ? " NULL" : client.name ) );
                    if ( client != null && client.HitTest( hits [ i ] ) )
                    {
                        physicsClient = client;
                        lastHit = hits [ i ];
                        
                        break;
                    }
                }
            }
        }
        pos = Cam.ScreenToWorldPoint( pos );
        if ( UseObserverColliders )
        {
            observerClients.Sort( comparer );
            for ( int i = 0; i < observerClients.Count; i++ )
            {
                ObserverCollider client = observerClients [ i ] as ObserverCollider;
                if ( ( currentMask & ( 1 << client.gameObject.layer ) ) != 0 )
                {
                    if ( client.HitTest( pos ) )
                    {
                        dragObserverClient = client;

                        break;
                    }
                }
            }            
        }


        if ( dragObserverClient != null && physicsClient != null )
        {
            if ( lastHit.point.z < dragObserverClient.Trans.position.z )
            {
                dragObserverClient = null;
            }
            else
            {
                physicsClient = null;
            }
        }
        if ( dragObserverClient != null )
        {
            physicsHit = false;

            return dragObserverClient;
        }
        else if ( physicsClient != null )
        {
            physicsHit = true;

            return physicsClient;
        }

        return null;
    }

    public int GetFirstTouch( )
    {
        return GetNextTouch( -1 );
   }

    public int GetNextTouch( int startTouch )
    {
        for ( int i = startTouch + 1; i < MaxTouchCount; ++i )
        {
            if ( Touches [ i ].Running )
            {
                return i;
            }
        }
        return -1;
    }

    public int GetFirstNonMultiTouch( )
    {
        return GetNextNonMultiTouch( -1 );
    }

    public int GetNextNonMultiTouch( int startTouch )
    {
        for ( int i = startTouch + 1; i < MaxTouchCount; ++i )
        {
            if ( Touches [ i ].Running && !Touches [ i ].MultiTouch )
            {
                return i;
            }
        }
        return -1;
    }

    public void Update( )
    {
#if (!WINDOWS)
        for ( int i = 0; i < Input.touchCount; ++i )
        {            
            Touch touch = Input.GetTouch( i );
            Vector3 touchPos = Input.GetTouch( i ).position;
            ObserverCollider current = HitTestDelegate.HitTest( ref touchPos );               
            
            switch ( touch.phase )
            {
                case TouchPhase.Began:
                    int idx = FindAvailableTouch( );
                    if ( idx >= 0 )
                    {                        
                        if ( current != null )
                        {
                            int existingIdx = FindObserverTouch( current );
                            if ( existingIdx >= 0 )
                            {
                                if ( !Touches [ existingIdx ].AllowManyTouches )
                                {
                                    // jesli nie ma byc wielu touchy na collider, to nullujemy znaleziony
                                    current = null;
                                }
                            }
                        }
                        Touches [ idx ].BeginTouch( touch, i, touchPos, current, lastHit, physicsHit );
                    }
                    break;

                case TouchPhase.Canceled:
                case TouchPhase.Ended:
                    idx = FindObserverTouch( touch.fingerId );
                    if ( idx >= 0 )
                    {
                        Touches [ idx ].EndTouch( i, touchPos );                        
                    }
                    break;

                case TouchPhase.Moved:
                    idx = FindObserverTouch( touch.fingerId );
                    if ( idx >= 0 )
                    {
                        Touches [ idx ].MoveTouch( i, touchPos );
                    }
                    break;
            }
        }
        
        if ( Input.touchCount == 0 && EmulateMouse )
        {
#endif
            if ( Input.GetMouseButtonDown( 0 ) || Input.GetMouseButtonDown( 1 ) )
            {
                Vector3 touchPos = Input.mousePosition;
                ObserverCollider current = HitTestDelegate.HitTest( ref touchPos );

                mouseButton = Input.GetMouseButtonDown( 0 ) ? 0 : 1;
                mouseTouch.BeginTouch( virtualMouseTouch, -1, touchPos, current, lastHit, physicsHit );
            }
            else if ( Input.GetMouseButtonUp( 0 ) || Input.GetMouseButtonUp( 1 ) )
            {
                Vector3 touchPos = Cam.ScreenToWorldPoint( Input.mousePosition );
                mouseTouch.EndTouch( -1, touchPos );
                mouseButton = -1;
            }
            else if ( mouseButton >= 0 )
            {
                Vector3 touchPos = Cam.ScreenToWorldPoint( Input.mousePosition );
                if ( touchPos != Touches [ MouseTouchIndex ].LastPos )
                {
                    mouseTouch.MoveTouch( -1, touchPos );
                }
            }
#if (!WINDOWS)
        }
#endif
    }

    public ObserverMultiTouch CreateMultiTouch( params int [] touchesIdxs )
    {
        ObserverMultiTouch touch = null;
        int multiIdx = FindAvailableMultiTouch( );
        if ( multiIdx >= 0 )
        {
            touch = MultiTouches [ multiIdx ];
            touch.CreateMultiTouch( );
            touch.TouchCount = touchesIdxs.Length;
            for ( int i = 0; i < touchesIdxs.Length; ++i )
            {
                ObserverTouch singleTouch = Touches [ touchesIdxs [ i ] ];
                singleTouch.MultiTouch = true;
                touch.Touches [ i ] = singleTouch;
                singleTouch.MultiTouchId = multiIdx;
            }
        }

        return touch;
    }

    public void EndMultiTouch( ObserverMultiTouch touch )
    {
        touch.EndMultiTouch( );
    }

    int FindObserverTouch( int fingerId )
    {
        for ( int i = 0; i < MaxTouchCount; ++i )
        {
            if ( Touches[ i ].Running && Touches [ i ].FingerId == fingerId )
            {
                return i;
            }
        }

        return -1;

    }

    int FindObserverTouch( ObserverCollider source )
    {
        if ( source != null )
        {

            for ( int i = 0; i < MaxTouchCount; ++i )
            {
                if ( Touches [ i ].Running && Touches [ i ].TouchCollider == source )
                {
                    return i;
                }
            }
        }

        return -1;
    }

    int FindAvailableTouch( )
    {
        for ( int i = 0; i < MaxTouchCount; ++i )
        {
            if ( !Touches [ i ].Running )
            {
                return i;
            }
        }

        return -1;
    }

    int FindObserverMultiTouch( int fingerId )
    {
        for ( int i = 0; i < MaxTouchCount; ++i )
        {
            if ( Touches[ i ].Running && Touches [ i ].FingerId == fingerId && Touches[ i ].MultiTouch )
            {
                return i;
            }
        }

        return -1;

    }

    int FindAvailableMultiTouch( )
    {
        for ( int i = 0; i < MaxTouchCount; ++i )
        {
            if ( !MultiTouches [ i ].Running )
            {
                return i;
            }
        }

        return -1;
    }


    #region IDragObserverHitTest Members

    ObserverCollider IDragObserverHitTest.HitTest( ref Vector3 position )
    {
        return DefaultHitTest( ref position );
    }

    #endregion

    public void DebugLog( )
    {
        int touchCount = 0;
        int multTouchCount = 0;
        for ( int i = 0; i < MaxTouchCount; ++i )
        {
            if ( Touches [ i ].Running )
            {
                touchCount++;
            }
            if ( MultiTouches [ i ].Running )
            {
                multTouchCount++;
            }
        }
        Debug.Log( "Touches: " + touchCount + "; MultiTouches: " + multTouchCount );
    }
}

public interface IDragObserverHitTest
{
    ObserverCollider HitTest( ref Vector3 position );    
}

public class ObserverTouch
{
    public bool                 Running;
    public int                  FingerId;           // fingerId z iPhoneInput.GetTouch( n ).fingerId
    public int                  SystemTouchId;      // PhoneInput.GetTouch( SystemTouchId )
    public int                  MultiTouchId;       // DragObserver.Instance.MultiTouches[ MultiTouchId ]
    public int                  TouchId;            // DragObserver.Instance.Touches[ TouchId ]
    public Vector3              StartPos;
    public Vector3              LastPos;
    public ObserverCollider     TouchCollider;
    public FingerState          CurrentState;
    public bool                 MultiTouch;
    public bool                 SendMessagesWhileMultiTouch;
    public bool                 AllowManyTouches;    
    public DragObserver         Observer;
    public RaycastHit           Hit;
    public bool                 IsPhysics;

    public Object               UserData;

    public bool SendMessage
    {
        get { return !MultiTouch || ( MultiTouch && SendMessagesWhileMultiTouch ); }
    }

    public ObserverTouch( int touchId )
    {
        Running = false;
        Observer = DragObserver.Instance;
        TouchId = touchId;
    }

    public void BeginTouch( Touch touch, int systemTouchId, Vector3 startPos, ObserverCollider obj, RaycastHit hit, bool isPhysics )
    {
        Running = true;
        AllowManyTouches = false;
        FingerId = touch.fingerId;        
        SystemTouchId = systemTouchId;
        CurrentState = FingerState.FingerDown;
        IsPhysics = isPhysics;
        Hit = hit;
        TouchCollider = obj;
        StartPos = startPos;
        LastPos = startPos;
        MultiTouch = false;
        SendMessagesWhileMultiTouch = true;

        if ( Observer.DispatchDelegate != null )
        {
            Observer.DispatchDelegate( );
        }
        if ( TouchCollider != null && SendMessage )
        {
            TouchCollider.OnTouchDown( this );
        }
        else
        {
            Observer.OnNCTouchDown( this );
        }
        if ( MultiTouch )
        {
            Observer.MultiTouches [ MultiTouchId ].BeginMultiTouch( TouchId );
        }
    }

    public void EndTouch( int systemTouchId, Vector3 pos )
    {
        SystemTouchId = systemTouchId;
        LastPos = pos;
        bool send = SendMessage;
        if ( TouchCollider != null )
        {
            if ( CurrentState == FingerState.FingerMove )
            {
                if ( send )
                {
                    TouchCollider.OnEndMove( this );
                }
            }
            if ( send )
            {
                TouchCollider.OnTouchUp( this );
            }
        }
        else
        {
            if ( CurrentState == FingerState.FingerMove )
            {
                if ( send )
                {
                    Observer.OnNCEndMove( this );
                }
            }
            if ( send )
            {
                Observer.OnNCTouchUp( this );
            }
            
        }

        CurrentState = FingerState.Ended;
        if ( MultiTouch )
        {
            Observer.MultiTouches [ MultiTouchId ].MultiTouch( TouchId );
        }        
        Running = false;
    }

    public void MoveTouch( int systemTouchId, Vector3 pos )
    {
        LastPos = pos;
        SystemTouchId = systemTouchId;
        if ( CurrentState != FingerState.FingerMove )
        {
            CurrentState = FingerState.FingerMove;
            if ( TouchCollider != null && SendMessage )
            {
                TouchCollider.OnBeginMove( this );
            }
            else
            {
                Observer.OnNCBeginMove( this );
            }
        }
        else
        {
            if ( TouchCollider != null && SendMessage )
            {
                TouchCollider.OnMoving( this );
            }
            else
            {
                Observer.OnNCMoving( this );
            }

        }
        if ( MultiTouch )
        {
            Observer.MultiTouches [ MultiTouchId ].MultiTouch( TouchId );
        }
    }
}

public class ObserverMultiTouch
{
    public bool                 Running;
    public bool                 Start;
    public int                  ModifiedTouchIdx;
    public int                  TouchCount;    
    public int                  MultiTouchId;    
    public ObserverTouch []     Touches;
    public Object               UserData;
    public MultiTouchDelegate   TouchDelegate;
    public DragObserver         Observer;

    public ObserverTouch        ModifiedTouch
    {
        get { return Observer.Touches [ ModifiedTouchIdx ]; }
    }

    public ObserverMultiTouch( int multiId )
    {
        Running = false;
        Touches = new ObserverTouch [ DragObserver.MaxTouchCount ];
        MultiTouchId = multiId;
        Observer = DragObserver.Instance;
    }

    public void BeginMultiTouch( int touchId )
    {
        ModifiedTouchIdx = touchId;
        Start = true;
        if ( TouchDelegate != null )
        {
            TouchDelegate( this );
        }
        Start = false;
    }

    public void EndMultiTouch( )
    {
        TouchDelegate = null;
        Running = false;
        for ( int i = 0; i < TouchCount; ++i )
        {
            Touches [ i ].MultiTouch = false;
        }
    }

    public void CreateMultiTouch( )
    {
        Running = true;
    }

    public void MultiTouch( int TouchId )
    {
        ModifiedTouchIdx = TouchId;
        if ( TouchDelegate != null )
        {
            TouchDelegate( this );
        }
    }
}

public delegate void MultiTouchDispatchDelegate( );
public delegate void MultiTouchDelegate( ObserverMultiTouch touch );