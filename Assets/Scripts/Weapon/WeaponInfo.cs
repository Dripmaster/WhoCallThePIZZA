using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponInfo : MonoBehaviour
{
    public WeaponType wType;
    Vector2 tempPos;
    Quaternion tempRot;
    private void Awake()
    {
        tempPos = transform.localPosition;
        tempRot = transform.localRotation;
    }
    private void OnEnable()
    {
        transform.localPosition = tempPos;
        transform.localRotation = tempRot;
    }
}
