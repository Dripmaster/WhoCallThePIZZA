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

    public Collider2D[] GetTargetList(Vector2 point, float Range, int layerMask) {
        return Physics2D.OverlapCircleAll(point, Range,layerMask);
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
        return colliders.ToArray();
    }
    public void HandleDamage(float atkPoint, FSMbase target) {
        target.TakeAttack(atkPoint);
        Debug.Log(atkPoint+"데미지->"+target.name);
    }

}
