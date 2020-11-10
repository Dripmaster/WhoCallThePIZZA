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

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
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
        }

    }
    public void SetStep(StepForwardValues sfv,Vector2 stepDir)
    {
        Debug.Log("step");
        sf_eTime = 0;
        this.stepDir = stepDir;
        this.sfv = sfv;
        stepSpeed = sfv.distance / sfv.duration;
    }
}
