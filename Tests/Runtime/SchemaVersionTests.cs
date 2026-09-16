using System;
using System.IO;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace LB.LocalDataManager.Tests
{
    public class SchemaVersionTests
    {
        [Serializable]
        private class Unversioned
        {
            public int Id;
        }

        [Serializable]
        private class VersionedTwo : IVersionedData
        {
            public int SchemaVersion { get { return 2; } }
            public int Id;
            public string DisplayName;
        }

        /// <summary>Rewrites the version 1 field name into the version 2 one.</summary>
        private class RenameNameToDisplayName : IDataMigration
        {
            public int FromVersion { get { return 1; } }
            public int ToVersion { get { return 2; } }

            public string Migrate(string data)
            {
                return data.Replace("\"Name\":", "\"DisplayName\":");
            }
        }

        /// <summary>Picks up files written before versioning existed.</summary>
        private class LegacyToVersionOne : IDataMigration
        {
            public int FromVersion { get { return 0; } }
            public int ToVersion { get { return 1; } }

            public string Migrate(string data)
            {
                return data.Replace("\"OldName\":", "\"Name\":");
            }
        }

        private class ThrowingMigration : IDataMigration
        {
            public int FromVersion { get { return 1; } }
            public int ToVersion { get { return 2; } }

            public string Migrate(string data)
            {
                throw new InvalidOperationException("migration said no");
            }
        }

        private string _fileName;
        private string _path;

        [SetUp]
        public void SetUp()
        {
            _fileName = "__ldm_schema_" + Guid.NewGuid().ToString("N");
            _path = LocalDataPath.GetPathFor(_fileName);
        }

        [TearDown]
        public void TearDown()
        {
            LocalData.Migrations.Clear();
            LocalData.Processor = null;
            LocalData.Serializer = null;

            if (File.Exists(_path))
            {
                File.Delete(_path);
            }
        }

        [Test]
        public void SavedFile_CarriesAHeaderForAVersionedType()
        {
            LocalData.Save(new VersionedTwo { Id = 1, DisplayName = "Ada" }, _fileName);

            StringAssert.StartsWith("#ldmv2\n", File.ReadAllText(_path));
        }

        [Test]
        public void SavedFile_HasNoHeaderForATypeThatDoesNotOptIn()
        {
            LocalData.Save(new Unversioned { Id = 1 }, _fileName);

            StringAssert.StartsWith("{", File.ReadAllText(_path));
        }

        [Test]
        public void VersionedType_RoundTripsWithItsHeader()
        {
            LocalData.Save(new VersionedTwo { Id = 4, DisplayName = "Ada" }, _fileName);

            var loaded = LocalData.Load<VersionedTwo>(_fileName);

            Assert.AreEqual(4, loaded.Id);
            Assert.AreEqual("Ada", loaded.DisplayName);
        }

        [Test]
        public void Migration_BringsAVersionOneFileUpToTheCurrentShape()
        {
            // What version 1 of the type used to write.
            File.WriteAllText(_path, "#ldmv1\n{\"Id\":7,\"Name\":\"Ada\"}");
            LocalData.Migrations.Register<VersionedTwo>(new RenameNameToDisplayName());

            var loaded = LocalData.Load<VersionedTwo>(_fileName);

            Assert.AreEqual(7, loaded.Id);
            Assert.AreEqual("Ada", loaded.DisplayName);
        }

        [Test]
        public void Migrations_ChainAcrossSeveralVersions()
        {
            // A pre-versioning file, two migrations away from the current shape.
            File.WriteAllText(_path, "{\"Id\":9,\"OldName\":\"Ada\"}");
            LocalData.Migrations.Register<VersionedTwo>(new LegacyToVersionOne());
            LocalData.Migrations.Register<VersionedTwo>(new RenameNameToDisplayName());

            var loaded = LocalData.Load<VersionedTwo>(_fileName);

            Assert.AreEqual(9, loaded.Id);
            Assert.AreEqual("Ada", loaded.DisplayName);
        }

        [Test]
        public void Migrations_OnlyApplyToTheirOwnType()
        {
            File.WriteAllText(_path, "#ldmv1\n{\"Id\":3,\"Name\":\"Ada\"}");
            LocalData.Migrations.Register<Unversioned>(new RenameNameToDisplayName());

            var loaded = LocalData.Load<VersionedTwo>(_fileName);

            Assert.AreEqual(3, loaded.Id);
            Assert.IsTrue(string.IsNullOrEmpty(loaded.DisplayName), "the other type's migration must not run");
        }

        [Test]
        public void Load_LeavesAnOldFileAloneWhenNoMigrationIsRegistered()
        {
            File.WriteAllText(_path, "#ldmv1\n{\"Id\":5,\"Name\":\"Ada\"}");

            var loaded = LocalData.Load<VersionedTwo>(_fileName);

            Assert.AreEqual(5, loaded.Id, "fields that did not change still load");
        }

        [Test]
        public void TryLoad_ReportsFailureWhenAMigrationThrows()
        {
            File.WriteAllText(_path, "#ldmv1\n{\"Id\":1}");
            LocalData.Migrations.Register<VersionedTwo>(new ThrowingMigration());
            LogAssert.ignoreFailingMessages = true;

            VersionedTwo loaded;
            var ok = LocalData.TryLoad(_fileName, out loaded);

            LogAssert.ignoreFailingMessages = false;
            Assert.IsFalse(ok);
        }

        [Test]
        public void VersionHeader_SurvivesEncryption()
        {
            LocalData.Processor = new AesDataProcessor("a passphrase");
            LocalData.Save(new VersionedTwo { Id = 6, DisplayName = "Ada" }, _fileName);

            // The header is inside the ciphertext, so it is not readable on disk.
            StringAssert.DoesNotContain("#ldmv2", File.ReadAllText(_path));
            Assert.AreEqual(6, LocalData.Load<VersionedTwo>(_fileName).Id);
        }

        [Test]
        public void Register_RejectsAMigrationThatDoesNotMoveForward()
        {
            Assert.Throws<ArgumentException>(
                () => LocalData.Migrations.Register<VersionedTwo>(new BackwardsMigration()));
        }

        [Test]
        public void Register_RejectsASecondMigrationFromTheSameVersion()
        {
            LocalData.Migrations.Register<VersionedTwo>(new RenameNameToDisplayName());

            Assert.Throws<ArgumentException>(
                () => LocalData.Migrations.Register<VersionedTwo>(new RenameNameToDisplayName()));
        }

        private class BackwardsMigration : IDataMigration
        {
            public int FromVersion { get { return 3; } }
            public int ToVersion { get { return 1; } }

            public string Migrate(string data)
            {
                return data;
            }
        }
    }
}
