using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pool : MonoBehaviour
{
    public GameObject poolPrefab;
    public int initialCount;
    public int incrementCount = 1;

    Queue<PoolableObject> waitingQueue = new Queue<PoolableObject>();

    void Awake()
    {
        poolPrefab.SetActive(false);
        Initialize();
    }
    void AddObject(int count)
    {
        for(int i=0; i<count; i++)
        {
            PoolableObject poolable = Instantiate(poolPrefab, transform).GetComponent<PoolableObject>();
            poolable.SetPool(this);
            waitingQueue.Enqueue(poolable);
        }
    }
    void Initialize()
    {
        if(poolPrefab.GetComponent<PoolableObject>() == null)
        {
        #if UNITY_EDITOR
            Debug.LogError("Pool " + gameObject.name + "'s prefab doesn't have PoolableObject attatched.");
        #endif
        }
        AddObject(initialCount);
    }
    public PoolableObject GetObjectDisabled(Transform parent = null)
    {
        if(waitingQueue.Count == 0)
            AddObject(incrementCount);
        PoolableObject poolable = waitingQueue.Dequeue(); 
        poolable.transform.parent = parent;
        return poolable;
    }
    public void ReturnObjectDisabled(PoolableObject poolable)
    {
        waitingQueue.Enqueue(poolable);
        StartCoroutine(ChangeParent(poolable));
    }
    
    IEnumerator ChangeParent(PoolableObject poolable)
    {
        yield return null;
        poolable.transform.parent = transform;
    }


}
