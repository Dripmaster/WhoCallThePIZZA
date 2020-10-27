using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class ItemBase { 
    public float[] Stats;
    bool hadEquiped;

    public DroppedItemBase ItemInfo;
    public bool HadEquiped
    {
        get
        {
            return hadEquiped;
        }
    }
    public int ItemNumber
    {
        get
        {
            return ItemInfo.ItemNumber;
        }
    }
    public int StackSize
    {
        get
        {
            return ItemInfo.StackSize;
        }
    }
    public Sprite MyIcon
    {
        get
        {
            return ItemInfo.MyIcon;
        }
    }
    public ItemBase()
    {
        Stats = new float[Enum.GetValues(typeof(STAT)).Length];
    }
    public bool Use()
    {
        hadEquiped = true;
        return ItemInfo.Use(this);
    }

}
