using UnityEngine;

namespace LB.LocalDataManager.Samples
{
    /// <summary>
    /// Saves a <see cref="PlayerData"/> and loads it straight back. Drop this on a
    /// GameObject and press Play.
    /// </summary>
    public class LocalDataSample : MonoBehaviour
    {
        private const string FileName = "PlayerData";

        private void Start()
        {
            var playerData = new PlayerData { Id = 1, Name = "Halil" };

            var saver = new LocalDataSaver();
            if (!saver.SaveData(playerData, FileName))
            {
                return;
            }

            var loader = new LocalDataLoader();
            var loadedData = loader.LoadData<PlayerData>(FileName);

            Debug.Log(loadedData.Id);
            Debug.Log(loadedData.Name);
            Debug.Log("Saved to: " + LocalDataPath.GetPathFor(FileName));
        }
    }
}
