using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemBase
{
    /*여기다가 아이템 스탯 구조체든 클래스든 뭐 만들어서 하면 될듯*/
    public DroppedItemBase ItemInfo;
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
    public bool Use()
    {
        return ItemInfo.Use(this);
    }

}
