using UnityEngine;
using System;
using System.Collections;

public enum WindingMode : int
{
    CCW,
    CW,
}

public enum CloneMode : uint
{
    Default = 0,
    TheSameMaterial = 1
}
public enum AnimationType : uint
{
    Normal = 0,
    Atlas = 1,       
}


[System.Flags]
public enum ProgressBarFlags
{
    Horizontal = 1,
    Vertical = 2,
    Increment = 4,

}

[AddComponentMenu( "Animation2D/Animation2D" )]
public class Animation2D : MonoBehaviour
{
    /************************************************************************/
    /*                          Animation Class                             */
    /************************************************************************/

    #region public events

    public event Animation2DEventHandler            Started;
    public event Animation2DEventHandler            Finished;
    public event Animation2DEventHandler            FrameChanged;
    public event Animation2DEventHandler            ProgressEnd;


    public void OnStarted( )
    {
        if ( Started != null )
        {
            Started( this );
        }
    }

    public void OnFinished( )
    {
	//	Debug.Log(gameObject.name);
        if ( Finished != null )
        {
			
            Finished( this );
        }
    }

    public void OnFrameChanged( )
    {
        if ( FrameChanged != null )
        {
            FrameChanged( this );
        }
    }

    public void OnProgressEnd( )
    {
        if ( ProgressEnd != null )
        {
            ProgressEnd( this );
        }
    }

    #endregion

    public const float ScreenWidth  = 48.0f;
    public const float ScreenHeight = 32.0f;

    public const float ScreenScaleX = ScreenWidth  / 480.0f;
    public const float ScreenScaleY = ScreenHeight / 320.0f;
    
    public AnimationAsset           EventsAsset;
    
    public Animation2DEvent           CurrentEventPtr
    {
        get { return EventsAsset.Events [ CurrentEvent ]; }
    }

    public Animation2DFrame           CurrentFramePtr
    {
        get { return CurrentEventPtr.Frames [ CurrentFrame ]; }
    }

    public int                      CurrentFrame;
    public int                      CurrentEvent;
        
    public bool Autoplay            = false;
    public WindingMode Winding = WindingMode.CCW;

    public bool             LocalTransform  = true;
    public bool             Interpolate     = false;

    public bool             CanInterpolate
    {
        get { return Interpolate && AnimationContainer.Instance.AllowInterpolations; }
    }

    public Transform        trans;
    //public Material         mat;
    //public Renderer         ren;

    private bool            initialized     = false;    


    public float           currentFps      = 1F / 30F;///!!!!!!!!!!!!!!!!!!!! urw
    private AnimationFlags  currentFlags    = AnimationFlags.EVENT_ONCE;

    private Vector3         currentPosition;

    public Vector3          CurrentPosition
    {
        get { return currentPosition; }
    }

    private bool isPaused;
    
    public bool IsPaused 
    {
        get { return isPaused; }
    }

    private bool isPlaying = false;

    public bool IsPlaying
    {
        get { return isPlaying; }
    }

    [NonSerialized]
    public object                   UserData;

    protected MeshFilter            annMeshFilter;
    protected Mesh                  annMesh;
    protected Vector3[] vertices    = new Vector3 [ 4 ];
    protected Color[] colors        = new Color [ 4 ];
    protected Vector2[] uvs         = new Vector2 [ 4 ];
    protected int[] faces           = new int [ 6 ];
    protected float                 width;
    protected float                 height;
    protected Vector2               topLeft;
    protected Vector2               bottomRight;
    protected Color                 color;
    protected Vector3               offset = Vector3.zero;
    protected Rect                  uvRect;

    protected int                   eventFrame = -100;
    protected Animation2DEventHandler eventHandler;

    void Awake( )
    {        
        Initialize( );        
    }

    void Start( )
    {
        if ( Autoplay )
        {
            Play( );
        }
    }

    public void Initialize( )
    {
        if ( !initialized )
        {
            Initialize_Internal( );

            initialized = true;
        }
    }

