using System;
using System.IO;
using UnityEngine;

namespace LB.LocalDataManager
{
    /// <summary>
    /// Reads a file written by <see cref="LocalDataSaver"/> and deserializes it with
    /// <see cref="JsonUtility"/>.
    /// </summary>
    public class LocalDataLoader
    {
        /// <summary>
        /// Loads <paramref name="fileName"/> and deserializes it into
        /// <typeparamref name="T"/>. Returns <c>default(T)</c> when the file is missing,
        /// unreadable or empty.
        /// </summary>
        public T LoadData<T>(string fileName)
        {
            var path = LocalDataPath.GetPathFor(fileName);
            var data = ReadDataFromPath(path);

            if (string.IsNullOrEmpty(data))
            {
                return default(T);
            }

            try
            {
                return JsonUtility.FromJson<T>(data);
            }
            catch (Exception ex)
            {
                Debug.LogError("[LocalDataManager] Failed to parse '" + path + "': " + ex.Message);
                return default(T);
            }
        }

        /// <summary>
        /// Reads the whole file at <paramref name="path"/>, or <c>null</c> when it cannot
        /// be read.
        /// </summary>
        public string ReadDataFromPath(string path)
        {
            if (!File.Exists(path))
            {
                return null;
            }

            try
            {
                using (var fs = new FileStream(path, FileMode.Open))
                using (var reader = new StreamReader(fs))
                {
                    return reader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[LocalDataManager] Failed to read '" + path + "': " + ex.Message);
                return null;
            }
        }
    }
}
