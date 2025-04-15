using System;
using System.Collections.Generic;
using Logger = Common.Logger;

public class UserSettingData : IUserData
{
    public bool IsLoaded { get; set; }

    public void Initialize()
    {
        Logger.Log($"{GetType()}::Setting");
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
        
    }
    
    private Dictionary<string, object> ConvertToFirestore()
    {
        Dictionary<string, object> result = new Dictionary<string, object>()
        {
        };
        
        return result;
    }
}
