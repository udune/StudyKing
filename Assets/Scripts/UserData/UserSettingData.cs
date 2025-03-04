using System;
using Logger = Common.Logger;

public class UserSettingData : IUserData
{
    public void Setting()
    {
        Logger.Log($"{GetType()}::Setting");
    }

    public bool LoadData()
    {
        Logger.Log($"{GetType()}::LoadData");
        bool result = false;
        try
        {
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
            result = true;
        }
        catch (Exception e)
        {
            Logger.Log($"{GetType()}::Save Failed: {e.Message}");
        }

        return result;
    }
}
