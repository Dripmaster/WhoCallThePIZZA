using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponEffects : MonoBehaviour
{
    public Transform effectParent;
    public GameObject[] Effects;
    // Start is called before the first frame update
    void Awake()
    {
        effectParent =  EffectManager.Instance.effectParent;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
