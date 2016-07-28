// Converted from UnityScript to C# at http://www.M2H.nl/files/js_to_c.php - by Mike Hergaarden
// Do test the code! You usually need to change a few small bits.

using UnityEngine;
using System.Collections;

public class enemyBase : MonoBehaviour {
	
	public GameObject[] enemy;
	
	public GameObject wall1;
	public GameObject wall2;
	public GameObject wall3;
	//public GameObject bigEnemy;
	float interval=3;
	float timer=0;
	float timer2=0;
	public bool  activate =true;
	int count1 =0;
	int count2 =0;
	int count3 =0;
	
	void  Update (){
		timer2 += Time.deltaTime;
		
		if(Mathf.Round(timer2) >60 && Mathf.Round(timer2) <140){//if we have played more than 60 second
			interval = 2.5f; //interval is decreased and the enemy airplanes is instantiated more fast
		}
		if(Mathf.Round(timer2) > 140 && Mathf.Round(timer2) <200){
			interval = 1.7f;
		}
		if(Mathf.Round(timer2) > 200){
			interval = 1.3f;
		}
		if(Mathf.Round(timer2) > 210){
			activate=true;//disable small enemy instantiation
		}
//		if(Mathf.Round(timer2) >= 215 ){
//			Instantiate(enemy, transform.position, transform.rotation);//instantiate big enemy
//			enabled=false;
//		}
		
		timer += Time.deltaTime;
		
		if(timer >= interval){ //if timer is more than interval
			InstantiateEnemy(); //cal instantiateenemy function
			timer = 0;
		}
	}
	
	void  InstantiateEnemy (){//instantiates enemy airplane on random position
		if(activate){
			Vector3 randomPosition;
			randomPosition= new Vector3(transform.position.x,Random.value+2 ,0);
			int enemyNo = Random.Range(0, enemy.Length);
			if(enemyNo == 1)
				Instantiate(enemy[enemyNo],randomPosition,Quaternion.Euler(new Vector3(0,90,90)));
			else
				Instantiate(enemy[enemyNo],randomPosition,Quaternion.Euler(new Vector3(0,90,0)));
				
			count1++;
			count2++;
			count3++;
		}
		if (count1 == 1) {
			Vector3 randomPosition;
			randomPosition= new Vector3(transform.position.x,Random.value ,0);
			Instantiate(wall1,randomPosition,transform.rotation);
			count1=0;
		}
		if (count2 == 3) {
			Vector3 randomPosition;
			randomPosition= new Vector3(transform.position.x,Random.value ,0);
			Instantiate(wall2,randomPosition,transform.rotation);
			count2=0;
		}
		if (count3 == 5) {
			Vector3 randomPosition;
			randomPosition= new Vector3(transform.position.x,Random.value ,0);
			Instantiate(wall3,randomPosition,transform.rotation);
			count3=0;
		}

	}
}