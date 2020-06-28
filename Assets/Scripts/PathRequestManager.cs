using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathRequestManager : MonoBehaviour {
    Queue<PathResult> results = new Queue<PathResult>();
    public static PathRequestManager Instance;
    Pathfinding pathfinding;

    void Awake() {
        Instance = this;
        pathfinding = GetComponent<Pathfinding>();
    }

    //TODO Maybe limit the concurrent requests to cpucores - 1
    public static void RequestPath(PathRequest request) {
        ThreadStart threadStart = delegate {
            Instance.pathfinding.FindPath(request, Instance.FinishedProcessingPath);
        };
        Thread thread = new Thread(threadStart);  
        thread.Start();
    }

    void Update() {
        if (results.Count > 0) {
            lock (results) {
            int itemsInQueue = results.Count;
                for (int i = 0; i < itemsInQueue; i++) {
                    PathResult result = results.Dequeue();
                    result.callback(result.path, result.success);
                }
            }
        }
    }

    public void FinishedProcessingPath(PathResult result) {
        lock (results) {
            results.Enqueue(result);
        }
    }
}

public struct PathResult {
    public Node[] path;
    public bool success;
    public Action<Node[], bool> callback;

    public PathResult(Node[] path, bool success, Action<Node[], bool> callback) {
        this.path = path;
        this.success = success;
        this.callback = callback;
    }
}

public struct PathRequest {
    public Node startPos;
    public List<PlanetNode> targetPos;
    public bool useWeights;
    public Action<Node[], bool> callback;

    public PathRequest(Node startPos, List<PlanetNode> targetPos, Action<Node[], bool> callback) {
        this.startPos = startPos;
        this.targetPos = targetPos;
        this.callback = callback;
        this.useWeights = true;
    }

    public PathRequest(Node startPos, List<PlanetNode> targetPos, bool useWeights, Action<Node[], bool> callback) {
        this.startPos = startPos;
        this.targetPos = targetPos;
        this.callback = callback;
        this.useWeights = useWeights;
    }
}
