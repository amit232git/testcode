using UnityEngine;
using System.Collections;

[AddComponentMenu("Animation2D/Animate")]

public delegate float EaseFloat( float time, float valueStart, float valueEnd, float duration );
public delegate Vector3 EaseVector3( float time, Vector3 valueStart, Vector3 valueEnd, float duration );


public class Animate : MonoBehaviour
{
    public static readonly EaseFloat [] FloatDelegates = new EaseFloat [] 
    {
        Equations.EaseNone,
        Equations.EaseInQuad,
        Equations.EaseOutQuad,
        Equations.EaseInOutQuad
    };

    //public EaseVector3 [] Vector3Delegates = new EaseVector3 [] 
    //{
    //    Equations.EaseNone,
    //    Equations.EaseInQuad,
    //    Equations.EaseOutQuad,
    //    Equations.EaseInOutQuad
    //};

    public static bool StopSequence = false;
 
    public interface IAnimateKeyFrame
    {
        void Start();
        void Stop();
        bool Update();
        bool IsEqual(object obj);
        int GetId();
    }

    // EaseKeyFrame
    //
    public class EaseFloatKeyFrame : IAnimateKeyFrame
    {
        public int Id;
        protected EaseFloat easeFloatDelegate;
        protected System.Reflection.FieldInfo animatedField;
        protected object target;
        protected float valueStart;
        protected float valueEnd;
        protected float duration;
        protected float elapsed;

        public EaseFloatKeyFrame( int id, string propertyName, object target, float valueStart, float valueEnd, float duration, Ease easeFunction )
        {
            Id = id;
            this.target = target;
            this.valueStart = valueStart;
            this.valueEnd = valueEnd;
            this.duration = duration;
            easeFloatDelegate = FloatDelegates [ ( int )easeFunction ];

            animatedField = target.GetType( ).GetField( propertyName );
        }

        public bool Update( )
        {
            elapsed += Time.deltaTime;
            if ( elapsed > duration )
            {
                animatedField.SetValue( target, valueEnd );

                return false;
            }
            animatedField.SetValue( target, easeFloatDelegate( elapsed, valueStart, valueEnd, duration ) );

            return true;
        }

        public void Start( )
        {            
            elapsed = 0;
        }

        public void Stop( )
        {
        }


        public bool IsEqual( object obj )
        {
            return false;
        }

        public int GetId( )
        {
            return Id;
        }
    }

    public interface ICommand
    {
        void Execute( );
    }

    public class SetVisibleAnimation2D : ICommand
    {
        public Animation2D Source;
        public bool IsVisible;

        public SetVisibleAnimation2D( Animation2D src, bool show )
        {
            Source = src;
            IsVisible = show;
        }

        public void Execute( )
        {
            if ( IsVisible )
            {
                Source.Show( );
            }
            else
            {
                Source.Hide( );
            }
        }
    }

    public class SetLocalPosition : ICommand
    {
        public Transform Source;
        public Vector3 LocalPosition;

        public SetLocalPosition( Transform src, Vector3 localPos)
        {
            Source = src;
            LocalPosition = localPos;
        }

        public void Execute( )
        {
            Source.localPosition = LocalPosition;
        }
    }

    public class SetLocalScale: ICommand
    {
        public Transform Source;
        public Vector3 LocalScale;

        public SetLocalScale( Transform src, Vector3 localScale )
        {
            Source = src;
            LocalScale = localScale;
        }

        public void Execute( )
        {
            Source.localScale = LocalScale;
        }
    }

    public class CommandGroupKeyFrame : IAnimateKeyFrame
    {
        public int Id;
        public ICommand [] Commands;

        #region IAnimateKeyFrame Members

        public CommandGroupKeyFrame( int id, params ICommand [] commands )
        {
            Id = id;
            Commands = commands;
        }

        public void Start( )
        {
            for ( int i = 0; i < Commands.Length; ++i )
            {
                Commands [ i ].Execute( );
            }
        }

        public void Stop( )
        {
            
        }

        public bool Update( )
        {
            return false;
        }

        public bool IsEqual( object obj )
        {
            return false;
        }

        public int GetId( )
        {
            return Id;
        }

        #endregion
    }



    // MoveToKeyFrame
    //
    public class MoveToKeyFrame : IAnimateKeyFrame
    {
        public const float MinArrivalDistance = 0.005f;

        public Transform Source;
        public Vector3 Destination;
        public float Speed;
        public Vector3 StartPos;
        public float Elapsed;
        public int Id;
        private bool isPlaying;

        public MoveToKeyFrame(int id, Transform src, Vector3 dst, float speed)
        {
            Source = src;
            Destination = dst;
            Speed = speed;
            Id = id;
        }

        public bool Update()
        {
            Elapsed += Time.deltaTime * Speed;
            if ( Elapsed > 1F )
            {
                isPlaying = false;
                Elapsed = 1F;
            }
            Source.localPosition = Vector3.Lerp(StartPos, Destination, Elapsed);

            return isPlaying;
        }

        public void Start()
        {
            StartPos = Source.localPosition;
            Elapsed = 0;
            isPlaying = true;
        }

        public void Stop()
        {
        }


        public bool IsEqual(object obj)
        {
            MoveToKeyFrame keyFrame = obj as MoveToKeyFrame;
            if (keyFrame != null)
            {
                //if (keyFrame.Source == this.Source)
                //{
                //    Debug.Log("EQUAL :" + keyFrame.Source.name + " ?= " + this.Source.name);
                //}
                return keyFrame.Source == this.Source;
            }

            return false;
        }

        public int GetId()
        {
            return Id;
        }
    }

    public class DelayKeyframe : IAnimateKeyFrame
    {        
        public int Id;
        public float Delay;

        private float elapsed;

        public DelayKeyframe( int id, float delay)
        {
            Id = id;
            Delay = delay;
        }

        public bool Update( )
        {
            elapsed += Time.deltaTime;

            return ( elapsed < Delay );
        }

        public void Start( )
        {
            elapsed = 0F;
        }

        public void Stop( )
        {
        }

        public bool IsEqual( object obj )
        {
            return false;
        }

        public int GetId( )
        {
            return Id;
        }
    }



    // InvokeDelagate
    //    

    public class InvokeDelagate : IAnimateKeyFrame
    {
        public AnimateInvokeHandler handler;
        public int Id;
        public object invokeParam;


        public InvokeDelagate( int id, AnimateInvokeHandler hdlr, object param )
        {
            Id = id;
            handler = hdlr;
            invokeParam = param;
        }

