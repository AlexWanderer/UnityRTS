using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Formation : MonoBehaviour {
    public int maxSize = 20;
    public float unitSpacing = 3.0f;
    public float depthToWidthRatio = 1.0f/4.0f;

    [HideInInspector] public List<GameObject> units;

    private GameObject objTerrain;
    private int alliance = 0;
    private int width = 0;

    void Start() {
        objTerrain = GameObject.Find("Terrain");

        StartCoroutine(PopulateFormation());
        StartCoroutine(MoveFormation());
    }

    public IEnumerator PopulateFormation() {
        while (true) {
            Debug.Log("Populating...");
            yield return new WaitForSeconds(1.0f);
            if ( units.Count >= maxSize ) continue;

            var battleSystem = objTerrain.GetComponent<BattleSystemFree>();
            for (int i = 0; i < battleSystem.aliveUnits.Count; ++i) {
                var unit = battleSystem.aliveUnits[i];
                var unitPars = unit.GetComponent<UnitParsFree>();
                if ( unitPars.formation == null && unitPars.alliance == alliance ) {
                    units.Add(unit);

                    if ( units.Count == 1 ) {
                        transform.position = unit.transform.position;
                        transform.rotation = unit.transform.rotation;
                    }

                    unitPars.formation = this;
                    unitPars.mode = Mode.NONE;
                    unit.GetComponent<NavMeshObstacle>().enabled = false;
                    unit.GetComponent<NavMeshAgent>().enabled = true;
                    if ( units.Count == maxSize ) break;
                }
            }
            ComputeFormationParameters();
        }
    }

    public IEnumerator MoveFormation() {
        while (true) {
            Debug.Log("Moving...");
            yield return new WaitForSeconds(0.5f);
            if ( units.Count == 0 ) continue;

            for (int i = 0; i < units.Count; ++i) {
                units[i].GetComponent<NavMeshAgent>().SetDestination(GetFormationSlot(i));
            }
        }
    }

    public void ComputeFormationParameters() {
        // width * depth ~= units.Count
        // depth / width = depthToWidthRatio
        // depth = depthToWidthRatio * width
        // width * (depthToWidthRatio * width) ~= units.Count
        // width ^ 2 = units.Count / depthToWidthRatio
        // width = sqrt(units.Count / depthToWidthRatio)

        float tmpWidth = Mathf.Sqrt(units.Count / depthToWidthRatio);
        width = depthToWidthRatio > 1.0 ? Mathf.CeilToInt(tmpWidth) : Mathf.FloorToInt(tmpWidth);
    }

    public Vector3 GetFormationSlot(int i) {
        return transform.position + unitSpacing * ((i % width) * transform.right - (i / width) * transform.forward);
    }
}
