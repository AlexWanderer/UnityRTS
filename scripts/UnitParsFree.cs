using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UnitParsFree : MonoBehaviour {
    public bool isBuilding = false;

    public bool isReady = false;
    public bool isApproaching = false;
    public bool isAttacking = false;
    [HideInInspector] public bool isAttackable = true;
    [HideInInspector] public bool onTargetSearch = false;
    public bool isDead = false;
    public bool isSinking = false;

    public GameObject target = null;
    public List<GameObject> attackers = new List<GameObject>();

    public int maxAttackers = 3;

    [HideInInspector] public float prevR;
    [HideInInspector] public int failedR = 0;
    public int critFailedR = 10;

    public float health = 100.0f;

    //  [HideInInspector] public float deathStart = 0.0f;
    //  public float deathDuration = 5.0f;

    //  [HideInInspector] public float sinkStart = 0.0f;
    //  public float sinkDuration = 10.0f;

    [HideInInspector] public int deathCalls = 0;
    public int maxDeathCalls = 5;

    [HideInInspector] public int sinkCalls = 0;
    public int maxSinkCalls = 5;

    [HideInInspector] public bool changeMaterial = true;

    public int alliance = 1;

    // Use this for initialization
    void Start () {
    }
}
