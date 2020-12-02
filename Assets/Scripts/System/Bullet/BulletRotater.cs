using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletRotater : MonoBehaviour
{
    Vector2 tempPos;
    float  turnSpeed= 720f;
    private void Update()
    {
        //transform.rotation = Quaternion.RotateTowards(transform.rotation, getDir(tempPos, transform.position), turnSpeed * Time.deltaTime);
        transform.rotation = getDir(tempPos,transform.position);
        tempPos = transform.position;
    }





    public Quaternion  getDir(Vector2 lastPos, Vector2 nowPos)
    {
        Vector2 targetDir = nowPos - lastPos;

        if (targetDir == Vector2.zero)
            return transform.rotation;
        targetDir.Normalize();

        Vector3 rotatedVectorToTarget = Quaternion.Euler(0, 0, 90) * targetDir;

        Quaternion targetRotation = Quaternion.LookRotation(forward: Vector3.forward, upwards: rotatedVectorToTarget);

        return targetRotation;
    }
}
