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
        private static IDataSerializer _serializer = new JsonUtilitySerializer();
        private static IDataProcessor _processor;
        private static readonly MigrationRegistry Registry = new MigrationRegistry();
        private static LocalDataSaver _saver = new LocalDataSaver(_serializer, _processor);
        private static LocalDataLoader _loader =
            new LocalDataLoader(_serializer, _processor, Registry);

        /// <summary>
        /// Serializer every call through this facade uses. Defaults to
        /// <see cref="JsonUtilitySerializer"/>; setting it to <c>null</c> restores that.
        /// <para>
        /// Set it once during startup, before anything saves or loads: files already
        /// written with a different serializer will not read back.
        /// </para>
        /// </summary>
        public static IDataSerializer Serializer
        {
            get { return _serializer; }
            set
            {
                _serializer = value ?? new JsonUtilitySerializer();
                Rebuild();
            }
        }

        /// <summary>
        /// Stage applied between serializing and writing - <see cref="AesDataProcessor"/>,
        /// for instance. <c>null</c> (the default) writes the serialized text as it is.
        /// <para>
        /// Set it once during startup, before anything saves or loads. Files written with a
        /// processor do not load without it, and files written without one do not load with
        /// it: changing this does not convert what is already on disk.
        /// </para>
        /// </summary>
        public static IDataProcessor Processor
        {
            get { return _processor; }
            set
            {
                _processor = value;
                Rebuild();
            }
        }

        /// <summary>
        /// Migrations that bring older saves up to the current schema. Register them during
        /// startup:
        /// <code>
        /// LocalData.Migrations.Register&lt;PlayerData&gt;(new RenameNameToDisplayName());
        /// </code>
        /// A saved type only carries a version if it implements <see cref="IVersionedData"/>;
        /// files written before that are treated as version 0, so a 0 -&gt; 1 migration can
        /// pick them up.
        /// </summary>
        public static MigrationRegistry Migrations
        {
            get { return Registry; }
        }

        private static void Rebuild()
        {
            _saver = new LocalDataSaver(_serializer, _processor);
            _loader = new LocalDataLoader(_serializer, _processor, Registry);
        }

        /// <summary>
        /// Saves <paramref name="dataObject"/> under <paramref name="fileName"/>, replacing
        /// any existing file. Returns <c>false</c> when the write failed.
        /// </summary>
        public static bool Save<T>(T dataObject, string fileName)
        {
            return _saver.SaveData(dataObject, fileName);
        }

        /// <summary>
        /// Loads <paramref name="fileName"/>, or <c>default(T)</c> when there is nothing to
        /// load.
        /// </summary>
        public static T Load<T>(string fileName)
        {
            return _loader.LoadData<T>(fileName);
        }

        /// <summary>
        /// Loads <paramref name="fileName"/>, or <paramref name="fallback"/> when there is
        /// nothing to load.
        /// </summary>
        public static T Load<T>(string fileName, T fallback)
        {
            return _loader.LoadData(fileName, fallback);
        }

        /// <summary>
        /// Loads <paramref name="fileName"/> into <paramref name="value"/> and reports
        /// whether anything was loaded - the way to tell "no save yet" from "the save is
        /// damaged".
        /// </summary>
        public static bool TryLoad<T>(string fileName, out T value)
        {
            return _loader.TryLoadData(fileName, out value);
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
