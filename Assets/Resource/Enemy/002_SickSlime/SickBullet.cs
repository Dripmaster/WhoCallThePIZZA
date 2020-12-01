using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SickBullet : BulletBase
{
    public Vector2 targetPos;//목표지점
    public Vector2 startPos;//시작점

    Vector2 lerpPos;//프레임 당 이동(방향벡터)

    float vy = 5f;//y가짜 포물선 속도         몇으로 해야하는 거임???
    float vyCalculated;//y가짜 포물선 속도를 이동거리에 반비례하게 적용
    float h;
    float t;//경과시간
    float all_t;//전체 이동 시간
    float l;//이동거리

    ZSystem zSystem;
    void PreCalculate()
    {
        lerpPos = targetPos - startPos;

        
        l = lerpPos.magnitude;
        lerpPos.Normalize();

        lerpPos *= speed;

        h = 0;
        t = 0;
        all_t = l / speed;  
        vyCalculated = vy / all_t;     
    }

    new void Awake()
    {
        myrigid = GetComponent<Rigidbody2D>();
        zSystem = GetComponent<ZSystem>();
        PreCalculate();
    }

    new void FixedUpdate()
    {
        t += Time.deltaTime;

        float x = lerpPos.x;
        float y = lerpPos.y;
        if (t <= all_t * 0.2f)
        {
            h = vyCalculated;
        }
        else if (t <= all_t * 0.8f)
        {

            if (t <= all_t * 0.25f)
            {
                h = Mathf.Lerp(h, 0, Time.deltaTime * 10);
            }
            else if (t >= all_t * 0.75f)
            {
                h = Mathf.Lerp(h, -vyCalculated, Time.deltaTime * 10);
            }
            else
            {
                h = 0;
            }
        }
        else if (t <= all_t)
        {
            h = -vyCalculated;
        }
        else
        {
            h = 0;
        }


        if (h != 0)
        {
            zSystem.Z += h * Time.deltaTime;
        }
        Vector3 movePos = new Vector3(x, y, 0);
        myrigid.MovePosition(transform.position + movePos * Time.deltaTime);

    }
}
