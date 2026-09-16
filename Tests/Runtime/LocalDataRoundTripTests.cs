using System;
using System.IO;
using NUnit.Framework;
using UnityEngine.TestTools;
using UnityEngine;

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

        [SetUp]
        public void SetUp()
        {
            _fileName = "__ldm_test_" + Guid.NewGuid().ToString("N");
        }

        [TearDown]
        public void TearDown()
        {
            var path = LocalDataPath.GetPathFor(_fileName);
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            // The editor writes into Assets/, so Unity generates a .meta next to the file.
            if (File.Exists(path + ".meta"))
            {
                File.Delete(path + ".meta");
            }
        }

        [Test]
        public void SaveData_WritesFileAndReportsSuccess()
        {
            var saved = new LocalDataSaver().SaveData(new Fixture { Id = 7, Name = "Ada" }, _fileName);

            Assert.IsTrue(saved);
            FileAssert.Exists(LocalDataPath.GetPathFor(_fileName));
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
        public void LoadData_ReturnsDefaultWhenFileIsMissing()
        {
            var loaded = new LocalDataLoader().LoadData<Fixture>(_fileName);

            Assert.IsNull(loaded);
        }

        [Test]
        public void ReadDataFromPath_ReturnsNullWhenFileIsMissing()
        {
            var data = new LocalDataLoader().ReadDataFromPath(LocalDataPath.GetPathFor(_fileName));

            Assert.IsNull(data);
        }

        [Test]
        public void SaveData_ReturnsFalseOnAnInvalidFileName()
        {
            LogAssert.ignoreFailingMessages = true;

            var saved = new LocalDataSaver().SaveData(new Fixture(), "missing-folder/nested");

            LogAssert.ignoreFailingMessages = false;
            Assert.IsFalse(saved);
        }
    }
}
