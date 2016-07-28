using UnityEngine;
using System.Collections;

public class objectDestroyer : MonoBehaviour {

	void OnTriggerEnter ( Collider collider  ){
		Destroy(collider.gameObject);//destroys every object which gous through this object
	}
}

