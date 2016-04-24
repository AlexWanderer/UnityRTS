using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BattleSystemFree : MonoBehaviour {
    // BSystem is core component for simulating RTS battles
    // It has 6 phases for attack and gets all different game objects parameters inside.
    // Attack phases are: Search, Approach target, Attack, Self-Heal, Die, Rot (Sink to ground).
    // All 6 phases are running all the time and checking if object is matching criteria, then performing actions
    // Movements between different phases are also described
    public float attackDistance = 70.0f;

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
        GameObject objSP1 =  GameObject.Find("spawnpointBTetra");
        GameObject objSP2 =  GameObject.Find("spawnpointBCube");

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

        StartCoroutine(ManualMover());
    }

    // Update is called once per frame
    void Update () {
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
            Debug.Log("Search");
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

                if(unitPars.isReady){
                    attackers[alliance].Add(unit);
                    searchCounter++;
                }
                if(unitPars.isAttackable == true)
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
            for (int i = 0; i < attackers.Length; i++)
                kdTrees.Add(KDTreeFree.MakeFromPoints(defenderPoints[i]));

            timeEnd = Time.realtimeSinceStartup;
            yield return new WaitForSeconds(0.5f);
            Debug.Log("Search");
            timeloops[1] = timeEnd - timeBegin;
            timeBegin = Time.realtimeSinceStartup;

            for (int i = 0; i < attackers.Length; i++) {
                if ( defenders[1-i].Count == 0 ) continue;

                for (int j = 0; j < attackers[i].Count; ++j) {
                    var att = attackers[i][j];
                    var attPars = att.GetComponent<UnitParsFree>();

                    var defenderId = kdTrees[1-i].FindNearest(att.transform.position);
                    var def = defenders[1-i][defenderId];
                    var defPars = def.GetComponent<UnitParsFree>();

                    if ( !defPars.isAttackable ) continue;

                    attPars.target = def;
                    attPars.isReady = false;
                    attPars.isApproaching = true;

                    defPars.attackers.Add(att);
                    if ( defPars.attackers.Count > defPars.maxAttackers )
                        defPars.isAttackable = false;
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

        float stopDist = 2.0f;
        float stoppDistance;

        while(true){
            Debug.Log("Approach");
            timeBegin = Time.realtimeSinceStartup;

            int approachCounter = 0;

            for ( int i = 0; i < unitss.Count; i++ ) {
                GameObject appr = unitss[i];
                UnitParsFree apprPars = appr.GetComponent<UnitParsFree>();

                if ( !apprPars.isApproaching ) continue;
                approachCounter++;

                GameObject targ = apprPars.target;

                NavMeshAgent apprNav = appr.GetComponent<NavMeshAgent>();
                NavMeshAgent targNav = targ.GetComponent<NavMeshAgent>();

                if ( targ.GetComponent<UnitParsFree>().isDead ) {
                    apprPars.target = null;
                    apprNav.SetDestination(appr.transform.position);

                    apprPars.isApproaching = false;
                    apprPars.isReady = true;

                    if(apprPars.changeMaterial)
                            appr.GetComponent<Renderer>().material.color = Color.yellow;
                    continue;
                }

                // stopping condition for NavMesh
                apprNav.stoppingDistance = apprNav.radius/(appr.transform.localScale.x) + targNav.radius/(targ.transform.localScale.x);

                // distance between approacher and target
                var distance = Vector3.Distance(appr.transform.position, targ.transform.position);
                stoppDistance = (stopDist + appr.transform.localScale.x*targ.transform.localScale.x*apprNav.stoppingDistance);

                // counting increased distances (failure to approach) between attacker and target;
                // if counter failedR becomes bigger than critFailedR, preparing for new target search.
                if(apprPars.prevR < distance){
                    apprPars.failedR = apprPars.failedR + 1;
                    if(apprPars.failedR > apprPars.critFailedR){
                        apprPars.isApproaching = false;
                        apprPars.isReady = true;
                        apprPars.failedR = 0;

                        if(apprPars.changeMaterial)
                            appr.GetComponent<Renderer>().material.color = Color.yellow;
                    }
                } else {
                    // If approachers already close to their targets
                    if(distance < stoppDistance){
                        // pre-setting for attacking
                        apprPars.isApproaching = false;
                        setAttacking(appr);
                    } else {
                        if(apprPars.changeMaterial)
                            appr.GetComponent<Renderer>().material.color = Color.green;

                        if ( !apprPars.isBuilding ) {
                            apprNav.SetDestination(targ.transform.position);
                            apprNav.speed = 3.5f;
                        }
                    }
                }
                // saving previous R
                apprPars.prevR = distance;
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

    public void setAttacking(GameObject unit) {
        unit.GetComponent<NavMeshAgent>().SetDestination(unit.transform.position);

        UnitParsFree unitPars = unit.GetComponent<UnitParsFree>();
        unitPars.isAttacking = true;

        if(unitPars.changeMaterial)
            unit.GetComponent<Renderer>().material.color = Color.red;
    }

    // Attacking phase set attackers to attack their targets and cause damage when they already approached their targets
    public IEnumerator AttackPhase() {
        float timeBegin, timeEnd;

        float stopDist = 2.5f;
        float stoppDistance;

        var deadUnits = new List<GameObject>();

        while(true){
            Debug.Log("Attack");
            timeBegin = Time.realtimeSinceStartup;

            int attackerCounter = 0;
            deadUnits.Clear();

            // checking through main unitss array which units are set to approach (isAttacking)
            for(int i = 0; i<unitss.Count; i++){
                GameObject att = unitss[i];
                UnitParsFree attPars = att.GetComponent<UnitParsFree>();

                if ( !attPars.isAttacking ) continue;
                attackerCounter++;

                GameObject targ = attPars.target;
                UnitParsFree targPars = targ.GetComponent<UnitParsFree>();

                if ( targPars.isDead ) {
                    attPars.isAttacking = false;
                    attPars.isReady = true;

                    targPars.attackers.Remove(att);

                    if(attPars.changeMaterial)
                        att.GetComponent<Renderer>().material.color = Color.yellow;

                    continue;
                }

                var attPos = att.transform.position;
                var targPos = targ.transform.position;

                NavMeshAgent attNav = att.GetComponent<NavMeshAgent>();
                NavMeshAgent targNav = targ.GetComponent<NavMeshAgent>();

                attNav.stoppingDistance = attNav.radius/(att.transform.localScale.x) + targNav.radius/(targ.transform.localScale.x);

                // distance between attacker and target
                var distance = Vector3.Distance(attPos, targPos);
                stoppDistance = (stopDist + att.transform.localScale.x*targ.transform.localScale.x*attNav.stoppingDistance);

                // if target moves away, resetting back to approach target phase
                if(distance > stoppDistance){
                    attPars.isApproaching = true;
                    attPars.isAttacking = false;

                    targPars.attackers.Remove(att);
                    if ( targPars.attackers.Count < targPars.maxAttackers )
                        targPars.isAttackable = true;
                } else {
                    // If attack passes target through target defence, cause damage to target
                    if ( Random.value > 0.5 ) {
                        targPars.health -= 20f * Random.value;

                        if(targPars.health < 0.0f) {
                            targPars.isDead = true;
                            deadUnits.Add(targ);
                        }
                    }
                }
            }
            for (int i = 0; i < deadUnits.Count; ++i)
                setDead(deadUnits[i]);
            timeEnd = Time.realtimeSinceStartup;
            yield return new WaitForSeconds(1.0f);

            timeloops[3] = timeEnd - timeBegin;
            timeall[3] = timeloops[3] + 1.0f;

            performance[3] = timeloops[3] * 100.0f / timeall[3];

            message3 = "Attacker: " + attackerCounter.ToString() + "; " + timeloops[3].ToString() + "; " + timeall[3].ToString() + "%";
        }
    }

    public void setDead(GameObject unit) {
        deadUnits.Add(unit);
        unitss.Remove(unit);

        var unitPars = unit.GetComponent<UnitParsFree>();

        unitPars.isDead = true;
        unitPars.isReady = false;
        unitPars.isApproaching = false;
        unitPars.isAttacking = false;
        unitPars.isAttackable = false;
        unitPars.target = null;

        unit.GetComponent<NavMeshAgent>().SetDestination(unit.transform.position);

        unit.SendMessage("OnUnselected", SendMessageOptions.DontRequireReceiver);
        unit.transform.gameObject.tag = "Untagged";

        if(unitPars.changeMaterial)
            unit.GetComponent<Renderer>().material.color = Color.blue;
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

                // If unit is dead long enough, prepare for rotting (sinking) phase and removing from the unitss list
                if(deadPars.deathCalls > deadPars.maxDeathCalls){
                    setSinking(dead);
                    deadUnits.RemoveAt(i--);
                } else
                    deadPars.deathCalls = deadPars.deathCalls + 1;
            }
            timeEnd = Time.realtimeSinceStartup;
            yield return new WaitForSeconds(1.0f);

            timeloops[5] = timeEnd - timeBegin;
            timeall[5] = timeloops[5] + 1.0f;

            performance[5] = timeloops[5]*100.0f/timeall[5];

            message5 = "Dead: " + deadUnits.Count.ToString() + "; " + timeloops[5].ToString() + "; " + performance[5].ToString() + "%";
        }
    }

    public void setSinking(GameObject unit) {
        unit.GetComponent<NavMeshAgent>().enabled = false;
        sinkingUnits.Add(unit);

        var unitPars = unit.GetComponent<UnitParsFree>();
        if ( unitPars.changeMaterial )
            unit.GetComponent<Renderer>().material.color = new Color((148.0f/255.0f),(0.0f/255.0f),(211.0f/255.0f),1.0f);

        unitPars.isSinking = true;
        unitPars.deathCalls = 0; // We reuse this for sinking ticks
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

                if (sinkPars.deathCalls > 10) {
                    sinkingUnits.RemoveAt(i--);
                    Destroy(sink.gameObject);
                } else {
                    sinkPars.deathCalls = sinkPars.deathCalls + 1;
                    // Moving sinking object down into the ground
                    sink.transform.Translate(0.0f, -0.1f, 0.0f, Space.World);
                }
            }
            timeEnd = Time.realtimeSinceStartup;
            yield return new WaitForSeconds(1.0f);

            timeloops[6] = timeEnd - timeBegin;
            timeall[6] = timeloops[6] + 1.0f;

            performance[6] = timeloops[6]*100.0f/timeall[6];

            message6 = "Sink: " + sinkingUnits.Count.ToString() + "; " + timeloops[6].ToString() + "; " + performance[6].ToString() + "%";
        }
    }

    // additional conditions check to set bool values
    public IEnumerator BoolChecker() {
        while(true){
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
        while(true){
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

        goPars.isApproaching = false;
        goPars.isAttacking = false;
        goPars.target = null;

        go.GetComponent<NavMeshAgent>().SetDestination(go.transform.position);

        if(goPars.changeMaterial)
            go.GetComponent<Renderer>().material.color = Color.yellow;

        goPars.isReady = true;
    }

    public void UnSetSearching(GameObject go) {
        UnitParsFree goPars = go.GetComponent<UnitParsFree>();
        // unitsBuffer.Remove(go);

        goPars.isReady = false;
        goPars.isApproaching = false;
        goPars.isAttacking = false;
        goPars.target = null;

        go.GetComponent<NavMeshAgent>().SetDestination(go.transform.position);

        if(goPars.changeMaterial){
            go.GetComponent<Renderer>().material.color = Color.grey;
        }
    }
}
