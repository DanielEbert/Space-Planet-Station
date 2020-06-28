using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstantiateJob : Job
{
    public GameObject instantiate;
    public Building building;

    List<GameObject> constructionMarkers;

    public InstantiateJob(Building _building) {
        building = _building;
        constructionMarkers = new List<GameObject>();

        foreach (BuildingNode g in building.buildingNodes) {
            GameObject gameObject = ObjectPool.Instance.pools["PurpleTransparentHexagon"].get(g.worldPos, Quaternion.identity);
            constructionMarkers.Add(gameObject);
            gameObject.SetActive(true);
        }
    }

    public override void OnComplete() {
        foreach (GameObject g in constructionMarkers) {
            ObjectPool.Instance.pools["PurpleTransparentHexagon"].put(g);
        }
        constructionMarkers = null;
        building.OnBuilt();
    }
}
