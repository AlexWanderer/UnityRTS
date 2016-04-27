using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpawnPointFree : MonoBehaviour {
    public GameObject box;

    public float timestep = 2.0f;
    public int spawnNumber = 0;
    public int objectsPerSpawn = 10;

    public float radius = 20.0f;

    private GameObject objTerrain;

    void Start () {
        StartCoroutine(MakeBox());
    }

    public IEnumerator MakeBox(){
        objTerrain = GameObject.Find("Terrain");

        var spawnPoints = new List<Vector3>();
        var counter = 0;
        for (int i = 0; i < spawnNumber && counter < spawnNumber * 3; ++i) {
            var point = new Vector3(
                transform.position.x+Random.Range(-radius,radius)+5,
                transform.position.y,
                transform.position.z+Random.Range(-radius,radius));
            Debug.Log(string.Format("Making point in {0}", point.ToString()));
            NavMeshHit hit;
            if ( NavMesh.SamplePosition(point, out hit, 2.0f, NavMesh.AllAreas) )
                spawnPoints.Add(hit.position);
            else
                i--;
            counter++;
        }
        Debug.Log(string.Format("Created {0} spawns..", spawnPoints.Count));

        for (int i = 0; i < objectsPerSpawn; ++i) {
            yield return new WaitForSeconds(timestep);

            for (int j = 0; j < spawnPoints.Count; ++j) {
                GameObject cubeSpawn = (GameObject)Instantiate(box, spawnPoints[j], transform.rotation);

                cubeSpawn.GetComponent<UnitParsFree>().setSearch();

                objTerrain.GetComponent<BattleSystemFree>().unitsBuffer.Add(cubeSpawn);
            }
        }
    }
}
