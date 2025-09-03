using System;
using System.Collections.Generic;
using System.Linq;
using _01.Scripts.UserData;
using Logger = Common.Logger;

public class UserDataManager : SingletonBehaviour<UserDataManager>, IUserDataManager
{
    public bool IsExistSaveData { get; set; }
    private List<IUserData> UserDataList { get; set; } = new List<IUserData>();

    protected override void Init()
    {
        base.Init();
        
        UserDataList.Clear();
        
        UserDataList.Add(new UserTimeData());
        UserDataList.Add(new UserSubjectTimeData());
        UserDataList.Add(new UserDailyTimeData());
        UserDataList.Add(new UserSettingData());
        UserDataList.Add(new UserStudyData());
        UserDataList.Add(new UserHistoryData());
        UserDataList.Add(new UserLastAdviceData());
        UserDataList.Add(new UserInventoryData());
    }

    public void InitializeUserData()
    {
        foreach (var userData in UserDataList)
        {
            userData.Initialize();
        }
    }

    public void LoadUserData()
    {
        foreach (var userData in UserDataList)
        {
            userData.LoadData();
        }
    }

    public void SaveUserData()
    {
        foreach (var userData in UserDataList)
        {
            userData.SaveData();
        }
    }
    
    public void SaveUserSettingData(UserSettingsData settingData)
    {
        settingData?.SaveData();
    }

    public T GetUserData<T>() where T : class, IUserData
    {
        try
        {
            var result = UserDataList?.OfType<T>().FirstOrDefault();
            if (result == null)
            {
                Logger.LogWarning($"{GetType()}:: UserData is not found");
            }
            return result;
        }
        catch (Exception)
        {
            Logger.LogError($"{GetType()}:: Error in GetUserData");
            return null;
        }
    }

    public bool IsUserDataLoaded()
    {
        foreach (var data in UserDataList)
        {
            if (!data.IsLoaded)
            {
                return false;
            }
        }

        return true;
    }
    
    public void ClearAllUserData() => UserDataList.Clear();
}