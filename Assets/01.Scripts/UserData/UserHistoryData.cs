using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Logger = Common.Logger;

[Serializable]
public class HistoryItemData
{
    public string Date;
    public List<string> SubjectList;
}

public class UserHistoryData : IUserData
{
    public bool IsLoaded { get; set; }

    public List<HistoryItemData> HistoryItemDataList { get; set; } = new List<HistoryItemData>();
    
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
        
        FirebaseManager.Instance.LoadUserData<UserHistoryData>(() =>
        {
            IsLoaded = true;
        });
    }

    public void SaveData()
    {
        Logger.Log($"{GetType()}::SaveData");
        
        FirebaseManager.Instance.SaveUserData<UserHistoryData>(ConvertToFirestore());
    }
    
    private void ConvertToData(Dictionary<string, object> firestoreDict)
    {
        if (firestoreDict.TryGetValue("HistoryItemDataList", out var obj) && obj is List<object> historyItemDataList)
        {
            HistoryItemDataList.Clear();
            
            foreach (var itemData in historyItemDataList)
            {
                if (itemData is Dictionary<string, object> itemDataDict)
                {
                    HistoryItemData historyItemData = new HistoryItemData();

                    if (itemDataDict.TryGetValue("Date", out var dateValue) && dateValue is string date)
                    {
                        historyItemData.Date = date;
                    }

                    if (itemDataDict.TryGetValue("SubjectList", out var subjectListValue) && subjectListValue is List<object> subjectList)
                    {
                        historyItemData.SubjectList = subjectList.OfType<string>().ToList();
                    }
                    else
                    {
                        historyItemData.SubjectList = new List<string>();
                    }
                    
                    HistoryItemDataList.Add(historyItemData);
                }
            }
        }
    }
    
    private Dictionary<string, object> ConvertToFirestore()
    {
        Dictionary<string, object> result = new Dictionary<string, object>();
        List<Dictionary<string, object>> convertedHistoryItemDataList = new List<Dictionary<string, object>>();
        foreach (var itemData in HistoryItemDataList)
        {
            var convertedDict = new Dictionary<string, object>()
            {
                { "Date", itemData.Date },
                { "SubjectList", itemData.SubjectList }
            };
            
            convertedHistoryItemDataList.Add(convertedDict);
        }
        
        result["HistoryItemDataList"] = convertedHistoryItemDataList;
        
        return result;
    }
}
