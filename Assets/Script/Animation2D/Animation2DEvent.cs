using UnityEngine;
using System.Collections;

[System.Flags]
public enum AnimationFlags : int
{
    EVENT_ONCE                  = 0x0000,
    EVENT_LOOP                  = 0x0001,
    EVENT_HIDE_AFTER_STOPPED    = 0x0002,
    EVENT_DONT_STOP_WAV         = 0x0004,
    EVENT_PING_PONG             = 0x0008,
    EVENT_REWIND                = 0x0010,
    EVENT_REVERSE               = 0x1000,
};

[System.Serializable( )]
public class Animation2DEvent : System.Object
{
    public float            Fps;
    public AnimationFlags   Flags; 
    public Animation2DFrame[] Frames;

    public Animation2DEvent( )
    {
        Frames = null;
        Flags = AnimationFlags.EVENT_ONCE;
        Fps = 0.0f;
    }

    public Animation2DEvent( float fps, AnimationFlags flags, Animation2DFrame [] frames )
    {
        this.Fps = fps;
        this.Frames = frames;
        this.Flags = flags;

    }

    public Animation2DEvent( float fps, int flags, int framesNo )
    {
        this.Fps = fps;
        this.Flags = ( AnimationFlags ) flags;
        this.Frames = new Animation2DFrame [ framesNo ];
    }

    public void SetFrame( int idx, Animation2DFrame frame )
    {
        Frames [ idx ] = frame;
    }

    public bool Loop
    {
        get { return ( Flags & AnimationFlags.EVENT_LOOP ) != 0; }
    }

    public bool HideAfterStop
    {
        get { return ( Flags & AnimationFlags.EVENT_HIDE_AFTER_STOPPED ) != 0; }
    }

    public bool DontStopWav
    {
        get { return ( Flags & AnimationFlags.EVENT_DONT_STOP_WAV ) != 0; }
    }
}