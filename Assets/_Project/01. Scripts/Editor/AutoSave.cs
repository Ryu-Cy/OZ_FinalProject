using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// 자동 저장 기능을 제공하는 에디터 스크립트입니다. <br/>
/// 지정된 시간 간격 및 플레이 시 자동으로 저장합니다.
/// </summary>
[InitializeOnLoad]
public static class AutoSave
{
    private const int SaveIntervalMinutes = 5;
    private static double _nextSaveTime;

    static AutoSave()
    {
        _nextSaveTime = EditorApplication.timeSinceStartup + (SaveIntervalMinutes * 60);
        EditorApplication.update += OnEditorUpdate;
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    private static void OnEditorUpdate()
    {
        if (Application.isPlaying || EditorApplication.isCompiling) return;

        if (EditorApplication.timeSinceStartup >= _nextSaveTime)
        {
            SaveActiveScenesAndAssets();
            _nextSaveTime = EditorApplication.timeSinceStartup + (SaveIntervalMinutes * 60);
        }
    }

    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode)
        {
            SaveActiveScenesAndAssets();
        }
    }

    private static void SaveActiveScenesAndAssets()
    {
        EditorSceneManager.SaveOpenScenes();
        AssetDatabase.SaveAssets();
        Debug.Log($"<color=#70C050>[AutoSave]</color> 씬 및 에셋 자동 저장이 완료되었습니다. ({System.DateTime.Now:HH:mm:ss})");
    }
}