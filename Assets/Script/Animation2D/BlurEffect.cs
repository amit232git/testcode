using UnityEngine;
using System.Collections;

public class BlurEffect : MonoBehaviour 
{
    public int BackgroundClones = 3;
    public float Speed = 3.0f;    
    private Animation2D SourceAnimation2D;
    private Animation2D [] clones = null;
    private Color [] startColors;
    private Color [] endColors;
    private Color [] currentStart;
    private Color [] currentEnd;
    private float elapsed;
    // Use this for initialization

    public bool IsFadeIn
    {
        get { return startColors == currentStart; }
    }

	void Start () 
    {
        PrepareClones( );
        if ( enabled )
        {
            ResetBlur( );
        }
	}

    void PrepareClones( )
    {        
        if ( clones == null )
        {
            SourceAnimation2D = GetComponent( typeof( Animation2D ) ) as Animation2D;
            clones = new Animation2D [ BackgroundClones ];
            startColors = new Color [ clones.Length ];
            endColors = new Color [ clones.Length ];
            for ( int i = 0; i < BackgroundClones; ++i )
            {
                clones [ i ] = SourceAnimation2D.CloneAnimationOnly( );
                Material mat = new Material( clones [ i ].renderer.sharedMaterial );
                mat.shader = Shader.Find( "2D/UnlitAlpha Opacity" );
                clones [ i ].renderer.material = mat;
                mat.color = new UnityEngine.Color( 1.0f, 1.0f, 1.0f, 0.7f - ( float )i * 0.1f );
                clones [ i ].transform.Translate( 0, 0, 0.5f * ( float )i );
                clones [ i ].transform.localScale = Vector3.one * ( 1.0f + ( float )( i + 1 ) * 0.1f );                
                clones [ i ].SetFrame( );

                startColors [ i ] = clones [ i ].renderer.sharedMaterial.color;
                endColors [ i ] = startColors [ i ];
                endColors [ i ].a = 0.0f;
            }
        }
    }

    public void ShowBlur( )
    {
        if ( !enabled )
        {
            ResetBlur( );
            SetVisible( true );
            enabled = true;
            elapsed = 0F;
        }
    }

    public void HideBlur( )
    {
        if ( enabled )
        {
            SetVisible( false );
            enabled = false;
        }
    }

    public void DestroyBlur( )
    {
        enabled = false;
        for ( int i = 0; i < clones.Length; ++i )
        {
            GameObject.Destroy( clones [ i ].gameObject );
        }
    }

    void SetVisible( bool isVisible )
    {
        for ( int i = 0; i < clones.Length; ++i )
        {
            if ( isVisible )
            {
                clones[ i ].Show( );
            }
            else
            {
                clones[ i ].Hide( );
            }            
        }
    }

    void ResetBlur( )
    {
        currentEnd = startColors;
        currentStart = endColors;
        for ( int i = 0; i < clones.Length; ++i )
        {
            clones [ i ].renderer.sharedMaterial.color = currentStart [ i ];
        }
    }

	// Update is called once per frame
	void Update ()
    {
        elapsed += Time.deltaTime * Speed;
        for ( int i = 0; i < clones.Length; ++i )
        {
            if ( SourceAnimation2D.CurrentEvent != clones[ i ].CurrentEvent || SourceAnimation2D.CurrentFrame != clones[ i ].CurrentFrame )
            {
                clones [ i ].SetFrame( SourceAnimation2D.CurrentEvent, SourceAnimation2D.CurrentFrame, false );
            }
            clones [ i ].renderer.sharedMaterial.color = Color.Lerp( currentStart [ i ], currentEnd [ i ], elapsed );
        }
        if ( elapsed > 1F )
        {
            elapsed = 0F;
            if ( currentStart == startColors )
            {
                currentStart = endColors;
                currentEnd = startColors;
            }
            else
            {
                currentStart = startColors;
                currentEnd = endColors;
            }
        }
	}
}
