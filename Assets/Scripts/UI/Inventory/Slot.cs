using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Slot : MonoBehaviour
{
    public Image icon;

    Stack<ItemBase> items = new Stack<ItemBase>();
    public bool IsEmpty {
        get
        {
            return items.Count <= 0;
        }
    }

    public ItemBase MyItem
    {
        get
        {
            if (!IsEmpty)
                return items.Peek();
            else
                return null;
        }
    }
    public bool AddItem(ItemBase item)
    {
        items.Push(item);
        icon.sprite = item.MyIcon;
        icon.color = Color.white;
        return true;
    }
    public void RemoveItem(ItemBase item)
    {
        // 자기 자신이 빈슬롯이 아니라면
        if (!IsEmpty)
        {
            // Items 의 제일 마지막 아이템을 꺼냅니다.
            items.Pop();

            // 해당 슬롯의 아이템아이콘을 투명화시킵니다.
            InventorySystem.MyInstance.UpdateStackSize(this);
        }
    }
    // Start is called before the first frame update
    void Awake()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void UseItem()
    {
        var item = MyItem;
        if (item.Use())
        {
            RemoveItem(item);
        }
    }

}
