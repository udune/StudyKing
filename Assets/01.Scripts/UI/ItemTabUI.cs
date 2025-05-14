using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class InventoryItem
{
    public string id;
    public string name;
}

public class ItemTabUI : BaseUI
{
    [SerializeField] List<InventorySlot> slotList = new List<InventorySlot>();
    [SerializeField] List<InventoryItem> inventoryItemList = new List<InventoryItem>();
    List<InventoryItem> equippedItemList = new List<InventoryItem>();

    [SerializeField] GameObject itemPrefab;
    [SerializeField] Transform content;

    private void OnEnable()
    {
        PlayerCustom.Instance.character.GetComponent<ObjRotator>().enabled = true;
        
        if (content.childCount.Equals(0))
        {
            foreach (var item in inventoryItemList)
            {
                var go = Instantiate(itemPrefab, content);
                go.transform.Find("Icon").GetComponent<Image>().sprite = Resources.Load<Sprite>($"Texture/{item.name}");
                go.GetComponent<Button>().onClick.AddListener(() =>
                {
                    if (OnClickItem(item))
                    {
                        PlayerCustom.Instance.Equip(item.name);
                        go.transform.Find("outline").GetComponent<Image>().color = new Color(125/255f, 128/255f, 118/255f, 1f);
                    }
                    else
                    {
                        PlayerCustom.Instance.UnEquip(item.name);
                        go.transform.Find("outline").GetComponent<Image>().color = new Color(208/255f, 214/255f, 197/255f, 1f);
                    }
                });
            }
        }
    }

    private void OnDisable()
    {
        PlayerCustom.Instance.character.GetComponent<ObjRotator>().enabled = false;
    }

    private bool OnClickItem(InventoryItem item)
    {
        if (equippedItemList.Contains(item))
        {
            equippedItemList.Remove(item);
            SortSlots();
            return false;
        }
        
        if (equippedItemList.Count >= slotList.Count)
        {
            return false;
        }
            
        equippedItemList.Add(item);
        SortSlots();
        return true;
    }

    private void SortSlots()
    {
        for (int i = 0; i < slotList.Count; i++)
        {
            if (i < equippedItemList.Count)
            {
                slotList[i].Apply(equippedItemList[i]);
            }
            else
            {
                slotList[i].Clear();
            }
        }
    }
}