        public bool Update( )
        {
            handler( Id, invokeParam );
            return false;
        }

        public void Start( )
        {
        }

        public void Stop( )
        {
        }

        public bool IsEqual( object obj )
        {
            return false;
        }

        public int GetId( )
        {
            return Id;
        }
    }


    public class MoveToKeyFrameParam : IAnimateKeyFrame
    {
        TweenPositionObject tween;
        public Transform Source;
        public int Id;

        public MoveToKeyFrameParam(int id, Transform src, Vector3 dst, float speed, float delay, Ease ease)
        {
            Id = id;

            tween = new TweenPositionObject();
            Source = src;
            tween.ease = ease;
            tween.totalTime = speed;
            tween.startValue = src.position;
            tween.tweenValue = dst; 
            tween.startTime = Time.time;
            tween.delay = delay;
            if (delay != 0F)
            {
                tween.canStart = false;
            }
            else
            {
                tween.canStart = true;
            }

            tween.Init();
        }

        public bool Update()
        {
            this.DetectDelay();
            return this.UpdateTween();
        }

        private void DetectDelay()
        {
            if (Time.time > tween.startTime + tween.delay && !tween.canStart)
            {
                tween.canStart = true;
            }
        }

        public bool UpdateTween()
        {
            if (tween.canStart && !tween.ended)
            {
                Vector3 begin = tween.startValue;
                Vector3 finish = tween.tweenValue;
                Vector3 change = finish - begin;
                float duration = tween.totalTime;
                float currentTime = Time.time - (tween.startTime + tween.delay);

                if (duration == 0)
                {
                    this.EndTween(tween);
                    Source.position = finish;
                    return false;
                }


                if (Time.time > tween.startTime + tween.delay + duration)
                {
                    this.EndTween(tween);
                    Source.position = finish;
                    return false;
                }

                Source.position = Equations.ChangeVector(currentTime, begin, change, duration, tween.ease);
                return true;
            }
            else
            {
                return true;
            }
        }

        public void Start()
        {
        }

        public void Stop()
        {
        }

        private void EndTween(BaseTweenObject tween)
        {
            tween.ended = true;
        }

        public bool IsEqual(object obj)
        {
            MoveToKeyFrameParam keyFrame = obj as MoveToKeyFrameParam;
            if (keyFrame != null)
            {
                return keyFrame.Source == this.Source;
            }

            return false;
        }

        public int GetId( )
        {
            return Id;
        }
    }

    // AnRotateToKeyFrame
    public class AnnRotateToKeyFrame : IAnimateKeyFrame
    {
        public const float MinArrivalDistance = 0.05f;

        public Animation2D Source;
        public float Destination;
        public float Speed;
        public float StartPos;
        public float Elapsed;
        public int Id;

        float Current;

        public AnnRotateToKeyFrame(int id, Animation2D src, float dst, float speed)
        {
            Source = src;
            Destination = dst;
            Speed = speed;
            Id = id;
        }

        public bool Update()
        {
            bool retVal = true;
            Elapsed += Time.deltaTime * Speed;
            //Current = Mathf.Lerp( Current /*StartPos*/, Destination, Elapsed );
            Current = Mathf.Lerp(Current /*StartPos*/, Destination, Time.deltaTime * Speed);

            if (Mathf.Abs(Current - Destination) < MinArrivalDistance)
            {
                Current = Destination;

                retVal = false;
            }
            //Source.Angle = Current;
            Source.SetFrame();

            return retVal;

        }

        public void Start()
        {
            //StartPos = Source.Angle;
            Current = StartPos;
            Elapsed = 0;
        }

        public void Stop()
        {
        }

        public bool IsEqual(object obj)
        {
            AnnRotateToKeyFrame keyFrame = obj as AnnRotateToKeyFrame;
            if (keyFrame != null)
            {
                return keyFrame.Source == this.Source;
            }

            return false;
        }

        public int GetId( )
        {
            return Id;
        }
    }

    public class MoveToAnimation2DKeyFrame : IAnimateKeyFrame
    {
        public const float MinArrivalDistance = 0.005f;

        public Animation2D Source;
        public Vector2 Destination;
        public float Speed;
        public Vector2 StartPos;
        public float Elapsed;
        public int Id;
        public Vector2 TargetPos;

        public MoveToAnimation2DKeyFrame(int id, Animation2D src, Vector2 dst, float speed)
        {
            Source = src;
            Destination = dst;
            Speed = speed;
            Id = id;
        }

        public bool Update()
        {
            Elapsed += Time.deltaTime * Speed;
            TargetPos.x = Mathf.Lerp(StartPos.x, Destination.x, Elapsed);
            TargetPos.y = Mathf.Lerp(StartPos.y, Destination.y, Elapsed);
            //Source.localPosition = Vector3.Lerp( StartPos, Destination, Elapsed );

            if ((TargetPos - Destination).magnitude < MinArrivalDistance)
            {
                TargetPos = Destination;
                //Source.SetAbsolutePosition(TargetPos);

                return false;
            }
            //Source.SetAbsolutePosition(TargetPos);

            return true;
        }

        public void Start()
        {
            //StartPos = Source.GetAbsolutePosition();
            Elapsed = 0;
        }

        public void Stop()
        {
        }


        public bool IsEqual(object obj)
        {
            MoveToAnimation2DKeyFrame keyFrame = obj as MoveToAnimation2DKeyFrame;
            if (keyFrame != null)
            {
                return keyFrame.Source == this.Source;
            }

            return false;
        }

        public int GetId( )
        {
            return Id;
        }
    }

    public class SizeCamOrtho : IAnimateKeyFrame
    {
        public const float MinArrivalDistance = 0.01f;

        public Camera Cam;
        public float Speed;
        public float Target;
        public float Elapsed;
        public int Id;
        public SizeCamOrtho(int id, float target, float speed)
            : this(id, Camera.main, target, speed) { }


        public SizeCamOrtho(int id, Camera c, float target, float speed)
        {
            Cam = c;
            Target = target;
            Speed = speed;
            Id = id;
        }

        public bool Update()
        {
            //	Debug.Log("LLLLLLLLLL");
            //	Debug.Log(Target);
            Elapsed += Time.deltaTime * Speed;
            Cam.orthographicSize = Mathf.Lerp(Cam.orthographicSize, Target, Elapsed);
            //    TargetPos.x = Mathf.Lerp( StartPos.x, Destination.x, Elapsed );
            //  TargetPos.y = Mathf.Lerp( StartPos.y, Destination.y, Elapsed );
            //Source.localPosition = Vector3.Lerp( StartPos, Destination, Elapsed );

            if (Mathf.Abs(Target - Cam.orthographicSize) < MinArrivalDistance)
            {
                Cam.orthographicSize = Target;
                //      Source.SetAbsolutePosition( TargetPos );

                return false;
            }
            //Source.SetAbsolutePosition( TargetPos );

            return true;
        }

