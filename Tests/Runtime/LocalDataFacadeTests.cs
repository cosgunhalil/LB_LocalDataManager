using System;
using System.IO;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace LB.LocalDataManager.Tests
{
    public class LocalDataFacadeTests
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
            _fileName = "__ldm_facade_" + Guid.NewGuid().ToString("N");
            _path = LocalData.GetPath(_fileName);
        }

        [TearDown]
        public void TearDown()
        {
            if (File.Exists(_path))
            {
                File.Delete(_path);
            }
        }

        [Test]
        public void SaveAndLoad_RoundTripThroughTheFacade()
        {
            Assert.IsTrue(LocalData.Save(new Fixture { Id = 4, Name = "Grace" }, _fileName));

            var loaded = LocalData.Load<Fixture>(_fileName);

            Assert.AreEqual(4, loaded.Id);
            Assert.AreEqual("Grace", loaded.Name);
        }

        [Test]
        public void Exists_IsFalseBeforeTheFirstSaveAndTrueAfter()
        {
            Assert.IsFalse(LocalData.Exists(_fileName));

            LocalData.Save(new Fixture { Id = 1 }, _fileName);

            Assert.IsTrue(LocalData.Exists(_fileName));
        }

        [Test]
        public void Delete_RemovesTheFileAndReportsIt()
        {
            LocalData.Save(new Fixture { Id = 1 }, _fileName);

            Assert.IsTrue(LocalData.Delete(_fileName));
            Assert.IsFalse(LocalData.Exists(_fileName));
        }

        [Test]
        public void Delete_ReportsFalseWhenThereIsNothingToDelete()
        {
            Assert.IsFalse(LocalData.Delete(_fileName));
        }

        [Test]
        public void TryLoad_ReportsFalseWhenNothingWasSaved()
        {
            Fixture loaded;

            Assert.IsFalse(LocalData.TryLoad(_fileName, out loaded));
            Assert.IsNull(loaded);
        }

        [Test]
        public void TryLoad_ReportsFalseForADamagedFile()
        {
            File.WriteAllText(_path, "{ this is not json");
            LogAssert.ignoreFailingMessages = true;

            Fixture loaded;
            var ok = LocalData.TryLoad(_fileName, out loaded);

            LogAssert.ignoreFailingMessages = false;
            Assert.IsFalse(ok, "a damaged save must not look like a successful load");
            Assert.IsNull(loaded);
        }

        [Test]
        public void TryLoad_ReportsTrueForASavedFile()
        {
            LocalData.Save(new Fixture { Id = 9, Name = "Ada" }, _fileName);

            Fixture loaded;

            Assert.IsTrue(LocalData.TryLoad(_fileName, out loaded));
            Assert.AreEqual(9, loaded.Id);
        }

        [Test]
        public void Load_ReturnsTheFallbackWhenNothingWasSaved()
        {
            var fallback = new Fixture { Id = -1, Name = "new player" };

            var loaded = LocalData.Load(_fileName, fallback);

            Assert.AreSame(fallback, loaded);
        }

        [Test]
        public void Load_IgnoresTheFallbackWhenAFileExists()
        {
            LocalData.Save(new Fixture { Id = 5, Name = "saved" }, _fileName);

            var loaded = LocalData.Load(_fileName, new Fixture { Id = -1, Name = "new player" });

            Assert.AreEqual(5, loaded.Id);
        }

        [Test]
        public void Exists_RejectsAnInvalidFileName()
        {
            Assert.Throws<ArgumentException>(() => LocalData.Exists("../outside"));
        }
    }
}
