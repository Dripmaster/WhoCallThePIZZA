using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ItemBase : ScriptableObject
{
    [SerializeField]
    private int itemNumber;
    [SerializeField]
    private Sprite icon;

    [SerializeField]
    private int stackSize;

    public int ItemNumber
    {
        get
        {
            return itemNumber;
        }
    }


    public int StackSize
    {
        get
        {
            return stackSize;
        }
    }

    public Sprite MyIcon
    {
        get
        {
            return icon;
        }
    }

    public abstract bool Use();

}
