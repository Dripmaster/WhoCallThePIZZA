using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroppedItem : MonoBehaviour{

    public ItemBase info;


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Player")
        {
            InventorySystem.MyInstance.AddItem(info);
            gameObject.SetActive(false);
        }
    }
}