        public void Start()
        {
            //StartPos = Source.GetAbsolutePosition();
            Elapsed = 0;
        }

        public void Stop()
        {
        }


        public bool IsEqual(object obj)
        {
            SizeCamOrtho keyFrame = obj as SizeCamOrtho;
            if (keyFrame != null)
            {
                return keyFrame.Cam == this.Cam;
            }

            return false;
        }

        public int GetId( )
        {
            return Id;
        }
    }

    public class MusicFade : IAnimateKeyFrame
    {
        public const float MinArrivalDistance = 0.01f;

        public AudioSource Audio;
        public float Speed;
        public float Target;
        public float Elapsed;
        public int Id;

        public MusicFade(int id, AudioSource a, float target, float speed)
        {
            Audio = a;
            Target = target;
            Speed = speed;
            Id = id;
        }

        public bool Update()
        {
            //	Debug.Log("LLLLLLLLLL");
            //	Debug.Log(Target);
            Elapsed += Time.deltaTime * Speed;
            Audio.volume = Mathf.Lerp(Audio.volume, Target, Elapsed);
            //    TargetPos.x = Mathf.Lerp( StartPos.x, Destination.x, Elapsed );
            //  TargetPos.y = Mathf.Lerp( StartPos.y, Destination.y, Elapsed );
            //Source.localPosition = Vector3.Lerp( StartPos, Destination, Elapsed );

            if (Mathf.Abs(Target - Audio.volume) < MinArrivalDistance)
            {
                Audio.volume = Target;
                //      Source.SetAbsolutePosition( TargetPos );

                return false;
            }
            //Source.SetAbsolutePosition( TargetPos );

            return true;
        }

        public void Start()
        {
            //StartPos = Source.GetAbsolutePosition();
            Elapsed = 0;
        }

        public void Stop()
        {
        }


        public bool IsEqual(object obj)
        {
            MusicFade keyFrame = obj as MusicFade;
            if (keyFrame != null)
            {
                return keyFrame.Audio == this.Audio;
            }

            return false;
        }

        public int GetId( )
        {
            return Id;
        }
    }

    // PlayAnimation2DKeyFrame
    //
    public class PlayAnimation2DKeyFrame : IAnimateKeyFrame
    {
        public const float MinArrivalDistance = 0.05f;

        public Animation2D Source;
        public int EventName;
        public int Id;
        public bool Playing;
        public Animation2DEventHandler handler;
        public int frameNo;
        

        public PlayAnimation2DKeyFrame(int id, Animation2D anim, int eventName)
            : this( id, anim, eventName, -1, null )
        {

        }

        public PlayAnimation2DKeyFrame(int id, Animation2D anim, int eventName,int evtFrame, Animation2DEventHandler hdlr )
        {
            Source = anim;
            EventName = eventName;
            Id = id;
            handler = hdlr;
            frameNo = evtFrame;
        }

        public bool Update()
        {
            return Playing;
        }

        public void Start()
        {
            Playing = true;
            if ( frameNo >= 0 && handler != null )
            {
                Source.SetEvent( frameNo, handler );
            }
            
            Source.Finished += new Animation2DEventHandler(OnAnimationFinished);
            //Debug.Log( "Playing animation " + EventName );
            Source.Play(EventName);
        }

        void OnAnimationFinished(Animation2D source)
        {
            //Debug.Log( "OnFinished " + Id );
            Source.Finished -= new Animation2DEventHandler(OnAnimationFinished);
            Playing = false;
        }

        public void Stop()
        {
            Source.Stop();

            Playing = false;
        }


        public bool IsEqual(object obj)
        {
            //PlayAnimation2DKeyFrame keyFrame = obj as PlayAnimation2DKeyFrame;
            //if (keyFrame != null)
            //{
            //    return keyFrame.Source == this.Source;
            //}

            return false;
        }

        public int GetId( )
        {
            return Id;
        }
    }

    //////////////////////////////////////////////////////////////////////////

    // RotateToKeyFrame
    public class RotateToKeyFrame : IAnimateKeyFrame
    {
        public const float MinArrivalDistance = 0.05f;

        public Transform Source;
        public Quaternion Destination;
        public float Speed;
        public Quaternion StartPos;
        public float Elapsed;
        public int Id;

        public RotateToKeyFrame(int id, Transform src, Quaternion dst, float speed)
        {
            Source = src;
            Destination = dst;
            Speed = speed;
            Id = id;
        }

        public bool Update()
        {
            Elapsed += Time.deltaTime * Speed;
            Source.localRotation = Quaternion.Slerp(StartPos, Destination, Elapsed);

            if (Quaternion.Angle(Source.localRotation, Destination) < MinArrivalDistance)
            {
                Source.localRotation = Destination;

                return false;
            }

            return true;

        }

        public void Start()
        {
            StartPos = Source.localRotation;
            Elapsed = 0;
        }

        public void Stop()
        {
        }

        public bool IsEqual(object obj)
        {
            RotateToKeyFrame keyFrame = obj as RotateToKeyFrame;
            if (keyFrame != null)
            {
                return keyFrame.Source == this.Source;
            }

            return false;
        }

        public int GetId( )
        {
            return Id;
        }

    }

    public class RotateToKeyFrameParam : IAnimateKeyFrame
    {
        TweenRotationObject tween;
        public Transform Source;
        public int Id;

        public RotateToKeyFrameParam(int id, Transform src, Vector3 dst, float speed, float delay, Ease ease)
        {
            Id = id;

            tween = new TweenRotationObject();
            Source = src;
            tween.ease = ease;
            tween.totalTime = speed;
            tween.startValue = src.rotation.eulerAngles;
            tween.tweenValue = dst;
            tween.startTime = Time.time;
            tween.delay = delay;
            if (delay != 0F)
            {
                tween.canStart = false;
            }
            else
            {
                tween.canStart = true;
            }

            tween.Init();
        }

        public bool Update()
        {
            this.DetectDelay();
            return this.UpdateTween();
        }

        private void DetectDelay()
        {
            if (Time.time > tween.startTime + tween.delay && !tween.canStart)
            {
                tween.canStart = true;
            }
        }

