using UnityEngine;

public class LocalDataSample : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var playerData = new PlayerData {Id = 1, Name = "Halil" };

        LB_LocalDataSaver dataSaver = new LB_LocalDataSaver();
        dataSaver.SaveData(playerData);

        LB_LocalDataLoader dataLoader = new LB_LocalDataLoader();
        var loadedData = dataLoader.LoadData<PlayerData>("LocalData");

        Debug.Log(loadedData.Id);
        Debug.Log(loadedData.Name);
    }
}
