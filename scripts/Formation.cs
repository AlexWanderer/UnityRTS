using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Formation : MonoBehaviour {
    public int size = 20;

    [HideInInspector] public List<GameObject> units;

    private GameObject objTerrain;
    private int alliance = 0;

    void Start() {
        objTerrain = GameObject.Find("Terrain");

        StartCoroutine(PopulateFormation());
        StartCoroutine(MoveFormation());
    }

    public IEnumerator PopulateFormation() {
        while (true) {
            Debug.Log("Populating...");
            yield return new WaitForSeconds(1.0f);
            if ( units.Count >= size ) continue;

            var battleSystem = objTerrain.GetComponent<BattleSystemFree>();
            for (int i = 0; i < battleSystem.aliveUnits.Count; ++i) {
                var unit = battleSystem.aliveUnits[i];
                var unitPars = unit.GetComponent<UnitParsFree>();
                if ( unitPars.formation == null && unitPars.alliance == alliance ) {
                    units.Add(unit);
                    unitPars.formation = this;
                    unitPars.mode = Mode.NONE;
                    unit.GetComponent<NavMeshObstacle>().enabled = false;
                    unit.GetComponent<NavMeshAgent>().enabled = true;
                    if ( units.Count == size ) break;
                }
            }
        }
    }

    public IEnumerator MoveFormation() {
        while (true) {
            Debug.Log("Moving...");
            yield return new WaitForSeconds(0.5f);
            if ( units.Count == 0 ) continue;

            var space = 3;

            units[0].GetComponent<NavMeshAgent>().speed = 1.5f;
            for (int i = 1; i < units.Count; ++i) {
                units[i].GetComponent<NavMeshAgent>().SetDestination(units[0].transform.position
                        + space * ((i/2) % 10) * units[0].transform.right
                        - space * (i % 2)  * units[0].transform.forward);
            }
        }
    }
}
