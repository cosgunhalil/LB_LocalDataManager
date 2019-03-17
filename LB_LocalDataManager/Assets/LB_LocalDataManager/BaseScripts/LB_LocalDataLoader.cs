using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LB_LocalDataLoader : MonoBehaviour
{
    public T LoadData<T>(string fileName)
    {
#if UNITY_EDITOR
        string path = Application.dataPath + "/" + fileName + ".txt";
#else
        string path = Application.persistentDataPath + "/LocalData.txt";
#endif
        var data = ReadDataFromPath(path);
        return JsonUtility.FromJson<T>(data);
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
