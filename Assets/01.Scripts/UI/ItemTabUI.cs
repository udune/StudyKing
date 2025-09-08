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
    private readonly List<InventoryItem> _equippedItemList = new List<InventoryItem>();

    [SerializeField] GameObject itemPrefab;
    [SerializeField] Transform content;

    public override void Setting(BaseUIData data)
    {
        base.Setting(data);
        
        if (content == null)
        {
            Logger.LogError($"{GetType()}::Content Transform이 null입니다");
            return;
        }
    
        if (itemPrefab == null)
        {
            Logger.LogError($"{GetType()}::Item Prefab이 null입니다");
            return;
        }

        InitializeItems();
    }
    
    private void InitializeItems()
    {
        if (content.childCount > 0) return; // 이미 생성됨
    
        foreach (var item in inventoryItemList)
        {
            try
            {
                CreateItemButton(item);
            }
            catch (Exception e)
            {
                Logger.LogError($"{GetType()}::아이템 생성 실패 [{item?.id}]: {e.Message}");
            }
        }
    }
    
    private void CreateItemButton(InventoryItem item)
    {
        if (item == null) return;
    
        var go = Instantiate(itemPrefab, content);
        if (go == null) return;
    
        // 아이콘 설정
        var iconTransform = go.transform.Find("Icon");
        if (iconTransform != null)
        {
            var iconImage = iconTransform.GetComponent<Image>();
            if (iconImage != null)
            {
                var sprite = Resources.Load<Sprite>($"Texture/{item.id}");
                if (sprite != null)
                {
                    iconImage.sprite = sprite;
                }
            }
        }
    
        // 버튼 이벤트 설정
        var button = go.GetComponent<Button>();
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => OnItemClick(item, go));
        }
    }
    
    private void OnItemClick(InventoryItem item, GameObject itemObject)
    {
        try
        {
            if (OnClickItem(item))
            {
                PlayerCustom.Instance?.Equip(item.id);
                SetItemSelected(itemObject, true);
            }
            else
            {
                PlayerCustom.Instance?.UnEquip(item.id);
                SetItemSelected(itemObject, false);
            }
        
            SaveItems();
        }
        catch (Exception e)
        {
            Logger.LogError($"{GetType()}::아이템 클릭 처리 실패: {e.Message}");
        }
    }
    
    private void SetItemSelected(GameObject itemObject, bool selected)
    {
        var outline = itemObject.transform.Find("outline");
        if (outline != null)
        {
            var outlineImage = outline.GetComponent<Image>();
            if (outlineImage != null)
            {
                outlineImage.color = selected ? 
                    new Color(251/255f, 180/255f, 170/255f, 1f) : 
                    new Color(229/255f, 235/255f, 214/255f, 1f);
            }
        }
    }

    private void LoadItems()
    {
        try
        {
            var userInventoryData = UserDataManager.Instance?.GetUserData<UserInventoryData>();
            if (userInventoryData?.EquippedItemIdList == null) return;

            foreach (var equippedId in userInventoryData.EquippedItemIdList)
            {
                var item = inventoryItemList?.Find(x => x.id.Equals(equippedId));
                if (item != null && !_equippedItemList.Contains(item))
                {
                    _equippedItemList.Add(item);
                }
            }
        }
        catch (Exception e)
        {
            Logger.LogError($"{GetType()}::아이템 로드 실패: {e.Message}");
        }
    }

    private void SaveItems()
    {
        var userInventoryData = UserDataManager.Instance.GetUserData<UserInventoryData>();
        if (userInventoryData == null)
            return;
        
        userInventoryData.EquippedItemIdList = _equippedItemList.Select(x => x.id).ToList();
        userInventoryData.SaveData();
    }

    private void OnDisable()
    {
        PlayerCustom.Instance.character.GetComponent<ObjRotator>().enabled = false;
    }

    private bool OnClickItem(InventoryItem item)
    {
        if (_equippedItemList.Contains(item))
        {
            _equippedItemList.Remove(item);
            SortSlots();
            return false;
        }
        
        if (_equippedItemList.Count >= slotList.Count)
        {
            return false;
        }
            
        _equippedItemList.Add(item);
        SortSlots();
        return true;
    }

    private void SortSlots()
    {
        for (int i = 0; i < slotList.Count; i++)
        {
            if (i < _equippedItemList.Count)
            {
                slotList[i].ApplyItem(_equippedItemList[i]);
            }
            else
            {
                slotList[i].ClearSlot();
            }
        }
    }
}
