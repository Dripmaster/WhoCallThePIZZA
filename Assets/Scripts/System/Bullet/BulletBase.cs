using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletBase : MonoBehaviour
{
    public BulletTouched touched;
    public Vector2 dir;
    public float speed;
    protected Rigidbody2D myrigid;

    private void Awake()
    {
        myrigid = GetComponent<Rigidbody2D>();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        bool dest = false;
        if(touched != null)
            dest = touched(collision);
        if (dest)
            gameObject.SetActive(false);
    }
    private void FixedUpdate()
    {
        myrigid.MovePosition(transform.position +(Vector3) dir * speed * Time.deltaTime);
    }
}
public delegate bool BulletTouched(Collider2D collision);