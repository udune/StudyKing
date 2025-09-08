using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Logger = Common.Logger;

public enum SceneType
{
    Title,
    Account,
    Lobby,
}

public class SceneLoader : SingletonBehaviour<SceneLoader>
{
    public void LoadScene(SceneType sceneType)
    {
        Logger.Log($"{sceneType.ToString()} loading scene...");
        Time.timeScale = 1;
        SceneManager.LoadScene(sceneType.ToString());
    }

    public void ReloadScene()
    {
        Logger.Log($"{SceneManager.GetActiveScene().name} reloading scene...");
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public AsyncOperation LoadSceneAsync(SceneType sceneType)
    {
        Logger.Log($"{sceneType.ToString()} loading async scene...");
        Time.timeScale = 1;
        return SceneManager.LoadSceneAsync(sceneType.ToString());
    }

    public SceneType GetCurrentScene()
    {
        try
        {
            string sceneName = SceneManager.GetActiveScene().name;
            if (Enum.TryParse<SceneType>(sceneName, out SceneType result))
            {
                return result;
            }
            else
            {
                Logger.LogWarning($"{GetType()}::Unknown scene name: {sceneName}, defaulting to Title");
                return SceneType.Title;
            }
        }
        catch (Exception e)
        {
            Logger.LogError($"{GetType()}::Error getting current scene: {e.Message}");
            return SceneType.Title;
        }
    }
}
