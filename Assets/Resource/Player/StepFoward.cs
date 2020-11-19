using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StepFoward : MonoBehaviour
{
    float sf_eTime; //
    StepForwardValues sfv;
    float stepSpeed;
    Rigidbody2D rigid;
    Vector2 stepDir;
    FSMbase.StepFowardCallBack endCallBack;
    bool endValue;
    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
    }
    public bool IsProgress()
    {
        if (sfv == null)
            return false;
        return true;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (sfv == null) return;
        if (sf_eTime <= sfv.duration)
        {
            if (stepDir != Vector2.zero)
            {
                rigid.MovePosition((Vector2)transform.position + stepDir * stepSpeed*Time.deltaTime);
                sf_eTime += Time.deltaTime;
            }
        }
        else
        {
            sfv = null;
            endCallBack.Invoke(endValue);
        }

    }
    public void SetStep(StepForwardValues sfv,Vector2 stepDir, FSMbase.StepFowardCallBack callBack, bool v)
    {
        this.sfv = sfv;
        sf_eTime = 0;
        setAngle(sfv.angle, stepDir);
        stepSpeed = sfv.distance / sfv.duration;
        endCallBack = callBack;
        endValue = v;
    }
    void setAngle(float v, Vector2 dir)
    {
        stepDir = Quaternion.Euler(0, 0, v) * dir;
    }
}
