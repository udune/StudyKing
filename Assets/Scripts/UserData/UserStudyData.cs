using System;
using System.Collections.Generic;
using UnityEngine;
using Logger = Common.Logger;

[Serializable]
public class StudyItemData
{
    public string Name;

    public StudyItemData(string name)
    {
        Name = name;
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
        
        StudyItemDataList.Add(new StudyItemData("수학"));
        StudyItemDataList.Add(new StudyItemData("영어"));
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
                    Logger.Log($"Name:{studyItemData.Name}");
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
            
            Logger.Log($"StudyItemDataList");
            foreach (var studyItemData in StudyItemDataList)
            {
                Logger.Log($"Name:{studyItemData.Name}");
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
