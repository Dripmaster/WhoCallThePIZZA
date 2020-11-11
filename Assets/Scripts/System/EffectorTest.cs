using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectorTest : MonoBehaviour
{
    public Effector effector;
    // Start is called before the first frame update
    void Start()
    {
        effector.Scale(1f,3f).And().Alpha(0.5f,0.5f).Then().Wait(1f)
                .Then().RotateTo(1f,180f).Then().ColorChange(1f,new Color(0,0,0,1))
                .Then().Move(1f,new Vector2(2,2));
    }
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.E))
            effector.Play();
    }

}
