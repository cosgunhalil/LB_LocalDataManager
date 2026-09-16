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
        /// unreadable or not valid JSON. Use <see cref="TryLoadData{T}"/> when those cases
        /// need to be told apart.
        /// </summary>
        /// <exception cref="ArgumentException">
        /// <paramref name="fileName"/> is not a valid name - see <see cref="LocalDataPath.GetPathFor"/>.
        /// </exception>
        public T LoadData<T>(string fileName)
        {
            T value;
            TryLoadData(fileName, out value);
            return value;
        }

        /// <summary>
        /// Loads <paramref name="fileName"/>, or returns <paramref name="fallback"/> when
        /// there is nothing to load.
        /// </summary>
        public T LoadData<T>(string fileName, T fallback)
        {
            T value;
            return TryLoadData(fileName, out value) ? value : fallback;
        }

        /// <summary>
        /// Loads <paramref name="fileName"/> into <paramref name="value"/>.
        /// <para>
        /// Returns <c>false</c> - with <paramref name="value"/> set to <c>default(T)</c> -
        /// when the file does not exist, cannot be read, is empty, or does not parse. That
        /// is what separates "no save yet" from "the save is damaged".
        /// </para>
        /// <para>
        /// A file that parses but does not match <typeparamref name="T"/> still counts as
        /// loaded: <see cref="JsonUtility"/> fills in the fields it recognizes and leaves
        /// the rest at their defaults without reporting anything.
        /// </para>
        /// </summary>
        /// <exception cref="ArgumentException">
        /// <paramref name="fileName"/> is not a valid name - see <see cref="LocalDataPath.GetPathFor"/>.
        /// </exception>
        public bool TryLoadData<T>(string fileName, out T value)
        {
            var path = LocalDataPath.GetPathFor(fileName);
            var data = ReadDataFromPath(path);

            if (string.IsNullOrEmpty(data))
            {
                value = default(T);
                return false;
            }

            try
            {
                value = JsonUtility.FromJson<T>(data);
                return value != null;
            }
            catch (Exception ex)
            {
                Debug.LogError("[LocalDataManager] Failed to parse '" + path + "': " + ex.Message);
                value = default(T);
                return false;
            }
        }

        /// <summary>
        /// Reads the whole file at <paramref name="path"/>, or <c>null</c> when it cannot
        /// be read.
        /// </summary>
        private static string ReadDataFromPath(string path)
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