        public bool UpdateTween()
        {
            if (tween.canStart && !tween.ended)
            {
                Vector3 begin = tween.startValue;
                Vector3 finish = tween.tweenValue;
                Vector3 change = finish - begin;
                float duration = tween.totalTime;
                float currentTime = Time.time - (tween.startTime + tween.delay);

                if (duration == 0)
                {
                    this.EndTween(tween);
                    Source.transform.eulerAngles = finish;
                    return false;
                }


                if (Time.time > tween.startTime + tween.delay + duration)
                {
                    this.EndTween(tween);
                    Source.transform.eulerAngles = finish;
                    return false;
                }

                Source.rotation = Quaternion.Euler(Equations.ChangeVector(currentTime, begin, change, duration, tween.ease));
                return true;
            }
            else
            {
                return true;
            }
        }

        public void Start()
        {
        }

        public void Stop()
        {
        }

        private void EndTween(BaseTweenObject tween)
        {
            tween.ended = true;
        }

        public bool IsEqual(object obj)
        {
            RotateToKeyFrameParam keyFrame = obj as RotateToKeyFrameParam;
            if (keyFrame != null)
            {
                return keyFrame.Source == this.Source;
            }

            return false;
        }

        public int GetId( )
        {
            return Id;
        }
    }    //////////////////////////////////////////////////////////////////////////

    // RotateToKeyFrame
    public class RotateAroundKeyFrame : IAnimateKeyFrame
    {
        public const float MinArrivalDistance = 0.005f;

        public Transform Source;
        public float Speed;
        public float EndAngle;
        public Vector3 Point;
        public Vector3 Around;
        public float Elapsed;
        public int Id;

        private float curAngle;
        private float lastAngle;

        public RotateAroundKeyFrame(int id, Transform src, Vector3 point, Vector3 around, float end, float speed)
        {
            Source = src;
            Point = point;
            Around = around;
            EndAngle = end;
            Speed = speed;
            Id = id;
        }

        public bool Update()
        {
            //Elapsed += Time.deltaTime * Speed;
            curAngle = Mathf.Lerp(curAngle, EndAngle, Time.deltaTime * Speed);
            if (Mathf.Abs(curAngle - EndAngle) < MinArrivalDistance)
            {
                Source.RotateAround(Point, Around, EndAngle - lastAngle);

                return false;
            }
            Source.RotateAround(Point, Around, curAngle - lastAngle);
            lastAngle = curAngle;

            return true;

        }

        public void Start()
        {
            curAngle = Source.localEulerAngles.z;
            lastAngle = curAngle;
            Elapsed = 0;
        }

        public void Stop()
        {
        }

        public bool IsEqual(object obj)
        {
            RotateAroundKeyFrame keyFrame = obj as RotateAroundKeyFrame;
            if (keyFrame != null)
            {
                return keyFrame.Source == this.Source;
            }

            return false;
        }

        public int GetId( )
        {
            return Id;
        }

    }

    // ScaleToKeyFrame
    public class ScaleToKeyFrame : IAnimateKeyFrame
    {
        public const float MinArrivalDistance = 0.05f;

        public Transform Source;
        public Vector3 Destination;
        public float Speed;
        public Vector3 StartPos;
        public float Elapsed;
        public int Id;

        public ScaleToKeyFrame(int id, Transform src, Vector3 dst, float speed)
        {
            Source = src;
            Destination = dst;
            Speed = speed;
            Id = id;
        }

        public bool Update()
        {
            Elapsed += Time.deltaTime * Speed;
            Source.localScale = Vector3.Lerp(StartPos, Destination, Elapsed);

            if ((Source.localScale - Destination).magnitude < MinArrivalDistance)
            {
                Source.localScale = Destination;

                return false;
            }

            return true;

        }

        public void Start()
        {
            Elapsed = 0;
            StartPos = Source.localScale;
        }

        public void Stop()
        {
        }

        public bool IsEqual(object obj)
        {
            ScaleToKeyFrame keyFrame = obj as ScaleToKeyFrame;
            if (keyFrame != null)
            {
                return keyFrame.Source == this.Source;
            }

            return false;
        }

        public int GetId( )
        {
            return Id;
        }

    }

    // ScaleToKeyFrame
    public class ScaleToKeyFrameParam : IAnimateKeyFrame
    {
        TweenPositionObject tween;
        public Transform Source;
        public int Id;

        public ScaleToKeyFrameParam(int id, Transform src, Vector3 dst, float speed, float delay, Ease ease)
        {
            Id = id;

            tween = new TweenPositionObject();
            Source = src;
            tween.ease = ease;
            tween.totalTime = speed;
            tween.startValue = src.localScale;
            tween.tweenValue = dst; 
            tween.startTime = Time.time;
            tween.delay = delay;
            if (delay != 0F)
            {
                tween.canStart = false;
            }
            else
            {
                tween.canStart = true;
            }

            tween.Init();
        }

        public bool Update()
        {
            this.DetectDelay();
            return this.UpdateTween();
        }

        private void DetectDelay()
        {
            if (Time.time > tween.startTime + tween.delay && !tween.canStart)
            {
                tween.canStart = true;
            }
        }

        public bool UpdateTween()
        {
            if (tween.canStart && !tween.ended)
            {
                Vector3 begin = tween.startValue;
                Vector3 finish = tween.tweenValue;
                Vector3 change = finish - begin;
                float duration = tween.totalTime;
                float currentTime = Time.time - (tween.startTime + tween.delay);

                if (duration == 0)
                {
                    this.EndTween(tween);
                    Source.localScale = finish;
                    return false;
                }


                if (Time.time > tween.startTime + tween.delay + duration)
                {
                    this.EndTween(tween);
                    Source.localScale = finish;
                    return false;
                }

                Source.localScale = Equations.ChangeVector(currentTime, begin, change, duration, tween.ease);
                return true;
            }
            else
            {
                return true;
            }
        }

        public void Start()
        {
        }

        public void Stop()
        {
        }

        private void EndTween(BaseTweenObject tween)
        {
            tween.ended = true;
        }

        public bool IsEqual(object obj)
        {
            ScaleToKeyFrameParam keyFrame = obj as ScaleToKeyFrameParam;
            if (keyFrame != null)
            {
                return keyFrame.Source == this.Source;
            }

            return false;
        }

        public int GetId( )
        {
            return Id;
        }

    }

    [System.Flags]
    public enum CameraSettingsFlags : int
    {
        Fov = 0x0001,
        Position = 0x0002,
        Rotation = 0x0004,
        Scale = 0x0008,
        TurnOffAutoCam = 0x0010,
    }

