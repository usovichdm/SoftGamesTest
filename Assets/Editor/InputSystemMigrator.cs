using UnityEditor;
using UnityEngine;

namespace SoftGames.EditorTools
{
    /// <summary>
    /// Reminder helper — scenes were migrated via YAML to InputSystemUIInputModule.
    /// Menu kept for documentation / future scenes.
    /// </summary>
    public static class InputSystemMigrator
    {
        [MenuItem("SoftGames/Validate Input System UI Modules")]
        public static void Validate()
        {
            var scenes = EditorBuildSettings.scenes;
            for (var i = 0; i < scenes.Length; i++)
            {
                var path = scenes[i].path;
                var text = System.IO.File.ReadAllText(path);
                var hasLegacy = text.Contains("StandaloneInputModule");
                var hasModern = text.Contains("InputSystemUIInputModule");
                Debug.Log($"{path}: legacy={hasLegacy} inputSystemUI={hasModern}");
            }
        }
    }
}
