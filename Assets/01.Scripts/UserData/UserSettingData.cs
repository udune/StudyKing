using System;
using System.Collections.Generic;
using UnityEngine;
using Logger = Common.Logger;

public class UserSettingData : IUserData
{
    public bool IsLoaded { get; set; }

    public bool SoundEnabled { get; set; } = true;
    public bool NotificationEnabled { get; set; } = true;
    public float SoundVolume { get; set; } = 1.0f;
    public string Language { get; set; } = "ko";
    public bool AutoSaveEnabled { get; set; } = true;

    public void Initialize()
    {
        Logger.Log($"{GetType()}::Initialize");
        
        SoundEnabled = true;
        NotificationEnabled = true;
        SoundVolume = 1.0f;
        Language = "ko";
        AutoSaveEnabled = true;
    }

    public void Setting(Dictionary<string, object> firestoreDict)
    {
        Logger.Log($"{GetType()}::Setting");

        if (firestoreDict == null)
        {
            Logger.LogWarning($"{GetType()}::Setting firestoreDict is null");
            Initialize();
            return;
        }

        try
        {
            ConvertToData(firestoreDict);
        }
        catch (Exception e)
        {
            Logger.LogError($"{GetType()}::Setting Exception : {e}");
            Initialize();
        }
    }

    public void LoadData()
    {
        Logger.Log($"{GetType()}::LoadData");

        try
        {
            FirebaseManager.Instance.LoadUserData<UserSettingData>(() =>
            {
                IsLoaded = true;
                Logger.Log($"{GetType()}::LoadData IsLoaded");
            });
        }
        catch (Exception e)
        {
            Logger.LogError($"{GetType()}::LoadData Exception : {e}");
            IsLoaded = true;
        }
    }

    public void SaveData()
    {
        Logger.Log($"{GetType()}::SaveData");

        try
        {
            FirebaseManager.Instance.SaveUserData<UserSettingData>(ConvertToFirestore());
        }
        catch (Exception e)
        {
            Logger.LogError($"{GetType()}::SaveData Exception : {e}");
        }
    }
    
    private void ConvertToData(Dictionary<string, object> firestoreDict)
    {
        if (firestoreDict.TryGetValue("SoundEnabled", out var soundEnabledObj) && soundEnabledObj is bool soundEnabledValue)
        {
            SoundEnabled = soundEnabledValue;
        }

        if (firestoreDict.TryGetValue("SoundVolume", out var soundVolumeObj))
        {
            if (soundVolumeObj is double doubleVolume)
            {
                SoundVolume = (float)doubleVolume;
            }
            else if (soundVolumeObj is float floatVolume)
            {
                SoundVolume = floatVolume;
            }
        }

        if (firestoreDict.TryGetValue("NotificationEnabled", out var notificationEnabledObj) &&
            notificationEnabledObj is bool notificationEnabledValue)
        {
            NotificationEnabled = notificationEnabledValue;
        }
        
        if (firestoreDict.TryGetValue("Language", out var languageObj) && languageObj is string languageValue)
        {
            Language = languageValue;
        }
        
        if (firestoreDict.TryGetValue("AutoSaveEnabled", out var autoSaveEnabledObj) &&
            autoSaveEnabledObj is bool autoSaveEnabledValue)
        {
            AutoSaveEnabled = autoSaveEnabledValue;
        }
        
        Logger.Log($"{GetType()}::Setting Done");
    }
    
    private Dictionary<string, object> ConvertToFirestore()
    {
        var result = new Dictionary<string, object>()
        {
            { "SoundEnabled", SoundEnabled },
            { "SoundVolume", SoundVolume },
            { "NotificationEnabled", NotificationEnabled },
            { "Language", Language },
            { "AutoSaveEnabled", AutoSaveEnabled }
        };
        
        Logger.Log($"{GetType()}::ConvertToFirestore Done");
        return result;
    }

    public void SetSoundEnabled(bool enabled)
    {
        if (SoundEnabled != enabled)
        {
            SoundEnabled = enabled;
            SaveData();
            Logger.Log($"{GetType()}::SetSoundEnabled Done");
        }
    }

    public void SetSoundVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);
        if (Math.Abs(SoundVolume - volume) > 0.001f)
        {
            SoundVolume = volume;
            SaveData();
            Logger.Log($"{GetType()}::SetSoundVolume Done");
        }
    }

    public void SetNotificationEnabled(bool enabled)
    {
        if (NotificationEnabled != enabled)
        {
            NotificationEnabled = enabled;
            SaveData();
            Logger.Log($"{GetType()}::SetNotificationEnabled Done");
        }
    }
    
    public void SetLanguage(string language) 
    {
        if (!string.IsNullOrEmpty(language) && Language != language)
        {
            Language = language;
            SaveData();
            Logger.Log($"{GetType()}::SetLanguage Done");
        }
    }

    public void SetAutoSaveEnabled(bool enabled)
    {
        if (AutoSaveEnabled != enabled)
        {
            AutoSaveEnabled = enabled;
            SaveData();
            Logger.Log($"{GetType()}::SetAutoSaveEnabled Done");
        }
    }
}