    public class CameraSettings : IAnimateKeyFrame
    {
        public Camera Cam;
        public float Fov;
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Scale;
        public CameraSettingsFlags Flags;
        public int Id;

        CameraSettings(int id, Camera cam, float fov, Vector3 pos, Quaternion rot, Vector3 scale,
            CameraSettingsFlags flags)
        {
            Cam = cam;
            Position = pos;
            Rotation = rot;
            Scale = scale;
            Flags = flags;
            Id = id;
        }

        public bool Update()
        {
            if ((Flags & CameraSettingsFlags.TurnOffAutoCam) != 0)
            {
                //Camera2D cam2D = Cam.gameObject.GetComponent( typeof( Camera2D ) ) as Camera2D;
                //cam2D.FollowTarget = false;
                // turn off autocamera here
            }
            if ((Flags & CameraSettingsFlags.Fov) != 0)
            {
                Cam.fov = Fov;
            }
            if ((Flags & CameraSettingsFlags.Position) != 0)
            {
                Cam.transform.localPosition = Position;
            }
            if ((Flags & CameraSettingsFlags.Rotation) != 0)
            {
                Cam.transform.localRotation = Rotation;
            }
            if ((Flags & CameraSettingsFlags.Scale) != 0)
            {
                Cam.transform.localScale = Scale;
            }

            return false;
        }

        public void Start()
        {
        }

        public void Stop()
        {
        }

        public bool IsEqual(object obj)
        {
            CameraSettings keyFrame = obj as CameraSettings;
            return (keyFrame != null);
        }

        public int GetId( )
        {
            return Id;
        }

    }


    // sequence
    //
    public class SequenceKeyFrame : IAnimateKeyFrame
    {
        public int index;
        public IAnimateKeyFrame[] KeyFrames;
        public int Id;

        public SequenceKeyFrame(int id, IAnimateKeyFrame[] keyFrames)
        {
            Id = id;
            KeyFrames = keyFrames;
        }

        public void Start()
        {
            index = 0;
            KeyFrames[index].Start();
        }

        public void Stop()
        {
        }

        public bool Update()
        {
            if (!KeyFrames[index].Update())
            {
                Animate.Instance.OnFinished((KeyFrames[index] as IAnimateKeyFrame).GetId());
                //   Debug.Log((KeyFrames[index] as IAnimateKeyFrame).GetId( ) );
                KeyFrames[index].Stop();
                index += 1;
                if (index >= KeyFrames.Length || StopSequence)
                {
                    StopSequence = false;
                    KeyFrames[index - 1].Stop();
                    IAnimateKeyFrame frame = Animate.Instance.FindKeyframe(KeyFrames[index - 1]) as IAnimateKeyFrame;
                    if (frame != null)
                    {
                        //  frame.Stop( );
                        //   Animate.Instance.Animations.Remove( frame );
                        //        frame.Stop( );
                        // Animations.Remove( frame );

                    }
                    return false;
                }
                KeyFrames[index].Start();
            }

            return true;
        }

        public bool IsEqual(object obj)
        {
            SequenceKeyFrame keyFrame = obj as SequenceKeyFrame;
            if (keyFrame != null)
            {
                return keyFrame.Id == this.Id;
            }
            if (index < KeyFrames.Length)
            {
                return (KeyFrames[index] as IAnimateKeyFrame).IsEqual(obj);
            }

            return false;
        }

        public int GetId()
        {
            return Id;
        }

    }

    public class SleepKeyFrame : IAnimateKeyFrame
    {
        public float Duration;
        public float Elapsed;
        public int Id;

        public SleepKeyFrame(int id, float dur)
        {
            Duration = dur;
            Id = id;
        }

        public void Start()
        {
            Elapsed = 0;
        }

        public void Stop()
        {

        }

        public bool Update()
        {
            Elapsed += Time.deltaTime;
            return (Elapsed <= Duration);
        }

        public bool IsEqual(object obj)
        {
            //SleepKeyFrame keyFrame = obj as SleepKeyFrame;

            return false;
        }

        public int GetId()
        {
            return Id;
        }

    }

    public class TimeKeyFrame : IAnimateKeyFrame
    {
        public float StartTime;
        public float Elapsed;
        public IAnimateKeyFrame KeyFrame;
        public bool Started;
        public int Id;

        public TimeKeyFrame(int id, float startTime, IAnimateKeyFrame keyFrame)
        {
            StartTime = startTime;
            KeyFrame = keyFrame;
            Id = id;
        }

        public void Start()
        {
            Elapsed = 0;
            Started = false;
        }

        public void Stop()
        {

        }

        public bool Update()
        {
            if (Started)
            {
                return KeyFrame.Update();
            }
            else
            {
                Elapsed += Time.deltaTime;

                if (Elapsed > StartTime)
                {
                    IAnimateKeyFrame frame = Animate.Instance.FindKeyframe(KeyFrame) as IAnimateKeyFrame;
                    if (frame != null)
                    {
                        frame.Stop();
                        Animate.Instance.Animations.Remove(frame);
                    }
                    Started = true;
                    KeyFrame.Start();
                }
            }

            return true;

        }

        public bool IsEqual(object obj)
        {
            TimeKeyFrame keyFrame = obj as TimeKeyFrame;
            if (keyFrame != null)
            {
                return keyFrame.KeyFrame == this.KeyFrame;
            }

            return KeyFrame.IsEqual(obj);
        }

        public int GetId()
        {
            return Id;
        }

    }

    public class SimultaneousKeyFrame : IAnimateKeyFrame
    {
        public IAnimateKeyFrame[] KeyFrames;
        ArrayList playingFrames;
        public int Id;

        public SimultaneousKeyFrame(int id, params IAnimateKeyFrame[] keyFrames)
        {
            Id = id;
            KeyFrames = keyFrames;
            playingFrames = new ArrayList();
        }

        public void Start()
        {
            playingFrames.Clear();
        }

        public void Stop()
        {
        }

        public bool Update()
        {
            int index = 0;
            while (index < playingFrames.Count)
            {
                if (!(playingFrames[index] as IAnimateKeyFrame).Update())
                {
                    Animate.Instance.OnFinished((playingFrames[index] as IAnimateKeyFrame).GetId());
                    playingFrames.RemoveAt(index);

                    continue;
                }

                index++;
            }

            return playingFrames.Count > 0;
        }

