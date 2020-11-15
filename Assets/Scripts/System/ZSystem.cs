using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZSystem : MonoBehaviour
{
    public Transform ImageTransform;
    float z;
    public float Z
    {
        get { return z; }
        set
        {
            z = value;
            Vector3 v = ImageTransform.localPosition;
            v.y = z;
            ImageTransform.localPosition = v;
        }
    }


    // Start is called before the first frame update
    void Awake()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
