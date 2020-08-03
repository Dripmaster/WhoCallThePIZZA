using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AttackManager
{
    static AttackManager Instance = null;
    public static AttackManager GetInstance()
    {
        if (Instance == null)
            Instance = new AttackManager();

        return Instance;
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

}
