using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    [SerializeField] GameObject prefab;
    [SerializeField] int initialSize = 20;
    [SerializeField] bool expandable = true;
    [SerializeField] Transform container;

    Queue<GameObject> available = new Queue<GameObject>();

    void Awake()
    {
        if (container == null)
        {
            container = new GameObject($"Pool_{prefab.name}").transform;
            container.SetParent(transform);
        }

        for (int i = 0; i < initialSize; i++)
            CreateInstance();
    }

    void CreateInstance()
    {
        GameObject obj = Instantiate(prefab, container);
        obj.SetActive(false);
        available.Enqueue(obj);
    }

    public GameObject Get(Vector3 position, Quaternion rotation)
    {
        if (available.Count == 0)
        {
            if (!expandable) return null;
            CreateInstance();
        }

        GameObject obj = available.Dequeue();
        obj.transform.SetPositionAndRotation(position, rotation);
        obj.SetActive(true);
        return obj;
    }

    public void Return(GameObject obj)
    {
        obj.SetActive(false);
        available.Enqueue(obj);
    }

    public void ReturnAll()
    {
        foreach (Transform child in container)
        {
            if (child.gameObject.activeSelf)
            {
                child.gameObject.SetActive(false);
                available.Enqueue(child.gameObject);
            }
        }
    }
}
