
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance { get; private set; }

    [Header("Default Pool Settings")]

    [SerializeField, Min(0)]
    private int initialSize = 10;

    [SerializeField, Min(1)]
    private int maxSize = 50;


    private readonly Dictionary<GameObject, ObjectPool<GameObject>>
        pools = new Dictionary<GameObject, ObjectPool<GameObject>>();

    private Transform inactiveRoot;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple PoolManagers detected!");
            Destroy(gameObject);
            return;
        }

        Instance = this;

        GameObject root = new GameObject("Inactive Pool Objects");

        root.transform.SetParent(transform, false);

        root.SetActive(false);

        inactiveRoot = root.transform;
    }


    public GameObject Spawn(
        GameObject prefab,
        Vector3 position,
        Quaternion rotation,
        Transform parent = null)
    {
        if (prefab == null)
        {
            Debug.LogError("PoolManager: Prefab is missing!");
            return null;
        }

        ObjectPool<GameObject> pool = GetOrCreatePool(prefab);

        GameObject instance = pool.Get();

        PooledObject pooledObject = instance.GetComponent<PooledObject>();

        if (pooledObject == null)
        {
            pooledObject = instance.AddComponent<PooledObject>();
        }

        pooledObject.SetOwner(pool);

        instance.transform.SetParent(parent, true);

        instance.transform.SetPositionAndRotation(
            position,
            rotation
        );

        instance.SetActive(true);

        return instance;
    }


    public void Preload(GameObject prefab)
    {
        if (prefab == null)
        {
            Debug.LogError("Cannot preload a null prefab.");
            return;
        }

        GetOrCreatePool(prefab);
    }


    private ObjectPool<GameObject> GetOrCreatePool(
        GameObject prefab)
    {
        if (pools.TryGetValue(prefab, out var existingPool))
        {
            return existingPool;
        }

        ObjectPool<GameObject> newPool =
            new ObjectPool<GameObject>(
                createFunc: () =>
                {
                    GameObject obj = Instantiate(
                        prefab,
                        inactiveRoot
                    );

                    obj.name = prefab.name + " (Pooled)";

                    obj.SetActive(false);

                    if (!obj.TryGetComponent<PooledObject>(out _))
                    {
                        obj.AddComponent<PooledObject>();
                    }

                    return obj;
                },

                actionOnGet: null,

                actionOnRelease: obj =>
                {
                    obj.SetActive(false);

                    obj.transform.SetParent(
                        inactiveRoot,
                        false
                    );
                },

                actionOnDestroy: obj =>
                {
                    Destroy(obj);
                },

                collectionCheck: true,

                defaultCapacity: initialSize,

                maxSize: maxSize
            );

        pools.Add(prefab, newPool);

        Prewarm(newPool, initialSize);

        return newPool;
    }


    private void Prewarm(
        ObjectPool<GameObject> pool,
        int count)
    {
        List<GameObject> objects =
            new List<GameObject>(count);

        for (int i = 0; i < count; i++)
        {
            objects.Add(pool.Get());
        }

        foreach (GameObject obj in objects)
        {
            pool.Release(obj);
        }
    }


    private void OnValidate()
    {
        maxSize = Mathf.Max(1, maxSize);

        initialSize = Mathf.Clamp(
            initialSize,
            0,
            maxSize
        );
    }


    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }

        foreach (var pool in pools.Values)
        {
            pool.Clear();
        }

        pools.Clear();
    }
}
