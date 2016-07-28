using UnityEngine;
using System.Collections;

[System.Flags]
public enum FrameFlags : int
{
    WindingCCW = 0x0001,
    WindingCW = 0x0002,
}

[System.Serializable( )]
public class Animation2DFrame : System.Object
{
    public FrameFlags Flags;
    public Vector2 FramePos;
    public Vector2 Correct;
    public Rect uvRect;
    public Rect ptRect;
    public int MaterialIndex;

    public bool WindingCCW
    {
        get { return ( Flags & FrameFlags.WindingCCW ) == FrameFlags.WindingCCW; }
    }

    public Animation2DFrame( int materialIdx, string name, int opacity, float scaleX, float scaleY,
        float angle, int anchorX, int anchorY, string wavName, int positionX, int positionY,
        int frameWidth, int frameHeight, float oryginalWidth, float oryginalHeight, float left, float top )
    {
        MaterialIndex = materialIdx;        
        //float Opacity = ( float ) opacity / 255.0f;
        float ScaleX = ( float ) oryginalWidth * scaleX * Animation2D.ScreenScaleX;
        float ScaleY = ( float ) oryginalHeight * scaleY * Animation2D.ScreenScaleY;
        float Angle = angle;
        float AnchorX = ( 0.5f * ( float ) oryginalWidth - ( float ) anchorX ) * scaleX * Animation2D.ScreenScaleX;
        float AnchorY = ( 0.5f * ( float ) -oryginalHeight + ( float ) anchorY ) * scaleY * Animation2D.ScreenScaleY;
        float PositionX = ( float ) ( positionX ) * Animation2D.ScreenScaleX;
        float PositionY = ( float ) ( positionY ) * Animation2D.ScreenScaleY;
        float corrX = AnchorX;
        float corrY = AnchorY;
        float AncX = ( float ) anchorX * Animation2D.ScreenScaleX;
        float AncY = ( float ) anchorY * Animation2D.ScreenScaleY;
        float alpha = angle * ( float ) Mathf.PI / 180.0f;
        float cosa = ( float ) Mathf.Cos( alpha );
        float sina = ( float ) Mathf.Sin( alpha );

        float CorrectX = corrX * cosa + corrY * sina;
        float CorrectY = corrY * cosa - corrX * sina;

        Vector2 TextureScale = new Vector2( oryginalWidth / frameWidth, oryginalHeight / frameHeight );
        Vector2 TextureOffset = new Vector2( left / frameWidth, 1.0f - TextureScale.y - ( top / frameHeight ) );
        //Vector2 OryginalSize = new Vector2( oryginalWidth * Animation2D.ScreenScaleX, oryginalHeight * Animation2D.ScreenScaleY );

        FramePos.x = PositionX + CorrectX - AncX;
        FramePos.y = PositionY - CorrectY - AncY;
        Correct.x = CorrectX;
        Correct.y = CorrectY;

        bool ccw = ScaleX < 0F;

        uvRect.x = TextureOffset.x;
        uvRect.y = TextureOffset.y;
        uvRect.xMax = TextureOffset.x + TextureScale.x;
        uvRect.yMax = TextureOffset.y + TextureScale.y;

        Angle = -Angle;

        if ( ccw )
        {
            float temp = uvRect.x;
            uvRect.x = TextureOffset.x + TextureScale.x;
            uvRect.xMax = temp;

            Flags |= FrameFlags.WindingCCW;
        }
        else
        {
            Flags |= FrameFlags.WindingCW;
        }

        FramePos.y = Animation2D.ScreenHeight - FramePos.y;

        Vector2 topLeft = new Vector2( ScaleX * -0.5f, ScaleY * -0.5f );
        Vector2 bottomRight = new Vector2( ScaleX * 0.5f, ScaleY * -0.5f );

        Matrix4x4 matrix = Matrix4x4.identity;
        cosa = Mathf.Cos( Angle * ( float ) Mathf.PI / 180.0f );
        sina = Mathf.Sin( Angle * ( float ) Mathf.PI / 180.0f );
        matrix [ 0, 0 ] = cosa;
        matrix [ 0, 1 ] = -sina;
        matrix [ 1, 0 ] = sina;
        matrix [ 1, 1 ] = cosa;
        Vector3 pt1 = matrix.MultiplyPoint( new Vector3( topLeft.x, topLeft.y, 0.0f ) );
        Vector3 pt2 = matrix.MultiplyPoint( new Vector3( bottomRight.x, bottomRight.y, 0.0f ) );
        ptRect.x = pt1.x;
        ptRect.y = pt1.y;
        ptRect.xMax = pt2.x;
        ptRect.yMax = pt2.y;
    }
}