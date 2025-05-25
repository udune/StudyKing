using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Logger = Common.Logger;

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

    public override void Setting(BaseUIData data)
    {
        base.Setting(data);
        
        if (content.childCount.Equals(0))
        {
            foreach (var item in inventoryItemList)
            {
                var go = Instantiate(itemPrefab, content);
                var thisItem = item;
                
                go.transform.Find("Icon").GetComponent<Image>().sprite = Resources.Load<Sprite>($"Texture/{thisItem.id}");
                go.GetComponent<Button>().onClick.RemoveAllListeners();
                go.GetComponent<Button>().onClick.AddListener(() =>
                {
                    try
                    {
                        if (OnClickItem(thisItem))
                        {
                            PlayerCustom.Instance.Equip(thisItem.id);
                            go.transform.Find("outline").GetComponent<Image>().color = new Color(251/255f, 180/255f, 170/255f, 1f);
                        }
                        else
                        {
                            PlayerCustom.Instance.UnEquip(thisItem.id);
                            go.transform.Find("outline").GetComponent<Image>().color = new Color(229/255f, 235/255f, 214/255f, 1f);
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.LogError($"{GetType()}:: Error in OnClickItem");
                    }

                    SaveItems();
                });
            }
        }

        LoadItems();
    }

    private void LoadItems()
    {
        var userInventoryData = UserDataManager.Instance.GetUserData<UserInventoryData>();
        if (userInventoryData == null)
            return;

        foreach (var equippedId in userInventoryData.EquippedItemIdList)
        {
            var item = inventoryItemList.Find(x => x.id.Equals(equippedId));
            if (item != null)
            {
                if (equippedItemList.Contains(item))
                    return;
                
                equippedItemList.Add(item);
                PlayerCustom.Instance.Equip(item.id);
            }
        }
        
        SortSlots();
    }

    private void SaveItems()
    {
        var userInventoryData = UserDataManager.Instance.GetUserData<UserInventoryData>();
        if (userInventoryData == null)
            return;
        
        userInventoryData.EquippedItemIdList = equippedItemList.Select(x => x.id).ToList();
        userInventoryData.SaveData();
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
