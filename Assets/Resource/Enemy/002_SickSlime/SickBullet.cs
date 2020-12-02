using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SickBullet : BulletBase
{
    public Vector2 targetPos;//목표지점
    public Vector2 startPos;//시작점

    Vector2 lerpPos;//프레임 당 이동(방향벡터)

    [HideInInspector]
    public float vy = 5f;//y가짜 포물선 속도         몇으로 해야하는 거임???
    [HideInInspector]
    public float all_t = 1;//전체 이동 시간

    float vyCalculated;//y가짜 포물선 속도를 이동거리에 반비례하게 적용
    float h;
    float t;//경과시간
    float l;//이동거리

    ZSystem zSystem;
    void PreCalculate()
    {
        lerpPos = targetPos - startPos;

        
        l = lerpPos.magnitude;
        lerpPos.Normalize();


        h = 0;
        t = 0;

        speed = l / all_t;


        vyCalculated = vy / all_t;

        lerpPos *= speed;
    }

    private new void Awake()
    {
        myrigid = GetComponent<Rigidbody2D>();
        zSystem = GetComponent<ZSystem>();
    }
    private void OnEnable()
    {
        PreCalculate();
    }
    new void FixedUpdate()
    {
        t += Time.deltaTime;

        float x = lerpPos.x;
        float y = lerpPos.y;

        float curveHeight = 0;

        curveHeight = Mathf.Sin((t/all_t)*Mathf.PI);

       if(t<all_t)
        h = curveHeight * vyCalculated*10;
        else
        {
            h = 0;
        }
            zSystem.Z = h * Time.deltaTime;
        if (t <= all_t)
        {
            Vector3 movePos = new Vector3(x, y, 0);
            Vector3 nextPos = transform.position + movePos * Time.deltaTime;
            myrigid.MovePosition(nextPos);
        }

    }
}
