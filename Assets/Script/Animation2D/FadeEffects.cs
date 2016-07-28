using UnityEngine;
using System.Collections;

[AddComponentMenu( "Animation2D/FadeEffects" )]
public class FadeEffects : MonoBehaviour 
{
    private static FadeEffects instance = null;

    public static FadeEffects Instance
    {
        get
        {
            if ( instance == null )
            {
                instance = FindObjectOfType( typeof( FadeEffects ) ) as FadeEffects;
                if ( instance == null )
                {
                    Debug.Log( "Could not locate an FadeEffects object. You have to have exactly one FadeEffects in the scene." );
                }
            }

            return instance;
        }
    }

    [UnityEngine.HideInInspector()]
    public EventDispatcher  EffectFinished;
    public float            Alpha;
    public float            FadeDir;
    public float            FadeSpeed;
    public Texture2D        FadeOutTexture;

	void Start () 
    {

        if ( FadeOutTexture == null )
        {
            FadeOutTexture = new Texture2D( 1, 1 );
            FadeOutTexture.SetPixel( 0, 0, new Color( 1, 1, 1, 1 ) );
            FadeOutTexture.Apply( );
        }

        FadeIn( );
	}


    public void OnEffectFinished( )
    {
        EffectFinished.Dispatch( this );
    }

    public void FadeIn( )
    {
        Alpha = 0.0f;
        FadeDir = -1;
        enabled = true;
    }

    public void FadeOut( )
    {
        Alpha = 0.5f;
        FadeDir = 1;
        enabled = true;
    }

	void OnGUI( )
    {
        Alpha += FadeDir * FadeSpeed * Time.deltaTime;
        Alpha = Mathf.Clamp01( Alpha );

        GUI.color = new Color( 0, 0, 0, Alpha );
        GUI.depth = -1000;
        GUI.DrawTexture( new Rect( -Screen.width, 0, Screen.width * 2, Screen.height ), FadeOutTexture );

        if ( Alpha == 0 || Alpha == 1 )
        {
            enabled = false;
            OnEffectFinished( );
        }
    }
}
