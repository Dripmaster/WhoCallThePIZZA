using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZSystem : MonoBehaviour
{
    public Transform ImageTransform;
    int airLayer = 19;
    int defaultLayer;
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
            if (z > 0)
            {
                gameObject.layer = airLayer;
            }
            else{
                gameObject.layer = defaultLayer;
            }
        }
    }


    // Start is called before the first frame update
    void Awake()
    {
        defaultLayer = gameObject.layer;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