    public void ForceInitialize( )
    {
        Initialize_Internal();

        initialized = true;
    }


    public void SetColor( Color c )
    {
        color = c;

        // Update vertex colors:
        colors [ 0 ] = color;
        colors [ 1 ] = color;
        colors [ 2 ] = color;
        colors [ 3 ] = color;

        annMesh.colors = colors;
    }

    protected static int [][] Faces   = new int [ 2 ] [] { new int [ 6 ] { 0, 1, 3, 3, 1, 2 }, new int [ 6 ] { 0, 3, 1, 3, 2, 1 } };

    public void SetWindingOrder( WindingMode order )
    {
        Winding = order;

        annMesh.triangles = Animation2D.Faces[ ( int ) Winding ];
    }

    public void CalcEdges( )
    {
        topLeft.x = width * -0.5f;
        topLeft.y = height * 0.5f;
        bottomRight.x = width * 0.5f;
        bottomRight.y = height * -0.5f;
    }

    public void UpdateUVs( ref Rect uvRect )
    {
        if ( Winding == WindingMode.CW )
        {
            uvs [ 0 ].x = uvRect.x;     uvs [ 0 ].y = uvRect.yMax;
            uvs [ 1 ].x = uvRect.x;     uvs [ 1 ].y = uvRect.y;
            uvs [ 2 ].x = uvRect.xMax;  uvs [ 2 ].y = uvRect.y;
            uvs [ 3 ].x = uvRect.xMax;  uvs [ 3 ].y = uvRect.yMax;
        }
        else
        {
            uvs [ 3 ].x = uvRect.x;     uvs [ 3 ].y = uvRect.yMax;
            uvs [ 2 ].x = uvRect.x;     uvs [ 2 ].y = uvRect.y;
            uvs [ 1 ].x = uvRect.xMax;  uvs [ 1 ].y = uvRect.y;
            uvs [ 0 ].x = uvRect.xMax;  uvs [ 0 ].y = uvRect.yMax;
        }

        annMesh.uv = uvs;
    }
    protected void SetFrameSizeNoCorrect( ref Rect ptRect )
    {
        vertices [ 0 ].x = -ptRect.xMax;
        vertices [ 0 ].y = -ptRect.yMax;
        vertices [ 0 ].z = 0F;

        vertices [ 1 ].x = + ptRect.x;
        vertices [ 1 ].y = + ptRect.y;
        vertices [ 1 ].z = 0F;

        vertices [ 2 ].x = + ptRect.xMax;
        vertices [ 2 ].y = + ptRect.yMax;
        vertices [ 2 ].z = 0F;

        vertices [ 3 ].x = + -ptRect.x;
        vertices [ 3 ].y = + -ptRect.y;
        vertices [ 3 ].z = 0F;

        annMesh.vertices = vertices;

        annMesh.RecalculateBounds( );
    }

    protected void SetFrameSize( ref Rect ptRect, ref Vector2 framePos )
    {
        vertices [ 0 ].x = framePos.x + -ptRect.xMax;
        vertices [ 0 ].y = framePos.y + -ptRect.yMax;
        vertices [ 0 ].z = 0F;

        vertices [ 1 ].x = framePos.x + ptRect.x;
        vertices [ 1 ].y = framePos.y + ptRect.y;
        vertices [ 1 ].z = 0F;

        vertices [ 2 ].x = framePos.x + ptRect.xMax;
        vertices [ 2 ].y = framePos.y + ptRect.yMax;
        vertices [ 2 ].z = 0F;

        vertices [ 3 ].x = framePos.x + -ptRect.x;
        vertices [ 3 ].y = framePos.y + -ptRect.y;
        vertices [ 3 ].z = 0F;

        annMesh.vertices = vertices;

        annMesh.RecalculateBounds( );
    }

