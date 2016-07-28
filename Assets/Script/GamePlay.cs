using UnityEngine;
using System.Collections;

public class GamePlay: MonoBehaviour
{
	public GUISkin skin;				//GUI skin
	public GameObject mesh;				//Mesh
	public GameObject cameraGo;			//Main camera
	public GameObject block;			//Block prefab
	public GameObject explosion;		//Explosion prefab
	public float moveSpeed;				//Move speed
	public float jumpSpeed;				//Jump speed
	private Vector3 dir;				//The direction the player is moving
	private float score;					//Score
	private GameObject rightTouchPad;	//Right touchpad
	public bool dead;					//Are we dead
	private bool start;					//Has the game started
	private Vector2 velocity;			//Velocity
	private int spawnPositionX;			//X spawn position
	private float spawnPositionY;		//Y spawn position
	private bool newPositionY;			//New y position
	private float addToPositionY;		//The amount we add to the y position
	public Transform bulletPlace;
	public GameObject player;
	public GameObject explosionPrefab;
	public GameObject explosionSound;
	public GameObject sparks;
	public GameObject bullet;
	enemyBase objEnemy;

	// Use this for initialization
	public ObserverCollider[] allColliders;
	//public GameObject goGUI;
	public GameObject goverGUI;
	public GameObject enemyBase;
	public GameObject gamePlaytime;
	public GameObject gamePausetime;
	TextMesh miles,finalscore;
	AudioSource auHeli,auTouch,auBgs;
	public static bool ispause;
	float dispMiles;
	float runDistance;
	public Transform btnpause,tfmiles;
	Camera camGUI;
	bool isTouch = true;
	bool isstart = false;
	void Awake(){
	
	}
	
	void Start ()
	{
		auHeli = GameObject.Find ("Sound/heli").audio;
		auTouch = GameObject.Find("Sound/touch").audio;
		auBgs = GameObject.Find("Sound/bgsound").audio;
		camGUI = GameObject.Find ("Main Camera").camera;
		
		miles = gamePlaytime.transform.FindChild("tmMiles/countMiles").GetComponent<TextMesh>();
		gamePlaytime.SetActive(false);
		
		Screen.sleepTimeout = SleepTimeout.NeverSleep;
		
		foreach (ObserverCollider actCollider in allColliders) {
			//			Debug.Log(actCollider.name);
			
			actCollider.TouchDown += new ObserverColliderTouchEventHandler (processButtonsDown);
			actCollider.TouchUp += new ObserverColliderTouchEventHandler (processButtonsUp);
		}
		// tmDistance.text = (carscript.runDistance * 0.0005399568f).ToString ("f2");
		if (iPhone.generation == iPhoneGeneration.iPhone5 || camGUI.aspect >= 1.7f && camGUI.aspect < 2) {
						btnpause.localPosition = new Vector3 (-10, 3.1f, 10);
						tfmiles.localPosition = new Vector3 (8, 3.1f, 10);

				}

	}

	//Find right touchpad
	//rightTouchPad = GameObject.Find("RightTouchPad");
	//Start SetupJoysticks
	//StartCoroutine("SetupJoysticks");
	//Set label color to black

	//Set sleep time to never
	void startGame(){
		if (isstart) {
			InvokeRepeating ("fireBullets", 1.5f, 2.0f);
			auBgs.Play ();
			auHeli.Play ();
		}
	}

	void Update ()
	{
		if (ispause)
			return;


		//If we are not dead and the game has started
		if (!dead && start)
		{
			//Update player
			MovePlayer();

			//Instantiate(bullet, bulletPlace.position,Quaternion.identity);

		}
		else
		{
			//If the game is not running on a android device
			if (Application.platform != RuntimePlatform.IPhonePlayer)
			{
				//Get Space key down
				if (Input.GetKey(KeyCode.Space))
				{
					//Start the game
					start = true;
				}
			}
		}
		if (start) {
			//Update camera
			MoveCamera ();

			//InvokeRepeating("fireBullets",1.5f,1.0f);

			//Instantiate Level
			InstantiateLevel ();
//			float lastPosition = player.transform.position;
//			runDistance += (int) Vector3.Distance(0.0f, lastPosition);
//			dispMiles = float.Parse (runDistance * 0.0005399568f).ToString();
			//Get score
			score = (int)Vector3.Distance (new Vector3 (0, 0, -0.2f), transform.position);
			//miles.text = " "+ (score * 0.0368f).ToString("f2");

			miles.text = (score * 0.05399568f).ToString ("f2");
			dispMiles = float.Parse ((score * 0.05399568f).ToString ("f2"));
		}
	}

