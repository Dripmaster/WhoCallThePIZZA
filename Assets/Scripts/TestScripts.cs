using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScripts : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        print(GetComponent<Animator>().runtimeAnimatorController.animationClips[0].frameRate);
    }

    // Update is called once per frame
    void Update()
    {
    }
}