    protected void SetFrameSize( ref Rect ptRect )
    {
        vertices [ 0 ].x =  -ptRect.xMax;
        vertices [ 0 ].y =  -ptRect.yMax;
        vertices [ 0 ].z = 0F;

        vertices [ 1 ].x = ptRect.x;
        vertices [ 1 ].y = ptRect.y;
        vertices [ 1 ].z = 0F;

        vertices [ 2 ].x = ptRect.xMax;
        vertices [ 2 ].y = ptRect.yMax;
        vertices [ 2 ].z = 0F;

        vertices [ 3 ].x = -ptRect.x;
        vertices [ 3 ].y = -ptRect.y;
        vertices [ 3 ].z = 0F;

        annMesh.vertices = vertices;

        annMesh.RecalculateBounds( );
    }

    protected float elapsedTime = 0F;

    public void Tick( )
    {
        if ( CanInterpolate )
        {
            InterpolateTick( );
        }
        else
        {
            FrameTick( );
        }
    }

    public void FrameTick( )
    {
        elapsedTime += Time.deltaTime;
        while ( elapsedTime >= currentFps )
        {
            if ( !IsPlaying )
            {
                break;
            }
            NextFrame( );
            elapsedTime -= currentFps;
        }
    }

    public void InterpolateTick( )
    {        
        elapsedTime += Time.deltaTime;
        int frameNo = ( int ) ( elapsedTime / currentFps );
        //Debug.Log( "Tick: " + frameNo + " : " + elapsedTime + " : " + currentFps + "===" + ( elapsedTime * currentFps ) );
        if ( frameNo != CurrentFrame )
        {
            CurrentFrame = frameNo;
            //Debug.Log( CurrentFrame );
            if ( GoForward )
            {
                if ( CurrentFrame >= CurrentEventPtr.Frames.Length )
                {
                    if ( CurrentEventPtr.Loop )
                    {
                        elapsedTime = 0F;
                        CurrentFrame = 0;
                        SetFrameInterpolate( CurrentFramePtr, true );
                        OnFinished( );
                    }
                    else
                    {
                        CurrentFrame = CurrentEventPtr.Frames.Length - 1;

                        if ( CurrentEventPtr.HideAfterStop )
                        {
                            Hide( );
                        }
                        Stop( );
                    }
                }
                else
                {
                    if ( CurrentFrame == eventFrame )
                    {
                        eventHandler( this );
                    }
                    SetFrameInterpolate( CurrentFramePtr, true );
                }
            }
            else
            {
                if ( CurrentFrame < 0 )
                {
                    if ( CurrentEventPtr.Loop )
                    {
                        CurrentFrame = CurrentEventPtr.Frames.Length - 1;
                        elapsedTime = currentFps * ( float ) CurrentEventPtr.Frames.Length;
                        SetFrameInterpolate( CurrentFramePtr, true );
                        OnFinished( );
                    }
                    else
                    {
                        CurrentFrame = 0;

                        if ( CurrentEventPtr.HideAfterStop )
                        {
                            Hide( );
                        }
                        Stop( );
                    }
                }
                else
                {
                    if ( CurrentFrame == eventFrame )
                    {
                        eventHandler( this );
                    }
                    SetFrameInterpolate( CurrentFramePtr, true );
                }
            }
        }
        else
        {
            InterpolateFramePosition( );
        }
    }



    public void SetMesh( Mesh mesh )
    {
        annMesh = mesh;
        annMeshFilter = GetComponent( typeof( MeshFilter ) ) as MeshFilter;
        annMeshFilter.sharedMesh = mesh;
    }

    public void InitAnimationMesh( )
    {
        annMeshFilter = GetComponent( typeof( MeshFilter ) ) as MeshFilter;
        if ( annMeshFilter == null )
        {
            annMeshFilter = gameObject.AddComponent( typeof( MeshFilter ) ) as MeshFilter;
        }
        annMesh = new Mesh( );
        annMeshFilter.sharedMesh = annMesh;
        annMesh.name = gameObject.name;
        annMesh.Clear( );
        annMesh.vertices = vertices;
        annMesh.uv = uvs;
        annMesh.colors = colors;
        annMesh.triangles = faces;
        SetWindingOrder( Winding );
        SetColor( Color.white );
        offset = Vector3.zero;           
    }

