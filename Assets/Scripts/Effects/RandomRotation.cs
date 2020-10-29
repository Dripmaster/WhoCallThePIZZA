using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomRotation : MonoBehaviour
{
    public int angleSegments;
    float originalRotation;
    void Awake()
    {
        originalRotation = transform.eulerAngles.z;
    }
    void OnEnable()
    {
        transform.rotation = Quaternion.Euler(0, 0, Random.Range(0, angleSegments) * 360/angleSegments);
    }
    void OnDisable()
    {
        transform.rotation = Quaternion.Euler(0, 0, originalRotation);
    }
}
