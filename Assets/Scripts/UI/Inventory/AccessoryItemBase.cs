using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 상단 메뉴 버튼 만들기
[CreateAssetMenu(fileName = "New Accessory", menuName = "Items/New Accessory", order = 2)]
public class AccessoryItemBase : DroppedItemBase
{
    /*여기다가 아이템 스탯 구조체든 클래스든 뭐 만들어서 하면 될듯*/
    public float maxHP;
    public float moveSpeed;
    public float atkPoint;
    public float criticalPoint;
    public float criticalDamage;
    public float DefensePoint;
    public override bool Use(ItemBase itemBase)
    {
        return InventorySystem.MyInstance.EquipAcc(itemBase);
    }
}
