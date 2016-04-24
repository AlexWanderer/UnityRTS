using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum Mode {
    NONE = 0,
    SEARCH = 1,
    APPROACH = 2,
    ATTACK = 3,
    DEAD = 4,
    SINK = 5
};

public class UnitParsFree : MonoBehaviour {
    public bool isBuilding = false;

    public Mode mode = Mode.SEARCH;

    public GameObject target = null;

    public float previousTargetDistance;
    public int failedReachTarget = 0;
    public static int maxFailedReachTarget = 3;

    public float health = 100.0f;

    [HideInInspector] public int deathCalls = 0;
    public static int maxDeathCalls = 5;

    [HideInInspector] public int sinkCalls = 0;
    public static int maxSinkCalls = 5;

    [HideInInspector] public bool changeMaterial = true;

    public int alliance = 1;

    void Start() {
    }

    void OnTriggerEnter(Collider collider) {
        var target = collider.gameObject;
        var targetPar = target.GetComponent<UnitParsFree>();
        if ( targetPar.alliance == 1 - alliance && mode <= Mode.APPROACH && targetPar.mode < Mode.DEAD )
            setAttacking(collider.gameObject);
    }

    public void setSearch() {
        GetComponent<NavMeshAgent>().enabled = false;
        GetComponent<NavMeshObstacle>().enabled = true;

        target = null;
        if ( mode >= Mode.DEAD )
            Debug.Log("Set DEAD to search!");
        mode = Mode.SEARCH;

        if(changeMaterial)
            GetComponent<Renderer>().material.color = Color.yellow;
    }

    public void setApproach(GameObject t) {
        GetComponent<NavMeshObstacle>().enabled = false;
        GetComponent<NavMeshAgent>().enabled = true;

        previousTargetDistance = 0;
        failedReachTarget = 0;
        target = t;

        if ( mode >= Mode.DEAD )
            Debug.Log("Set DEAD to approach!");
        mode = Mode.APPROACH;

        if ( !isBuilding ) {
            GetComponent<NavMeshAgent>().SetDestination(t.transform.position);
            GetComponent<NavMeshAgent>().speed = 3.5f;
        }

        if(changeMaterial)
            GetComponent<Renderer>().material.color = Color.green;
    }

    public void setAttacking(GameObject t) {
        GetComponent<NavMeshAgent>().enabled = false;
        GetComponent<NavMeshObstacle>().enabled = true;

        if ( mode >= Mode.DEAD )
            Debug.Log("Set DEAD to attack!");
        mode = Mode.ATTACK;
        target = t;

        if(changeMaterial)
            GetComponent<Renderer>().material.color = Color.red;
    }

    public void setDead() {
        mode = Mode.DEAD;
        target = null;

        GetComponent<NavMeshAgent>().enabled = false;
        GetComponent<NavMeshObstacle>().enabled = true;

        SendMessage("OnUnselected", SendMessageOptions.DontRequireReceiver);
        transform.gameObject.tag = "Untagged";

        if(changeMaterial)
            GetComponent<Renderer>().material.color = Color.blue;
    }

    public void setSinking() {
        if ( mode == Mode.SINK )
            Debug.Log("TWO SINKS!!");

        GetComponent<NavMeshObstacle>().enabled = false;

        mode = Mode.SINK;

        if ( changeMaterial )
            GetComponent<Renderer>().material.color = new Color((148.0f/255.0f),(0.0f/255.0f),(211.0f/255.0f),1.0f);
    }
}
