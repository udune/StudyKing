#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using Logger = Common.Logger;

public enum BuildType
{
    Dev,
    Test,
    Real
}

public class BuildManager : Editor
{
    private const string DevScriptingDefineSymbols = "DEV_VER";
    private const string RealScriptingDefineSymbols = "";

    private static BuildType _buildType = BuildType.Dev;
    
    [MenuItem("Builds/Set AOS DEV Build Settings")]
    public static void SetAosDevBuildSettings()
    {
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
        EditorUserBuildSettings.buildAppBundle = false;
        PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.Android, DevScriptingDefineSymbols);
        
        _buildType = BuildType.Dev;
    }

    [MenuItem("Builds/Set AOS TEST Build Settings")]
    public static void SetAosTestBuildSettings()
    {
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
        EditorUserBuildSettings.buildAppBundle = true;
        PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.Android, DevScriptingDefineSymbols);
        
        _buildType = BuildType.Test;
    }

    [MenuItem("Builds/Set AOS REAL Build Settings")]
    public static void SetAosRealBuildSettings()
    {
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
        EditorUserBuildSettings.buildAppBundle = true;
        PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.Android, RealScriptingDefineSymbols);
        
        _buildType = BuildType.Real;
    }

    [MenuItem("Builds/Start AOS Build")]
    public static void StartAosBuild()
    {
        PlayerSettings.Android.keystoreName = "Builds/AOS/minchankim.keystore";
        PlayerSettings.Android.keystorePass = "alscks3507";
        PlayerSettings.Android.keyaliasName = "minchankim";
        PlayerSettings.Android.keyaliasPass = "alscks3507";
        
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = new[]
            { 
                "Assets/00.Scenes/Title.unity",
                "Assets/00.Scenes/Lobby.unity",
            },
            target = BuildTarget.Android
        };
        string fileExtension = string.Empty;
        BuildOptions compressOption = BuildOptions.None;

        switch (_buildType)
        {
            case BuildType.Dev:
                fileExtension = "apk";
                compressOption = BuildOptions.CompressWithLz4;
                break;
            case BuildType.Test:
            case BuildType.Real:
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