    public void Initialize_Internal( )
    {       
        annMeshFilter = GetComponent( typeof( MeshFilter ) ) as MeshFilter;
        annMesh = annMeshFilter.sharedMesh;
        if ( trans == null )
        {
            trans = transform;
        }
    }

    public void PlayFromFrameSafe( int startFrame )
    {
        if ( IsPlaying )
        {
            Stop( false );
        }
        CurrentFrame = startFrame;
        if ( CurrentFrame < 0 )
        {
            CurrentFrame = 0;
        } else if ( CurrentFrame >= CurrentEventPtr.Frames.Length )
        {
            CurrentFrame = CurrentEventPtr.Frames.Length - 1;
        }
        Animation2DFrame frame = CurrentEventPtr.Frames [ CurrentFrame ];
        //SetFrame( frame, true );
        
        StartPlaying( );
    }

    public void PlayFromFrame( int startFrame )
    {
        if ( IsPlaying )
        {
            Stop( false );
        }
        CurrentFrame = startFrame;
        //SetFrame( CurrentFramePtr, true );

        StartPlaying( );
    }

    public bool GoForward
    {
        get { return !GoReverse; }
    }

    public bool GoReverse
    {
        get { return ( currentFlags & AnimationFlags.EVENT_REVERSE ) == AnimationFlags.EVENT_REVERSE; }
    }

    private void StartPlaying( )
    {
        isPlaying = true;
        elapsedTime = 0F;

        if ( CanInterpolate )
        {
            SetFrameInterpolate( CurrentFramePtr, true );
        }
        else
        {
            SetFrame( CurrentFramePtr, true );
        }
        AnimationContainer.Instance.AddAnimation2D( this );

        OnStarted( );
    }

    public void Play( int evt, int frame, float fps, AnimationFlags flags )
    {
        if ( IsPlaying )
        {
            Stop( false );
        }
        CurrentEvent = evt;
        CurrentFrame = frame;
        currentFps = fps;
        currentFlags = flags;

        StartPlaying( );
    }

    public void Play( )
    {
        Play( CurrentEvent );
    }

    public void Play( int evt )
    {
        Play( evt, 1F / EventsAsset.Events [ evt ].Fps );
    }

    public void Play( int evt, float fps, AnimationFlags flags )
    {        
        Play( evt, ( flags & AnimationFlags.EVENT_REVERSE ) == 0 ? 0 : CurrentEventPtr.Frames.Length - 1, fps, flags );
    }

    public void Play( int evt, float fps )
    {        
        Play( evt, fps, EventsAsset.Events [ evt ].Flags );
    }

    public void Play( int evt, AnimationFlags flags )
    {
        Play( evt, 1F / EventsAsset.Events [ evt ].Fps );
    }

    public void Play( int evt, int frame )
    {
        Play( evt, frame, 1F / EventsAsset.Events [ evt ].Fps, EventsAsset.Events [ evt ].Flags );
    }

    public void Stop( )
    {
        Stop( true );
    }

    public void Stop( bool SendSignal )
    {        
        isPlaying = false;
        AnimationContainer.Instance.RemoveAnimation2D( this );
        ClearEvent( );
        if ( SendSignal )
        {
            OnFinished( );
        }
    }

    public void SetFrame( int frame )
    {
        CurrentFrame = frame;
        SetFrame( CurrentEventPtr.Frames[CurrentFrame], true );
    }

    public void SetFrame( int frame, bool SendSignal )
    {
        CurrentFrame = frame;
        SetFrame( CurrentEventPtr.Frames[CurrentFrame], SendSignal );
    }

    public void SetFrame( )
    {
        SetFrame( CurrentEventPtr.Frames[CurrentFrame], false );
    }    

	public bool stayOnPosition = false;

