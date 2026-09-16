using UnityEngine;

namespace LB.LocalDataManager.Samples
{
    /// <summary>
    /// Saves a <see cref="PlayerData"/>, loads it back, and deletes it again. Drop this on a
    /// GameObject and press Play.
    /// </summary>
    public class LocalDataSample : MonoBehaviour
    {
        private const string FileName = "PlayerData";

        private void Start()
        {
            Debug.Log("Saving to: " + LocalData.GetPath(FileName));

            if (!LocalData.Save(new PlayerData { Id = 1, Name = "Halil" }, FileName))
            {
                return;
            }

            // TryLoad tells "no save yet" apart from "the save is damaged"; Load with a
            // fallback is the shorter version when you do not care which it was.
            PlayerData loaded;
            if (LocalData.TryLoad(FileName, out loaded))
            {
                Debug.Log(loaded.Id);
                Debug.Log(loaded.Name);
            }
            else
            {
                Debug.Log("Nothing saved yet.");
            }

            Debug.Log("Exists: " + LocalData.Exists(FileName));
            Debug.Log("Deleted: " + LocalData.Delete(FileName));
        }
    }
}
