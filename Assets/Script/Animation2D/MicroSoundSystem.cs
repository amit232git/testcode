using UnityEngine;
using System.Collections;

[AddComponentMenu( "Animation2D/MicroSoundSystem" )]
public class MicroSoundSystem : MonoBehaviour 
{
    private static MicroSoundSystem instance = null;

    public static MicroSoundSystem Instance
    {
        get
        {
            if ( instance == null )
            {
                instance = FindObjectOfType( typeof( MicroSoundSystem ) ) as MicroSoundSystem;
                instance.Initialize( );
                if ( instance == null )
                {
                    Debug.Log( "Could not locate an MicroSoundSystem object. You have to have exactly one MicroSoundSystem in the scene." );
                }
            }

            return instance;
        }
    }

    void Start( )
    {
        MicroSoundSystem.instance = this;
        Initialize( );

    }

    public AudioSource [] AudioSources;
    public AudioClip [] AudioClips;

    public Hashtable Clips = new Hashtable( );

    public AudioSource GetAudioSource( )
    {
        for ( int i = 0; i < AudioSources.Length; ++i )
        {
            if ( !AudioSources[ i ].isPlaying )
            {
                return AudioSources[ i ];
            }
        }

        return null;
    }

    public void Initialize( )
    {

        Debug.Log( "INITIALIZATION" );
        AddClips( AudioClips );
    }

    public void AddClips( AudioClip [] clips )
    {
        for ( int i = 0; i < clips.Length; ++i )
        {
            Debug.Log( "Adding " + clips[ i ].name );
            if ( !Clips.ContainsKey( clips[ i ].name ) )
            {
                Clips.Add( clips[i].name, clips[i] );
            }
        }

    }

    public bool Play( string clipName )
    {
        AudioSource source = GetAudioSource();
        if ( source == null )
        {
            return false;
        }
        AudioClip clip = Clips[ clipName ] as AudioClip;
        if ( clip == null )
        {
            return false;
        }
        source.PlayOneShot( clip );

        return true;
    }
}
