#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using Logger = Common.Logger;

public enum BuildType
{
    DEV,
    TEST,
    REAL
}

public class BuildManager : Editor
{
    public const string DEV_SCRIPTING_DEFINE_SYMBOLS = "DEV_VER";
    public const string REAL_SCRIPTING_DEFINE_SYMBOLS = "";

    private static BuildType BuildType = BuildType.DEV;
    
    [MenuItem("Builds/Set AOS DEV Build Settings")]
    public static void SetAOSDevBuildSettings()
    {
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
        EditorUserBuildSettings.buildAppBundle = false;
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, DEV_SCRIPTING_DEFINE_SYMBOLS);
        
        BuildType = BuildType.DEV;
    }

    [MenuItem("Builds/Set AOS TEST Build Settings")]
    public static void SetAOSTestBuildSettings()
    {
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
        EditorUserBuildSettings.buildAppBundle = true;
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, DEV_SCRIPTING_DEFINE_SYMBOLS);
        
        BuildType = BuildType.TEST;
    }

    [MenuItem("Builds/Set AOS REAL Build Settings")]
    public static void SetAOSRealBuildSettings()
    {
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
        EditorUserBuildSettings.buildAppBundle = true;
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, REAL_SCRIPTING_DEFINE_SYMBOLS);
        
        BuildType = BuildType.REAL;
    }

    [MenuItem("Builds/Start AOS Build")]
    public static void StartAOSBuild()
    {
        PlayerSettings.Android.keystoreName = "Builds/AOS/minchankim.keystore";
        PlayerSettings.Android.keystorePass = "alscks3507";
        PlayerSettings.Android.keyaliasName = "minchankim";
        PlayerSettings.Android.keyaliasPass = "alscks3507";
        
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = new[]
        { 
            "Assets/Scenes/Title.unity",
            "Assets/Scenes/Account.unity",
            "Assets/Scenes/Lobby.unity",
        };
        buildPlayerOptions.target = BuildTarget.Android;
        string fileExtension = string.Empty;
        BuildOptions compressOption = BuildOptions.None;

        switch (BuildType)
        {
            case BuildType.DEV:
                fileExtension = "apk";
                compressOption = BuildOptions.CompressWithLz4;
                break;
            case BuildType.TEST:
            case BuildType.REAL:
                fileExtension = "aab";
                compressOption = BuildOptions.CompressWithLz4HC;
                break;
        }
        
        buildPlayerOptions.locationPathName = $"Builds/AOS/StudyKing_{Application.version}_{DateTime.Now:yyMMdd_HHmmss}.{fileExtension}";
        buildPlayerOptions.options = compressOption;
        
        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;
        if (summary.result == BuildResult.Succeeded)
        {
            Logger.Log($"Build succeeded: {summary.totalSize} bytes");
        }
        else if (summary.result == BuildResult.Failed)
        {
            Logger.LogError("Build failed");
        }
    }
}
#endif