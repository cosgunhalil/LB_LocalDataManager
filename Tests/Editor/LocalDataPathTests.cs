using System;
using NUnit.Framework;
using UnityEngine;

namespace LB.LocalDataManager.Tests.Editor
{
    public class LocalDataPathTests
    {
        [Test]
        public void GetPathFor_AppendsTheExtensionToABareName()
        {
            Assert.AreEqual(
                LocalDataPath.RootDirectory + "/PlayerData" + LocalDataPath.Extension,
                LocalDataPath.GetPathFor("PlayerData"));
        }

        [Test]
        public void GetPathFor_DoesNotStripAnExtensionTheCallerPassed()
        {
            // Callers must pass a bare name; "PlayerData.txt" becomes "PlayerData.txt.txt".
            StringAssert.EndsWith("PlayerData.txt.txt", LocalDataPath.GetPathFor("PlayerData.txt"));
        }

        [Test]
        public void GetPathFor_KeepsSubfolders()
        {
            StringAssert.EndsWith("/slots/autosave.txt", LocalDataPath.GetPathFor("slots/autosave"));
        }

        [Test]
        public void GetPathFor_NormalizesBackslashesToForwardSlashes()
        {
            Assert.AreEqual(
                LocalDataPath.GetPathFor("slots/autosave"),
                LocalDataPath.GetPathFor("slots\\autosave"));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void GetPathFor_RejectsAnEmptyName(string fileName)
        {
            Assert.Throws<ArgumentException>(() => LocalDataPath.GetPathFor(fileName));
        }

        [TestCase("../outside")]
        [TestCase("slots/../../outside")]
        [TestCase("./here")]
        public void GetPathFor_RejectsNamesThatEscapeTheDataFolder(string fileName)
        {
            Assert.Throws<ArgumentException>(() => LocalDataPath.GetPathFor(fileName));
        }

        [TestCase("/rooted")]
        [TestCase("C:/rooted")]
        [TestCase("\\rooted")]
        public void GetPathFor_RejectsAbsoluteNames(string fileName)
        {
            Assert.Throws<ArgumentException>(() => LocalDataPath.GetPathFor(fileName));
        }

        [TestCase("slots//autosave")]
        [TestCase("what?")]
        [TestCase("star*")]
        [TestCase("pipe|name")]
        [TestCase("quote\"name")]
        [TestCase("trailing.")]
        [TestCase("trailing ")]
        public void GetPathFor_RejectsNamesThatAreNotPortable(string fileName)
        {
            Assert.Throws<ArgumentException>(() => LocalDataPath.GetPathFor(fileName));
        }

        [Test]
        public void RootDirectory_IsThePersistentDataPathEvenInTheEditor()
        {
            Assert.AreEqual(Application.persistentDataPath, LocalDataPath.RootDirectory);
            Assert.AreNotEqual(Application.dataPath, LocalDataPath.RootDirectory);
        }
    }
}
