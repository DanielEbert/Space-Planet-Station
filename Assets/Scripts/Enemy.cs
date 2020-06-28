using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour, IPoolObject {
    public int totalHealth = 10;
    int health;
    int futureHealth;

    public float speed = 1f;

    public List<IBullet> lockedOnBullets;

    private Vector3 dir;

    void Update() {
        if (transform.position.magnitude < 1) {
            ObjectPool.Instance.pools["Enemy"].put(this.gameObject);
        }

        //TODO: check if it went the whole way through the planet to the other side. in that case just destroy and continue
        transform.position +=  Time.deltaTime * speed * dir;

        Vector2Int c = World.nodeCoordFromWorldPos(transform.position);
        if (World.nodes.ContainsKey(c) && !World.blankNodes.ContainsKey(c)) {
            //ObjectPool.Instance.pools["EnemyUnit"].get(World.planetNodes[c].worldPos, Quaternion.identity, true);
            print("health --");
            Kill();
        }
    }

    public void OnObjectSpawn() {
        health = totalHealth;
        futureHealth = totalHealth;
        lockedOnBullets = new List<IBullet>();
        dir = -transform.position.normalized;
    }

    public void OnDamageTaken(int dmg) {
        health -= dmg;
        if (health <= 0)
            Kill();
    }

    public bool OnFutureDamageTaken(int dmg) {
        if (health < futureHealth)
            futureHealth = health - dmg;
        else
            futureHealth -= dmg;
        return futureHealth <= 0;    
    }

    public void Kill() {
        foreach (IBullet b in lockedOnBullets)
            b.OnTargetDestroyed();
        if (Enemies.Instance.enemies.Contains(gameObject)) {
            Enemies.Instance.RemoveEnemy(this.gameObject);
        }
        ObjectPool.Instance.pools["Enemy"].put(this.gameObject);
    }
}
