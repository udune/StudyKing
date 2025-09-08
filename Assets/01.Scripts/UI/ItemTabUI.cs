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

        // 저장된 장착 아이템 데이터를 먼저 로드
        LoadItems();
        
        // UI 초기화
        InitializeItems();
        
        // UI 상태 복원 (장착된 아이템들의 선택 표시)
        RestoreItemStates();
    }
    
    private void InitializeItems()
    {
        // UI가 재활성화될 때마다 아이템이 중복 생성되는 것을 방지
        if (content.childCount > 0)
        {
            Logger.Log($"{GetType()}::아이템이 이미 생성되어 있음, 건너뜀");
            return; 
        }
    
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
        
        Logger.Log($"{GetType()}::총 {inventoryItemList.Count}개의 아이템 버튼 생성 완료");
    }
    
    /// <summary>
    /// 저장된 데이터를 바탕으로 UI 상태를 복원하는 메서드
    /// </summary>
    private void RestoreItemStates()
    {
        try
        {
            // 장착된 아이템들의 UI 상태를 복원
            foreach (Transform child in content)
            {
                RestoreItemState(child.gameObject);
            }
            
            // 슬롯 상태도 업데이트
            SortSlots();
            
            Logger.Log($"{GetType()}::UI 상태 복원 완료 - 장착된 아이템: {_equippedItemList.Count}개");
        }
        catch (Exception e)
        {
            Logger.LogError($"{GetType()}::UI 상태 복원 실패: {e.Message}");
        }
    }
    
    /// <summary>
    /// 개별 아이템의 UI 상태를 복원하는 메서드
    /// </summary>
    private void RestoreItemState(GameObject itemObject)
    {
        try
        {
            // 게임오브젝트 이름에서 아이템 ID 추출 (형식: "Item_아이템ID")
            if (itemObject.name.StartsWith("Item_"))
            {
                string itemId = itemObject.name.Substring(5); // "Item_" 제거
                var item = inventoryItemList.Find(x => x.id == itemId);
                
                if (item != null)
                {
                    bool isEquipped = _equippedItemList.Any(x => x.id == item.id);
                    SetItemSelected(itemObject, isEquipped);
                    
                    if (isEquipped)
                    {
                        PlayerCustom.Instance?.Equip(item.id);
                        Logger.Log($"{GetType()}::아이템 {item.id} 장착 상태 복원");
                    }
                }
                else
                {
                    Logger.LogWarning($"{GetType()}::아이템 ID {itemId}를 inventoryItemList에서 찾을 수 없음");
                }
            }
        }
        catch (Exception e)
        {
            Logger.LogError($"{GetType()}::개별 아이템 상태 복원 실패: {e.Message}");
        }
    }
    
    private void CreateItemButton(InventoryItem item)
    {
        if (item == null) return;
    
        var go = Instantiate(itemPrefab, content);
        if (go == null) return;
        
        // 아이템 ID를 게임오브젝트 이름에 저장하여 나중에 찾기 쉽게 함
        go.name = $"Item_{item.id}";
    
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
