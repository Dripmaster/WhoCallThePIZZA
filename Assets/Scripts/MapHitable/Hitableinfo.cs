using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

[CreateAssetMenu(fileName = "New MapObject", menuName = "MapObject/New MapObject", order = 2)]
[System.Serializable]
public class Hitableinfo : ScriptableObject
{
    [SerializeField]
    private int objectID = 0;
    [SerializeField]
    private int maxHp = 0;
    [SerializeField]
    private Sprite defaultSprite;
    [SerializeField]
    private Material outLineMat;
    [SerializeField]
    private bool chainAble;
    [SerializeField]
    private bool showEffect;


    //[SerializeField]
    private int hitType = 0;
    //[SerializeField]
    private int collisionType = 0;
    [SerializeField]
    private int takeType = 0;

    public int HitType
    {
        get
        {
            return hitType;
        }
    }
    public bool ShowEffect
    {
        get
        {
            return showEffect;
        }
    }
    public int ObjectID
    {
        get
        {
            return objectID;
        }
    }
    public bool ChainAble
    {
        get
        {
            return chainAble;
        }
    }
    public int CollisionType
    {
        get
        {
            return collisionType;
        }
    }
    public int TakeType
    {
        get
        {
            return takeType;
        }
    }

    public Sprite DefaultSprite { get => defaultSprite; }
    public int MaxHp { get => maxHp;}
    public Material OutLineMat { get => outLineMat; }
}
