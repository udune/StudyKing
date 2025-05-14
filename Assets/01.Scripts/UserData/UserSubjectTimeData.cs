using System;
using System.Collections.Generic;
using UnityEngine;
using Logger = Common.Logger;

[Serializable]
public class SubjectTimeItemData
{
    public String Name;
    public long Time;
}

public class UserSubjectTimeData : IUserData
{
    public bool IsLoaded { get; set; }
    
    public List<SubjectTimeItemData> SubjectTimeItemDataList = new List<SubjectTimeItemData>();
    
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
        
        FirebaseManager.Instance.LoadUserData<UserSubjectTimeData>(() =>
        {
            IsLoaded = true;
        });
    }

    public void SaveData()
    {
        Logger.Log($"{GetType()}::SaveData");
        
        FirebaseManager.Instance.SaveUserData<UserSubjectTimeData>(ConvertToFirestore());
    }

    private void ConvertToData(Dictionary<string, object> firestoreDict)
    {
        if (firestoreDict.TryGetValue("SubjectTimeItemDataList", out var obj) &&
            obj is List<object> subjectTimeItemDataList)
        {
            SubjectTimeItemDataList.Clear();
            
            foreach (var itemData in subjectTimeItemDataList)
            {
                if (itemData is Dictionary<string, object> itemDataDict)
                {
                    SubjectTimeItemData subjectTimeItemData = new SubjectTimeItemData();

                    if (itemDataDict.TryGetValue("Name", out var nameValue) && nameValue is string name)
                    {
                        subjectTimeItemData.Name = name;   
                    }

                    if (itemDataDict.TryGetValue("Time", out var timeValue) && timeValue is long time)
                    {
                        subjectTimeItemData.Time = time;
                    }
                    
                    SubjectTimeItemDataList.Add(subjectTimeItemData);
                }
            }
        }
    }

    private Dictionary<string, object> ConvertToFirestore()
    {
        Dictionary<string, object> result = new Dictionary<string, object>();
        List<Dictionary<string, object>> convertedSubjectTimeItemDataList = new List<Dictionary<string, object>>();
        foreach (var itemData in SubjectTimeItemDataList)
        {
            var convertedDict = new Dictionary<string, object>()
            {
                { "Name", itemData.Name },
                { "Time", itemData.Time }
            };
            
            convertedSubjectTimeItemDataList.Add(convertedDict);
        }
        
        result["SubjectTimeItemDataList"] = convertedSubjectTimeItemDataList;
        
        return result;
    }
}
