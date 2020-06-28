using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyUnit : MonoBehaviour, IPoolObject {

    public Vector2 target = new Vector2(10.7f, -6.4f);

    public float moveSpeed = 5;
    public float rotationSpeed = 180f;

    private Node occupiedNode = null;
    private Node[] path;

    public void OnObjectSpawn() {
        Vector2Int targetNodeCoord = World.nodeCoordFromWorldPos(target);
        if (World.planetNodes.ContainsKey(targetNodeCoord)) {
            PathRequestManager.RequestPath(new PathRequest(
                World.nodes[World.nodeCoordFromWorldPos(transform.position)],
                new List<PlanetNode>() {World.planetNodes[targetNodeCoord]},
                OnPathFound));
        }
    }

    void Update() {
        if (Vector2.Distance(transform.position, target) < 1f) {
            ObjectPool.Instance.pools["EnemyUnit"].put(gameObject);
        }
    }

    public void OnPathFound(Node[] waypoints, bool pathSuccessful) {
        if (pathSuccessful) {
            StopCoroutine("FollowPath");
            path = waypoints;
            StartCoroutine("FollowPath");
        } else {
            //TODO
            ObjectPool.Instance.pools["EnemyUnit"].put(gameObject);
        }
    }

    void occupyNode(Node n) {
        if (occupiedNode != null) {
            //occupiedNode.units.Remove(this);
        }
        //n.units.Add(this);
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
            PathRequestManager.RequestPath(new PathRequest(
                World.nodes[World.nodeCoordFromWorldPos(transform.position)],
                new List<PlanetNode>() {World.planetNodes[World.nodeCoordFromWorldPos(target)]},
                OnPathFound));
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
                PathRequestManager.RequestPath(new PathRequest(
                    World.nodes[World.nodeCoordFromWorldPos(transform.position)],
                    new List<PlanetNode>() {World.planetNodes[World.nodeCoordFromWorldPos(target)]},
                    OnPathFound));
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
}
