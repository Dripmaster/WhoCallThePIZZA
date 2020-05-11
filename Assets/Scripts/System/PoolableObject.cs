using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolableObject : MonoBehaviour
{
    Pool parentPool;
    public void SetPool(Pool pool)
    {
        parentPool = pool;
    }
    void OnDisable()
    {
        parentPool.ReturnObjectDisabled(this);
    }
}
