// Converted from UnityScript to C# at http://www.M2H.nl/files/js_to_c.php - by Mike Hergaarden
// Do test the code! You usually need to change a few small bits.

using UnityEngine;
using System.Collections;

public class playerBullet : MonoBehaviour {
	float bulletSpeed =500;

	void  Start (){
		rigidbody.AddForce(Vector3.right * bulletSpeed);//add force
	}	
}