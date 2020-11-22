using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleAnimatior : MonoBehaviour
{
    // Start is called before the first frame update
    public SpriteRenderer spriteRenderer;
    public float duration;
    public Sprite[] sprites;
    float eTime;
    void Awake()
    {
        eTime = 0f;
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        spriteRenderer.sprite = sprites[(int)(sprites.Length * (eTime / duration))];
        eTime += Time.deltaTime;
        if (eTime >= duration)
            eTime -= duration;
    }
}
