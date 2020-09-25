using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 상단 메뉴 버튼 만들기
[CreateAssetMenu(fileName = "New Item", menuName = "Items/New Item", order = 2)]
public class AccessoryItemBase : DroppedItemBase
{
    public override bool Use(ItemBase itemBase)
    {
        return InventorySystem.MyInstance.EquipAcc(itemBase);
    }
}
