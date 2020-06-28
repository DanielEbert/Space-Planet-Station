using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour, IPoolObject
{
    public int dmg = 3;

    public float speed = 20f;

    public float TTL = 3f;

    private float creationTime = 0f;
    private bool isAlife = false;

    void Update() {
        if (isAlife && Time.time > creationTime + TTL)
            OnDestroy();

        Vector2 movementThisFrame = Time.deltaTime * speed * transform.up;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, movementThisFrame, movementThisFrame.magnitude);

        if (hit.collider != null) {
            if (hit.transform.name == "Enemy(Clone)") {
                hit.transform.GetComponent<Enemy>().OnDamageTaken(dmg);
                OnDestroy();
            }
        }

        transform.position += (Vector3)movementThisFrame;
    }

    public void OnTargetDestroyed() {
        OnDestroy();
    }

    public void OnObjectSpawn() {
        creationTime = Time.time;
        isAlife = true;
    }

    void OnDestroy() {
        isAlife = false;
        ObjectPool.Instance.pools["Bullet"].put(gameObject);
    }

    void LookAt(Vector2 targetPos) {
        Vector2 dir = targetPos - (Vector2)transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle - 90, Vector3.forward);
    }
}
