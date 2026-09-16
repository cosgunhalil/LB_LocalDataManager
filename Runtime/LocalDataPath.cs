using UnityEngine;

namespace LB.LocalDataManager
{
    /// <summary>
    /// Resolves where local data files live. Both <see cref="LocalDataSaver"/> and
    /// <see cref="LocalDataLoader"/> go through here so the two can never drift apart.
    /// </summary>
    public static class LocalDataPath
    {
        /// <summary>Extension appended to every file name.</summary>
        public const string Extension = ".txt";

        /// <summary>
        /// Directory the data files are written to: <see cref="Application.dataPath"/> in the
        /// editor, <see cref="Application.persistentDataPath"/> in a player build. Editor and
        /// player therefore do not share saved data.
        /// </summary>
        public static string RootDirectory
        {
            get
            {
#if UNITY_EDITOR
                return Application.dataPath;
#else
                return Application.persistentDataPath;
#endif
            }
        }

        /// <summary>
        /// Full path for <paramref name="fileName"/>. Pass a bare name: the directory and the
        /// <c>.txt</c> extension are added here, so "PlayerData" - not "PlayerData.txt".
        /// </summary>
        public static string GetPathFor(string fileName)
        {
            return RootDirectory + "/" + fileName + Extension;
        }
    }
}
