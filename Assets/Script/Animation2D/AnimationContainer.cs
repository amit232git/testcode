using UnityEngine;
using System.Collections;
using System.Reflection;

[AddComponentMenu( "Animation2D/AnimationContainer" )]
public class AnimationContainer : MonoBehaviour
{
    #region singleton

    private static AnimationContainer instance = null;

    public static AnimationContainer Instance
    {
        get
        {
            if ( instance == null || Application.isEditor )
            {
                instance = FindObjectOfType( typeof( AnimationContainer ) ) as AnimationContainer;
                if ( instance == null )
                {                    
                    GameObject go = new GameObject( "AnimationContainer" );
                    instance = go.AddComponent( typeof( AnimationContainer ) ) as AnimationContainer;
                }
            }

            return instance;
        }
    }

    #endregion

    public bool AllowInterpolations = false;

    protected int loopIndex = -1;

    public bool InLoop
    {
        get { return loopIndex >= 0; }
    }

    protected ArrayList animationList;

    public ArrayList AnimationList
    {
        get { return animationList; }
    }

    protected ArrayList toBeRemoved = new ArrayList( );

    void Awake( )
    {
        animationList = new ArrayList( );

        //test( );
    }

    //void test( )
    //{
    //    GameObject obj = GameObject.Find( "/hero/heroAnimation" );
    //    System.Type componentType = typeof( Transform );
    //    string fieldName = "localPosition.x";

    //    string [] properties  = fieldName.Split( new char [] { '.' } );
    //    Component component = obj.GetComponent( componentType );
    //    System.Type currentType = component.GetType( );
    //    for ( int i = 0; i < properties.Length - 1; i++ )
    //    {
    //        Debug.Log( "component: " + component.ToString( ) + ", " + properties [ i ] );

    //        MemberInfo mi = currentType.GetMember( properties[ i ] );
    //        if ( mi.MemberType == MemberTypes.Property )
    //        {
    //            PropertyInfo propInfo = mi as PropertyInfo;
    //            propInfo.GetValue( )
    //        }
            
    //    }
    //}

    public void AddAnimation2D( Animation2D ann )
    {
        if ( InLoop )
        {
            // dupencja niezadowolencja
            // w przypadku gdy odpytujemy annki , ktos zastopuje lub animacja sie zakonczy a dalej ponownie wystartuje
            if ( toBeRemoved.Contains( ann ) )
            {
                toBeRemoved.Remove( ann );
            }
        }
        if ( !animationList.Contains( ann ) )
        {            
            animationList.Add( ann );
        }
    }

    public void RemoveAnimation2D( Animation2D ann )
    {
        if ( InLoop )
        {
            if ( !toBeRemoved.Contains( ann ) )
            {
                toBeRemoved.Add( ann );
            }
        }
        else
        {
            animationList.Remove( ann );
        }
    }

    void Update( )
    {
        if ( animationList == null || animationList.Count == 0 )
        {
            return;
        }
        loopIndex = 0;
        while ( loopIndex < animationList.Count )
        {
            Animation2D ann = animationList [ loopIndex ] as Animation2D;
            if ( ann != null )
            {
                ann.Tick( );
            }
            loopIndex++;
        }
        loopIndex = -1;

        if ( toBeRemoved.Count > 0 )
        {
            for ( int i = 0; i < toBeRemoved.Count; ++i )
            {
                if ( animationList.Contains( toBeRemoved [ i ] ) )
                {
                    animationList.Remove( toBeRemoved [ i ] );
                }
            }
            toBeRemoved.Clear( );
        }
    }
}