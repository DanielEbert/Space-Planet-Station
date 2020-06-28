using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public class Pool {
        public string tag;
        public GameObject prefab;

        public Transform parentHolder;

        public Queue<GameObject> free;

        public Pool(string tag, GameObject prefab, int initialSize, Transform parentHolder) {
            this.tag = tag;
            this.prefab = prefab;
            this.parentHolder = parentHolder;
            free = new Queue<GameObject>();

            for(int i = 0; i < initialSize; i++) {
                GameObject g = Instantiate(prefab);
                g.SetActive(false);
                g.transform.parent = parentHolder;
                free.Enqueue(g);
            }
        }

        public GameObject get() {
            if (free.Count == 0) {
                GameObject g = Instantiate(prefab);
                g.SetActive(false);
                free.Enqueue(g);
            }
            GameObject ret = free.Dequeue();
            ret.transform.parent = null;
            IPoolObject IpO = ret.GetComponent<IPoolObject>();
            if (IpO != null) {
                IpO.OnObjectSpawn();
            }
            return ret;
        }

        public GameObject get(Vector3 position, Quaternion rotation) {
            if (free.Count == 0) {
                GameObject g = Instantiate(prefab);
                g.SetActive(false);
                free.Enqueue(g);
            }
            GameObject ret = free.Dequeue();
            ret.transform.parent = null;
            ret.transform.position = position;
            ret.transform.rotation = rotation;
            IPoolObject IpO = ret.GetComponent<IPoolObject>();
            if (IpO != null) {
                IpO.OnObjectSpawn();
            }
            return ret;
        }

        public GameObject get(Vector3 position, Quaternion rotation, bool setActive) {
            if (free.Count == 0) {
                GameObject g = Instantiate(prefab);
                g.SetActive(false);
                free.Enqueue(g);
            }
            GameObject ret = free.Dequeue();
            ret.transform.parent = null;
            ret.transform.position = position;
            ret.transform.rotation = rotation;
            ret.SetActive(setActive);
            IPoolObject IpO = ret.GetComponent<IPoolObject>();
            if (IpO != null) {
                IpO.OnObjectSpawn();
            }
            return ret;
        }

        public void put(GameObject g) {
            g.SetActive(false);
            g.transform.parent = parentHolder;
            free.Enqueue(g);
        }
    }

    [System.Serializable]
    public class PoolTemplate {
        public string tag;
        public GameObject prefab;

        public Transform parentHolder;

        public int initialSize;
    }

    #region Singleton

    public static ObjectPool Instance;

    private void Awake() {
        Instance = this;
    }

    #endregion

    public Dictionary<string, Pool> pools;

    public List<PoolTemplate> definedPools = new List<PoolTemplate>();

    void Start() {
        pools = new Dictionary<string, Pool>();

        foreach (PoolTemplate pt in definedPools) {
            Pool p = new Pool(pt.tag, pt.prefab, pt.initialSize, pt.parentHolder);
            pools[p.tag] = p;
        }
    }
}
