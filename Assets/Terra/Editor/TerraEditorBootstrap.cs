using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace TerraTactix.EditorTools
{
    [InitializeOnLoad]
    public static class TerraEditorBootstrap
    {
        static readonly string[] Scenes =
        {
            "Assets/Terra/Scenes/Boot.unity",
            "Assets/Terra/Scenes/MainMenu.unity",
            "Assets/Terra/Scenes/WorldMap.unity",
            "Assets/Terra/Scenes/Battle.unity",
            "Assets/Terra/Scenes/Shop.unity",
            "Assets/Terra/Scenes/DailyWheel.unity",
            "Assets/Terra/Scenes/Leaderboard.unity"
        };

        static int attempts;

        static TerraEditorBootstrap()
        {
            EditorApplication.delayCall += Ensure;
        }

        static void Ensure()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            var boot = AssetDatabase.LoadAssetAtPath<SceneAsset>(Scenes[0]);
            if (boot == null)
            {
                if (attempts++ < 5)
                    EditorApplication.delayCall += Ensure;
                return;
            }

            if (EditorSceneManager.playModeStartScene != boot)
                EditorSceneManager.playModeStartScene = boot;

            var current = EditorBuildSettings.scenes;
            var same = current.Length == Scenes.Length;
            if (same)
            {
                for (var i = 0; i < Scenes.Length; i++)
                {
                    if (current[i].path != Scenes[i])
                    {
                        same = false;
                        break;
                    }
                }
            }

            if (same)
                return;

            var next = new EditorBuildSettingsScene[Scenes.Length];
            for (var i = 0; i < Scenes.Length; i++)
                next[i] = new EditorBuildSettingsScene(Scenes[i], true);
            EditorBuildSettings.scenes = next;
        }
    }
}
