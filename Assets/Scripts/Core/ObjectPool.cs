using System.Collections.Generic;
using UnityEngine;

public sealed class ObjectPool : MonoBehaviour
{
    private static ObjectPool instance;

    public static ObjectPool Instance
    {
        get
        {
            if (instance != null)
                return instance;

            instance = FindFirstObjectByType<ObjectPool>();

            if (instance != null)
                return instance;

            GameObject go = new GameObject("ObjectPool");
            instance = go.AddComponent<ObjectPool>();
            return instance;
        }
    }

    [SerializeField] private bool dontDestroyOnLoad = true;

    private Transform poolRoot;
    private readonly Dictionary<GameObject, Stack<GameObject>> pools = new();
    private readonly Dictionary<GameObject, GameObject> spawnedToPrefab = new();

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        if (dontDestroyOnLoad)
            DontDestroyOnLoad(gameObject);

        EnsureRoot();
    }

    private void EnsureRoot()
    {
        if (poolRoot != null)
            return;

        GameObject root = new GameObject("PooledObjects");
        root.transform.SetParent(transform);
        poolRoot = root.transform;
    }

    public GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null)
    {
        if (prefab == null)
        {
            Debug.LogError("ObjectPool.Get called with null prefab.");
            return null;
        }

        EnsureRoot();

        if (!pools.TryGetValue(prefab, out Stack<GameObject> stack))
        {
            stack = new Stack<GameObject>();
            pools.Add(prefab, stack);
        }

        GameObject instance = stack.Count > 0 ? stack.Pop() : Instantiate(prefab);

        spawnedToPrefab[instance] = prefab;

        instance.transform.SetParent(parent, worldPositionStays: false);
        instance.transform.SetPositionAndRotation(position, rotation);
        instance.SetActive(true);

        return instance;
    }

    public T Get<T>(T prefabComponent, Vector3 position, Quaternion rotation, Transform parent = null) where T : Component
    {
        if (prefabComponent == null)
            return null;

        GameObject instance = Get(prefabComponent.gameObject, position, rotation, parent);
        return instance != null ? instance.GetComponent<T>() : null;
    }

    public void Return(GameObject instance)
    {
        if (instance == null)
            return;

        if (!spawnedToPrefab.TryGetValue(instance, out GameObject prefab) || prefab == null)
        {
            Destroy(instance);
            return;
        }

        if (!pools.TryGetValue(prefab, out Stack<GameObject> stack))
        {
            stack = new Stack<GameObject>();
            pools.Add(prefab, stack);
        }

        instance.SetActive(false);
        instance.transform.SetParent(poolRoot, worldPositionStays: false);
        stack.Push(instance);
    }

    public void Return(Component component)
    {
        if (component != null)
            Return(component.gameObject);
    }
}