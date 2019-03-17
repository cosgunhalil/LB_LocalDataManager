using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LB_LocalDataLoader : MonoBehaviour
{
    public T LoadData<T>()
    {
#if UNITY_EDITOR
        string path = Application.dataPath + "/LocalData.txt";
#else
        string path = Application.persistentDataPath + "/LocalData.txt";
#endif
        var data = ReadDataFromPath(path);

        var cryptoManager = new LB_CryptoManager();
        var decryptedData = cryptoManager.Decrypt(data);

        return (T)Convert.ChangeType(decryptedData, typeof(T));
    }

    public string ReadDataFromPath(string path)
    {
        string data = null;
        try
        {
            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                using (StreamReader reader = new StreamReader(fs))
                {
                    data = reader.ReadToEnd();
                }
            }
        }
        catch (System.Exception ex)
        {

        }

        return data;
    }

}
