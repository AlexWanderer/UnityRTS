using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ManualControlFree : MonoBehaviour {
    private GameObject objTerrain;

    void Start() {
        objTerrain = GameObject.Find("Terrain");
    }

    void Update () {
        if (Input.GetMouseButtonDown(0)){
            Debug.Log("In input mousedown");
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            objTerrain.GetComponent<TerrainCollider>().enabled = true;
            if (Physics.Raycast (ray, out hit)) {
                Debug.Log("Got hit, setting destination!");
                Debug.Log(string.Format("Point is {0}", hit.point.ToString()));
                GetComponent<NavMeshAgent>().SetDestination(hit.point);
            }
            objTerrain.GetComponent<TerrainCollider>().enabled = false;
        }
    }
}
