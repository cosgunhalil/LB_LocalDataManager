using System;
using System.IO;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace LB.LocalDataManager.Tests
{
    public class SerializerSeamTests
    {
        [Serializable]
        private class Fixture
        {
            public int Id;
            public string Name;
        }

        /// <summary>
        /// Deliberately not JSON: if a file written through this reads back, the seam is
        /// really being used end to end rather than falling through to JsonUtility.
        /// </summary>
        private class PipeSerializer : IDataSerializer
        {
            public int SerializeCalls;
            public int DeserializeCalls;

            public string Serialize<T>(T value)
            {
                SerializeCalls++;
                var fixture = value as Fixture;
                return fixture.Id + "|" + fixture.Name;
            }

            public T Deserialize<T>(string data)
            {
                DeserializeCalls++;
                var parts = data.Split('|');
                return (T)(object)new Fixture { Id = int.Parse(parts[0]), Name = parts[1] };
            }
        }

        private class ThrowingSerializer : IDataSerializer
        {
            public string Serialize<T>(T value)
            {
                throw new InvalidOperationException("serializer said no");
            }

            public T Deserialize<T>(string data)
            {
                throw new InvalidOperationException("serializer said no");
            }
        }

        private string _fileName;
        private string _path;

        [SetUp]
        public void SetUp()
        {
            _fileName = "__ldm_serializer_" + Guid.NewGuid().ToString("N");
            _path = LocalDataPath.GetPathFor(_fileName);
        }

        [TearDown]
        public void TearDown()
        {
            LocalData.Serializer = null;

            if (File.Exists(_path))
            {
                File.Delete(_path);
            }

            if (File.Exists(_path + ".tmp"))
            {
                File.Delete(_path + ".tmp");
            }
        }

        [Test]
        public void InjectedSerializer_RoundTripsThroughItsOwnFormat()
        {
            var serializer = new PipeSerializer();

            new LocalDataSaver(serializer).SaveData(new Fixture { Id = 3, Name = "Ada" }, _fileName);
            var loaded = new LocalDataLoader(serializer).LoadData<Fixture>(_fileName);

            Assert.AreEqual("3|Ada", File.ReadAllText(_path));
            Assert.AreEqual(3, loaded.Id);
            Assert.AreEqual("Ada", loaded.Name);
            Assert.AreEqual(1, serializer.SerializeCalls);
            Assert.AreEqual(1, serializer.DeserializeCalls);
        }

        [Test]
        public void FacadeSerializer_AppliesToLocalDataCalls()
        {
            LocalData.Serializer = new PipeSerializer();

            LocalData.Save(new Fixture { Id = 8, Name = "Grace" }, _fileName);

            Assert.AreEqual("8|Grace", File.ReadAllText(_path));
            Assert.AreEqual(8, LocalData.Load<Fixture>(_fileName).Id);
        }

        [Test]
        public void FacadeSerializer_DefaultsToJsonUtility()
        {
            Assert.IsInstanceOf<JsonUtilitySerializer>(LocalData.Serializer);

            LocalData.Save(new Fixture { Id = 2, Name = "Ada" }, _fileName);

            StringAssert.Contains("\"Id\":2", File.ReadAllText(_path));
        }

        [Test]
        public void FacadeSerializer_ResetsToTheDefaultWhenSetToNull()
        {
            LocalData.Serializer = new PipeSerializer();
            LocalData.Serializer = null;

            Assert.IsInstanceOf<JsonUtilitySerializer>(LocalData.Serializer);
        }

        [Test]
        public void SaveData_ReportsFailureWhenTheSerializerThrows()
        {
            LogAssert.ignoreFailingMessages = true;

            var saved = new LocalDataSaver(new ThrowingSerializer())
                .SaveData(new Fixture { Id = 1 }, _fileName);

            LogAssert.ignoreFailingMessages = false;
            Assert.IsFalse(saved);
            FileAssert.DoesNotExist(_path);
        }

        [Test]
        public void TryLoadData_ReportsFailureWhenTheSerializerThrows()
        {
            File.WriteAllText(_path, "anything");
            LogAssert.ignoreFailingMessages = true;

            Fixture loaded;
            var ok = new LocalDataLoader(new ThrowingSerializer()).TryLoadData(_fileName, out loaded);

            LogAssert.ignoreFailingMessages = false;
            Assert.IsFalse(ok);
            Assert.IsNull(loaded);
        }

        [Test]
        public void JsonUtilitySerializer_PrettyPrintsWhenAsked()
        {
            var compact = new JsonUtilitySerializer().Serialize(new Fixture { Id = 1, Name = "Ada" });
            var pretty = new JsonUtilitySerializer(true).Serialize(new Fixture { Id = 1, Name = "Ada" });

            Assert.IsFalse(compact.Contains("\n"));
            Assert.IsTrue(pretty.Contains("\n"));
        }
    }
}
