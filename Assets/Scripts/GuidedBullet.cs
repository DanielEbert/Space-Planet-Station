using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuidedBullet : MonoBehaviour, IPoolObject, IBullet {
    public int dmg = 3;
    public Transform target;

    public float speed = 20f;

    void Update() {
        Vector2 movementThisFrame = Time.deltaTime * speed * (target.position - transform.position).normalized;

        if (Vector2.Distance(target.position, transform.position) <= movementThisFrame.magnitude) {
            target.GetComponent<Enemy>().OnDamageTaken(dmg);
            OnDestroy();
        }
        else {
            LookAt(transform.position + (Vector3)movementThisFrame);
            transform.position += (Vector3)movementThisFrame;
        }
    }

    public void OnTargetDestroyed() {
        OnDestroy();
    }

    public void OnObjectSpawn() {

    }

    void OnDestroy() {
        ObjectPool.Instance.pools["Bullet"].put(gameObject);
    }

    void LookAt(Vector2 targetPos) {
        Vector2 dir = targetPos - (Vector2)transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle - 90, Vector3.forward);
    }
}
