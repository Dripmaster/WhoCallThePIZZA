using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroppedItem : MonoBehaviour{

    public ItemBase info;

    public void Awake()
    {
    }
    public void SetInfo(DroppedItemBase itemBase)
    {
        info = new ItemBase();
        info.ItemInfo = itemBase;
        GetComponent<SpriteRenderer>().sprite = itemBase.MyIcon;
        //info = itemBase;

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Player")
        {
            InventorySystem.MyInstance.AddItem(info);
            gameObject.SetActive(false);
        }
    }
}