    public void SetFrame( Animation2DFrame frame, bool SendSignal )
    {
        bool rotCCW = CurrentFramePtr.WindingCCW;
		
		annMeshFilter.renderer.sharedMaterial = EventsAsset.Materials[frame.MaterialIndex];
		//Debug.Log(gameObject.name +": "+EventsAsset.Materials[frame.MaterialIndex].name);
		//annMeshFilter.renderer.sharedMaterial. = EventsAsset.Materials[frame.MaterialIndex].
		
        if ( rotCCW && Winding == WindingMode.CW )
        {
            SetWindingOrder( WindingMode.CCW );
        }
        else if ( !rotCCW && Winding == WindingMode.CCW )
        {
            SetWindingOrder( WindingMode.CW );
        }

        currentPosition = frame.FramePos - frame.Correct;
        if ( LocalTransform )
        {
       	 	if(!stayOnPosition)
	            currentPosition = trans.localPosition;/////
            trans.localPosition = currentPosition;
            UpdateUVs( ref frame.uvRect );
            SetFrameSize( ref frame.ptRect, ref frame.Correct );
        }
        else
        {
            UpdateUVs( ref frame.uvRect );
            SetFrameSize( ref frame.ptRect, ref frame.FramePos );
        }

        if ( SendSignal )
        {
            OnFrameChanged( );
        }
    }

    public Animation2DFrame NextFramePtr
    {
        get { return CurrentEventPtr.Frames [ Mathf.Clamp( CurrentFrame + 1, 0, CurrentEventPtr.Frames.Length - 1 ) ]; }
    }

    public void SetFrameInterpolate( Animation2DFrame frame, bool SendSignal )
    {
        bool rotCCW = CurrentFramePtr.WindingCCW;
        if ( rotCCW && Winding == WindingMode.CW )
        {
            SetWindingOrder( WindingMode.CCW );
        }
        else if ( !rotCCW && Winding == WindingMode.CCW )
        {
            SetWindingOrder( WindingMode.CW );
        }
        
        Animation2DFrame nextFrame = NextFramePtr;
        float currentTime = ( float ) CurrentFrame * currentFps;
        currentPosition = Vector3.Lerp( frame.FramePos - frame.Correct, nextFrame.FramePos - nextFrame.Correct, ( elapsedTime - currentTime ) / currentFps );
        currentPosition.z = trans.localPosition.z;
        transform.localPosition = currentPosition;
        //Debug.Log( transform.localPosition );

        UpdateUVs( ref frame.uvRect );
        SetFrameSize( ref frame.ptRect, ref frame.Correct );

        if ( SendSignal )
        {
            OnFrameChanged( );
        }
    }

    public void InterpolateFramePosition( )
    {
        Animation2DFrame frame = CurrentFramePtr;
        Animation2DFrame nextFrame = NextFramePtr;
        float currentTime = ( float ) CurrentFrame * currentFps;
        currentPosition = Vector3.Lerp( frame.FramePos - frame.Correct, nextFrame.FramePos - nextFrame.Correct, ( elapsedTime - currentTime ) / currentFps );
        currentPosition.z = trans.localPosition.z;
        transform.localPosition = currentPosition;
        //Debug.Log( transform.localPosition );
    }


    public void SetFrame( int evt, int frame )
    {
        SetFrame( evt, frame, true );
    }

    public void SetFrame( int evt, int frame, bool sendSignal )
    {
        CurrentEvent = evt;
        SetFrame( frame, sendSignal );
    }

    public void SetFrameInEdit( )
    {
        trans = gameObject.transform;

        if ( EventsAsset == null )
        {
            Debug.Log( "[Animation2D] EventsAsset does not exists for " + gameObject.name );

            return;
        }
        CurrentEvent = Mathf.Clamp( CurrentEvent, 0, EventsAsset.Events.Length - 1 );
        CurrentFrame = Mathf.Clamp( CurrentFrame, 0, EventsAsset.Events [ CurrentEvent ].Frames.Length - 1 );
        SetFrameInEdit( CurrentEventPtr.Frames [ CurrentFrame ] );
    }

