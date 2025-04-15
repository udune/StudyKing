using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UserDataManager : SingletonBehaviour<UserDataManager>
{
    public bool IsExistSaveData { get; private set; }
    public List<IUserData> UserDataList { get; private set; } = new List<IUserData>();

    protected override void Init()
    {
        base.Init();
        
        UserDataList.Add(new UserTimeData());
        UserDataList.Add(new UserSettingData());
        UserDataList.Add(new UserStudyData());
    }

    public void SettingUserData()
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
        bool error = false;
        foreach (var userData in UserDataList)
        {
            userData.SaveData();
        }
    }

    public T GetUserData<T>() where T : class, IUserData
    {
        return UserDataList.OfType<T>().FirstOrDefault();
    }
}
