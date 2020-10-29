using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableWithAnimationEnd : MonoBehaviour
{
    public AnimationClip animationClip;
    Effector effector;

    void Awake()
    {
        effector = GetComponent<Effector>();
    }

    void OnEnable()
    {
        effector.Disable(animationClip.length).Play();
    }

}
