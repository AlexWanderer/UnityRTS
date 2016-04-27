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
    private int width = 1;
    private int currentWidth = 1, currentDepth = 1;

    void Start() {
        objTerrain = GameObject.Find("Terrain");

        StartCoroutine(PopulateFormation());
        StartCoroutine(MoveFormation());
    }

    public IEnumerator PopulateFormation() {
        while (true) {
            yield return new WaitForSeconds(1.0f);
            if ( units.Count >= maxSize ) continue;

            var battleSystem = objTerrain.GetComponent<BattleSystemFree>();
            for (int i = 0; i < battleSystem.aliveUnits.Count; ++i) {
                var unit = battleSystem.aliveUnits[i];
                var unitPars = unit.GetComponent<UnitParsFree>();
                if ( unitPars.formation == null && unitPars.alliance == alliance ) {
                    units.Add(unit);

                    if ( units.Count == 1 ) {
                        GetComponent<NavMeshAgent>().enabled = false;
                        transform.position = unit.transform.position;
                        transform.rotation = unit.transform.rotation;
                        GetComponent<NavMeshAgent>().enabled = true;
                    }

                    unitPars.formation = this;
                    unitPars.mode = Mode.NONE;
                    unit.GetComponent<NavMeshObstacle>().enabled = false;
                    unit.GetComponent<NavMeshAgent>().enabled = true;
                    if ( units.Count == maxSize ) break;
                }
            }
            ComputeWidth();
        }
    }

    public IEnumerator MoveFormation() {
        while (true) {
            yield return new WaitForSeconds(0.5f);
            if ( units.Count == 0 ) continue;
            if ( false /* check if forward points are both in mesh */ ) {
                ReconfigureFormation(currentWidth - 1);
            } else if ( currentWidth < width ) {
                ReconfigureFormation(currentWidth + 1);
            }

            for (int i = 0; i < units.Count; ++i) {
                if ( units[i].GetComponent<UnitParsFree>().mode < Mode.ATTACK )
                    units[i].GetComponent<NavMeshAgent>().SetDestination(GetFormationSlot(i));
            }
        }
    }

    public void ComputeWidth() {
        // width * depth ~= units.Count
        // depth / width = depthToWidthRatio
        // depth = depthToWidthRatio * width
        // width * (depthToWidthRatio * width) ~= units.Count
        // width ^ 2 = units.Count / depthToWidthRatio
        // width = sqrt(units.Count / depthToWidthRatio)
        if ( !units.Count ) return;
        float tmpWidth = Mathf.Sqrt(units.Count / depthToWidthRatio);
        width = depthToWidthRatio > 1.0 ? Mathf.CeilToInt(tmpWidth) : Mathf.FloorToInt(tmpWidth);
    }

    public void ReconfigureFormation(int w) {
        currentWidth = w;
        currentDepth = Mathf.CeilToInt(units.Count / (float)currentWidth);

        transform.localScale = new Vector3(currentWidth * unitSpacing, 1, currentDepth * unitSpacing);
    }

    public Vector3 GetFormationSlot(int i) {
        return transform.position + unitSpacing * (((i % currentWidth) - (currentWidth - 1) / 2.0f) * transform.right
                                                   - (i / currentWidth - (currentDepth - 1) / 2.0f) * transform.forward);
    }
}
