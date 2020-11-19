using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireworkBullet : BulletBase
{
    public Pool DestroyEffectPool;
    public Transform effectParent;
    public int curve;  //0 직선,1:위로, 2:아래로
    private float time = 0;
    public float explosionTime;

    private void Awake()
    {
        myrigid = GetComponent<Rigidbody2D>();

    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        bool dest = false;
        if (touched != null)
            dest = touched(collision);
        if (dest)
            Explosion();
    }
    private void FixedUpdate()
    {
        myrigid.MovePosition(transform.position + (Vector3)dir * speed * Time.deltaTime);

        time += Time.deltaTime;
        if (time >= explosionTime)
        {
            time = 0;
            Explosion();
        }
    }

    private void Explosion()
    {
        var t = DestroyEffectPool.GetObjectDisabled(effectParent);
        t.transform.position = this.gameObject.transform.position;
        t.gameObject.SetActive(true);
        gameObject.SetActive(false);
    }
}


