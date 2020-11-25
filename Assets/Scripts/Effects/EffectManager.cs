using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//EffectManager은 global한 이펙트들의 관리를 담당
public class EffectManager : MonoBehaviour
{
    Shaker cameraShaker;
    public Transform effectParent;

    public GameObject[] hitEffects;
    public int hitEffectInitialCount;
    public int hitEffectIncrementCount = 1;
    Pool[] hitEffectPools;

    public GameObject[] dustEffects;
    public int dustEffectsInitialCount;
    public int dustEffectsIncrementCount = 1;
    Pool[] dustEffectPools;

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
                    var newSingleton = new GameObject("EffectManager Class").AddComponent<EffectManager>();
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
        dustEffectPools = new Pool[dustEffects.Length];
        for (int i = 0; i < hitEffectPools.Length; i++)
        {
            hitEffectPools[i] = effectParent.gameObject.AddComponent<Pool>();
            hitEffectPools[i].poolPrefab = hitEffects[i];
            hitEffectPools[i].initialCount = hitEffectInitialCount;
            hitEffectPools[i].incrementCount = hitEffectIncrementCount;
            hitEffectPools[i].Initialize();
        }

        dustEffectPools = new Pool[dustEffects.Length];
        for (int i = 0; i < dustEffectPools.Length; i++)
        {
            dustEffectPools[i] = effectParent.gameObject.AddComponent<Pool>();
            dustEffectPools[i].poolPrefab = dustEffects[i];
            dustEffectPools[i].initialCount = dustEffectsInitialCount;
            dustEffectPools[i].incrementCount = dustEffectsIncrementCount;
            dustEffectPools[i].Initialize();
        }
    }
    public void defaultEffect(IHitable target, EffectType effectType)
    {
        //!TODO :콜리전 정보를 받아와서 살짝의 랜덤을 준 위치에 생성하는것도 만들 것
        if (effectType == EffectType.NONE)
            return;
        if (target.showEffect == false)
            return;
        var e = GetEffectFromPool(effectType);
        e.transform.position = target.transform.position;
        e.gameObject.SetActive(true);
        //e.GetComponent<Effector>().Disable(0.5f).Play();
        if (effectType == EffectType.SMALL)
        {
            smallHit();
        }
        if (effectType == EffectType.MID)
        {
            midHit();
        }

    }
    PoolableObject GetEffectFromPool(EffectType type)
    {
        if (type == EffectType.SMALL)
            return hitEffectPools[0].GetObjectDisabled();
        else// if (type == EffectType.MID)
            return hitEffectPools[1].GetObjectDisabled();
    }
    public PoolableObject GetDust(DustType type)
    {
        //if (type == DustType.WALK)
            return dustEffectPools[0].GetObjectDisabled();
    }
    void smallHit()
    {
        cameraShaker.StartShake(0.2f, 0.15f, 10f);
    }
    void midHit()
    {
        cameraShaker.StartShake(0.3f, 0.25f, 15f);
    }
}

public enum EffectType
{
    NONE = -1,
    SMALL = 0,
    MID = 1,
    BIG = 2,
    CRIT = 3
}
public enum DustType
{
    WALK = 0
}