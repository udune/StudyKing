using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Logger = Common.Logger;

[Serializable]
public class StudyItemData
{
    public int Id;
    public string Name;
    public bool Check;
}

public class UserStudyData : IUserData
{
    public bool IsLoaded { get; set; }
    
    public List<StudyItemData> StudyItemDataList { get; set; } = new List<StudyItemData>();

    public void Initialize()
    {
        Logger.Log($"{GetType()}::Initialize");
        
        StudyItemDataList.Add(new StudyItemData { Id = 1, Name = "수학", Check = false });
        StudyItemDataList.Add(new StudyItemData { Id = 2, Name = "영어", Check = false });
    }

    public void Setting(Dictionary<string, object> firestoreDict)
    {
        Logger.Log($"{GetType()}::Setting");
        ConvertToData(firestoreDict);
    }

    public void LoadData()
    {
        Logger.Log($"{GetType()}::LoadData");
        
        FirebaseManager.Instance.LoadUserData<UserStudyData>(() =>
        {
            IsLoaded = true;
        });
    }

    public void SaveData()
    {
        Logger.Log($"{GetType()}::SaveData");
        
        FirebaseManager.Instance.SaveUserData<UserStudyData>(ConvertToFirestore());
    }

    private void ConvertToData(Dictionary<string, object> firestoreDict)
    {
        if (firestoreDict.TryGetValue("StudyItemDataList", out var obj) && obj is List<object> studyItemDataList)
        {
            StudyItemDataList.Clear();
            
            foreach (var itemData in studyItemDataList)
            {
                if (itemData is Dictionary<string, object> itemDataDict)
                {
                    StudyItemData studyItemData = new StudyItemData();

                    if (itemDataDict.TryGetValue("Id", out var idValue) && idValue != null)
                    {
                        studyItemData.Id = Convert.ToInt32(idValue);
                    }

                    if (itemDataDict.TryGetValue("Name", out var nameValue) && nameValue is string name)
                    {
                        studyItemData.Name = name;
                    }

                    if (itemDataDict.TryGetValue("Check", out var checkValue) && checkValue is bool check)
                    {
                        studyItemData.Check = check;
                    }
                    
                    StudyItemDataList.Add(studyItemData);
                }
            }
        }
    }

    private Dictionary<string, object> ConvertToFirestore()
    {
        Dictionary<string, object> result = new Dictionary<string, object>();
        List<Dictionary<string, object>> convertedStudyItemDataList = new List<Dictionary<string, object>>();
        foreach (var itemData in StudyItemDataList)
        {
            var convertedDict = new Dictionary<string, object>()
            {
                { "Id", itemData.Id },
                { "Name", itemData.Name },
                { "Check", itemData.Check },
            };

            convertedStudyItemDataList.Add(convertedDict);
        }
        
        result["StudyItemDataList"] = convertedStudyItemDataList;
        
        return result;
    }
}
