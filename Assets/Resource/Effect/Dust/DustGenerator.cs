using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DustGenerator : MonoBehaviour
{
    public float interval;
    float eTime;
    float restTime;
    public GameObject dustPrefab;
    public GameObject effectParent;

    bool isOn;
    void Awake()
    {
        eTime = 0f;
        restTime = 0f;
        isOn = false;
    }
    void CreateDust()
    {
        PoolableObject dust = EffectManager.Instance.GetDust(DustType.WALK);
        dust.transform.position = transform.position;
        dust.transform.localScale = transform.lossyScale;
        float duration = dust.GetComponent<SimpleAnimation>().duration;
        dust.gameObject.SetActive(true);
        dust.GetComponent<Effector>().Scale(duration, transform.lossyScale * 1.2f).Then().Disable().Play();
    }
    public void Activate()
    {
        Debug.Log(restTime);
        if (restTime >= 0.2f)
            CreateDust();
        isOn = true;
        eTime = 0f;
    }
    public void Deactivate()
    {
        isOn = false;
        if (eTime >= 0.15f)
            CreateDust();
        restTime = 0f;
    }
    // Update is called once per frame
    void Update()
    {
        if (!isOn)
            restTime += Time.deltaTime;
        else
        {
            eTime += Time.deltaTime;
            if (eTime >= interval)
            {
                CreateDust();
                eTime -= interval;
            }
        }
        
    }
}
