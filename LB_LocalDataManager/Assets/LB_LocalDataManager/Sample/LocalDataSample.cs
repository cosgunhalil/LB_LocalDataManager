using UnityEngine;

public class LocalDataSample : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var playerData = new PlayerData {Id = 1, Name = "Halil" };

        LB_LocalDataSaver dataSaver = new LB_LocalDataSaver();
        dataSaver.SaveData(playerData, "PlayerData");

        LB_LocalDataLoader dataLoader = new LB_LocalDataLoader();
        var loadedData = dataLoader.LoadData<PlayerData>("PlayerData");

        Debug.Log(loadedData.Id);
        Debug.Log(loadedData.Name);
    }
}
