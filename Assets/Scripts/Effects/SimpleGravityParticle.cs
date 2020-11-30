using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleGravityParticle : MonoBehaviour
{
    public float gravity = 1f;
    float ySpeed;
    Vector2 moveDir = Vector2.zero;
    Vector3 originalPos;
    void Awake()
    {
        originalPos = transform.position;
    }
    public void SetDir(Vector2 dir)
    {
        moveDir = dir;
    }
    void OnEnable()
    {
        ySpeed = 0f;
        transform.position = originalPos;
    }

    // Update is called once per frame
    void Update()
    {
        ySpeed += gravity * Time.deltaTime;
        transform.position += new Vector3(moveDir.x, moveDir.y - ySpeed, 0) * Time.deltaTime;
    }
}
