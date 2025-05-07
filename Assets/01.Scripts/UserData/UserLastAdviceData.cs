using System;
using System.Collections.Generic;
using UnityEngine;
using Logger = Common.Logger;

public class UserLastAdviceData : IUserData
{
    public bool IsLoaded { get; set; }

    public string Date { get; set; } = "";
    public string Advice { get; set; } = "";
    
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
        
        FirebaseManager.Instance.LoadUserData<UserLastAdviceData>(() =>
        {
            IsLoaded = true;
        });
    }

    public void SaveData()
    {
        Logger.Log($"{GetType()}::SaveData");
        
        FirebaseManager.Instance.SaveUserData<UserLastAdviceData>(ConvertToFirestore());
    }

    private void ConvertToData(Dictionary<string, object> firestoreDict)
    {
        if (firestoreDict.TryGetValue("Date", out var objDate) && objDate is string date)
        {
            Date = date;
        }

        if (firestoreDict.TryGetValue("Advice", out var objAdvice) && objAdvice is string advice)
        {
            Advice = advice;
        }
    }

    private Dictionary<string, object> ConvertToFirestore()
    {
        Dictionary<string, object> result = new Dictionary<string, object>()
        {
            { "Date", Date },
            { "Advice", Advice }
        };
        
        return result;
    }
}
