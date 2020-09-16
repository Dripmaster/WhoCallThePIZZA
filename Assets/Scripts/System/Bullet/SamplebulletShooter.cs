using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SamplebulletShooter : MonoBehaviour
{
    public BulletBase bulletPrefab;
    Pool bulletPool;
    public Vector3 dir;
    public float Speed;

    public string tmpMessage;

    float time = 0;
    // Start is called before the first frame update
    void Awake()
    {
        bulletPool = gameObject.AddComponent<Pool>();
        bulletPool.incrementCount = 1;
        bulletPool.initialCount = 10;
        bulletPool.poolPrefab = bulletPrefab.gameObject;
        bulletPool.Initialize();
    }

    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;
        if (time >= 2)
        {
            time = 0;
            var b = bulletPool.GetObjectDisabled().GetComponent<BulletBase>();
            //위에 실제 다른곳에서 호출 시 parent 설정 해줘야함
            b.transform.position = transform.position;
            b.dir = dir;
            b.speed = Speed;
            b.touched += sampleBulletTouched;
            b.gameObject.SetActive(true);
            
        }
    }
    bool sampleBulletTouched(Collider2D collision)
    {
        touchMessage();
        Debug.Log("충돌 with:" + collision.name);
        return true;
    }
    void touchMessage()
    {

        Debug.Log("충돌요 m:" + tmpMessage);
    }
}
