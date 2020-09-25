using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class DroppedItemBase : ScriptableObject
{
    [SerializeField]
    private int itemNumber = 0;
    [SerializeField]
    private Sprite icon = null;

    [SerializeField]
    private int stackSize = 0;

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
    public abstract bool Use(ItemBase itemBase);
   
}
