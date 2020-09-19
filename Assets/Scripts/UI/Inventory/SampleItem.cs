using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New SampleItem", menuName = "Item/SampleItem", order = 1)]
public class SampleItem : ItemBase
{

    public override bool Use()
    {
        Debug.Log(name+" Used!");
        return true;
    }
}
