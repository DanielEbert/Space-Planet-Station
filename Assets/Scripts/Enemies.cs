using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemies : MonoBehaviour
{
    public static Enemies Instance;

    public GameObject[] initialEnemies;

    public HashSet<GameObject> enemies = new HashSet<GameObject>();

    public float timeBetweenSpawns = .1f;

    private float nextSpawnTime = -.1f;

    void Awake() {
        Instance = this;

        foreach (GameObject g in initialEnemies) {
            enemies.Add(g);
        }
    }

    void Update() {
        if (Time.time >= nextSpawnTime) {
            nextSpawnTime = Time.time + timeBetweenSpawns;

            Vector2 spawnPos = Random.insideUnitCircle.normalized * 50;

            float angle = Mathf.Atan2(-spawnPos.y, -spawnPos.x) * Mathf.Rad2Deg;

            GameObject g = ObjectPool.Instance.pools["Enemy"].get(spawnPos, Quaternion.AngleAxis(angle - 90, Vector3.forward));
            g.SetActive(true);

            enemies.Add(g);
        }
    }

    public void AddEnemy(GameObject g) {
        enemies.Add(g);
    }

    public void RemoveEnemy(GameObject g) {
        enemies.Remove(g);
    }

    public bool EnemiesAlive() {
        return enemies.Count > 0;
    }
}