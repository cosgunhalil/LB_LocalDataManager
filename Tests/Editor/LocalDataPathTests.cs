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
        public void RootDirectory_IsTheProjectAssetsFolderInTheEditor()
        {
            Assert.AreEqual(Application.dataPath, LocalDataPath.RootDirectory);
        }
    }
}