    public void SetFrameInEdit( Animation2DFrame frame )
    {
        if ( annMesh == null )
        {
            Initialize( );

            return;
        }
        trans = gameObject.transform;
        Material wantMat = EventsAsset.Materials [ frame.MaterialIndex ];
        if ( renderer.sharedMaterial == null || wantMat != renderer.sharedMaterial )
        {
            SetExternalMaterialInEdit( wantMat );
        }
        bool rotCCW = frame.WindingCCW;
        if ( rotCCW && Winding == WindingMode.CW )
        {
            SetWindingOrder( WindingMode.CCW );
        }
        else if ( !rotCCW && Winding == WindingMode.CCW )
        {
            SetWindingOrder( WindingMode.CW );
        }

        trans.localScale = Vector3.one;

        currentPosition = frame.FramePos - frame.Correct;
        if ( LocalTransform )
        {            
            currentPosition.z = trans.localPosition.z;
            trans.localPosition = currentPosition;
            UpdateUVs( ref frame.uvRect );
            SetFrameSize( ref frame.ptRect, ref frame.Correct );
        }
        else
        {
            UpdateUVs( ref frame.uvRect );        
            SetFrameSize( ref frame.ptRect, ref frame.FramePos );
        }
    }

    public void NextFrame( )
    {
        if ( GoForward )
        {
            if ( ++CurrentFrame >= CurrentEventPtr.Frames.Length )
            {
                if ( CurrentEventPtr.Loop )
                {
                    CurrentFrame = 0;
                    //if ( !StringHelper.IsNullOrEmpty( CurrentEvent.Frames [ CurrentFrame ].WavName ) )
                    //{
                    //    SoundSystem.Instance.PlayEvent( CurrentEvent.Frames [ CurrentFrame ].WavName );
                    //}
                    SetFrame( CurrentEventPtr.Frames [ CurrentFrame ], true );
                    OnFinished( );
                }
                else
                {
                    CurrentFrame = CurrentEventPtr.Frames.Length - 1;

                    if ( CurrentEventPtr.HideAfterStop )
                    {
                        Hide( );
                    }
                    Stop( );
                }
               // DispatchEvent( EventAnimFinished, this );
            }
            else
            {
//                 if ( !StringHelper.IsNullOrEmpty( CurrentEvent.Frames [ CurrentFrame ].WavName ) )
//                 {
//                     SoundSystem.Instance.PlayEvent( CurrentEvent.Frames [ CurrentFrame ].WavName );
//                 }
                if ( CurrentFrame == eventFrame )
                {
                    eventHandler( this );
                }
                SetFrame( CurrentEventPtr.Frames [ CurrentFrame ], true );
            }
        }
        else
        {
            if ( --CurrentFrame < 0 )
            {
                if ( CurrentEventPtr.Loop )
                {
                    CurrentFrame = CurrentEventPtr.Frames.Length - 1;
//                     if ( !StringHelper.IsNullOrEmpty( CurrentEvent.Frames [ CurrentFrame ].WavName ) )
//                     {
//                         SoundSystem.Instance.PlayEvent( CurrentEvent.Frames [ CurrentFrame ].WavName );
//                     }
                    SetFrame( CurrentEventPtr.Frames [ CurrentFrame ], true );
                    OnFinished( );
                } 
                else
                {
                    CurrentFrame = 0;

                    if ( CurrentEventPtr.HideAfterStop )
                    {
                        Hide( );
                    }
                    Stop( );
                }
                //DispatchEvent( EventAnimFinished, this );
            }
            else
            {
                //if ( !StringHelper.IsNullOrEmpty( CurrentEvent.Frames [ CurrentFrame ].WavName ) )
                //{
                //    SoundSystem.Instance.PlayEvent( CurrentEvent.Frames [ CurrentFrame ].WavName );
                //}
                if ( CurrentFrame == eventFrame )
                {
                    eventHandler( this );
                }
                SetFrame( CurrentEventPtr.Frames [ CurrentFrame ], true );
            }
        }
    }

