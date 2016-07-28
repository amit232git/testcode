// Converted from UnityScript to C# at http://www.M2H.nl/files/js_to_c.php - by Mike Hergaarden
// Do test the code! You usually need to change a few small bits.

using UnityEngine;
using System.Collections;

public class enemy : MonoBehaviour {
	
//	public GameObject bullet;
//	public Transform bulletPlace;
	public GameObject sparks;
	public GameObject explosionPrefab;
	public GameObject explosionSound;
//	public GameObject lifeBar;
//	public GameObject rocketBonus;
//	public GameObject bulletSpeedBonus;
//	public GameObject capsuleBonus;
	
//	float shootingRandomTime=2.0f;
	//int bulletResistance=4; 
	float YSpeed= 0.8f; //moving speed on Y axis
	float XSpeed= 100; // moving speed on X axis
//	int randomRange=5;
	
//	AudioClip bulletSound;
//	AudioClip impact;
	
	public static bool follow=true;
	
	private GameObject Player;
	private float yVelocity= 0.0f;
	private float timer = 0.0f;
	private float timer2 = 0.0f;
	public static bool  Enabled = true;
	public static bool enemyFrz = false;

	void  Start (){
		Player=GameObject.FindWithTag("Player");
		follow = true;
		Enabled = true;
	}
	
	void  Update (){
		//Debug.Log (Player.ispause + " - " + Enabled + ", " + follow);
		
		if (GamePlay.ispause) {
			rigidbody.velocity = new Vector3(0,0,0);
			return;
		}
		if (!Enabled)
			return;

			rigidbody.velocity = Vector3.right * -XSpeed * Time.deltaTime; //add force 
			if (follow && Enabled) {

				Debug.Log ("position positive...");
				float newPositionY = Mathf.SmoothDamp (transform.position.y, Player.transform.position.y, ref yVelocity, YSpeed);//change smoothly position
				transform.position = new Vector3 (transform.position.x, newPositionY, transform.position.z); //assign position
			}

	}
	
	void  OnCollisionEnter ( Collision collision  ){
//		if(collision.gameObject.tag == "rocket"){ //if it is hitted by rocket
//	//		GUIText score = gameObject.Find("scores").GetComponent<GUIText>(); //change scores guitext
//	//		score.text ="" + (int.Parse(score.text) + 50);
//			Destroy(collision.gameObject); //destroy rocket 
//			Explosion(); //call explosion function 
//		}
		 if(collision.gameObject.tag == "Player")
		{
			Explosion();
		}
	}
		
	void  Explosion (){
		Instantiate(explosionSound, transform.position, Quaternion.identity); //instantiate explosionsound object which has attached audio source and plays sound on awake
		Instantiate(explosionPrefab, transform.position, Quaternion.identity);
		Destroy(gameObject); //destroy this object
	}
	
	void  OnTriggerEnter ( Collider collision  ){
		
		if(collision.gameObject.tag == "bullet"){ //if it is hitted by bullet
			
			Destroy(collision.gameObject); //destroy bullet
			//audio.PlayOneShot(impact); //play impact sound
			Instantiate(sparks, collision.transform.position, Quaternion.identity);//instantiate sparks
			Enabled = false;
			
		//	if(bulletResistance <= 0){//if bullet resistance is lower than 0
	//			GUIText score = gameObject.Find("scores").GetComponent<GUIText>();//change score guitext
	//			score.text ="" + (int.Parse(score.text) + 100);
				Explosion();//call explosion function
				
		//	}else{//if bullet resistance is more than 0
		//		bulletResistance--; //decrease bullet resistance
				//lifeBar.transform.localScale -= new Vector3(0.1f,0,0); //scale lifeBar

		}
	}
}