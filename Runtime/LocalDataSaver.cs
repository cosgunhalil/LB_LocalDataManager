using System;
using System.IO;
using UnityEngine;

namespace LB.LocalDataManager
{
    /// <summary>
    /// Serializes an object with <see cref="JsonUtility"/> and writes it to local storage.
    /// </summary>
    public class LocalDataSaver
    {
        /// <summary>
        /// Saves <paramref name="dataObject"/> as JSON under <paramref name="fileName"/>,
        /// overwriting any existing file. <typeparamref name="T"/> must satisfy the
        /// <see cref="JsonUtility"/> rules: a [Serializable] type with public fields.
        /// </summary>
        /// <returns><c>true</c> when the file was written, <c>false</c> when it failed.</returns>
        public bool SaveData<T>(T dataObject, string fileName)
        {
            var path = LocalDataPath.GetPathFor(fileName);
            var data = JsonUtility.ToJson(dataObject);

            try
            {
                using (var fs = new FileStream(path, FileMode.Create))
                using (var writer = new StreamWriter(fs))
                {
                    writer.Write(data);
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError("[LocalDataManager] Failed to save '" + path + "': " + ex.Message);
                return false;
            }
        }
    }
}
