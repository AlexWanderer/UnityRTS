using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ManualControlFree : MonoBehaviour {
    public bool isSelected = false;
    public bool prepareMoving = false;
    public bool isMoving = false;

    [HideInInspector] public float prevDist = 0.0f;
    [HideInInspector] public int failedDist = 0;
    public int critFailedDist = 10;

    public Vector3 manualDestination;

    // private GameObject terrain =  GameObject.Find("Terrain");
    // private BattleSystem bs = terrain.GetComponent<BattleSystem>();

    private void OnSelected() {
        isSelected = true;
        // renderer.material.color = Color.red;
    }

    private void OnUnselected() {
        isSelected = false;
        // renderer.material.color = Color.white;
    }

    // void Start() {
    //     StartCoroutine(SelectChecker());
    // }

    void Update () {
        // public IEnumerator SelectChecker(){

        // var clickPoint : Vector3;
        // var ray = Camera.main.ScreenPointToRay (Input.mousePosition);
        // var hit : RaycastHit;
        // Float hitdist = 0.0;
        // while(true){
        if(isSelected == true){
            if (Input.GetMouseButtonUp(1)){
                // Plane playerPlane = new Plane(Vector3.up, transform.position);
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                // Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
                if (Physics.Raycast (ray, out hit)) {
                    // var targetPoint = ray.GetPoint(hitdist);
                    // targetPosition = ray.GetPoint(hitdist);
                    // GetComponent<AIPath>().targetPositioner=hit.point; //ray.GetPoint(hitdist);
                    // GameObject thisgo = this.gameObject;
                    // if(GetComponent<UnitPars>().isMovable){
                    // PrepareMove();
                        manualDestination = hit.point;
                        this.prepareMoving = true;
                    // GameObject.Find("Terrain").GetComponent<BattleSystem>().UnSetSearching(this.gameObject);
                    // GetComponent<NavMeshAgent>().SetDestination(hit.point);
                // }
                }
                // (Random.value)*2000;
                // GetComponent<AIPath>().Start();
            }
        }
        // yield return new WaitForSeconds(0.05f);
        // }
    }
}
