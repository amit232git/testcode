using UnityEngine;
using System.Collections;
using System.Reflection;

[System.Serializable]
public class EventDispatcher
{
    private ArrayList   delegates   = new ArrayList( );
    private bool        dispatching = false;
    private int         idx;
    private int         count;
    
    
    public void AttachListener( System.Delegate delegateMethod )
    {
        if ( !delegates.Contains( delegateMethod ) )
        {
            delegates.Add( delegateMethod );
        }
    }

    public void RemoveListener( System.Delegate delegateMethod )
    {
        if ( dispatching && HasListener( delegateMethod ) )
        {
            idx--;
            count--;
        }

        delegates.Remove( delegateMethod );
    }

    public bool HasListener( System.Delegate delegateMethod )
    {
        return delegates.Contains( delegateMethod );
    }

    public static EventDispatcher operator +( EventDispatcher evt, System.Delegate delegateMethod )
    {
        evt.AttachListener( delegateMethod );

        return evt;
    }

    public static EventDispatcher operator -( EventDispatcher evt, System.Delegate delegateMethod )
    {
        evt.RemoveListener( delegateMethod );

        return evt;
    }

    public void Dispatch( params object [] parameters )
    {
        dispatching = true;
        count = delegates.Count;
        idx = 0;
        
        while ( idx < count )        
        {
            //Debug.Log( "Dispatching " + idx + " / " + count + " : " + (delegates[idx] as System.Delegate).Method + " : " + (delegates[idx] as System.Delegate).Target );
            (delegates[idx] as System.Delegate).DynamicInvoke( parameters );                    

            idx++;
        }

        dispatching = false;
    }

    object [] objTemp = new object [ 1 ];

    public void Dispatch( object param )
    {
        objTemp [ 0 ] = param;

        Dispatch( new object [ 1 ] { param } );
    }

    public void Dispatch( )
    {
        Dispatch( ( object [] ) null );
    }

    public override string ToString( )
    {
        string desc = "[EventsDispatcher] " + delegates.Count;

        return desc;
    }

    
}