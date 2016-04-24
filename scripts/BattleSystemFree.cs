using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BattleSystemFree : MonoBehaviour {
    // BSystem is core component for simulating RTS battles
    // It has 6 phases for attack and gets all different game objects parameters inside.
    // Attack phases are: Search, Approach target, Attack, Self-Heal, Die, Rot (Sink to ground).
    // All 6 phases are running all the time and checking if object is matching criteria, then performing actions
    // Movements between different phases are also described
    private float[] timeloops = new float[7];
    private float[] timeall = new float[7];
    private float[] performance = new float[7];

    private string message = " ";
    private string message1 = " ";
    private string message2 = " ";
    private string message3 = " ";
    private string message4 = " ";
    private string message5 = " ";
    private string message6 = " ";
    private bool displayMessage = true;

    public List<GameObject> unitss = new List<GameObject>();
    public List<GameObject> unitsBuffer = new List<GameObject>();

    public List<GameObject> deadUnits = new List<GameObject>();
    public List<GameObject> sinkingUnits = new List<GameObject>();

    // Use this for initialization
    void Start () {
        // starting spawner
        GameObject objSP1 = GameObject.Find("spawnpointBTetra");
        GameObject objSP2 = GameObject.Find("spawnpointBCube");

        objSP1.GetComponent<SpawnPointFree>().enabled = true;
        objSP2.GetComponent<SpawnPointFree>().enabled = true;

        // starting to add units to main unitss array
        StartCoroutine(AddBuffer());
        StartCoroutine(BoolChecker());

        // Starts all 6 coroutines to start searching for possible units in unitss array.
        StartCoroutine(SearchPhase());
        StartCoroutine(ApproachTargetPhase());
        StartCoroutine(AttackPhase());
        StartCoroutine(DeathPhase());
        StartCoroutine(SinkPhase());
    }

    // Update is called once per frame
    void Update () {
    }

    void OnDestroy() {
        Debug.Log("Script destroyed!!");
    }
    void OnDisable() {
        Debug.Log("Script disabled!!");
    }

    // Display performance
    void OnGUI (){
        if ( displayMessage ) {
            GUI.Label(new Rect(Screen.width * 0.05f, Screen.height * 0.05f, 500f, 20f), message);

            GUI.Label(new Rect(Screen.width * 0.05f, Screen.height * 0.2f, 500f, 20f), message1);
            GUI.Label(new Rect(Screen.width * 0.05f, Screen.height * 0.3f, 500f, 20f), message2);
            GUI.Label(new Rect(Screen.width * 0.05f, Screen.height * 0.4f, 500f, 20f), message3);
            GUI.Label(new Rect(Screen.width * 0.05f, Screen.height * 0.5f, 500f, 20f), message4);
            GUI.Label(new Rect(Screen.width * 0.05f, Screen.height * 0.6f, 500f, 20f), message5);
            GUI.Label(new Rect(Screen.width * 0.05f, Screen.height * 0.7f, 500f, 20f), message6);
        }
    }

    // The main coroutine, which starts to search for nearest enemies neighbours and set them for attack
    // NN search works with kdtree.cs NN search class, implemented by A. Stark at 2009.
    // Target candidates are put on kdtree, while attackers used to search for them.
    // NN searches are based on position coordinates in 3D.
    public IEnumerator SearchPhase() {
        float timeBegin, timeEnd;

        List<GameObject>[] attackers = new List<GameObject>[2];
        List<GameObject>[] defenders = new List<GameObject>[2];

        for (int i = 0; i < attackers.Length; i++) {
            attackers[i] = new List<GameObject>();
            defenders[i] = new List<GameObject>();
        }

        while(true) {
            Debug.Log("Search1");
            timeBegin = Time.realtimeSinceStartup;

            for (int i = 0; i < attackers.Length; i++) {
                attackers[i].Clear();
                defenders[i].Clear();
            }

            // adding back units which becomes attackable (if they get less attackers than defined by critical number)
            var searchCounter = 0;
            for(int i = 0; i<unitss.Count; i++){
                var unit = unitss[i];
                var unitPars = unit.GetComponent<UnitParsFree>();
                int alliance = unitPars.alliance;

                if(unitPars.mode == Mode.SEARCH){
                    attackers[alliance].Add(unit);
                    searchCounter++;
                }
                defenders[alliance].Add(unit);
            }

            var defenderPoints = new List<Vector3[]>();
            for (int i = 0; i < attackers.Length; i++) {
                var points = new Vector3[defenders[i].Count];
                for (int j = 0; j < defenders[i].Count; ++j)
                    points[j] = defenders[i][j].transform.position;
                defenderPoints.Add(points);
            }

            var kdTrees = new List<KDTreeFree>();
            Debug.Log("Before KDMake");
            for (int i = 0; i < attackers.Length; i++)
                kdTrees.Add(KDTreeFree.MakeFromPoints(defenderPoints[i]));

            timeEnd = Time.realtimeSinceStartup;
            Debug.Log("Yield");
            yield return new WaitForSeconds(0.5f);
            Debug.Log("Search2");
            timeloops[1] = timeEnd - timeBegin;
            timeBegin = Time.realtimeSinceStartup;

            for (int i = 0; i < attackers.Length; i++) {
                if ( defenders[1-i].Count == 0 ) continue;

                for (int j = 0; j < attackers[i].Count; ++j) {
                    var att = attackers[i][j];
                    var attPars = att.GetComponent<UnitParsFree>();
                    // Skipp all units which may have changed states during the yield
                    if ( attPars.mode != Mode.SEARCH ) continue;

                    var defenderId = kdTrees[1-i].FindNearest(att.transform.position);
                    var def = defenders[1-i][defenderId];

                    attPars.setApproach(def);
                }
            }
            timeEnd = Time.realtimeSinceStartup;
            yield return new WaitForSeconds(0.5f);

            timeloops[1] += timeEnd - timeBegin;
            timeall[1] = timeloops[1] + 1.0f;
            performance[1] = timeloops[1] * 100.0f / timeall[1];

            message1 = "Search: " + searchCounter.ToString() + "; " + timeloops[1].ToString() + "; " + performance[1].ToString() + "%";
        }
    }

    // this phase starting attackers to move towards their targets
    public IEnumerator ApproachTargetPhase() {
        float timeBegin, timeEnd;

        while(true){
            Debug.Log("Approach");
            timeBegin = Time.realtimeSinceStartup;

            int approachCounter = 0;

            for ( int i = 0; i < unitss.Count; i++ ) {
                GameObject appr = unitss[i];
                UnitParsFree apprPars = appr.GetComponent<UnitParsFree>();

                if ( apprPars.mode != Mode.APPROACH ) continue;
                approachCounter++;

                GameObject targ = apprPars.target;

                if ( targ.GetComponent<UnitParsFree>().mode >= Mode.DEAD ) {
                    apprPars.setSearch();
                    continue;
                }

                if ( appr.GetComponent<NavMeshAgent>().remainingDistance < 0.5f ) {
                    apprPars.setSearch();
                    continue;
                }

                // distance between approacher and target
                var distance = Vector3.Distance(appr.transform.position, targ.transform.position);

                // counting increased distances (failure to approach) between attacker and target;
                // if counter failedR becomes bigger than critFailedR, preparing for new target search.
                if(apprPars.previousTargetDistance < distance) {
                    ++apprPars.failedReachTarget;
                    if ( apprPars.failedReachTarget > UnitParsFree.maxFailedReachTarget) {
                        apprPars.setSearch();
                        continue;
                    }
                } else
                    apprPars.failedReachTarget = 0;
                // saving previous R
                apprPars.previousTargetDistance = distance;
            }
            // main coroutine wait statement and performance information collection from approach coroutine
            timeEnd = Time.realtimeSinceStartup;
            yield return new WaitForSeconds(1.0f);

            timeloops[2] = timeEnd - timeBegin;
            timeall[2] = timeloops[2] + 1.0f;

            performance[2] = timeloops[2] * 100.0f / timeall[2];

            message2 = "Approacher: " + approachCounter.ToString() + "; " + timeloops[2].ToString() + "; " + performance[2].ToString() + "%";
        }
    }

    // Attacking phase set attackers to attack their targets and cause damage when they already approached their targets
    public IEnumerator AttackPhase() {
        float timeBegin, timeEnd;

        var deadUnitsLocal = new List<GameObject>();

        while (true) {
            Debug.Log("Attack");
            timeBegin = Time.realtimeSinceStartup;

            int attackerCounter = 0;
            deadUnitsLocal.Clear();

            // checking through main unitss array which units are set to approach (isAttacking)
            for (int i = 0; i < unitss.Count; i++) {
                GameObject att = unitss[i];
                UnitParsFree attPars = att.GetComponent<UnitParsFree>();

                if ( attPars.mode != Mode.ATTACK ) continue;
                attackerCounter++;

                GameObject targ = attPars.target;
                UnitParsFree targPars = targ.GetComponent<UnitParsFree>();

                if ( targPars.mode >= Mode.DEAD ) {
                    attPars.setSearch();
                    continue;
                }

                // If attack passes target through target defence, cause damage to target
                if ( Random.value > 0.5 ) {
                    targPars.health -= 20f * Random.value;

                    if (targPars.health < 0.0f) {
                        if ( targPars.mode >= Mode.DEAD )
                            Debug.Log("PRE-TWO-DEAD!!");
                        if ( deadUnitsLocal.Contains(targ) )
                            Debug.Log("PRE-DUPLICATE!!");
                        if ( deadUnits.Contains(targ) )
                            Debug.Log(string.Format("DEAD IN UNITS MAN, state is: {0}", targPars.mode.ToString()));
                        targPars.mode = Mode.DEAD;
                        deadUnitsLocal.Add(targ);
                    }
                }
            }
            for (int i = 0; i < deadUnitsLocal.Count; ++i) {
                if ( deadUnits.Contains(deadUnitsLocal[i]) )
                    Debug.Log("DUPLICATE!!");
                deadUnits.Add(deadUnitsLocal[i]);
                if ( !unitss.Remove(deadUnitsLocal[i]) )
                    Debug.Log("Failed to removed dead unit from unitss");

                deadUnitsLocal[i].GetComponent<UnitParsFree>().setDead();
            }
            timeEnd = Time.realtimeSinceStartup;
            yield return new WaitForSeconds(1.0f);

            timeloops[3] = timeEnd - timeBegin;
            timeall[3] = timeloops[3] + 1.0f;

            performance[3] = timeloops[3] * 100.0f / timeall[3];

            message3 = "Attacker: " + attackerCounter.ToString() + "; " + timeloops[3].ToString() + "; " + timeall[3].ToString() + "%";
        }
    }

    // Death phase unset all unit activity and prepare to die
    public IEnumerator DeathPhase(){
        float timeBegin, timeEnd;

        while(true){
            Debug.Log("Death");
            timeBegin = Time.realtimeSinceStartup;

            // Getting dying units
            for(int i = 0; i < deadUnits.Count; i++){
                GameObject dead = deadUnits[i];
                UnitParsFree deadPars = dead.GetComponent<UnitParsFree>();

                if ( deadPars.mode == Mode.SINK )
                    Debug.Log("SANK IN DEAD");

                // If unit is dead long enough, prepare for rotting (sinking) phase and removing from the unitss list
                if(deadPars.deathCalls > UnitParsFree.maxDeathCalls){
                    deadPars.setSinking();
                    sinkingUnits.Add(dead);
                    deadUnits.RemoveAt(i--);
                } else
                    deadPars.deathCalls++;
            }
            timeEnd = Time.realtimeSinceStartup;
            yield return new WaitForSeconds(1.0f);

            timeloops[5] = timeEnd - timeBegin;
            timeall[5] = timeloops[5] + 1.0f;

            performance[5] = timeloops[5] * 100.0f / timeall[5];

            message5 = "Dead: " + deadUnits.Count.ToString() + "; " + timeloops[5].ToString() + "; " + performance[5].ToString() + "%";
        }
    }

    // rotting or sink phase includes time before unit is destroyed: for example to perform rotting animation or sink object into the ground
    public IEnumerator SinkPhase() {
        float timeBegin, timeEnd;

        while(true){
            Debug.Log("Sink");
            timeBegin = Time.realtimeSinceStartup;
            for (int i = 0; i < sinkingUnits.Count; i++){
                GameObject sink = sinkingUnits[i];
                var sinkPars = sink.GetComponent<UnitParsFree>();

                if (sinkPars.sinkCalls > UnitParsFree.maxSinkCalls) {
                    sinkingUnits.RemoveAt(i--);
                    Destroy(sink);
                } else {
                    sinkPars.sinkCalls++;
                    // Moving sinking object down into the ground
                    sink.transform.Translate(0.0f, -0.1f, 0.0f, Space.World);
                }
            }
            timeEnd = Time.realtimeSinceStartup;
            yield return new WaitForSeconds(1.0f);

            timeloops[6] = timeEnd - timeBegin;
            timeall[6] = timeloops[6] + 1.0f;

            performance[6] = timeloops[6] * 100.0f / timeall[6];

            message6 = "Sink: " + sinkingUnits.Count.ToString() + "; " + timeloops[6].ToString() + "; " + performance[6].ToString() + "%";
        }
    }

    // additional conditions check to set bool values
    public IEnumerator BoolChecker() {
        while (true) {
            Debug.Log("BoolChecker");
            // total performance calculation from Battle System
            timeloops[0] = timeloops[1] + timeloops[2] + timeloops[3] + timeloops[4] + timeloops[5] + timeloops[6];
            timeall[0] = timeall[1] + timeall[2] + timeall[3] + timeall[4] + timeall[5] + timeall[6];

            performance[0] = (performance[1] +
                              performance[2] +
                              performance[3] +
                              performance[4] +
                              performance[5] +
                              performance[6])/6.0f;

            message = ("BSystem: " + (unitss.Count).ToString() + "; "
                                 + (timeloops[0]).ToString() + "; "
                                 + (timeall[0]).ToString() + "; "
                                 + (performance[0]).ToString() + "% ");

            yield return new WaitForSeconds(0.5f);
        }
    }

    // adding new units from buffer to BSystem :
    // units, which are wanted to be used on BSystem should be placed to unitsBuffer array first
    public IEnumerator AddBuffer() {
        while (true) {
            Debug.Log("AddBuffer");
            int maxbuffer = unitsBuffer.Count;
            for(int i =0; i<maxbuffer; i++)
                unitss.Add(unitsBuffer[i]);

            // cleaning up buffer
            for(int i =0; i<unitss.Count; i++)
                unitsBuffer.Remove(unitss[i]);
            yield return new WaitForSeconds(0.5f);
        }
    }

    // ManualMover controls unit if it is selected and target is defined by player
    public IEnumerator ManualMover() {
        float r;

        float ax;
        float ay;
        float az;

        float tx;
        float ty;
        float tz;

        while(true){
            Debug.Log("ManualMover");
            for(int i =0; i<unitss.Count; i++){
                GameObject obj = unitss[i];
                ManualControlFree objSel = obj.GetComponent<ManualControlFree>();

                if(objSel.isMoving){
                    ax = obj.transform.position.x;
                    ay = obj.transform.position.y;
                    az = obj.transform.position.z;

                    tx = objSel.manualDestination.x;
                    ty = objSel.manualDestination.y;
                    tz = objSel.manualDestination.z;

                    r = Mathf.Sqrt((tx-ax)*(tx-ax)+(ty-ay)*(ty-ay)+(tz-az)*(tz-az));

                    if(r >= objSel.prevDist){
                        objSel.failedDist = objSel.failedDist+1;
                        if(objSel.failedDist > objSel.critFailedDist){
                            objSel.failedDist = 0;
                            objSel.isMoving = false;
                            ResetSearching(obj);
                        }
                    }
                    objSel.prevDist = r;
                }
                if(objSel.prepareMoving){
                    UnSetSearching(obj);

                    objSel.prepareMoving = false;
                    objSel.isMoving = true;

                    obj.GetComponent<NavMeshAgent>().SetDestination(objSel.manualDestination);
                }
            }
            yield return new WaitForSeconds(0.5f);
        }
    }

    // single action functions
    public void AddUnit(GameObject go) {
        unitsBuffer.Add(go);
    }

    public void ResetSearching(GameObject go) {
        UnitParsFree goPars = go.GetComponent<UnitParsFree>();
        goPars.setSearch();
    }

    public void UnSetSearching(GameObject go) {
        UnitParsFree goPars = go.GetComponent<UnitParsFree>();

        goPars.mode = Mode.NONE;
        goPars.target = null;

        go.GetComponent<NavMeshAgent>().SetDestination(go.transform.position);

        if(goPars.changeMaterial)
            go.GetComponent<Renderer>().material.color = Color.grey;
    }
}
