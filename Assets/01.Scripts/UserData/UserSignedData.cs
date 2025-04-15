using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Logger = Common.Logger;

public class UserSignedData : IUserData
{
    public bool IsLoaded { get; set; }
    
    public bool HasSignedWithGoogle { get; set; }
    public bool HasSignedWithApple { get; set; }
    
    public void Initialize()
    {
        Logger.Log($"{GetType()}::Initialize");
        HasSignedWithGoogle = false;
        HasSignedWithApple = false;
    }

    public void Setting(Dictionary<string, object> firestoreDict)
    {
        Logger.Log($"{GetType()}::Setting");
        ConvertToData(firestoreDict);
    }

    public void LoadData()
    {
        Logger.Log($"{GetType()}::LoadData");
        
        FirebaseManager.Instance.LoadUserData<UserSignedData>(() =>
        {
            IsLoaded = true;
        });
    }

    public void SaveData()
    {
        Logger.Log($"{GetType()}::SaveData");
        
        FirebaseManager.Instance.SaveUserData<UserSignedData>(ConvertToFirestore());
    }
    
    private void ConvertToData(Dictionary<string, object> firestoreDict)
    {
        HasSignedWithGoogle = (bool)firestoreDict["HasSignedWithGoogle"];
        HasSignedWithApple = (bool)firestoreDict["HasSignedWithApple"];
    }
    
    private Dictionary<string, object> ConvertToFirestore()
    {
        Dictionary<string, object> result = new Dictionary<string, object>()
        {
            { "HasSignedWithGoogle", HasSignedWithGoogle },
            { "HasSignedWithApple", HasSignedWithApple }
        };
        
        return result;
    }
}
