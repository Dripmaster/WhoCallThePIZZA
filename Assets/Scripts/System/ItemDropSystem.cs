using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemDropSystem : MonoBehaviour
{
    public GameObject DroppedItemPrefab;
    private static ItemDropSystem instance;

    public static ItemDropSystem MyInstance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<ItemDropSystem>();
            }
            return instance;
        }
        set
        {
            instance = value;
        }
    }
    public void DropItem(Vector2 pos,DroppedItemBase itemBase)
    {
        var g = Instantiate(DroppedItemPrefab,pos,Quaternion.identity).GetComponent<DroppedItem>();
        g.SetInfo(itemBase);
    }

}
