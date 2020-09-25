using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

[CreateAssetMenu(fileName = "New MapObject", menuName = "MapObject/New MapObject", order = 2)]
public class Hitableinfo : ScriptableObject
{
    [SerializeField]
    private int hitType = 0;
    [SerializeField]
    private int maxHp = 0;
    [SerializeField]
    private int collisionType = 0;
    [SerializeField]
    private int takeType = 0;

    [SerializeField]
    private Sprite defaultSprite;
    [SerializeField]
    private Material outLineMat;
    [SerializeField]
    private AnimatorController animController;

    public int HitType
    {
        get
        {
            return hitType;
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
    public AnimatorController AnimController { get => animController; }
    public int MaxHp { get => maxHp;}
    public Material OutLineMat { get => outLineMat; }
}
