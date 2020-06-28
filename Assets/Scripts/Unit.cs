using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Unit : MonoBehaviour {
    const float minPathUpdateTime = .2f;
    const float pathUpdateMoveThreshold = .1f;
    public Transform target;
    public float moveSpeed = 5;
    public float rotationSpeed = 180f;

    Job job = null;
    Job previousJob = null;

    public float sleepUntil = -.1f;
    private float sleepTimeAfterSameJobAssigned = .5f;

    private Node occupiedNode = null;

    Node[] path;

    [Header("DEBUG")]
    public bool showPath = false;

    void Start() {
        //StartCoroutine(UpdatePath());
    }

    /*
    
    if I have a job
    1) walk to one of the jobs workingnodes
    2) add completion to the job
    3) if job complete: repeat 0)

    */

    void Update() {
        if (Time.time < sleepUntil)
            return;

        if (job == null || job.IsComplete()) {
            job = JobManager.instance.TryGetJob();
            if (job == null) {
                sleepUntil = Time.time + .2f;
                return;
            } else {
                if (job == previousJob) {
                    sleepUntil = Time.time + sleepTimeAfterSameJobAssigned;
                    JobManager.instance.QueueJob(job);
                    job = null;
                    return;
                }
                InstantiateJob instantiateJob = job as InstantiateJob;
                if (instantiateJob != null) {
                    PathRequestManager.RequestPath(new PathRequest(
                        World.nodes[World.nodeCoordFromWorldPos(transform.position)],
                        instantiateJob.building.workerNodes.ToList(),
                        OnPathFound));
                }
                
            }
        }

        InstantiateJob instantiateJob2 = job as InstantiateJob;
        if (instantiateJob2 != null) {
            if (instantiateJob2.building.workerNodes.Contains(occupiedNode as PlanetNode)) {
                job.AddProcess(Time.deltaTime * .3f);
            }
        }
    }

    /*IEnumerator UpdatePath() {

        if (Time.timeSinceLevelLoad < .3f) {
            yield return new WaitForSeconds(.3f);
        }

        Vector2Int startNodeCoord;
        Vector2Int targetNodeCoord;
        
        float sqrMoveThreshold = pathUpdateMoveThreshold * pathUpdateMoveThreshold;
        targetPosOld = Vector2.up * 10000;

        while (true) {
            startNodeCoord = World.nodeCoordFromWorldPos(transform.position);
            targetNodeCoord = World.nodeCoordFromWorldPos(target.position);

            if (World.planetNodes.ContainsKey(startNodeCoord) && World.planetNodes.ContainsKey(targetNodeCoord) && 
                    (target.position - targetPosOld).sqrMagnitude > sqrMoveThreshold) {

                PathRequestManager.RequestPath(new PathRequest(
                    World.nodes[startNodeCoord],
                    new List<PlanetNode> { World.nodes[targetNodeCoord] as PlanetNode },
                    OnPathFound));
                targetPosOld = target.position;
            }

            yield return new WaitForSeconds(minPathUpdateTime);
        }
    }*/

    public void OnPathFound(Node[] waypoints, bool pathSuccessful) {
        if (pathSuccessful) {
            StopCoroutine("FollowPath");
            path = waypoints;
            StartCoroutine("FollowPath");
        } else {
            JobManager.instance.QueueJob(job);
            previousJob = job;
            job = null;
        }
    }

    void occupyNode(Node n) {
        if (occupiedNode != null) {
            occupiedNode.units.Remove(this);
        }
        n.units.Add(this);
        occupiedNode = n;
    }

    IEnumerator FollowPath() {
        Vector3 a, b, c = transform.position;
        
        if (path.Length == 0) {
            yield break;
        }
        else if (path.Length == 1) {
            yield return LookAt(path[0].worldPos);

            float ti = Time.deltaTime * moveSpeed;
            for (; ti < 1f; ti +=  Time.deltaTime * moveSpeed * 2) {
                transform.localPosition = Vector3.Lerp(c, path[0].worldPos, ti);
                yield return null;
            }
            transform.localPosition = path[0].worldPos;
            yield break;
        }
        
        Node currentNode;
        
        World.nodes.TryGetValue(new Vector2Int(path[1].Q, path[1].R), out currentNode);

        if (!World.planetNodes.ContainsKey(new Vector2Int(path[1].Q, path[1].R))) {
            InstantiateJob instantiateJob = job as InstantiateJob;
            if (instantiateJob != null) {
                PathRequestManager.RequestPath(new PathRequest(
                        World.nodes[World.nodeCoordFromWorldPos(transform.position)],
                        instantiateJob.building.workerNodes.ToList(),
                        OnPathFound));
            }
            yield break;
        }

        occupyNode(currentNode);
            
        yield return LookAt(path[1].worldPos);

        float t = Time.deltaTime * moveSpeed;

        for (; t < 1f; t +=  Time.deltaTime * moveSpeed * 2) {
            transform.localPosition = Vector3.Lerp(c, (path[1].worldPos + c) / 2, t); //Bezier.GetPoint(a, b, c, t);
            yield return null;
        }
        t -= 1f;

        c = transform.position;

		for (int i = 2; i < path.Length; i++) {
            World.nodes.TryGetValue(new Vector2Int(path[i].Q, path[i].R), out currentNode);

            if (!World.planetNodes.ContainsKey(new Vector2Int(path[i].Q, path[i].R))) {
                InstantiateJob instantiateJob = job as InstantiateJob;
                if (instantiateJob != null) {
                    PathRequestManager.RequestPath(new PathRequest(
                        World.nodes[World.nodeCoordFromWorldPos(transform.position)],
                        instantiateJob.building.workerNodes.ToList(),
                        OnPathFound));
                }
                for (; t < 1f; t += Time.deltaTime * moveSpeed) {
                    transform.localPosition = c + (path[i-1].worldPos - c) * t;
                    //we just go forward to the centre so we don't need to change the rotation here
                    yield return null;
                }
                yield break;
            }

            occupyNode(currentNode);

			a = c;
            b = path[i-1].worldPos;
			c = (b + path[i].worldPos) * 0.5f;
			for (; t < 1f; t += Time.deltaTime * moveSpeed) {
				transform.localPosition = Bezier.GetPoint(a, b, c, t);
                Vector3 d = Bezier.GetDerivative(a, b, c, t);
                float angle = Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg;
                transform.localRotation = Quaternion.AngleAxis(angle - 90, Vector3.forward);
                yield return null;
			}
            t -= 1f;
		}

        a = c;
        b = path[path.Length - 1].worldPos;
        c = b;
        for (; t < 1f; t += Time.deltaTime * moveSpeed) {
            transform.localPosition = Bezier.GetPoint(a, b, c, t);
            Vector3 d = Bezier.GetDerivative(a, b, c, t);
            float angle = Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg;
            transform.localRotation = Quaternion.AngleAxis(angle - 90, Vector3.forward);
            yield return null;
        }

        transform.localPosition = path[path.Length - 1].worldPos;

        path = null;
    }

    IEnumerator LookAt(Vector3 point) {
        point.z = transform.position.z;
        Vector2 d = point - transform.position;
        float angle = Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg;
        Quaternion startRot = transform.localRotation;
        Quaternion targetRot = Quaternion.AngleAxis(angle - 90, Vector3.forward);
        float a = Quaternion.Angle(startRot, targetRot);

        if (a > 0) {
            float speed = rotationSpeed / a;
            for (float t = Time.deltaTime * speed; t < 1f; t += Time.deltaTime * speed) {
                transform.localRotation = Quaternion.Slerp(startRot, targetRot, t);
                yield return null;
            }
        }
        
    }

    void OnDrawGizmos () {
		if (!showPath || path == null || path.Length == 0) {
			return;
		}

		Vector3 a, b, c = path[0].worldPos;

		for (int i = 1; i < path.Length; i++) {
			a = c;
            b = path[i-1].worldPos;
			c = (b + path[i].worldPos) * 0.5f;
			for (float t = 0f; t < 1f; t += 0.1f) {
				Gizmos.DrawSphere(Bezier.GetPoint(a, b, c, t), .3f);
			}
		}

        a = c;
        b = path[path.Length - 1].worldPos;
        c = b;
        for (float t = 0f; t < 1f; t += 0.1f) {
            Gizmos.DrawSphere(Bezier.GetPoint(a, b, c, t), .3f);
        }
    }
}
