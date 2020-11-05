using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleColorChanger : MonoBehaviour
{
    ParticleSystem particle = null;

    void Awake()
    {
    }
    
    public void SetColor(Color color)
    {
        if(particle == null)
            particle = GetComponent<ParticleSystem>();
        ParticleSystem.MainModule module = particle.main;
        module.startColor = color;
    }

}
