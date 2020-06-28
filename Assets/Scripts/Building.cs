using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building {
    public GameObject instantiatedGO;

    public GameObject buildingGO;

    public List<BuildingNode> buildingNodes;
    public HashSet<PlanetNode> workerNodes;

    public Building(GameObject buildingGO) {
        this.buildingGO = buildingGO;
    }

    public void OnBuilt() {
        instantiatedGO = MonoBehaviour.Instantiate(buildingGO, buildingNodes[0].worldPos, Quaternion.identity);
    }
}
