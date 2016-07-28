using UnityEngine;
using System.Collections;

public class Interpolations 
{
    public static float EaseOutQuart( float start, float distance, float elapsedTime, float duration )
    {
        elapsedTime = ( elapsedTime > duration ) ? 1.0f : elapsedTime / duration;
        elapsedTime--;
        return -distance * ( elapsedTime * elapsedTime * elapsedTime * elapsedTime - 1 ) + start;
    }

    public static AnimationCurve EaseOutCurve( float timeStart, float valueStart, float timeEnd, float valueEnd )
    {
        float tangent1 = ( valueEnd - valueStart ) / ( 0.5f * ( timeEnd - timeStart ) );
        return new AnimationCurve( new Keyframe [] { 
            new Keyframe( timeStart, valueStart, 0, tangent1  ),
            new Keyframe( timeEnd, valueEnd, 0, 0 ) } );        
    }
}