    public Animation2DFrame GetCurrentFrameSafe( )
    {
        if ( EventsAsset == null )
        {
            return null;
        }
        CurrentEvent = Mathf.Clamp( CurrentEvent, 0, EventsAsset.Events.Length - 1 );
        CurrentFrame = Mathf.Clamp( CurrentFrame, 0, CurrentEventPtr.Frames.Length - 1 );

        return CurrentFramePtr;
    }

    public void Hide( )
    {
        this.gameObject.active = false;
    }

    public void Show( )
    {
        this.gameObject.active = true;
    }

    public void SetExternalMaterialInEdit( Material newMat )
    {
        renderer.sharedMaterial = newMat;
        //mat = newMat;
    }

    public void EnableAndShow( )
    {
        renderer.enabled = true;
    }

    public void DisableAndHide( )
    {
        renderer.enabled = false;
    }

    internal bool WasPlaying;
    internal bool WasHidden;
    /// <summary>
    /// Pauses current animation
    /// </summary>
    public void OnPause( object param )
    {
        if ( Debug.isDebugBuild )
        {
            Debug.Log( "Animation2D paused" );
        }
        isPaused = true;
        WasPlaying = IsPlaying;
        WasHidden = gameObject.active;
        Stop( false );
        if ( !WasHidden )
        {
            ( GetComponent( typeof( MeshRenderer ) ) as MeshRenderer ).enabled = false;
        }
        
    }

    public void OnResume( object param )
    {
        if ( Debug.isDebugBuild )
        {
            Debug.Log( "Animation2D resumed" );
        }

        isPaused = false;
        if ( !WasHidden )
        {
            (GetComponent( typeof( MeshRenderer ) ) as MeshRenderer).enabled = true;
            Show( );
        }
        if ( WasPlaying )
        {
            Play( );
        }
    }
 
    void DumpVertexes( )
    {
        if ( Debug.isDebugBuild )
        {
            Vector3 [] vecs = ( GetComponent( typeof( MeshFilter ) ) as MeshFilter ).sharedMesh.vertices;
            for ( int i = 0; i < vecs.Length; ++i )
            {
                Debug.Log( i.ToString( ) + ": " + vecs [ i ].ToString( ) );
            }
        }
    }

    public static void SetFrameWithoutScript( AnimationAsset asset, GameObject go, Rect uvRect, int selectedEvent, int selectedFrame )
    {
        Animation2DFrame frame = asset.Events [ selectedEvent ].Frames [ selectedFrame ];
        MeshFilter filter = go.GetComponent( typeof( MeshFilter ) ) as MeshFilter;
        if ( filter != null )
        {
            DestroyImmediate( filter );            
        }
        filter = go.AddComponent( typeof( MeshFilter ) ) as MeshFilter;
        if ( go.GetComponent( typeof( MeshRenderer ) ) == null )
        {
            go.AddComponent( typeof( MeshRenderer ) );
        }
        go.renderer.sharedMaterial = asset.Materials[ frame.MaterialIndex ];

        Mesh annMesh = new Mesh( );
        filter.sharedMesh = annMesh;        
        annMesh.name = go.name;
        annMesh.Clear( );

        annMesh.vertices = new Vector3 [ 4 ];
        annMesh.uv = new Vector2 [ 4 ];
        annMesh.colors = new Color [ 4 ];
        annMesh.triangles = new int [ 6 ];       

        Animation2D.SetFrameWithoutScript( frame, go, annMesh, uvRect, annMesh.uv, annMesh.vertices );
    }

