using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI : MonoBehaviour {

    public static UI Instance;

    UIState state = UIState.IDLE;

    public GameObject Tower;

    private List<GameObject> blueTransparentHexagon = new List<GameObject>();
    private List<GameObject> redTransparentHexagon = new List<GameObject>();

    List<PlanetNode> b;
    public HashSet<PlanetNode> _workerNodes = null;

    [Header("Debug")]
    public bool debug = false;

    void Awake() {
        Instance = this;
    }

    void Update() {
        UIState newState = UpdateState();

        if (newState != state) {
            ExitState(state);
            EnterState(newState);
            state = newState;
        }

        OnState();
    }

    void EnterState(UIState state) {
        if (state == UIState.CREATE) {
            if(debug) print("Entering CREATE state.");
        }
        else if (state == UIState.IDLE) {
            if (debug) print("Entering IDLE state.");
        }
    }

    void ExitState(UIState state) {
        if (state == UIState.CREATE) {
            if (debug) print("Exiting CREATE state.");
        }
        else if (state == UIState.IDLE) {
            if (debug) print("Exiting IDLE state.");
        }
    }

    void OnState() {
        if (state == UIState.CREATE) {
            int buildingWidth = 5;
            int buildingHeight = 5;
            Vector2Int mouseCoord = World.nodeCoordFromWorldPos((Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition) - new Vector2((buildingWidth - 1) / 2f * 1.044f, 0));
            PlaceableReturn placeableReturn = Placeable(mouseCoord.x, mouseCoord.y, buildingWidth, buildingHeight);

            RemoveMarkers();
            
            if (Input.GetMouseButtonDown(0) && placeableReturn.placeable) {
                Building building = new Building(Tower);
                List<BuildingNode> newNodes = new List<BuildingNode>();
                foreach (PlanetNode n in placeableReturn.placeableNodes) {
                    BuildingNode newNode = new BuildingNode(n.Q, n.R, n.worldPos, building);
                    newNodes.Add(newNode);
                    World.nodes[new Vector2Int(n.Q, n.R)] = newNode;
                    World.planetNodes.TryRemove(new Vector2Int(n.Q, n.R), out _);
                }
                building.buildingNodes = newNodes;

                HashSet<PlanetNode> workerNodes = new HashSet<PlanetNode>();
                foreach (PlanetNode n in placeableReturn.placeableNodes) {
                    foreach (PlanetNode neighbour in n.getNeighbours()) {
                        if (!workerNodes.Contains(neighbour))
                            workerNodes.Add(neighbour);
                    }
                }
                building.workerNodes = workerNodes;

                InstantiateJob j = new InstantiateJob(building);
                JobManager.instance.QueueJob(j);

                _workerNodes = workerNodes;

                ExitState(UIState.CREATE);
                state = UIState.IDLE;
                EnterState(state);
            } else {
                SetMarkers(placeableReturn);
            }
        }
    }

    void RemoveMarkers() {
        foreach (GameObject g in blueTransparentHexagon) {
            ObjectPool.Instance.pools["BlueTransparentHexagon"].put(g);
        }
        blueTransparentHexagon = new List<GameObject>();
        foreach (GameObject g in redTransparentHexagon) {
            ObjectPool.Instance.pools["RedTransparentHexagon"].put(g);
        }
        redTransparentHexagon = new List<GameObject>();
    }

    void SetMarkers(PlaceableReturn placeableReturn) {
        b = placeableReturn.placeableNodes;
        foreach (PlanetNode n in placeableReturn.placeableNodes) {
            GameObject gameObject = ObjectPool.Instance.pools["BlueTransparentHexagon"].get(n.worldPos, Quaternion.identity);
            blueTransparentHexagon.Add(gameObject);
            gameObject.SetActive(true);
        }

        foreach (Node n in placeableReturn.nonPlaceableNodes) {
            GameObject gameObject = ObjectPool.Instance.pools["RedTransparentHexagon"].get(n.worldPos, Quaternion.identity);
            redTransparentHexagon.Add(gameObject);
            gameObject.SetActive(true);
        }
    }

    UIState UpdateState() {
        if (state == UIState.IDLE) {
            if (Input.GetKeyDown(KeyCode.C)) {
                return UIState.CREATE;
            }
        }

        return state;
    }

    struct PlaceableReturn {
        public bool placeable;
        public List<PlanetNode> placeableNodes;
        public List<Node> nonPlaceableNodes;

        public PlaceableReturn(bool placeable, List<PlanetNode> placeableNodes, List<Node> nonPlaceableNodes) {
            this.placeable = placeable;
            this.placeableNodes = placeableNodes;
            this.nonPlaceableNodes = nonPlaceableNodes;
        }
    }

    PlaceableReturn Placeable(int x, int y, int width, int height) {
        //x is always most left node (cornernode), middle height row

        bool placeable = true;

        List<PlanetNode> placeableNodes = new List<PlanetNode>();
        List<Node> nonPlaceableNodes = new List<Node>();

        for (int j = 0; j < height; j++) {
            Vector2Int a = new Vector2Int(x, y);
            if (j % 2 == 1) {
                a += new Vector2Int(0, (j + 1) / 2);
            } else {
                a += new Vector2Int(j / 2, (-j) / 2);
            }
            for (int i = 0; i < width - ((j > 0) ? ((j + 1) / 2) : j / 2); i++) {
                Vector2Int b = a + new Vector2Int(i, 0);
                if (World.planetNodes.ContainsKey(b) && World.planetNodes[b].units.Count == 0) {
                    placeableNodes.Add(World.planetNodes[b]);
                } else  {
                    placeable = false;
                    if (World.nodes.ContainsKey(b)) {
                        nonPlaceableNodes.Add(World.nodes[b]);
                    }
                }
                    
            }
        }

        return new PlaceableReturn(placeable, placeableNodes, nonPlaceableNodes);
    }

    void OnDrawGizmos() {
        if (!debug)
            return;

        if (b != null)
            foreach (PlanetNode n in b) {
                Gizmos.DrawSphere(n.worldPos, .3f);
            }
        if (_workerNodes != null) {
            foreach (PlanetNode n in _workerNodes) {
                Gizmos.DrawSphere(n.worldPos, .1f);
            }
        }
    }
}

public enum UIState {
    IDLE, CREATE
}