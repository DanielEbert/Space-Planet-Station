using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using UnityEngine;
using System.Diagnostics;

public class Pathfinding : MonoBehaviour {

    public bool showDebugInfo = true;

    class NodeData : IHeapItem<NodeData> {
        public PlanetNode planetNode; 
        public int gCost;
        public int hCost;
        int heapIndex;
        public Node parent;

        public NodeData(PlanetNode planetNode) {
            this.planetNode = planetNode;
        }

        public int HeapIndex {
            get {
                return heapIndex;
            }
            set {
                heapIndex = value;
            }
        }

        public int fCost {
            get {
                return gCost + hCost;
            }
        }

        public int CompareTo(NodeData nodeToCompare) {
            int compare = fCost.CompareTo(nodeToCompare.fCost);
            if (compare == 0) {
                compare = hCost.CompareTo(nodeToCompare.hCost);
            }
            return -compare;
        }
    }

    public void FindPath(PathRequest request, Action<PathResult> callback) {

        Stopwatch sw = new Stopwatch();
		sw.Start();

        Dictionary<Vector2Int, NodeData> nodeData = new Dictionary<Vector2Int, NodeData>();

        Node[] waypoints = new Node[0];
        bool pathSuccess = false;

        Heap<NodeData> openSet = new Heap<NodeData>(10);
        HashSet<NodeData> closedSet = new HashSet<NodeData>();

        HashSet<Vector2Int> targetCoords = new HashSet<Vector2Int>();

        PlanetNode targetNode = null;

        foreach (PlanetNode n in request.targetPos) {
            if (!targetCoords.Contains(n.Coord)) {
                PlanetNode cur;
                if (World.planetNodes.TryGetValue(n.Coord, out cur)) {
                    if (targetNode == null)
                        targetNode = cur;
                    targetCoords.Add(n.Coord);
                }
            }   
        }

        if (targetCoords.Count == 0) {
            if (showDebugInfo) {
                print("PF: No targetNode is a planetNode " + request.startPos);
            }
            callback(new PathResult(waypoints, pathSuccess, request.callback));
            return;
        }

        PlanetNode startNode;
        if (!World.planetNodes.TryGetValue(request.startPos.Coord, out startNode)) {
            if (showDebugInfo) {
                print("PF: startPos not in planetPointPointer: " + request.startPos);
            }
            callback(new PathResult(waypoints, pathSuccess, request.callback));
            return;
        }
        NodeData startNodeData = new NodeData(startNode);
        nodeData.Add(startNode.Coord, startNodeData);

        if (targetCoords.Contains(request.startPos.Coord)) {
            if (showDebugInfo) {
                print("PF: Already on the correct node: " + request.startPos);
            }
            waypoints = new Node[1];
            waypoints[0] = startNode;
            callback(new PathResult(waypoints, true, request.callback));
            return;
        }

        openSet.Add(startNodeData);

        while (openSet.Count > 0) {
            NodeData currentNode = openSet.RemoveFirst();
            closedSet.Add(currentNode);

            if (targetCoords.Contains(currentNode.planetNode.Coord)) {
                sw.Stop();
                if (showDebugInfo) {
                    print ("Path found: " + sw.ElapsedMilliseconds + " ms");
                }
                targetNode = currentNode.planetNode;
                pathSuccess = true;
                break;
            }

            foreach (PlanetNode n in currentNode.planetNode.getNeighbours()) {
                Vector2Int neighbourKey = new Vector2Int(n.Q, n.R);
                NodeData neighbour = null;
                if (nodeData.ContainsKey(neighbourKey)) {
                    neighbour = nodeData[neighbourKey];
                    if (closedSet.Contains(neighbour)) {
                        continue;
                    }
                } else {
                    nodeData[neighbourKey] = new NodeData(n);
                    neighbour = nodeData[neighbourKey];
                }
                
                int newMovementCostToNeighbour;
                if (request.useWeights) {
                    newMovementCostToNeighbour = currentNode.gCost + currentNode.planetNode.distanceTo(neighbour.planetNode) + neighbour.planetNode.movementPenalty;
                }
                else
                    newMovementCostToNeighbour = currentNode.gCost + currentNode.planetNode.distanceTo(neighbour.planetNode);

                if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour)) {
                    neighbour.gCost = newMovementCostToNeighbour;
                    neighbour.hCost = neighbour.planetNode.distanceTo(targetNode);
                    neighbour.parent = currentNode.planetNode;

                    if (!openSet.Contains(neighbour)) {
                        openSet.Add(neighbour);
                    }
                    else {
                        openSet.UpdateItem(neighbour);
                    }
                }
            }
        }

        if (pathSuccess) {
            waypoints = RetracePath(startNode, targetNode, nodeData);
            pathSuccess = waypoints.Length > 0;
        }
        callback(new PathResult(waypoints, pathSuccess, request.callback));
    }

    Node[] RetracePath(Node startNode, Node targetNode, Dictionary<Vector2Int, NodeData> nodeData) {
        List<Node> path = new List<Node>();
        Node currentNode = targetNode;

        while (currentNode != startNode) {
            path.Add(currentNode);
            currentNode = nodeData[new Vector2Int(currentNode.Q, currentNode.R)].parent;
        }

        path.Add(startNode);

        Node[] waypoints = path.ToArray();

        Array.Reverse(waypoints);

        return waypoints;
    }

    /* Vector3[] SimplifyPath(List<Node> path) {
        List<Vector3> waypoints = new List<Vector3>();

        //Vector3 directionOld = Vector3.zero;

        for (int i = 0; i < path.Count; i++) {
        //    Vector3 directionNew = path[i-1].pos - path[i].pos;
        //    if (directionNew != directionOld) {
                waypoints.Add(path[i].worldPos);
        //    }
        //    directionOld = directionNew;
        }
        return waypoints.ToArray();
    }*/

    int getDistance(Node a, Node b) {
        return a.distanceTo(b);
        //return Mathf.FloorToInt(Vector3.Distance(a.worldPos, b.worldPos) * 10);
    }
}
