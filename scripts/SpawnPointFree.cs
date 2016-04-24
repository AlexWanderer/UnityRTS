using UnityEngine;
using System.Collections;

public class SpawnPointFree : MonoBehaviour {
    public GameObject box;
    public bool readynow = true;
    public float timestep = 0.01f;
    public int count = 0;
    public int numberOfObjects = 10000;
    public float size = 1.0f;

    private GameObject objTerrain;

    void Starter() {
    }

    void Start () {
        StartCoroutine(MakeBox());
    }

    public IEnumerator MakeBox(){
         objTerrain =  GameObject.Find("Terrain");

         for(int i=0;i<numberOfObjects;i=i+1){
            readynow=false;
            yield return new WaitForSeconds(timestep);
            GameObject cubeSpawn = (GameObject)Instantiate(
                box,
                new Vector3(
                    transform.position.x+Random.Range(-size,size)+5,
                    transform.position.y,
                    transform.position.z+Random.Range(-size,size)
                ), transform.rotation
            );

            cubeSpawn.GetComponent<UnitParsFree>().isReady = true;

            objTerrain.GetComponent<BattleSystemFree>().unitsBuffer.Add(cubeSpawn);
            readynow=true;
            count = count+1;
        }
        Debug.Log("Done makebox");
    }

    // function OnGUI() {
    //     GUI.Label(new Rect( 450,5, 30,30),"Number of objects: "+count,textStyle);
    //     GUI.Label(new Rect( 450,15, 30,30),"FPS: "+lastFPS,textStyle);
    // }
}
