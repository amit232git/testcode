using UnityEngine;
using System.Collections;
using System;

[AddComponentMenu( "Animation2D/ObserverCollider" )]
public class ObserverCollider : MonoBehaviour
{
    #region events

    public event ObserverColliderTouchEventHandler      TouchUp;
    public event ObserverColliderTouchEventHandler      TouchDown;

    public event ObserverColliderTouchEventHandler      Click;
    public event ObserverColliderTouchEventHandler      DoubleClick;
    public event ObserverColliderTouchEventHandler      LongClick;

    public event ObserverColliderTouchEventHandler      BeginMove;
    public event ObserverColliderTouchEventHandler      Moving;
    public event ObserverColliderTouchEventHandler      EndMove;

    public event ObserverColliderTouchEventHandler      FocusOn;
    public event ObserverColliderTouchEventHandler      FocusOff;

    #endregion

    #region variables

    public Animation2D      animation2D;
    public bool             IsDraggable     = false;
    public bool             IsDragging      = false;
    public Vector3          ColliderSize    = Vector3.zero;
    public Vector3          ColliderPos     = Vector3.zero;
    [NonSerialized]
    public object           UserData;
    //public bool             AlphaTest       = false;
    //public float            MinAlpha        = 0F;
    //public bool             UsePhysics      = false;

    private bool            hasFocus        = false;

    public bool HasFocus
    {
        get { return hasFocus; }
        set { hasFocus = value; }
    }

    private Transform       trans;

    public Transform Trans
    {
        get { return trans; }
    }

    private Rect            rcBounds;
    private Bounds          bounds;
    private Renderer        ren;

    #endregion

    public void UpdateInit( )
    {
        trans = transform;
    }

    void Awake( )
    { 
        ren = renderer;
        trans = transform;
    }

    void Start( )
    {
        if ( enabled )
        {
            DragObserver.Instance.AddClient( this );
        }
    }

    void Reset( )
    {
        animation2D = GetComponent( typeof( Animation2D ) ) as Animation2D;
        ren = renderer;
        trans = transform;
        if ( animation2D != null )
        {
            UpdateCollider( );            
        }
    }

    public void UpdateCollider( )
    {
        Bounds rcBounds = ren.bounds;
        ColliderPos = trans.InverseTransformPoint( rcBounds.center );
        ColliderSize = rcBounds.size;
    }
   
    public void OnBeginMove( ObserverTouch touch )
    {
        if ( BeginMove != null )
        {
            BeginMove( touch );
        }        
    }

    public void OnMoving( ObserverTouch touch )
    {
        if ( Moving != null )
        {
            Moving( touch );
        }
    }

    public void OnEndMove( ObserverTouch touch )
    {
        if ( EndMove != null )
        {
            EndMove( touch );
        }
    }  

    public void OnTouchDown( ObserverTouch touch )
    {
        if ( TouchDown != null )
        {
            TouchDown( touch );
        }
    }

    public void OnTouchUp( ObserverTouch touch )
    {
        if ( TouchUp != null )
        {
            TouchUp( touch );
        }
    }

    public void OnClick( ObserverTouch touch )
    {
        if ( Click != null )
        {
            Click( touch );
        }
    }

    public void OnLongClick( ObserverTouch touch )
    {
        if ( LongClick != null )
        {
            LongClick( touch );
        }
    }

    public void OnDoubleClick( ObserverTouch touch )
    {
        if ( DoubleClick != null )
        {
            DoubleClick( touch );
        }
    }

    public void OnFocusOn( ObserverTouch touch )
    {
        hasFocus = true;

        if ( FocusOn != null )
        {
            FocusOn( touch );
        }
    }

    public void OnFocusOff( ObserverTouch touch )
    {
        HasFocus = false;

        if ( FocusOff != null )
        {
            FocusOff( touch );
        }
    }

    public Bounds RecalculateBounds( )
    {
        bounds.center = trans.position + ( Vector3.Scale( ColliderPos, trans.localScale ) );
        bounds.size = Vector3.Scale( ColliderSize, trans.localScale );

        return bounds;
    }

    public bool HitTest( Vector3 position )
    {
        if ( !enabled || !gameObject.active )
        {
			
            return false;
        }
        RecalculateBounds( );
        rcBounds.xMin = bounds.min.x;
        rcBounds.yMin = bounds.min.y;
        rcBounds.xMax = bounds.max.x;
        rcBounds.yMax = bounds.max.y;

        return rcBounds.Contains( position );
    }

    public bool HitTest( RaycastHit raycastHit )
    {
        return enabled && gameObject.active;
    }    
}

public delegate void ObserverColliderTouchEventHandler( ObserverTouch touch );