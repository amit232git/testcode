using UnityEngine;
using System.Collections;
using UnityEngine.SocialPlatforms;

public class Menu : MonoBehaviour {

	// Use this for initialization
	public ObserverCollider[] allColliders;
	public GameObject Heli;
	public float yaxis;
	AudioSource auHeli,auTouch,auBgs;
	Camera camGUI;
	void Awake() {
		
		CommonFunctions.initAds();
		CommonFunctions.showBannerAds();

	}	
	void Start () {
		
		CommonFunctions.showTurnAds();
		
		camGUI = GameObject.Find ("Main Camera").camera;
		auTouch = GameObject.Find("Sound/touch").audio;
		
		foreach (ObserverCollider actCollider in allColliders) {
//			Debug.Log(actCollider.name);
			actCollider.TouchDown += new ObserverColliderTouchEventHandler (processButtonsDown);
			actCollider.TouchUp += new ObserverColliderTouchEventHandler (processButtonsUp);
			}
		iTween.MoveTo (Heli, iTween.Hash ("y",yaxis, "time", 0.1, "easetype", iTween.EaseType.spring,"looptype",iTween.LoopType.pingPong));
		
		Social.localUser.Authenticate (success => {
		    if (success) {
//		        Debug.Log ("Authentication successful");
		        string userInfo = "Username: " + Social.localUser.userName + 
		            "\nUser ID: " + Social.localUser.id + 
		            "\nIsUnderage: " + Social.localUser.underage;
//		        Debug.Log (userInfo);
				
		    }
		    else
		        Debug.Log ("Authentication failed");
		});
		

	}
	
	// Update is called once per frame
	void Update () {

	}
	public void processButtonsUp(ObserverTouch source) {
		string buttonname = source.TouchCollider.name;
		Debug.Log (buttonname);
		switch (buttonname) {
			case "btnPlay":
				auTouch.Play();
				Application.LoadLevel ("gamePlay");
				break;
			case "btnGamecenter":
				auTouch.Play();
				Social.ShowLeaderboardUI();
				break;
			case "btnMoreGames":
				auTouch.Play();
				CommonFunctions.showTurnAds();
				break;
		}
	}
	public void processButtonsDown(ObserverTouch source){

	}
}