    public static void SetFrameWithoutScript( Animation2DFrame frame, GameObject go, Mesh annMesh, Rect uvRect, Vector2[] uvs, Vector3[] vertices )
    {        
        if ( frame.WindingCCW )
        {
            annMesh.triangles = Animation2D.Faces [ ( int ) WindingMode.CCW ];

            //Rect uvRect = frame.uvRect;
            uvs [ 0 ].x = uvRect.x; uvs [ 0 ].y = uvRect.yMax;
            uvs [ 1 ].x = uvRect.x; uvs [ 1 ].y = uvRect.y;
            uvs [ 2 ].x = uvRect.xMax; uvs [ 2 ].y = uvRect.y;
            uvs [ 3 ].x = uvRect.xMax; uvs [ 3 ].y = uvRect.yMax;        
            annMesh.uv = uvs;
        }
        else
        {
            annMesh.triangles = Animation2D.Faces [ ( int ) WindingMode.CW ];

            //Rect uvRect = frame.uvRect;
            uvs [ 3 ].x = uvRect.x; uvs [ 3 ].y = uvRect.yMax;
            uvs [ 2 ].x = uvRect.x; uvs [ 2 ].y = uvRect.y;
            uvs [ 1 ].x = uvRect.xMax; uvs [ 1 ].y = uvRect.y;
            uvs [ 0 ].x = uvRect.xMax; uvs [ 0 ].y = uvRect.yMax;
            annMesh.uv = uvs;
        }

        Rect ptRect = frame.ptRect;
        Vector2 framePos = frame.FramePos;

        vertices [ 0 ].x = framePos.x + -ptRect.xMax;
        vertices [ 0 ].y = framePos.y + -ptRect.yMax;
        vertices [ 0 ].z = 0F;
        
        vertices [ 1 ].x = framePos.x + ptRect.x;
        vertices [ 1 ].y = framePos.y + ptRect.y;
        vertices [ 1 ].z = 0F;

        vertices [ 2 ].x = framePos.x + ptRect.xMax;
        vertices [ 2 ].y = framePos.y + ptRect.yMax;
        vertices [ 2 ].z = 0F;

        vertices [ 3 ].x = framePos.x + -ptRect.x;
        vertices [ 3 ].y = framePos.y + -ptRect.y;
        vertices [ 3 ].z = 0F;

        annMesh.vertices = vertices;

        annMesh.RecalculateBounds( );                
    }

    public void SetEvent( int frameNo, Animation2DEventHandler handler )
    {
        eventFrame = frameNo;
        eventHandler = handler;
    }

    public void ClearEvent( )
    {
        eventFrame = -100;
        eventHandler = null;
    }

    public Animation2D Clone( )
    {
        GameObject newObj = GameObject.Instantiate( gameObject ) as GameObject;
        Animation2D newAnn = newObj.GetComponent( typeof( Animation2D ) ) as Animation2D;
        newAnn.InitAnimationMesh( );
        newAnn.transform.parent = transform.parent;
        newAnn.EventsAsset = this.EventsAsset;
        newAnn.CurrentFrame = this.CurrentFrame;
        newAnn.CurrentEvent = this.CurrentEvent;
        newAnn.Interpolate = this.Interpolate;
        newAnn.SetFrame( );                

        return newAnn;
    }

    public Animation2D CloneAnimationOnly( )
    {
        GameObject newObj = new GameObject( name );
        newObj.transform.position = transform.position;
        newObj.transform.parent = transform.parent;
        newObj.transform.localPosition = transform.localPosition;
        newObj.transform.localRotation = transform.localRotation;
        newObj.transform.localScale = transform.localScale;
        newObj.AddComponent( typeof( MeshRenderer ) );
        newObj.AddComponent( typeof( MeshFilter ) );
        Animation2D newAnn = newObj.AddComponent( typeof( Animation2D ) ) as Animation2D;        
        newObj.renderer.sharedMaterial = renderer.sharedMaterial;
        newAnn.InitAnimationMesh( );
        newAnn.Initialize( );
        newAnn.transform.parent = transform.parent;
        newAnn.EventsAsset = this.EventsAsset;
        newAnn.CurrentFrame = this.CurrentFrame;
        newAnn.CurrentEvent = this.CurrentEvent;
        newAnn.Interpolate = this.Interpolate;
        newAnn.SetFrame( );

        return newAnn;
    }
}

// delegates
public delegate void Animation2DEventHandler( Animation2D source );

public class StringHelper
{
    public static bool IsNullOrEmpty( string value )
    {
        return ( value == null || value == "" );
    }

    public static string Remove( string value, int index )
    {
        return value.Remove( index, value.Length - index );
    }
}