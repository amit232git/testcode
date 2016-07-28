using UnityEngine;
using System.Collections;


public enum Animation2DImportSettings
{
    Simple,
    UseLocalTransfromation,
}

public class AnimationAsset : ScriptableObject 
{
    public Animation2DImportSettings    ImportSettings;
    public Material []                  Materials;
    public Animation2DEvent []            Events;

    public Animation2DEvent this [ int eventId ]
    {
        get
        {
            return Events [ eventId ];
        }
    }
}
