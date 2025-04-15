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
        Logger.Log($"{GetType()}::Setting");
        
        StudyItemDataList.Add(new StudyItemData { Id = 1, Name = "수학", Check = false });
        StudyItemDataList.Add(new StudyItemData { Id = 2, Name = "영어", Check = false });
    }

    public void Setting(Dictionary<string, object> firestoreDict)
    {
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

    private void ConvertToData(Dictionary<string, object> dict)
    {
        if (dict.TryGetValue("StudyItemDataList", out object obj) && obj is List<object> studyItemDataList)
        {
            foreach (var itemData in studyItemDataList)
            {
                if (itemData is Dictionary<string, object> itemDataDict)
                {
                    StudyItemData studyItemData = new StudyItemData
                    {
                        Id = Convert.ToInt32(itemDataDict["Id"]),
                        Name = itemDataDict["Name"].ToString(),
                        Check = (bool) itemDataDict["Check"]
                    };
                    
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
