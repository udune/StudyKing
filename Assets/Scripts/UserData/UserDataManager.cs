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
    }

    public void SettingUserData()
    {
        foreach (var userData in UserDataList)
        {
            userData.Setting();
        }
    }

    public void LoadUserData()
    {
        IsExistSaveData = PlayerPrefs.GetInt("IsExistSaveData") == 1;
        if (IsExistSaveData)
        {
            foreach (var userData in UserDataList)
            {
                userData.LoadData();
            }
        }
    }

    public void SaveUserData()
    {
        bool error = false;
        foreach (var userData in UserDataList)
        {
            bool isSaveSuccess = userData.SaveData();
            if (!isSaveSuccess)
            {
                error = true;
            }
        }

        if (!error)
        {
            IsExistSaveData = true;
            PlayerPrefs.SetInt("IsExistSaveData", 1);
            PlayerPrefs.Save();
        }
    }

    public T GetUserData<T>() where T : class, IUserData
    {
        return UserDataList.OfType<T>().FirstOrDefault();
    }
}
