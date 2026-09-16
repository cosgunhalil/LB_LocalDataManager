using System;
using System.IO;
using UnityEngine;

namespace LB.LocalDataManager
{
    /// <summary>
    /// Entry point of the package: save, load, check and delete local data files.
    /// <code>
    /// LocalData.Save(playerData, "PlayerData");
    /// var playerData = LocalData.Load&lt;PlayerData&gt;("PlayerData");
    /// </code>
    /// <para>
    /// Every method here forwards to <see cref="LocalDataSaver"/> and
    /// <see cref="LocalDataLoader"/>, which stay public for code that would rather hold and
    /// inject its own instances.
    /// </para>
    /// </summary>
    public static class LocalData
    {
        private static readonly LocalDataSaver Saver = new LocalDataSaver();
        private static readonly LocalDataLoader Loader = new LocalDataLoader();

        /// <summary>
        /// Saves <paramref name="dataObject"/> under <paramref name="fileName"/>, replacing
        /// any existing file. Returns <c>false</c> when the write failed.
        /// </summary>
        public static bool Save<T>(T dataObject, string fileName)
        {
            return Saver.SaveData(dataObject, fileName);
        }

        /// <summary>
        /// Loads <paramref name="fileName"/>, or <c>default(T)</c> when there is nothing to
        /// load.
        /// </summary>
        public static T Load<T>(string fileName)
        {
            return Loader.LoadData<T>(fileName);
        }

        /// <summary>
        /// Loads <paramref name="fileName"/>, or <paramref name="fallback"/> when there is
        /// nothing to load.
        /// </summary>
        public static T Load<T>(string fileName, T fallback)
        {
            return Loader.LoadData(fileName, fallback);
        }

        /// <summary>
        /// Loads <paramref name="fileName"/> into <paramref name="value"/> and reports
        /// whether anything was loaded - the way to tell "no save yet" from "the save is
        /// damaged".
        /// </summary>
        public static bool TryLoad<T>(string fileName, out T value)
        {
            return Loader.TryLoadData(fileName, out value);
        }

        /// <summary>
        /// Whether <paramref name="fileName"/> has been saved. Says nothing about whether
        /// its contents still parse.
        /// </summary>
        /// <exception cref="ArgumentException">
        /// <paramref name="fileName"/> is not a valid name - see <see cref="LocalDataPath.GetPathFor"/>.
        /// </exception>
        public static bool Exists(string fileName)
        {
            return File.Exists(LocalDataPath.GetPathFor(fileName));
        }

        /// <summary>
        /// Deletes the file saved under <paramref name="fileName"/>.
        /// </summary>
        /// <returns>
        /// <c>true</c> when a file was there and is now gone; <c>false</c> when there was
        /// nothing to delete, or when the delete failed and logged an error.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="fileName"/> is not a valid name - see <see cref="LocalDataPath.GetPathFor"/>.
        /// </exception>
        public static bool Delete(string fileName)
        {
            var path = LocalDataPath.GetPathFor(fileName);

            if (!File.Exists(path))
            {
                return false;
            }

            try
            {
                File.Delete(path);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError("[LocalDataManager] Failed to delete '" + path + "': " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Full path <paramref name="fileName"/> is stored at. Useful for logging or for
        /// handing the file to something outside this package.
        /// </summary>
        public static string GetPath(string fileName)
        {
            return LocalDataPath.GetPathFor(fileName);
        }
    }
}
