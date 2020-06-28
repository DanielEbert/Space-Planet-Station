using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node {

    public int Q;
    public int R;
    public Vector3 worldPos;

    public HashSet<Unit> units = new HashSet<Unit>();
    public int movementPenalty = 0;

    public Node(int x, int y, Vector3 worldPos) {
        Q = x;
        R = y;
        this.worldPos = worldPos;
    }

    public int S {
		get {
			return -R - Q;
		}
	}

    public Vector2Int Coord {
        get {
            return new Vector2Int(Q, R);
        }
    }

    public List<Node> getNeighbours() {
        List<Node> neightbours = new List<Node>();
        Node W = getNeighbor(HexDirection.W);
        Node NW = getNeighbor(HexDirection.NW);
        Node NE = getNeighbor(HexDirection.NE);
        Node E = getNeighbor(HexDirection.E);
        Node SE = getNeighbor(HexDirection.SE);
        Node SW = getNeighbor(HexDirection.SW);
        if (W != null)
            neightbours.Add(W);
        if (NW != null)
            neightbours.Add(NW);
        if (NE != null)
            neightbours.Add(NE);
        if (E != null)
            neightbours.Add(E);
        if (SE != null)
            neightbours.Add(SE);
        if (SW != null)
            neightbours.Add(SW);
        return neightbours;
    }

    public Node getNeighbor(HexDirection dir) {
        return getNeighbor(dir, World.nodes);
    }

    public dynamic getNeighbor(HexDirection dir, dynamic dict) {
        if (dir == HexDirection.W) {
            PlanetNode n;
            if (!dict.TryGetValue(new Vector2Int(Q-1, R), out n)) {
                return null;
            }
            return n;
        }
        if (dir == HexDirection.E) {
            PlanetNode n;
            if (!dict.TryGetValue(new Vector2Int(Q+1, R), out n)) {
                return null;
            }
            return n;
        }
        if (dir == HexDirection.NW) {
            PlanetNode n;
            if (!dict.TryGetValue(new Vector2Int(Q-1, R+1), out n)) {
                return null;
            }
            return n;
        }
        if (dir == HexDirection.NE) {
            PlanetNode n;
            if (!dict.TryGetValue(new Vector2Int(Q, R+1), out n)) {
                return null;
            }
            return n;
        }
        if (dir == HexDirection.SE) {
            PlanetNode n;
            if (!dict.TryGetValue(new Vector2Int(Q+1, R-1), out n)) {
                return null;
            }
            return n;
        }
        if (dir == HexDirection.SW) {
            PlanetNode n;
            if (!dict.TryGetValue(new Vector2Int(Q, R-1), out n)) {
                return null;
            }
            return n;
        }
        return null;
    }

    public int distanceTo(Node other) {
        return (Q < other.Q ? other.Q - Q : Q - other.Q) +
               (R < other.R ? other.R - R : R - other.R) + 
               (S < other.S ? other.S - S : S - other.S);
    }
}

public class PlanetNode : Node {
    public GameObject gameObject;
    public PlanetNode(int x, int y, Vector3 worldPos, GameObject _gameObject) : base(x, y, worldPos) {
        gameObject = _gameObject;
    }

    public new List<PlanetNode> getNeighbours() {
        List<PlanetNode> neightbours = new List<PlanetNode>();
        PlanetNode W = getNeighbor(HexDirection.W);
        PlanetNode NW = getNeighbor(HexDirection.NW);
        PlanetNode NE = getNeighbor(HexDirection.NE);
        PlanetNode E = getNeighbor(HexDirection.E);
        PlanetNode SE = getNeighbor(HexDirection.SE);
        PlanetNode SW = getNeighbor(HexDirection.SW);
        if (W != null)
            neightbours.Add(W);
        if (NW != null)
            neightbours.Add(NW);
        if (NE != null)
            neightbours.Add(NE);
        if (E != null)
            neightbours.Add(E);
        if (SE != null)
            neightbours.Add(SE);
        if (SW != null)
            neightbours.Add(SW);
        return neightbours;
    }

    public new PlanetNode getNeighbor(HexDirection dir) {
        return getNeighbor(dir, World.planetNodes);
    }
}

public class BuildingNode : Node {
    public Building building;

    public BuildingNode(int x, int y, Vector3 worldPos, Building _building) : base(x, y, worldPos) {
        building = _building;
    }
}

public enum HexDirection {
	NE, E, SE, SW, W, NW
}