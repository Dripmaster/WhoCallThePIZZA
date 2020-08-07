using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;

public class AttackManager : MonoBehaviour
{
    #region Singletone
    private static AttackManager instance;
    public static AttackManager Instance
    {
        get
        {
            if (instance == null)
            {
                var obj = FindObjectOfType<AttackManager>();
                if (obj != null)
                {
                    instance = obj;
                }
                else
                {
                    var newSingleton = new GameObject("AttackManager Class").AddComponent<AttackManager>();
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
    public static AttackManager GetInstance() {
        return Instance;
    }
    #endregion
    public Transform effcetParent;
    public GameObject[] hitEffects;
    public int hitEffectinitialCount;
    public int hitEffectincrementCount = 1;
    Pool[] hitEffectPools;
    void Awake()
    {
        var objs = FindObjectsOfType<AttackManager>();
        if (objs.Length != 1)
        {
            Destroy(gameObject);
            return;
        }
        hitEffectPools = new Pool[hitEffects.Length];
        for (int i = 0; i < hitEffectPools.Length; i++)
        {
            hitEffectPools[i] = effcetParent.gameObject.AddComponent<Pool>();
            hitEffectPools[i].poolPrefab = hitEffects[i];
            hitEffectPools[i].initialCount = hitEffectinitialCount;
            hitEffectPools[i].incrementCount = hitEffectincrementCount;
            hitEffectPools[i].Initialize();
        }
    }
    public Collider2D[] GetTargetList(Vector2 point, float Range, int layerMask, List<Collider2D> exceptList)
    {

        Collider2D[] colliders = GetTargetList(point,Range,layerMask);
        List<Collider2D> colliderList = colliders.ToList();
        foreach (var item in exceptList)
        {
            colliderList.Remove(item);
        }
        return colliderList.ToArray();

    }
    public Collider2D[] GetTargetList(Vector2 point, float DegreeRange, Vector2 ViewDirection, float Range, int layerMask, List<Collider2D> exceptList)
    {

        Collider2D[] colliders = GetTargetList(point,DegreeRange,ViewDirection, Range, layerMask);
        List<Collider2D> colliderList = colliders.ToList();
        foreach (var item in exceptList)
        {
            colliderList.Remove(item);
        }
        return colliderList.ToArray();

    }

    public Collider2D[] GetTargetList(Vector2 point, float Range, int layerMask) {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(point, Range, layerMask);
        
        Collider2D temp;
        for (int i = 0; i < colliders.Length; i++)
        {
            for (int j = i; j < colliders.Length-i-1; j++)
            {
                if (((Vector2)colliders[j].transform.position-point).sqrMagnitude > ((Vector2)colliders[j+1].transform.position - point).sqrMagnitude)
                {
                    temp = colliders[j];
                    colliders[j] = colliders[j + 1];
                    colliders[j+1] = temp;

                }
            }
        }
        return colliders;
    }
    public Collider2D[] GetTargetList(Vector2 point, float DegreeRange,Vector2 ViewDirection ,float Range, int layerMask)
    {
        List<Collider2D> colliders = Physics2D.OverlapCircleAll(point, Range, layerMask).ToList();
        for (int i = colliders.Count-1; i >= 0; i--)
        {
            if (Mathf.Abs( Vector2.Angle(ViewDirection, (Vector2)colliders[i].transform.position- point)) > DegreeRange/2) {
                colliders.Remove(colliders[i]);
            }
        }

        Collider2D temp;
        for (int i = 0; i < colliders.Count; i++)
        {
            for (int j = i; j < colliders.Count - i - 1; j++)
            {
                if (((Vector2)colliders[j].transform.position - point).sqrMagnitude > ((Vector2)colliders[j + 1].transform.position - point).sqrMagnitude)
                {
                    temp = colliders[j];
                    colliders[j] = colliders[j + 1];
                    colliders[j + 1] = temp;

                }
            }
        }

        return colliders.ToArray();
    }
    public void HandleDamage(float atkPoint, FSMbase target) {
        target.TakeAttack(atkPoint);
        Debug.Log(atkPoint+"데미지->"+target.name);
    }
    public void HandleDamage(float atkPoint, FSMbase target,int hitEffectNum)
    {//!TODO :콜리전 정보를 받아와서 살짝의 랜덤을 준 위치에 생성
        HandleDamage(atkPoint,target);
        var e = hitEffectPools[hitEffectNum].GetObjectDisabled();
        e.transform.position = target.transform.position;
        e.gameObject.SetActive(true);
        e.GetComponent<Effector>().Alpha(0.5f,0.7f).And().Disable(0.5f).Play();

    }
}