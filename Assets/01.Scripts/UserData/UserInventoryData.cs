using System;
using System.Collections.Generic;
using UnityEngine;
using Logger = Common.Logger;

public class UserInventoryData : IUserData
{
    public bool IsLoaded { get; set; }
    
    public List<string> EquippedItemIdList { get; set; } = new List<string>();
    
    public void Initialize()
    {
        Logger.Log($"{GetType()}::Initialize");
    }

    public void Setting(Dictionary<string, object> firestoreDict)
    {
        Logger.Log($"{GetType()}::Setting");
        ConvertToData(firestoreDict);
    }

    public void LoadData()
    {
        Logger.Log($"{GetType()}::LoadData");
        FirebaseManager.Instance.LoadUserData<UserInventoryData>(() =>
        {
            IsLoaded = true;
        });
    }

    public void SaveData()
    {
        Logger.Log($"{GetType()}::SaveData");
        FirebaseManager.Instance.SaveUserData<UserInventoryData>(ConvertToFirestore());
    }

    private void ConvertToData(Dictionary<string, object> firestoreDict)
    {
        if (firestoreDict.TryGetValue("EquippedItemIdList", out var obj) &&
            obj is List<object> equippedItemIdList)
        {
            EquippedItemIdList.Clear();

            foreach (var itemId in equippedItemIdList)
            {
                if (itemId is string id)
                {
                    EquippedItemIdList.Add(id);
                }
            }
        }
    }

    private Dictionary<string, object> ConvertToFirestore()
    {
        Dictionary<string, object> result = new Dictionary<string, object>();
        
        result["EquippedItemIdList"] = EquippedItemIdList;
        
        return result;
    }
}
