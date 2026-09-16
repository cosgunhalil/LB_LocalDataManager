using System;
using System.IO;
using UnityEngine;

namespace LB.LocalDataManager
{
    /// <summary>
    /// Reads a file written by <see cref="LocalDataSaver"/> and deserializes it.
    /// </summary>
    public class LocalDataLoader
    {
        private readonly IDataSerializer _serializer;
        private readonly IDataProcessor _processor;
        private readonly MigrationRegistry _migrations;

        /// <summary>Loads with the default <see cref="JsonUtilitySerializer"/>.</summary>
        public LocalDataLoader() : this(null, null)
        {
        }

        /// <param name="serializer">
        /// Serializer to read with, or <c>null</c> for the default
        /// <see cref="JsonUtilitySerializer"/>. It has to match the one the file was
        /// written with.
        /// </param>
        public LocalDataLoader(IDataSerializer serializer) : this(serializer, null, null)
        {
        }

        /// <param name="serializer">Serializer to read with, or <c>null</c> for the default.</param>
        /// <param name="processor">
        /// Stage applied to the file contents before deserializing, or <c>null</c>.
        /// </param>
        public LocalDataLoader(IDataSerializer serializer, IDataProcessor processor)
            : this(serializer, processor, null)
        {
        }

        /// <param name="serializer">
        /// Serializer to read with, or <c>null</c> for the default
        /// <see cref="JsonUtilitySerializer"/>.
        /// </param>
        /// <param name="processor">
        /// Stage applied to the file contents before deserializing. It has to match the one
        /// the file was written with; a mismatch reads as a failed load.
        /// </param>
        /// <param name="migrations">
        /// Migrations to bring an older file up to the current schema, or <c>null</c> for
        /// none.
        /// </param>
        public LocalDataLoader(
            IDataSerializer serializer, IDataProcessor processor, MigrationRegistry migrations)
        {
            _serializer = serializer ?? new JsonUtilitySerializer();
            _processor = processor;
            _migrations = migrations ?? new MigrationRegistry();
        }

        /// <summary>
        /// Loads <paramref name="fileName"/> and deserializes it into
        /// <typeparamref name="T"/>. Returns <c>default(T)</c> when the file is missing,
        /// unreadable or does not deserialize. Use <see cref="TryLoadData{T}"/> when those
        /// cases need to be told apart.
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
        /// loaded when the serializer accepts it: <see cref="JsonUtility"/>, for one, fills
        /// in the fields it recognizes and leaves the rest at their defaults without
        /// reporting anything.
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
                if (_processor != null)
                {
                    data = _processor.Decode(data);
                }

                int version;
                data = SchemaHeader.Read(data, out version);
                data = _migrations.Apply(typeof(T), version, data);

                value = _serializer.Deserialize<T>(data);
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
