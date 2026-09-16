using System.IO;
using UnityEditor;
using UnityEngine;

namespace LB.LocalDataManager.EditorTools
{
    /// <summary>
    /// Menu shortcuts to the folders <see cref="LocalDataPath"/> writes to.
    /// </summary>
    public static class LocalDataManagerMenu
    {
        private const string MenuRoot = "Tools/Local Data Manager/";

        [MenuItem(MenuRoot + "Open Data Folder", priority = 0)]
        private static void OpenDataFolder()
        {
            Reveal(LocalDataPath.RootDirectory);
        }

        [MenuItem(MenuRoot + "Open Persistent Data Folder", priority = 1)]
        private static void OpenPersistentDataFolder()
        {
            Reveal(Application.persistentDataPath);
        }

        [MenuItem(MenuRoot + "Log Data Folder Path", priority = 20)]
        private static void LogDataFolder()
        {
            Debug.Log("[LocalDataManager] Data folder: " + LocalDataPath.RootDirectory);
        }

        private static void Reveal(string directory)
        {
            if (!Directory.Exists(directory))
            {
                Debug.LogWarning("[LocalDataManager] Folder does not exist yet: " + directory);
                return;
            }

            EditorUtility.RevealInFinder(directory + "/");
        }
    }
}
