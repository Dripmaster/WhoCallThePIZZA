using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolTest : MonoBehaviour
{
    public Pool pool;

    List<PoolableObject> poolables = new List<PoolableObject>();
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Q))
        {
            //PoolManager.GetObject<PoolableObject>(prefab);
            PoolableObject p = pool.GetObjectDisabled(transform);
            p.gameObject.SetActive(true);
            poolables.Add(p);
        }
        if(Input.GetKeyDown(KeyCode.W))
        {
            foreach(PoolableObject poolable in poolables)
                poolable.gameObject.SetActive(false);
            poolables.Clear();
        }
    }
}
