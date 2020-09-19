using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventorySystem : MonoBehaviour
{
    public List<Slot> slots = new List<Slot>();
    public Image slotParent;
    private static InventorySystem instance;
    private bool isOpen;
    public bool IsOpen
    {
        get
        {
            return isOpen;
        }
        set
        {
            isOpen = value;
            if (isOpen)
            {
                OpenInven();
            }
            else
            {
                CloseInven();
            }
        }
    }
    public static InventorySystem MyInstance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<InventorySystem>();
                instance.gameObject.SetActive(false);
                instance.initSlot();

            }
            return instance;
        }
        set
        {
            instance = value;
        }
    }
    void initSlot()
    {

        var s = slotParent.GetComponentsInChildren<Slot>();
        foreach (var item in s)
        {
            slots.Add(item);
        }
    }
    // Start is called before the first frame update
    void Awake()
    {
        isOpen = gameObject.activeInHierarchy;
    }

    // Update is called once per frame
    void Update()
    {
        if (InputSystem.Instance.getKeyDown(InputKeys.MB_R_click))
        {
            foreach (Slot slot in slots)
            {
                if (!slot.IsEmpty)
                {
                    slot.UseItem();

                    break;
                }
            }

        }
    }
    public bool AddItem(ItemBase item)
    {
        foreach (Slot slot in slots)
        {
            // 빈 슬롯이 있으면
            if (slot.IsEmpty)
            {
                // 해당 슬롯에 아이템을 추가한다.
                slot.AddItem(item);
                return true;
            }
        }

        return false;
    }
    public void UpdateStackSize(Slot slot)
    {
        if (slot.IsEmpty)
        {
            // 해당 슬롯의 아이콘 투명하게 만들기
            slot.icon.color = new Color(0, 0, 0, 0);
        }
    }

    void OpenInven()
    {
        gameObject.SetActive(true);
    }
    void CloseInven()
    {
        gameObject.SetActive(false);
    }
}
