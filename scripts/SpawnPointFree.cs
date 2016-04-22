using UnityEngine;
using System.Collections;

public class SpawnPointFree : MonoBehaviour {

public GameObject box;
public GameObject fakeObject;
public bool readynow = true;
public float timestep = 0.01f;
public int count = 0;
public int numberOfObjects = 10000;
public float size = 1.0f;

public int assignDiplomacy = 1;

public bool addToBS = true;
public bool createFakeObject = false;

private GameObject objTerrain;

void Starter() {

}

void Start () {
	StartCoroutine(MakeBox());
}
 
//function Update () {

//}
 
public IEnumerator MakeBox(){

 objTerrain =  GameObject.Find("Terrain");
 
 
 if(createFakeObject == true){
 	for(int i=0;i<2;i=i+1){
 		GameObject cubeSpawn = (GameObject)Instantiate(fakeObject, new Vector3(-9999999999999.99f-9999.99f*Random.Range(-1.0f,1.0f),-9999999999999.99f-9999.99f*Random.Range(-1.0f,1.0f),-9999999999999.99f-9999.99f*Random.Range(-1.0f,1.0f)), transform.rotation);
 
 		cubeSpawn.GetComponent<UnitParsFree>().isReady = true;
 		objTerrain.GetComponent<BattleSystemFree>().unitsBuffer.Add(cubeSpawn);
 	}
 }
 
 
 for(int i=0;i<numberOfObjects;i=i+1){
 	readynow=false;
 	yield return new WaitForSeconds(timestep);
 	GameObject cubeSpawn = (GameObject)Instantiate(box, new Vector3(transform.position.x+Random.Range(-size,size)+5,transform.position.y,transform.position.z+Random.Range(-size,size)), transform.rotation);
 
  //  cubeSpawn.GetComponent<DiplomacyFree>().team = assignDiplomacy;
    
    cubeSpawn.GetComponent<UnitParsFree>().isReady = true;
    
    objTerrain.GetComponent<BattleSystemFree>().unitsBuffer.Add(cubeSpawn);
    
    
 //   objTerrain.GetComponent<BattleSystemFree>().runits[assignDiplomacy].Add(cubeSpawn);
 //   objTerrain.GetComponent<BattleSystemFree>().rfunits[assignDiplomacy].Add(cubeSpawn);
    
    
    
 	readynow=true;
 	count = count+1;
 
 

 }
}

//function OnGUI()

 //   {
        

  //      GUI.Label(new Rect( 450,5, 30,30),"Number of objects: "+count,textStyle);
      //  GUI.Label(new Rect( 450,15, 30,30),"FPS: "+lastFPS,textStyle);
//}

}