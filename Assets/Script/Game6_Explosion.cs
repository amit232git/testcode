using UnityEngine;
using System.Collections;

public class Game6_Explosion : MonoBehaviour
{
	public GameObject mesh;				//Mesh
	public Texture2D[] tex;				//Textures
	private int showTex;				//Selected texture
	public float updateTexTime;			//Texture update time
	private float tmpUpdateTexTime;		//Tmp texture update time


	void Update ()
	{
		//If tmpUpdateTexTime is bigger than updateTexTime
		if (tmpUpdateTexTime > updateTexTime)
		{
			//Set tmpUpdateTexTime to 0
			tmpUpdateTexTime = 0;
			//Add 1 to showTex
			showTex++;
			//If showTex is bigger than tex.Length
			if (showTex >= tex.Length)
			{
				//Kill
				Destroy(gameObject);
			}
			//If showTex is less than tex.Length
			else
			{
				//Set main texture
				mesh.renderer.material.mainTexture = tex[showTex];
			}
		}
		//If tmpUpdateTexTime is less than updateTexTime
		else
		{
			//Add 1 to tmpUpdateTexTime
			tmpUpdateTexTime += 1 * Time.deltaTime;
		}
	}
}