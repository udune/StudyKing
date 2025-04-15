using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Logger = Common.Logger;

public class UserTimeData : IUserData
{
    public bool IsLoaded { get; set; }
    
    public long Time { get; set; }

    public void Initialize()
    {
        Logger.Log($"{GetType()}::Setting");
        Time = 0;
    }

    public void Setting(Dictionary<string, object> firestoreDict)
    {
        ConvertToData(firestoreDict);
    }

    public void LoadData()
    {
        Logger.Log($"{GetType()}::LoadData");
        
        FirebaseManager.Instance.LoadUserData<UserTimeData>(() =>
        {
            IsLoaded = true;
        });
    }

    public void SaveData()
    {
        Logger.Log($"{GetType()}::SaveData");
        
        FirebaseManager.Instance.SaveUserData<UserTimeData>(ConvertToFirestore());
    }

    private void ConvertToData(Dictionary<string, object> firestoreDict)
    {
        Time = (long)firestoreDict["Time"];
    }

    private Dictionary<string, object> ConvertToFirestore()
    {
        Dictionary<string, object> result = new Dictionary<string, object>()
        {
            { "Time", Time }
        };
        
        return result;
    }
}
