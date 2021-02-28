using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using UnityEngine;
using UnityEditor;

public class World : MonoBehaviour {
    public int width = 50;
    public int height = 50;
    public int border = 5;
    public float noiseThreshhold = .5f;
    public float noiseScale = 0.05f;
    public GameObject hexagon;
    public float xOffset, yOffset;
    public static ConcurrentDictionary<Vector2Int, Node> nodes;
    public static ConcurrentDictionary<Vector2Int, Node> blankNodes;
    public static ConcurrentDictionary<Vector2Int, PlanetNode> planetNodes;

    public static World instance;

    [Header("DEBUG")]
    public bool showNodeCoord = false;
    public bool showOccupied = false;

    Noise noise;

    private Transform nodeHolder;

    void Awake() {
        instance = this;
        noise = new Noise((int)(Time.time * 100));
        nodes = new ConcurrentDictionary<Vector2Int, Node>();
        blankNodes = new ConcurrentDictionary<Vector2Int, Node>();
        planetNodes = new ConcurrentDictionary<Vector2Int, PlanetNode>();
        nodeHolder = new GameObject().transform;
        nodeHolder.name = "Map";
        Generate(width, height);
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.G)) {
            foreach (Transform child in nodeHolder)
                Destroy(child.gameObject);
            noise = new Noise((int)(Time.time * 100));
            nodes = new ConcurrentDictionary<Vector2Int, Node>();
            blankNodes = new ConcurrentDictionary<Vector2Int, Node>();
            planetNodes = new ConcurrentDictionary<Vector2Int, PlanetNode>();
            Generate(width, height);
        }
		
		if(Input.GetKeyDown(KeyCode.Escape) == true)
		 {
			Application.Quit();
		 }
    }

    void Generate(int width, int height) {

        Vector2 planetCentre = new Vector2(width / 2f, height / 2f);
        float planetRadius = Mathf.Max(width, height) / 2f;

        for(int y = 0; y * yOffset <= height / 2 + 2 * border * yOffset; y++) {
            for(int x =  0; x * xOffset <= width / 2 + 2 * border * xOffset; x++) {
                Vector3 position = new Vector3(x * xOffset + y * yOffset * .5f + (y % 2) * 0.079f -  (y / 2) * yOffset, y * yOffset, 0);
                if (position.magnitude <= planetRadius) {
                    float noiseValue = (noise.Evaluate(position * noiseScale) + 1) * .5f;
                    if (noiseValue >= noiseThreshhold) {
                        CreatePlanetNode(x - y / 2, y, position);
                    } else {
                        CreateBlankNode(x - y / 2, y, position);
                    }
                } else {
                    CreateBlankNode(x - y / 2, y, position);
                }

                if (x != 0) {
                    x *= -1;
                    if (x < 0) {
                        x--;
                    }
                }
            }

            if (y != 0) {
                y *= -1;
                if (y < 0) {
                    y--;
                }
            }
        }

        ProcessMap();

        StaticBatchingUtility.Combine(nodeHolder.gameObject);
    }

    List<PlanetNode> GetPlanetRegionNodes(PlanetNode planetNode) {
        List<PlanetNode> regionNodes = new List<PlanetNode>();
        if (!planetNodes.ContainsKey(new Vector2Int(planetNode.Q, planetNode.R)))
            return regionNodes;
        HashSet<PlanetNode> visitedNodes = new HashSet<PlanetNode>();
        
        Queue<PlanetNode> queue = new Queue<PlanetNode>();
        queue.Enqueue(planetNode);
        visitedNodes.Add(planetNode);

        while (queue.Count > 0) {
            PlanetNode curNode = queue.Dequeue();
            regionNodes.Add(curNode);

            foreach (PlanetNode neighbour in curNode.getNeighbours()) {
                if (!visitedNodes.Contains(neighbour)) {
                    visitedNodes.Add(neighbour);
                    queue.Enqueue(neighbour);
                }
            }
        }

        return regionNodes;
    }

    List<List<PlanetNode>> GetPlanetRegions() {
        List<List<PlanetNode>> regions = new List<List<PlanetNode>>();
        HashSet<Vector2Int> visitedNodes = new HashSet<Vector2Int>();
        foreach(PlanetNode planetNode in planetNodes.Values) {
            if (visitedNodes.Contains(new Vector2Int(planetNode.Q, planetNode.R))) {
                continue;
            }
            
            List<PlanetNode> newRegion = GetPlanetRegionNodes(planetNode);
            regions.Add(newRegion);
            foreach (PlanetNode p in newRegion) 
                visitedNodes.Add(new Vector2Int(p.Q, p.R));
        }
        return regions;
    }

    void ProcessMap() {
        List<List<PlanetNode>> planetRegions = GetPlanetRegions();

        if (planetRegions.Count == 0)
            return;

        int curMax = -1;
        foreach (List<PlanetNode> region in planetRegions) {
            if (region.Count > curMax)
                curMax = region.Count;
        }

        foreach (List<PlanetNode> region in planetRegions) {
            if (region.Count < curMax)
                foreach (PlanetNode planetNode in region) {
                    RemoveNode(planetNode);
                }
        }
    }

    void CreatePlanetNode(int x, int y, Vector3 pos) {
        GameObject g = Instantiate(hexagon, pos, Quaternion.identity);
        g.transform.parent = nodeHolder;
        g.name = "Node_" + x + "_" + y;
        PlanetNode planetNode = new PlanetNode(x, y, pos, g);
        nodes[new Vector2Int(x, y)] = planetNode;
        planetNodes[new Vector2Int(x, y)] = planetNode;
    }

    void CreateBlankNode(int x, int y, Vector3 pos) {
        Node blankNode = new Node(x, y, pos);
        nodes[new Vector2Int(x, y)] = blankNode;
        blankNodes[new Vector2Int(x, y)] = blankNode;
    }

    void RemoveNode(Node node) {
        //We don't really remove the 'node' but we make the node to a blanknode
        Vector2Int key = new Vector2Int(node.Q, node.R);
        if (!nodes.ContainsKey(key))
            return;
            
        if (node is PlanetNode) {
            PlanetNode planetNode = node as PlanetNode;
            nodes.TryRemove(key, out _);
            planetNodes.TryRemove(key, out _);
            Destroy(planetNode.gameObject);
            CreateBlankNode(key.x, key.y, planetNode.worldPos);
        }
    }

    public static Vector2Int nodeCoordFromWorldPos(Vector2 p) {
        //Vector2 p = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition);

        int y = (int)((p.y + Mathf.Sign(p.y)*0.898f/2) / 0.898f);

        float posY = y * 0.898f;

        float oddRowAdd = 0f;

        if (Mathf.Abs(y) % 2 == 1) {
            oddRowAdd += 1.044f / 2f;
        }
        
        int x = (int)((p.x + Mathf.Sign(p.x)*0.898f/2 + oddRowAdd) / 1.044f);

        if (y % 2 == 1) // no abs(y) here because somehow its only for y > 0
            x--;

        return new Vector2Int(x - y / 2, y);
    }

    /*void OnDrawGizmos() {
        if (showNodeCoord && nodes != null)
            foreach(KeyValuePair<Vector2Int, Node> entry in nodes) {
                Handles.Label(entry.Value.worldPos, ""+ entry.Value.Q + "\n" + entry.Value.S + "\n" + entry.Value.R);
            }
        if (showOccupied && nodes != null)
            foreach(KeyValuePair<Vector2Int, Node> entry in nodes) {
                Handles.Label(entry.Value.worldPos, ""+ (entry.Value.units.Count > 0));
            }
    }*/
}
