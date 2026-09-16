using System;
using System.IO;
using UnityEngine;

namespace LB.LocalDataManager
{
    /// <summary>
    /// Serializes an object and writes it to local storage.
    /// </summary>
    public class LocalDataSaver
    {
        /// <summary>Suffix of the scratch file the data is written to before it replaces the save.</summary>
        private const string TempSuffix = ".tmp";

        private readonly IDataSerializer _serializer;
        private readonly IDataProcessor _processor;

        /// <summary>Saves with the default <see cref="JsonUtilitySerializer"/>.</summary>
        public LocalDataSaver() : this(null, null)
        {
        }

        /// <param name="serializer">
        /// Serializer to write with, or <c>null</c> for the default
        /// <see cref="JsonUtilitySerializer"/>.
        /// </param>
        public LocalDataSaver(IDataSerializer serializer) : this(serializer, null)
        {
        }

        /// <param name="serializer">
        /// Serializer to write with, or <c>null</c> for the default
        /// <see cref="JsonUtilitySerializer"/>.
        /// </param>
        /// <param name="processor">
        /// Stage applied to the serialized text before it is written - encryption, for
        /// instance - or <c>null</c> to write it as it is.
        /// </param>
        public LocalDataSaver(IDataSerializer serializer, IDataProcessor processor)
        {
            _serializer = serializer ?? new JsonUtilitySerializer();
            _processor = processor;
        }

        /// <summary>
        /// Saves <paramref name="dataObject"/> under <paramref name="fileName"/>, replacing
        /// any existing file. What <typeparamref name="T"/> may contain is up to the
        /// serializer; the default <see cref="JsonUtilitySerializer"/> wants a
        /// [Serializable] type with public fields.
        /// <para>
        /// The write is staged through a temporary file and only then moved into place, so a
        /// crash or a process kill during the save leaves the previous file intact instead of
        /// truncating it.
        /// </para>
        /// </summary>
        /// <returns><c>true</c> when the file was written, <c>false</c> when it failed.</returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="fileName"/> is not a valid name - see <see cref="LocalDataPath.GetPathFor"/>.
        /// </exception>
        public bool SaveData<T>(T dataObject, string fileName)
        {
            var path = LocalDataPath.GetPathFor(fileName);
            var tempPath = path + TempSuffix;

            try
            {
                // Inside the try: a custom serializer is free to throw, and that is a
                // failed save rather than an exception out of this method.
                var data = _serializer.Serialize(dataObject);

                var versioned = dataObject as IVersionedData;
                if (versioned != null)
                {
                    // Stamped before the processor runs, so an encrypted save keeps its
                    // version inside the ciphertext.
                    data = SchemaHeader.Write(data, versioned.SchemaVersion);
                }

                if (_processor != null)
                {
                    data = _processor.Encode(data);
                }

                var directory = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                using (var fs = new FileStream(tempPath, FileMode.Create))
                using (var writer = new StreamWriter(fs))
                {
                    writer.Write(data);
                }

                MoveIntoPlace(tempPath, path);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError("[LocalDataManager] Failed to save '" + path + "': " + ex.Message);
                DeleteQuietly(tempPath);
                return false;
            }
        }

        /// <summary>
        /// Replaces <paramref name="path"/> with <paramref name="tempPath"/>, atomically where
        /// the platform supports it.
        /// </summary>
        private static void MoveIntoPlace(string tempPath, string path)
        {
            if (!File.Exists(path))
            {
                File.Move(tempPath, path);
                return;
            }

            try
            {
                File.Replace(tempPath, path, null);
            }
            catch (PlatformNotSupportedException)
            {
                DeleteAndMove(tempPath, path);
            }
            catch (NotSupportedException)
            {
                DeleteAndMove(tempPath, path);
            }
            catch (IOException)
            {
                DeleteAndMove(tempPath, path);
            }
        }

        /// <summary>
        /// Non-atomic fallback for platforms without <see cref="File.Replace(string,string,string)"/>.
        /// </summary>
        private static void DeleteAndMove(string tempPath, string path)
        {
            File.Delete(path);
            File.Move(tempPath, path);
        }

        private static void DeleteQuietly(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[LocalDataManager] Could not clean up '" + path + "': " + ex.Message);
            }
        }
    }
}
