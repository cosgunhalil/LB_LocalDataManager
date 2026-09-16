using System;
using System.IO;
using System.Security.Cryptography;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace LB.LocalDataManager.Tests
{
    public class AesDataProcessorTests
    {
        [Serializable]
        private class Fixture
        {
            public int Coins;
            public string Name;
        }

        private const string Password = "a passphrase from the game";

        private string _fileName;
        private string _path;

        [SetUp]
        public void SetUp()
        {
            _fileName = "__ldm_aes_" + Guid.NewGuid().ToString("N");
            _path = LocalDataPath.GetPathFor(_fileName);
        }

        [TearDown]
        public void TearDown()
        {
            LocalData.Processor = null;
            LocalData.Serializer = null;

            if (File.Exists(_path))
            {
                File.Delete(_path);
            }
        }

        [Test]
        public void Encode_ThenDecode_ReturnsTheOriginalText()
        {
            var processor = new AesDataProcessor(Password);

            Assert.AreEqual("hello", processor.Decode(processor.Encode("hello")));
        }

        [Test]
        public void Encode_ProducesDifferentTextEveryTime()
        {
            var processor = new AesDataProcessor(Password);

            var first = processor.Encode("hello");
            var second = processor.Encode("hello");

            Assert.AreNotEqual(first, second, "salt and IV must be random per call");
            Assert.AreEqual("hello", processor.Decode(first));
            Assert.AreEqual("hello", processor.Decode(second));
        }

        [Test]
        public void SavedFile_DoesNotContainThePlainValues()
        {
            LocalData.Processor = new AesDataProcessor(Password);

            LocalData.Save(new Fixture { Coins = 99999, Name = "Halil" }, _fileName);

            var onDisk = File.ReadAllText(_path);
            StringAssert.DoesNotContain("99999", onDisk);
            StringAssert.DoesNotContain("Halil", onDisk);
            StringAssert.DoesNotContain("Coins", onDisk);
        }

        [Test]
        public void SaveAndLoad_RoundTripThroughTheFacade()
        {
            LocalData.Processor = new AesDataProcessor(Password);

            LocalData.Save(new Fixture { Coins = 12, Name = "Ada" }, _fileName);
            var loaded = LocalData.Load<Fixture>(_fileName);

            Assert.AreEqual(12, loaded.Coins);
            Assert.AreEqual("Ada", loaded.Name);
        }

        [Test]
        public void Decode_RefusesAFileWrittenWithAnotherPassword()
        {
            var encrypted = new AesDataProcessor("first password").Encode("hello");

            Assert.Throws<CryptographicException>(
                () => new AesDataProcessor("second password").Decode(encrypted));
        }

        [Test]
        public void Decode_RefusesATamperedFile()
        {
            var processor = new AesDataProcessor(Password);
            var encrypted = processor.Encode("hello");

            // Flip a byte in the ciphertext, the way a save editor would.
            var bytes = Convert.FromBase64String(encrypted);
            bytes[bytes.Length - 1] ^= 0x01;
            var tampered = Convert.ToBase64String(bytes);

            Assert.Throws<CryptographicException>(() => processor.Decode(tampered));
        }

        [Test]
        public void TryLoad_ReportsFailureForAFileEncryptedWithAnotherPassword()
        {
            LocalData.Processor = new AesDataProcessor("first password");
            LocalData.Save(new Fixture { Coins = 1, Name = "Ada" }, _fileName);

            LocalData.Processor = new AesDataProcessor("second password");
            LogAssert.ignoreFailingMessages = true;

            Fixture loaded;
            var ok = LocalData.TryLoad(_fileName, out loaded);

            LogAssert.ignoreFailingMessages = false;
            Assert.IsFalse(ok, "the wrong password must read as a failed load, not as an exception");
            Assert.IsNull(loaded);
        }

        [Test]
        public void TryLoad_ReportsFailureWhenThePlainFileIsReadWithAProcessor()
        {
            LocalData.Save(new Fixture { Coins = 1, Name = "Ada" }, _fileName);

            LocalData.Processor = new AesDataProcessor(Password);
            LogAssert.ignoreFailingMessages = true;

            Fixture loaded;
            var ok = LocalData.TryLoad(_fileName, out loaded);

            LogAssert.ignoreFailingMessages = false;
            Assert.IsFalse(ok);
        }

        [Test]
        public void Processor_DefaultsToNoneSoFilesStayPlainText()
        {
            Assert.IsNull(LocalData.Processor);

            LocalData.Save(new Fixture { Coins = 3, Name = "Ada" }, _fileName);

            StringAssert.Contains("Ada", File.ReadAllText(_path));
        }

        [TestCase(null)]
        [TestCase("")]
        public void Constructor_RejectsAnEmptyPassword(string password)
        {
            Assert.Throws<ArgumentException>(() => new AesDataProcessor(password));
        }

        [Test]
        public void Constructor_RejectsTooFewIterations()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new AesDataProcessor(Password, 10));
        }
    }
}
