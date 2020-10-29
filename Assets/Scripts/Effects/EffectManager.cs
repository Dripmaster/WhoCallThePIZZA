using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectManager : MonoBehaviour
{
    Shaker cameraShaker;
    public Transform effectParent;
    public GameObject[] hitEffects;
    public int hitEffectinitialCount;
    public int hitEffectincrementCount = 1;
    Pool[] hitEffectPools;

    #region Singletone
    private static EffectManager instance;
    public static EffectManager Instance
    {
        get
        {
            if (instance == null)
            {
                var obj = FindObjectOfType<EffectManager>();
                if (obj != null)
                {
                    instance = obj;
                }
                else
                {
                    var newSingleton = new GameObject("AttackManager Class").AddComponent<EffectManager>();
                    instance = newSingleton;
                }
            }
            return instance;
        }
        private set
        {
            instance = value;
        }
    }
    public static EffectManager GetInstance()
    {
        return Instance;
    }
    #endregion


    // Start is called before the first frame update
    void Awake()
    {
        cameraShaker = GetComponent<Shaker>();

        hitEffectPools = new Pool[hitEffects.Length];
        for (int i = 0; i < hitEffectPools.Length; i++)
        {
            hitEffectPools[i] = effectParent.gameObject.AddComponent<Pool>();
            hitEffectPools[i].poolPrefab = hitEffects[i];
            hitEffectPools[i].initialCount = hitEffectinitialCount;
            hitEffectPools[i].incrementCount = hitEffectincrementCount;
            hitEffectPools[i].Initialize();
        }
    }
    public void defaultEffect(FSMbase target, int hitEffectNum)
    {
        //!TODO :콜리전 정보를 받아와서 살짝의 랜덤을 준 위치에 생성하는것도 만들 것
        if (hitEffectNum == -1)
            return;
        Debug.Log("!");
        var e = hitEffectPools[hitEffectNum].GetObjectDisabled();
        e.transform.position = target.transform.position;
        e.gameObject.SetActive(true);
        //e.GetComponent<Effector>().Disable(0.5f).Play();
        if (hitEffectNum == 0)
        {
            smallHit();
        }
        
    }

    public void smallHit()
    {
        cameraShaker.StartShake(0.2f, 0.2f, 10f);
    }
}
