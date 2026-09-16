using System;
using System.IO;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace LB.LocalDataManager.Tests
{
    public class LocalDataRoundTripTests
    {
        [Serializable]
        private class Fixture
        {
            public int Id;
            public string Name;
        }

        private string _fileName;
        private string _path;

        [SetUp]
        public void SetUp()
        {
            _fileName = "__ldm_test_" + Guid.NewGuid().ToString("N");
            _path = LocalDataPath.GetPathFor(_fileName);
        }

        [TearDown]
        public void TearDown()
        {
            // Covers the file itself, the staging file, and the folder a nested name creates.
            DeletePath(_path);
            DeletePath(_path + ".tmp");
            DeletePath(LocalDataPath.RootDirectory + "/" + _fileName);
        }

        private static void DeletePath(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            else if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }

        [Test]
        public void SaveData_WritesFileAndReportsSuccess()
        {
            var saved = new LocalDataSaver().SaveData(new Fixture { Id = 7, Name = "Ada" }, _fileName);

            Assert.IsTrue(saved);
            FileAssert.Exists(_path);
        }

        [Test]
        public void LoadData_ReturnsTheSavedValues()
        {
            new LocalDataSaver().SaveData(new Fixture { Id = 7, Name = "Ada" }, _fileName);

            var loaded = new LocalDataLoader().LoadData<Fixture>(_fileName);

            Assert.IsNotNull(loaded);
            Assert.AreEqual(7, loaded.Id);
            Assert.AreEqual("Ada", loaded.Name);
        }

        [Test]
        public void SaveData_OverwritesAnExistingFile()
        {
            var saver = new LocalDataSaver();
            saver.SaveData(new Fixture { Id = 1, Name = "first" }, _fileName);
            saver.SaveData(new Fixture { Id = 2, Name = "second" }, _fileName);

            var loaded = new LocalDataLoader().LoadData<Fixture>(_fileName);

            Assert.AreEqual(2, loaded.Id);
            Assert.AreEqual("second", loaded.Name);
        }

        [Test]
        public void SaveData_LeavesNoStagingFileBehind()
        {
            new LocalDataSaver().SaveData(new Fixture { Id = 1, Name = "Ada" }, _fileName);

            FileAssert.DoesNotExist(_path + ".tmp");
        }

        [Test]
        public void SaveData_CreatesMissingFoldersForANestedName()
        {
            var nested = _fileName + "/slots/autosave";

            var saved = new LocalDataSaver().SaveData(new Fixture { Id = 3, Name = "nested" }, nested);

            Assert.IsTrue(saved);
            Assert.AreEqual(3, new LocalDataLoader().LoadData<Fixture>(nested).Id);
        }

        [Test]
        public void SaveData_KeepsThePreviousFileWhenTheWriteFails()
        {
            var saver = new LocalDataSaver();
            saver.SaveData(new Fixture { Id = 1, Name = "good" }, _fileName);

            // A folder sitting where the staging file goes makes the write fail after the
            // previous save already exists - the case that used to truncate it.
            Directory.CreateDirectory(_path + ".tmp");
            LogAssert.ignoreFailingMessages = true;

            var saved = saver.SaveData(new Fixture { Id = 2, Name = "bad" }, _fileName);

            LogAssert.ignoreFailingMessages = false;
            Assert.IsFalse(saved);

            var loaded = new LocalDataLoader().LoadData<Fixture>(_fileName);
            Assert.AreEqual(1, loaded.Id, "the previous save must survive a failed write");
            Assert.AreEqual("good", loaded.Name);
        }

        [Test]
        public void LoadData_ReturnsDefaultWhenFileIsMissing()
        {
            Assert.IsNull(new LocalDataLoader().LoadData<Fixture>(_fileName));
        }

        [Test]
        public void LoadData_ReturnsDefaultWhenTheFileIsNotJson()
        {
            File.WriteAllText(_path, "this is not json");
            LogAssert.ignoreFailingMessages = true;

            var loaded = new LocalDataLoader().LoadData<Fixture>(_fileName);

            LogAssert.ignoreFailingMessages = false;
            Assert.IsNull(loaded);
        }
    }
}
