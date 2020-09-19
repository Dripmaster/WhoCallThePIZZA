using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyCommander : MonoBehaviour
{
    InventorySystem info;
    // Start is called before the first frame update
    void Awake()
    {
        info = InventorySystem.MyInstance;
    }

    // Update is called once per frame
    void Update()
    {

        if (InputSystem.Instance.getKeyDown(InputKeys.InfoBtn))
        {
            
            info.IsOpen = !info.IsOpen;
        }

    }
}
