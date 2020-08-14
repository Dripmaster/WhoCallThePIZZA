using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    //!TODO :
    //카메라 확대 축소
    //타겟 크기 받아서
    //줌인 줌아웃 속도 받아서
   
    public float interpVelocity;
    public float cameraSpeed;
    public GameObject target;
    public Vector3 offset;

    public float minX;
    public float maxX;
    public float minY;
    public float maxY;

    Vector3 targetPos;
    // Use this for initialization
    void Start()
    {
        targetPos = transform.position;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (target)
        {
            Vector3 posNoZ = transform.position;
            posNoZ.z = target.transform.position.z;

            Vector3 targetDirection = (target.transform.position - posNoZ);

            interpVelocity = targetDirection.magnitude * 10f;

            targetPos = transform.position + (targetDirection.normalized * interpVelocity * (Time.deltaTime * cameraSpeed));

            transform.position = Vector3.Lerp(transform.position, targetPos + offset, 0.25f);

            Vector3 stopPosition = transform.position;

            if (stopPosition.x < minX)
                stopPosition.x = minX;
            else if (stopPosition.x > maxX)
                stopPosition.x = maxX;

            if (stopPosition.y < minY)
                stopPosition.y = minY;
            else if (stopPosition.y > maxY)
                stopPosition.y = maxY;

            transform.position = stopPosition;

        }
        else
        {
            target = GameObject.Find("player");
        }
    }
}
