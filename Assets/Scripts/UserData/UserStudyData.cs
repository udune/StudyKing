using System;
using System.Collections.Generic;
using UnityEngine;
using Logger = Common.Logger;

[Serializable]
public class StudyItemData
{
    public int Id;
    public string Name;
    public bool Check;

    public StudyItemData(int id, string name, bool check)
    {
        Id = id;
        Name = name;
        Check = check;
    }
}

[Serializable]
public class StudyItemDataListWrapper
{
    public List<StudyItemData> StudyItemDataList;
}

public class UserStudyData : IUserData
{
    public List<StudyItemData> StudyItemDataList { get; set; } = new List<StudyItemData>();
    
    public void Setting()
    {
        Logger.Log($"{GetType()}::Setting");
        
        StudyItemDataList.Add(new StudyItemData(1, "수학", false));
        StudyItemDataList.Add(new StudyItemData(2, "영어", false));
    }

    public bool LoadData()
    {
        Logger.Log($"{GetType()}::LoadData");
        bool result = false;

        try
        {
            string studyItemDataListJson = PlayerPrefs.GetString("StudyItemDataList");
            if (!string.IsNullOrEmpty(studyItemDataListJson))
            {
                StudyItemDataListWrapper studyItemDataListWrapper = JsonUtility.FromJson<StudyItemDataListWrapper>(studyItemDataListJson);
                StudyItemDataList = studyItemDataListWrapper.StudyItemDataList;
                
                Logger.Log($"{GetType()}::StudyItemDataList");
                foreach (var studyItemData in StudyItemDataList)
                {
                    Logger.Log($"Id:{studyItemData.Id}, Name:{studyItemData.Name}, Check:{studyItemData.Check}");
                }
            }

            result = true;
        }
        catch (Exception e)
        {
            Logger.Log($"{GetType()}::Load Failed: {e.Message}");
        }

        return result;
    }

    public bool SaveData()
    {
        Logger.Log($"{GetType()}::SaveData");
        bool result = false;

        try
        {
            StudyItemDataListWrapper studyItemDataListWrapper = new StudyItemDataListWrapper();
            studyItemDataListWrapper.StudyItemDataList = StudyItemDataList;
            string studyItemDataListJson = JsonUtility.ToJson(studyItemDataListWrapper);
            PlayerPrefs.SetString("StudyItemDataList", studyItemDataListJson);
            
            Logger.Log($"{GetType()}::StudyItemDataList");
            foreach (var studyItemData in StudyItemDataList)
            {
                Logger.Log($"Id:{studyItemData.Id}, Name:{studyItemData.Name} Check:{studyItemData.Check}");
            }
            
            result = true;
        }
        catch (Exception e)
        {
            Logger.Log($"{GetType()}::Save Failed: {e.Message}");
        }
        
        return result;
    }
}