        public bool IsEqual(object obj)
        {
            SimultaneousKeyFrame keyFrame = obj as SimultaneousKeyFrame;
            if (keyFrame != null)
            {
                return keyFrame.Id == this.Id;
            }
            for (int i = 0; i < playingFrames.Count; ++i)
            {
                if ((KeyFrames[i] as IAnimateKeyFrame).IsEqual(obj))
                {
                    return true;

                }
            }

            return false;
        }

        public int GetId()
        {
            return Id;
        }


    }

    private static Animate instance = null;

    public static Animate Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType( typeof( Animate ) ) as Animate;
                instance.Initialize();
                if (instance == null)
                {
                    Debug.Log( "Could not locate an Animate object. You have to have exactly one Animate in the scene." );
                }
            }

            return instance;
        }
    }

    public ArrayList Animations;
    //public EventDispatcher EventsDispatcher;

    private bool isInitialized = false;

    void Start()
    {
        Initialize();
    }

    internal void Initialize()
    {
        if (!isInitialized)
        {
            Animations = new ArrayList();
            isInitialized = true;
        }
    }

    public object FindKeyframe(object keyFrame)
    {
        for (int i = 0; i < Animations.Count; ++i)
        {
            if ((Animations[i] as IAnimateKeyFrame).IsEqual(keyFrame))
            {
                return Animations[i];
            }
        }

        return null;
    }

    public void StartKeyFrame(IAnimateKeyFrame keyFrame)
    {
        IAnimateKeyFrame frame = FindKeyframe(keyFrame) as IAnimateKeyFrame;
        if (frame != null)
        {
            frame.Stop();
            Animations.Remove(frame);
        }
        Animations.Add(keyFrame);
        keyFrame.Start();
    }

    public void StopKeyFrame(IAnimateKeyFrame keyFrame)
    {
        keyFrame.Stop();
        Animations.Remove(keyFrame);
    }


    public void Stop(int eventName)
    {
        for (int i = 0; i < Animations.Count; i++)
        {
            if ((Animations[i] as IAnimateKeyFrame).GetId().Equals(eventName))
            {
                (Animations[i] as IAnimateKeyFrame).Stop();
                Animations.RemoveAt(i);
            }

        }
    }

    public void StopAll()
    {

        //Debug.Log(Animations.Count);
        for (int i = 0; i < Animations.Count; i++)
        {
            //	(Animations[i] as  IAnimateKeyFrame).Stop();
        }

        //Animations.Clear();
    }

    public bool IsPlaying(int eventName)
    {
        for (int i = 0; i < Animations.Count; ++i)
        {
            if ((Animations[i] as IAnimateKeyFrame).GetId() == eventName)
            {
                return true;
            }
        }

        return false;
    }

    public bool IsPlaying(IAnimateKeyFrame keyFrame)
    {
        return Animations.Contains(keyFrame);
    }

    public void MoveTo(int id, Transform obj, Vector3 dst, float speed)
    {
        StartKeyFrame(new MoveToKeyFrame(id, obj, dst, speed));
    }

    public void MoveTo(int id, Transform obj, Vector3 dst, float speed, float delay, Ease ease)
    {
        StartKeyFrame(new MoveToKeyFrameParam(id, obj, dst, speed, delay, ease));
    }

    public void AlphaTo(int id, Material obj, float dst, float speed, float delay, Ease ease)
    {
        StartKeyFrame(new AlphaToParam(id, obj, dst, speed, delay, ease));
    }

    public void Animation2DMoveTo(int id, Animation2D obj, Vector2 dst, float speed)
    {
        StartKeyFrame(new MoveToAnimation2DKeyFrame(id, obj, dst, speed));
    }

    public void RotateTo(int id, Transform obj, Quaternion dst, float speed)
    {
        StartKeyFrame(new RotateToKeyFrame(id, obj, dst, speed));
    }

    public void RotateTo(int id, Transform obj, Vector3 dst, float speed, float delay, Ease ease)
    {
        StartKeyFrame(new RotateToKeyFrameParam(id, obj, dst, speed, delay, ease));
    }

    public void AnnRotateTo(int id, Animation2D obj, float dst, float speed)
    {
        StartKeyFrame(new AnnRotateToKeyFrame(id, obj, dst, speed));
    }

    public void ScaleTo(int id, Transform obj, Vector3 dst, float speed)
    {
        StartKeyFrame(new ScaleToKeyFrame(id, obj, dst, speed));
    }

    public void ScaleTo(int id, Transform obj, Vector3 dst, float speed, float delay, Ease ease)
    {
        StartKeyFrame(new ScaleToKeyFrameParam(id, obj, dst, speed, delay, ease));
    }

    public void PlaySequence(int id, params IAnimateKeyFrame[] keyFrames)
    {
        StartKeyFrame(new SequenceKeyFrame(id, keyFrames));
    }

    public void PlaySequence( params IAnimateKeyFrame [] keyFrames )
    {
        StartKeyFrame( new SequenceKeyFrame( -1, keyFrames ) );
    }

    public void PlayTimeKeyFrame(int id, float time, IAnimateKeyFrame frame)
    {
        StartKeyFrame(new TimeKeyFrame(id, time, frame));
    }

    public void PlayAnimation2D(int id, Animation2D anim, int eventName)
    {
        StartKeyFrame(new PlayAnimation2DKeyFrame(id, anim, eventName));
    }

    public void RotateAroundLerp(int id, Transform src, Vector3 point, Vector3 around, float end, float speed)
    {
        StartKeyFrame(new RotateAroundKeyFrame(id, src, point, around, end, speed));
    }

    public void CamSizeLerp(int id, Camera c, float target, float speed)
    {
        StartKeyFrame(new SizeCamOrtho(id, c, target, speed));
    }


    public void CamSizeLerp(int id, float target, float speed)
    {
        StartKeyFrame(new SizeCamOrtho(id, target, speed));
    }

    public void AudioFade(int id, AudioSource a, float target, float speed)
    {
        StartKeyFrame(new MusicFade(id, a, target, speed));
    }


    void Update()
    {
        if (Animations.Count == 0)
        {
            return;
        }
        int index = 0;

        while (index < Animations.Count)
        {
            if (!(Animations[index] as IAnimateKeyFrame).Update())
            {
                (Animations[index] as IAnimateKeyFrame).Stop();
                OnFinished((Animations[index] as IAnimateKeyFrame).GetId());
                Animations.RemoveAt(index);

                continue;
            }
            index++;
        }
    }

    public void DebugLog()
    {
        if (Animations.Count == 0)
        {
            // Debug.Log( "[Animate] Empty List" );
        }
        else
        {
            for (int i = 0; i < Animations.Count; ++i)
            {
                //   Debug.Log( Animations[i].ToString( ) );
            }
        }
    }


    //     public void DispatchEvent( string eventName, System.Object param )
    //     {
    //         if ( EventsDispatcher != null )
    //         {
    //             EventsDispatcher.DispatchEvent( eventName, param );
    //         }
    //     }

    public event  AnimateEventHandler Finished;

    public void OnFinished(int id)
    {
        if (Finished != null)
        {
            Finished(id);
        }
    }

    //////////////////////////////////////////////////////////////////////////
    // TextureLerp
    //////////////////////////////////////////////////////////////////////////
    public class TextureLerp : IAnimateKeyFrame
    {
        public const float MinArrivalDistance = 0.005f;
        public int Id;

        public GameObject Target;
        public Texture2D Destination;
        public Texture2D Source;
        public float Speed;

        private Material Material;
        private Material SaveMaterial;
        private float Elapsed;
        private float Alpha;
        private float StartAlpha;
        private float EndAlpha;

        private bool IsPlaying;

        private Color MatColor;

        public TextureLerp(int id, GameObject target, Material sharedMat, Texture2D destination, float speed)
        {
            Id = id;
            Speed = speed;
            Destination = destination;
            Target = target;
            Material = sharedMat;
            IsPlaying = false;
        }

        public bool Update()
        {
            Elapsed += Time.deltaTime * Speed;
            Alpha = Interpolations.Coserp(StartAlpha, EndAlpha, Elapsed);
            if (Mathf.Abs(Alpha - EndAlpha) < MinArrivalDistance)
            {
                Alpha = EndAlpha;

                IsPlaying = false;
            }
            // MatColor.a = 1.0f - Alpha;
            //Material.SetColor( "_Color", MatColor );
            MatColor.a = Alpha;
            Material.SetColor("_SubColor", MatColor);

            if (!IsPlaying)
            {
                Stop();
            }
            return IsPlaying;
        }

        public void Start()
        {
            Target.renderer.sharedMaterial.shader = Shader.Find("2D/Zbuf/UnlitAlphaMulti");

            Elapsed = 0;
            EndAlpha = 1.0f;
            IsPlaying = true;
            SaveMaterial = Target.renderer.sharedMaterial;
            Source = SaveMaterial.GetTexture("_MainTex") as Texture2D;

            MatColor = SaveMaterial.color;
            StartAlpha = 0.5f - MatColor.a;

            MatColor.a = 1.0f - StartAlpha;
            Material.SetColor("_Color", MatColor);
            MatColor.a = StartAlpha;
            Material.SetColor("_SubColor", MatColor);
            Material.SetTexture("_MainTex", Source);
            Material.SetTexture("_SubTex", Destination);
            //Material.SetTextureScale("_MainTex", PrepareScript.tiling);
            //Material.SetTextureScale("_SubTex", PrepareScript.tiling);

            Target.renderer.sharedMaterial = Material;
        }

        public void Stop()
        {
            SaveMaterial.SetTexture("_MainTex", Destination);
            SaveMaterial.SetColor("_Color", MatColor);
            Target.renderer.sharedMaterial.shader = Shader.Find("2D/Zbuf/UnlitAlphaColoring");
            Target.renderer.sharedMaterial = SaveMaterial;
        }

        public void ReverseNow()
        {
            ReverseNow(null);
        }

        public void ReverseNow(Texture2D destination)
        {
            if (destination == null)
            {
                Texture2D temp = Source;
                Source = Destination;
                Destination = temp;
            }
            else
            {
                Destination = destination;
                Elapsed = 1 - Elapsed;
                Alpha = 1 - Alpha;
            }

            Material.SetTexture("_MainTex", Source);
            Material.SetTexture("_SubTex", Destination);
        }

        public bool IsEqual(object obj)
        {
            RotateAroundKeyFrame keyFrame = obj as RotateAroundKeyFrame;
            if (keyFrame != null)
            {
                return keyFrame.Source == this.Source;
            }

            return false;
        }

        public int GetId()
        {
            return Id;
        }

    }

    //////////////////////////////////////////////////////////////////////////
    // TextureLerp
    //////////////////////////////////////////////////////////////////////////
    public class TextureLerpNoAlpha : IAnimateKeyFrame
    {


        //public Material  Target;
        public Color DestinationCol;
        public Color StartColor;

        public const float MinArrivalDistance = 0.005f;
        public int Id;

        public GameObject Target;
        public Texture2D Destination;
        public Texture2D Source;
        public float Speed;

        private Material Material;
        private Material SaveMaterial;
        private float Elapsed;
        private float Alpha;
        private float StartAlpha;
        private float EndAlpha;

        private bool IsPlaying;

        private Color MatColor;

        public TextureLerpNoAlpha(int id, GameObject target, Material sharedMat, Texture2D destination, float speed, Color destinationCol)
        {
            Id = id;
            Speed = speed;
            Destination = destination;
            DestinationCol = destinationCol;
            Target = target;
            Material = sharedMat;
            IsPlaying = false;
            StartColor = Material.color;
        }

        public bool Update()
        {
            Elapsed += Time.deltaTime * Speed;
            Alpha = Interpolations.Coserp(StartAlpha, EndAlpha, Elapsed);
            if (Mathf.Abs(Alpha - EndAlpha) < MinArrivalDistance)
            {
                Alpha = EndAlpha;

                IsPlaying = false;
            }

            //    DestinationCol = DestinationCol;
            //Target.SetColor("_SubColor", Destination);
            //	 Target.SetTexture( "_MainTex", empty );

            //	Material.color = Color.Lerp(StartColor, DestinationCol, Elapsed);
            //	Target.SetColor("_SubColor", Target.color);



            // MatColor.a = 1.0f - Alpha;
            //Material.SetColor( "_Color", MatColor );
            DestinationCol.a = Alpha;
            Material.SetColor("_SubColor", DestinationCol);

            if (!IsPlaying)
            {
                Stop();
            }
            return IsPlaying;
        }

        public void Start()
        {
            Target.renderer.sharedMaterial.shader = Shader.Find("2D/Zbuf/UnlitAlphaMulti");

            DestinationCol.a = 0.0f;
            Material.SetColor("_SubColor", DestinationCol);

            Elapsed = 0;
            EndAlpha = 1.0f;
            IsPlaying = true;
            SaveMaterial = Target.renderer.sharedMaterial;
            Source = SaveMaterial.GetTexture("_MainTex") as Texture2D;

            MatColor = SaveMaterial.color;
            StartAlpha = 0.5f - MatColor.a;

            MatColor.a = 1.0f - StartAlpha;

            MatColor.a = StartAlpha;
            Material.SetColor("_Color", MatColor);
            Material.SetColor("_SubColor", DestinationCol);
            Material.SetTexture("_MainTex", Source);
            Material.SetTexture("_SubTex", Destination);
            //Material.SetTextureScale("_MainTex", PrepareScript.tiling);
            //Material.SetTextureScale("_SubTex", PrepareScript.tiling);

            Target.renderer.sharedMaterial = Material;
        }

        public void Stop()
        {
            SaveMaterial.SetTexture("_MainTex", Destination);
            SaveMaterial.SetColor("_Color", DestinationCol);
            Target.renderer.sharedMaterial.shader = Shader.Find("2D/Zbuf/UnlitAlphaColoring");
            Target.renderer.sharedMaterial = SaveMaterial;
        }

        public void ReverseNow()
        {
            ReverseNow(null);
        }

        public void ReverseNow(Texture2D destination)
        {
            if (destination == null)
            {
                Texture2D temp = Source;
                Source = Destination;
                Destination = temp;
            }
            else
            {
                Destination = destination;
                Elapsed = 1 - Elapsed;
                Alpha = 1 - Alpha;
            }

            Material.SetTexture("_MainTex", Destination);
            Material.SetTexture("_SubTex", Destination);
        }

        public bool IsEqual(object obj)
        {
            RotateAroundKeyFrame keyFrame = obj as RotateAroundKeyFrame;
            if (keyFrame != null)
            {
                return keyFrame.Source == this.Source;
            }

            return false;
        }

        public int GetId()
        {
            return Id;
        }

    }

    //////////////////////////////////////////////////////////////////////////
    // Colorlerp
    //////////////////////////////////////////////////////////////////////////
    public class ColorLerp : IAnimateKeyFrame
    {
        public const float MinArrivalDistance = 0.005f;
        public int Id;

        public Material Target;
        public Color Destination;
        public float Speed;

        private float Elapsed;

        private bool IsPlaying;

        private Color StartColor;
        public Texture2D Empty;

        public ColorLerp(int id, Material target, Color destination, float speed, Texture2D empty)
        {
            Id = id;
            Speed = speed;
            Destination = destination;
            Target = target;
            IsPlaying = false;
            Empty = empty;
        }

        public bool Update()
        {
            Elapsed += Time.deltaTime * Speed;

            if (1.0f - Elapsed < MinArrivalDistance)
            {
                Target.color = Destination;
                //Target.SetColor("_SubColor", Destination);
                //	 Target.SetTexture( "_MainTex", empty );
                IsPlaying = false;
            }
            else
            {
                Target.color = Color.Lerp(StartColor, Destination, Elapsed);
                //	Target.SetColor("_SubColor", Target.color);
            }

            return IsPlaying;
        }

        public void Start()
        {
            // 	Target.SetTexture( "_MainTex", empty );

            Elapsed = 0;
            StartColor = Target.color;
            Destination.a = 1;
            IsPlaying = true;
            //   Material.SetTexture( "_MainTex",  );
            //oil dodane      Target.SetTexture( "_MainTex", Empty );

        }

        public void Stop()
        {
        }

        public bool IsEqual(object obj)
        {
            ColorLerp keyFrame = obj as ColorLerp;
            if (keyFrame != null)
            {
                return keyFrame.Id == this.Id;
            }

            return false;
        }

        public int GetId()
        {
            return Id;
        }

    }

    //////////////////////////////////////////////////////////////////////////
    // Alpha
    //////////////////////////////////////////////////////////////////////////
    public class AlphaToParam : IAnimateKeyFrame
    {
        public int Id;
        TweenAlphaObject tween;

        public Material Target;
        public float Speed;
        //private bool IsPlaying;

        public AlphaToParam(int id, Material target, float destination, float speed, float delay, Ease ease)
        {
            tween = new TweenAlphaObject();
            Id = id;
            Speed = speed;
            Target = target;
            tween.ease = ease;
            tween.startValue = target.color.a;
            tween.tweenValue = destination;
            tween.totalTime = speed;
            tween.startTime = Time.time;
            tween.delay = delay;
            if (delay != 0F)
            {
                tween.canStart = false;
            }
            else
            {
                tween.canStart = true;
            }

            tween.Init();
        }

        public bool Update()
        {
            this.DetectDelay();
            return this.UpdateTween();
        }

        private void DetectDelay()
        {
            if (Time.time > tween.startTime + tween.delay && !tween.canStart)
            {
                tween.canStart = true;
            }
        }

        public bool UpdateTween()
        {
            if (tween.canStart && !tween.ended)
            {
                float begin = tween.startValue;
                float finish = tween.tweenValue;
                float change = finish - begin;
                float duration = tween.totalTime;
                float currentTime = Time.time - (tween.startTime + tween.delay);

                float alpha = Equations.ChangeFloat(currentTime, begin, change, duration, tween.ease);
                float redColor;
                float redGreen;
                float redBlue;

                redColor = Target.color.r;
                redGreen = Target.color.g;
                redBlue = Target.color.b;

                Target.color = new Color(redColor, redGreen, redBlue, alpha);

                if (duration == 0)
                {
                    Target.color = new Color(redColor, redGreen, redBlue, finish);
                    return false;
                }

                if (Time.time > tween.startTime + tween.delay + duration)
                {
                    Target.color = new Color(redColor, redGreen, redBlue, finish);
                    return false;
                }

                return true;
            }
            else
            {
                return true;
            }
        }

        public void Start()
        {
        }

        public void Stop()
        {
        }

        public bool IsEqual(object obj)
        {
            ColorLerp keyFrame = obj as ColorLerp;
            if (keyFrame != null)
            {
                return keyFrame.Id == this.Id;
            }

            return false;
        }

        public int GetId()
        {
            return Id;
        }

    }

    public sealed class Interpolations
    {
        public static float Sinerp(float start, float end, float value)
        {
            return Mathf.Lerp(start, end, Mathf.Sin(value * Mathf.PI * 0.5f));
        }

        public static float Coserp(float start, float end, float value)
        {
            return Mathf.Lerp(start, end, 1.0f - Mathf.Cos(value * Mathf.PI * 0.5f));
        }
    }
}

public delegate void AnimateEventHandler(int id);
public delegate void AnimateInvokeHandler( int id, object param );