	public Transform trTouchPlay;
	public void processButtonsDown(ObserverTouch source) 
	{
		string buttonName = source.TouchCollider.name;
		Debug.Log ("down "+ buttonName);

		switch(buttonName) {
			case "touchPlay":

			tC = 1;
		//	auHeli.pitch += 0.020f;
			break;
		case "tmMenu":
			auTouch.Play();
			break;
		case "btnPause":
			auTouch.Play();
			break;
		case "Taptostart":
			auTouch.Play();
			break;
		case "tmResume":
			auTouch.Play();
			break;
		}
	}
	float tC = 0;
	public void processButtonsUp(ObserverTouch source) 
	{
		string buttonName = source.TouchCollider.name;
		switch (buttonName) {

			case "touchPlay":
			start = true;
			tC = 0;
		//	auHeli.pitch -= 0.02f;
			
			break;
			case "btnPause":
			enemy.Enabled = false;
			start = false;
			ispause = true;
			isstart = false;
			//Application.LoadLevel("Menu");
			//gameObject.SetActive(false);
			gamePausetime.SetActive(true);
			gamePlaytime.SetActive(false);
			//auTouch.Play();
			auBgs.Stop();
			auHeli.Stop();
			CancelInvoke();
			enemyBase.SetActive(false);

			trTouchPlay.collider.enabled=false;

			break;
		case "tmMenu":
			start=false;
			ispause = false;
			Application.LoadLevel("Menu");

			break;
		case "Taptostart":
			start = true;
			GameObject.Find("Taptostart").SetActive(false);
			gamePlaytime.SetActive(true);
			trTouchPlay.collider.enabled = true;
			isstart = true;
			startGame();
			enemyBase.SetActive(true);

			//goGUI.SetActive(false);

			break;
		case "tmResume":
			start = true;
			ispause = false;
			isstart = true;
			enemy.Enabled = true;
			trTouchPlay.collider.enabled = true;
			startGame();
			enemy.enemyFrz = false;
			gamePausetime.SetActive(false);
			gamePlaytime.SetActive(true);
			enemyBase.SetActive(true);

			break;


		}
	}

	void MovePlayer()
	{
		//If the game is not running on a android device
		if (Application.platform != RuntimePlatform.IPhonePlayer)
		{
			//Get Space key down
			if (Input.GetKey(KeyCode.Space))
			{
				//Set direction to up
				dir = new Vector3(moveSpeed,jumpSpeed,0);
			}
			else
			{
				//Set direction to down
				dir = new Vector3(moveSpeed,-jumpSpeed,0);
			}
		}
		//If the game is running on a android device
		else
		{
			//Get touchpad tapcount
			//float tC = rightTouchPad.GetComponent<Joystick>().tapCount;
			
			//If touchpad tapcount is not 0
			if (tC != 0)
			{
				//Set direction to up
				dir = new Vector3(moveSpeed,jumpSpeed,0);
			}
			//If touchpad tapcount is 0
			else
			{
				//Set direction to down
				dir = new Vector3(moveSpeed,-jumpSpeed,0);
			}
		}
		
		//Move player
		transform.Translate(dir * Time.smoothDeltaTime);
	}
	
	void MoveCamera()
	{
		//Move camera
		cameraGo.transform.position = new Vector3(Mathf.SmoothDamp(cameraGo.transform.position.x, transform.position.x+6, ref velocity.x, Time.smoothDeltaTime), cameraGo.transform.position.y,-10);
	}
	
