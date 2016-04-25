using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpawnPointFree : MonoBehaviour {
    public GameObject box;
    public float timestep = 0.01f;
    public int count = 0;
    public int numberOfObjects = 10000;
    public float size = 1.0f;

    private GameObject objTerrain;

    void Start () {
        StartCoroutine(MakeBox());
    }

    public IEnumerator MakeBox(){
        objTerrain = GameObject.Find("Terrain");

        var spawnPoints = new List<Vector3>();
        for (int i = 0; i < 10; ++i) {
            var point = new Vector3(
                transform.position.x+Random.Range(-size,size)+5,
                transform.position.y,
                transform.position.z+Random.Range(-size,size));
            NavMeshHit hit;
            if ( NavMesh.SamplePosition(point, out hit, 2.0f, NavMesh.AllAreas) )
                spawnPoints.Add(hit.position);
            else
                i--;
        }

        for(int i=0;i<numberOfObjects;i=i+1){
            yield return new WaitForSeconds(2.0f);

            for (int j = 0; j < spawnPoints.Count; ++j) {
                GameObject cubeSpawn = (GameObject)Instantiate(box, spawnPoints[j], transform.rotation);

                cubeSpawn.GetComponent<UnitParsFree>().setSearch();

                objTerrain.GetComponent<BattleSystemFree>().unitsBuffer.Add(cubeSpawn);
                count = count+1;
            }
        }
    }

    // function OnGUI() {
    //     GUI.Label(new Rect( 450,5, 30,30),"Number of objects: "+count,textStyle);
    //     GUI.Label(new Rect( 450,15, 30,30),"FPS: "+lastFPS,textStyle);
    // }
}
