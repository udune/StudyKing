using System;
using System.Collections.Generic;
using Logger = Common.Logger;

[Serializable]
public class StudyItemData
{
    public int id;
    public string name;
    public bool check;
}

public class UserStudyData : IUserData
{
    public bool IsLoaded { get; set; }
    
    public List<StudyItemData> StudyItemDataList { get; set; } = new List<StudyItemData>();

    public void Initialize()
    {
        Logger.Log($"{GetType()}::Initialize");
        
        StudyItemDataList.Add(new StudyItemData { id = 1, name = "수학", check = false });
        StudyItemDataList.Add(new StudyItemData { id = 2, name = "영어", check = false });
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
                        studyItemData.id = Convert.ToInt32(idValue);
                    }

                    if (itemDataDict.TryGetValue("Name", out var nameValue) && nameValue is string name)
                    {
                        studyItemData.name = name;
                    }

                    if (itemDataDict.TryGetValue("Check", out var checkValue) && checkValue is bool check)
                    {
                        studyItemData.check = check;
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
                { "Id", itemData.id },
                { "Name", itemData.name },
                { "Check", itemData.check },
            };

            convertedStudyItemDataList.Add(convertedDict);
        }
        
        result["StudyItemDataList"] = convertedStudyItemDataList;
        
        return result;
    }
}