	void InstantiateLevel()
	{
		//If spawnPositionX is less the player position + 15
		if (spawnPositionX < transform.position.x + 20)
		{
			//If we can set new y position
			if (newPositionY)
			{
				//Get random int
				int upDown = Random.Range(0,2);
				//If upDown is bigger than 0
				if (upDown > 0)
				{
					//If spawnPositionY is less than 2
					if (spawnPositionY < 2)
					{
						//Add 0.4 to y position
						addToPositionY = 0.4f;
					}
					//If spawnPositionY is bigger than 2
					else
					{
						//Set y addToPositionY to 0
						addToPositionY = 0;
					}
				}
				//If upDown is 0
				else
				{
					//If spawnPositionY is bigger than -2
					if (spawnPositionY > -2)
					{
						//Add -0.4 to y position
						addToPositionY = -0.4f;
					}
					//If spawnPositionY is less than -2
					else
					{
						//Set y addToPositionY to 0
						addToPositionY = 0;
					}
				}
				//We cant set new y position
				newPositionY = false;
			}
			//If we cant set new y position
			else
			{
				//We can set new y position
				newPositionY = true;
			}
			//Add addToPositionY to spawnPositionY
			spawnPositionY += addToPositionY;
			
			//Spwan new blocks
			Instantiate(block,new Vector3(spawnPositionX,spawnPositionY,0),Quaternion.identity);
			
			//Add 1 to spawnPositionX
			spawnPositionX++;
		}
	}
	
//	void OnGUI()
//	{
//		GUI.skin = skin;
//		
//		//Score
//		GUI.Label(new Rect(10,10,300,300),"Score: " + score.ToString());
//		
//		//Menu Button
//
//
//		
//		//If we are dead
//		if (dead)
//		{
//			//Play Again Button
//			if(GUI.Button(new Rect(Screen.width / 2 - 90,Screen.height / 2 - 60,180,50),"Play Again"))
//			{
//				Application.LoadLevel("Game 6");
//			}
//			//Menu Button
//			if(GUI.Button(new Rect(Screen.width / 2 - 90,Screen.height / 2,180,50),"Menu"))
//			{
//				Application.LoadLevel("Menu");
//			}
//		}
//	}
	void  fireBullets (){
		if (GamePlay.ispause)
			return;
		//if (isTouch) 
		{
			Instantiate (bullet, bulletPlace.position, Quaternion.Euler(new Vector3(0,90,0)));	
		}
	}

	void OnTriggerEnter(Collider other)
	{
		Debug.Log ("test");
		//If we are in a enemy trigger
		if (other.tag == "Enemy")
		{
			//Set dead to true
			auHeli.Stop();
			auBgs.Stop();
			start = false;
			isstart = false;
			//DestroyEnemies();
			Destroy(gameObject);
			enemy.follow = false;

			Debug.Log("enemy test");
			trTouchPlay.collider.enabled = false;
			enemyBase.SetActive(false);

			//dead = true;
			//Instantiate explosion
			CancelInvoke("fireBullets");
			CancelInvoke();


			Instantiate(explosionSound, transform.position, Quaternion.identity);//instantiate explosion sound prefab
			Instantiate(explosionPrefab, transform.position, transform.rotation);//instantiate explosion prefab
			gamePlaytime.SetActive(false);
			goverGUI.SetActive(true);
			finalscore = goverGUI.transform.FindChild("tmMilesScore").GetComponent<TextMesh>();
			finalscore.text = dispMiles.ToString();

			if(PlayerPrefs.HasKey("best")) {
			
				float tempScore = PlayerPrefs.GetFloat("best");

				if(tempScore < dispMiles) {
					PlayerPrefs.SetFloat("best",dispMiles);
				}
//				TextMesh bestscore = GameObject.Find("tmBestScore").GetComponent<TextMesh>();
//				bestscore.text=PlayerPrefs.GetFloat("best").ToString();
				Debug.Log("Best: ");
			
			} else {
				PlayerPrefs.SetFloat("best",dispMiles);
			}
			//Dont show renderer
			TextMesh bestscore = GameObject.Find("tmBestScore").GetComponent<TextMesh>();
			bestscore.text=PlayerPrefs.GetFloat("best").ToString();
			
			if(Application.platform == RuntimePlatform.IPhonePlayer){
				if(CommonFunctions.CheckInternetConnection())
					ReportScore((long)(dispMiles * 100.0f), "flappyheli.scores");
			}	
//			mesh.renderer.enabled = false;
			
			CommonFunctions.showTurnAds();
		}
	}
	
	void ReportScore (long score, string leaderboardID) {
	    Debug.Log ("Reporting score " + score + " on leaderboard " + leaderboardID);
		#if UNITY_IPHONE
	    Social.ReportScore (score, leaderboardID, success => {
	        Debug.Log(success ? "Reported score successfully" : "Failed to report score");
	    });
		#endif
	}
	
	void DestroyEnemies() {
		GameObject[] myTriggers = GameObject.FindGameObjectsWithTag("rocket");
		foreach (GameObject trigger in myTriggers){
			Destroy(trigger.transform.root);
		
			Debug.Log(trigger.transform.parent.name);
		}
	}
}