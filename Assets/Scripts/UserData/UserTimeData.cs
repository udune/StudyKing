using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Logger = Common.Logger;

public class UserTimeData : IUserData
{
    public long Time { get; set; }
    
    public void Setting()
    {
        Logger.Log($"{GetType()}::Setting");
        Time = 0;
    }

    public bool LoadData()
    {
        Logger.Log($"{GetType()}::LoadData");
        bool result = false;

        try
        {
            Time = long.Parse(PlayerPrefs.GetString("Time"));
            PlayerPrefs.Save();

            result = true;
            
            Logger.Log($"{GetType()}::Time: {Time}");
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
            PlayerPrefs.SetString("Time", Time.ToString());
            result = true;
            
            Logger.Log($"{GetType()}::Time: {Time}");
        }
        catch (Exception e)
        {
            Logger.Log($"{GetType()}::Save Failed: {e.Message}");
        }

        return result;
    }
}
