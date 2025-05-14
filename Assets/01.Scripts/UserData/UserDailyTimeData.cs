using System;
using System.Collections.Generic;
using UnityEngine;
using Logger = Common.Logger;

[Serializable]
public class DailyTimeItemData
{
    public string Date;
    public long Time;
}

public class UserDailyTimeData : IUserData
{
    public bool IsLoaded { get; set; }
    
    public List<DailyTimeItemData> DailyTimeItemDataList = new List<DailyTimeItemData>();
    
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
        
        FirebaseManager.Instance.LoadUserData<UserDailyTimeData>(() =>
        {
            IsLoaded = true;
        });
    }

    public void SaveData()
    {
        Logger.Log($"{GetType()}::SaveData");
        
        FirebaseManager.Instance.SaveUserData<UserDailyTimeData>(ConvertToFirestore());
    }

    private void ConvertToData(Dictionary<string, object> firestoreDict)
    {
        if (firestoreDict.TryGetValue("DailyTimeItemDataList", out var obj) &&
            obj is List<object> dailyTimeItemDataList)
        {
            DailyTimeItemDataList.Clear();
            
            foreach (var itemData in dailyTimeItemDataList)
            {
                if (itemData is Dictionary<string, object> itemDataDict)
                {
                    DailyTimeItemData dailyTimeItemData = new DailyTimeItemData();

                    if (itemDataDict.TryGetValue("Date", out var dateValue) && dateValue is string date)
                    {
                        dailyTimeItemData.Date = date;
                    }

                    if (itemDataDict.TryGetValue("Time", out var timeValue) && timeValue is long time)
                    {
                        dailyTimeItemData.Time = time;
                    }
                    
                    DailyTimeItemDataList.Add(dailyTimeItemData);
                }
            }
        }
    }

    private Dictionary<string, object> ConvertToFirestore()
    {
        Dictionary<string, object> result = new Dictionary<string, object>();
        List<Dictionary<string, object>> convertedDailyTimeItemDataList = new List<Dictionary<string, object>>();
        foreach (var itemData in DailyTimeItemDataList)
        {
            var convertedDict = new Dictionary<string, object>()
            {
                { "Date", itemData.Date },
                { "Time", itemData.Time }
            };
            
            convertedDailyTimeItemDataList.Add(convertedDict);
        }
        
        result["DailyTimeItemDataList"] = convertedDailyTimeItemDataList;
        
        return result;
    }
}
