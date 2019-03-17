using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LB_LocalDataSaver
{
    public void SaveData<T>(T dataObject)
    {
#if UNITY_EDITOR
        string path = Application.dataPath + "/LocalData.txt";
#else
        string path = Application.persistentDataPath + "/LocalData.txt";
#endif

        var data = JsonUtility.ToJson(dataObject);
        var cryptoManager = new LB_CryptoManager();
        var encryptedData = cryptoManager.Encrypt(data);

        try
        {
            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(fs))
                {
                    writer.Write(encryptedData);
                }
            }
        }
        catch (System.Exception ex)
        {
            //todo send error to remote analytic tool such as Crashlytics
        }
    }


}
