using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flower : MapObject
{
    Effector effector;
    Vector3 originalScale;
    public GameObject[] particles;
    public float gravity;
    public Vector2[] moveDirs;
    Effector[] particleEffectors;
    SimpleGravityParticle[] particleGravities;
    public override void OnOtherEnter(Collider2D collision)
    { }

    public override void OnOtherExit(Collider2D collision)
    { }

    public override void OnPlayerEnter()
    {
        effector.Scale(0.1f, 0.9f * originalScale).Then().Scale(0.15f, 1.15f * originalScale).Then().Scale(0.15f, 1.1f * originalScale).Play();
        ActivateParticles();
    }
    void ActivateParticles()
    {
        for (int i = 0; i < particles.Length; i++)
        {
            particles[i].SetActive(true);
            particleEffectors[i].Wait(0.15f).Then().Alpha(0.15f, 0.3f).Then().Disable().Play();
            particleGravities[i].SetDir(moveDirs[i] * Random.Range(0.8f, 1.1f));
        }
    }
    public override void OnPlayerExit()
    {
        transform.localScale = originalScale;
    }

    public override void TakeCC(int CCnum = 0)
    { }

    public override void TakeKnockBack(float force, Vector2 knockBackDir)
    { }

    // Start is called before the first frame update
    void Awake()
    {
        effector = GetComponent<Effector>();
        particleEffectors = new Effector[particles.Length];
        particleGravities = new SimpleGravityParticle[particles.Length];
        for (int i=0; i<particles.Length; i++)
        {
            particleEffectors[i] = particles[i].GetComponent<Effector>();
            particleGravities[i] = particles[i].GetComponent<SimpleGravityParticle>();
            particleGravities[i].gravity = gravity;
        }
        originalScale = transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        base.Update();
    }
}
