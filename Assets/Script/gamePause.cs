using UnityEngine;
using System.Collections;

public class gamePause : MonoBehaviour {
	
	// Use this for initialization
	public ObserverCollider[] allColliders;
	//public GameObject Heli;
	//public float yaxis;
	AudioSource auHeli,auTouch,auBgs;
	public Transform[] trScroll;
	void Start () {
		auHeli = GameObject.Find("Sound/heli").audio;
		auTouch = GameObject.Find("Sound/touch").audio;
		auBgs = GameObject.Find("Sound/bgsound").audio;
		foreach (ObserverCollider actCollider in allColliders) {
			//			Debug.Log(actCollider.name);
			
			actCollider.TouchDown += new ObserverColliderTouchEventHandler (processButtonsDown);
			actCollider.TouchUp += new ObserverColliderTouchEventHandler (processButtonsUp);
		}
	//	iTween.MoveTo (Heli, iTween.Hash ("y",yaxis, "time", 0.1, "easetype", iTween.EaseType.spring,"looptype",iTween.LoopType.pingPong));
	}
	
	// Update is called once per frame
	void Update () {



	}
	public void processButtonsDown(ObserverTouch source){
		string buttonname = source.TouchCollider.name;
		//Debug.Log (buttonname);
		switch (buttonname) {
			
		case "tmResume":
			//	Application.LoadLevel ("gamePlay");
			auTouch.Play();
			break;
			
		case "tmMenu":
			//start = false;
			//	Application.LoadLevel ("Menu");
			auTouch.Play();
			
			break;
		case "tmPlayagain":
			//start = true;
			
			//	Application.LoadLevel ("gamePlay");
			auTouch.Play();
			break;
		}
		
	}
	public void processButtonsUp(ObserverTouch source) {
		string buttonname = source.TouchCollider.name;
		Debug.Log (buttonname);
		switch (buttonname) {
			
		case "tmResume":
			Application.LoadLevel ("gamePlay");
		//	auTouch.Play();
			break;
			
		case "tmMenu":
			//start = false;
			Application.LoadLevel ("Menu");
		//	auTouch.Play();
			
			break;
		case "tmPlayagain":
			//start = true;

			Application.LoadLevel ("gamePlay");
		//	auTouch.Play();
			break;
		}
	}

}
