using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grass : MapObject
{
    Effector effector;
    Vector3 originalScale;
    public GameObject[] particles;
    public float gravity;
    public Vector2[] moveDirs;
    Effector[] particleEffectors;
    SimpleGravityParticle[] particleGravities;
    IEnumerator dieCoroutine;
    public override void OnOtherEnter(Collider2D collision)
    { }

    public override void OnOtherExit(Collider2D collision)
    { }

    public override void OnPlayerEnter()
    {
        effector.Scale(0.1f, new Vector2(originalScale.x*1.1f,originalScale.y*0.95f)).Then()
            .Scale(0.1f, new Vector2(originalScale.x*0.95f, originalScale.y*1.05f)).Then()
            .Scale(0.1f, originalScale).Play();
        ActivateParticles();
    }
    void ActivateParticles()
    {
        for (int i = 0; i < particles.Length; i++)
        {
            particles[i].SetActive(true);
            particleEffectors[i].Wait(0.1f).Then().Alpha(0.1f, 0.3f).Then().Disable().Play();
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
    public override void OnHit(float dmg)
    {
        getDamage(dmg);

    }
    public override void DoDestroy()
    {
        StartCoroutine(dieCoroutine);
    }
    IEnumerator DieCoroutine()
    {
        float duration = 0.7f;
        effector.Alpha(duration / 2, 0f).Play();
        
        base.DoDestroy();
        yield return null;

    }
    // Start is called before the first frame update
    void Awake()
    {
        effector = GetComponent<Effector>();
        particleEffectors = new Effector[particles.Length];
        particleGravities = new SimpleGravityParticle[particles.Length];
        for (int i = 0; i < particles.Length; i++)
        {
            particleEffectors[i] = particles[i].GetComponent<Effector>();
            particleGravities[i] = particles[i].GetComponent<SimpleGravityParticle>();
            particleGravities[i].gravity = gravity;
        }
        originalScale = transform.localScale;
        dieCoroutine = DieCoroutine();
    }

    // Update is called once per frame
    void Update()
    {
        base.Update();
    }
}
