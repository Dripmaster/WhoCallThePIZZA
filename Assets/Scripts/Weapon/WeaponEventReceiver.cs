using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponEventReceiver : MonoBehaviour
{
    public WeaponBase weaponBase;

    private void Awake()
    {
        if(weaponBase == null)
        {
            weaponBase = GameObject.Find("WeaponBase").GetComponent<WeaponBase>();
        }
    }

    public void motionEventInt(int value)
    {
        weaponBase.motionEvent(value);
    }
    public void motionEventString(string msg)
    {
        weaponBase.motionEvent(msg);
    }
}
