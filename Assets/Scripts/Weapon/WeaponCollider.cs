using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class WeaponCollider : MonoBehaviour
{
    public int myColliderType;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        WeaponBase.instance.onWeaponTouch(myColliderType, collision);
    }
}
