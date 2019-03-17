using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LB_LocalDataSaver
{
    public void SaveData<T>(T dataObject, string fileName)
    {
#if UNITY_EDITOR
        string path = Application.dataPath + "/" + fileName + ".txt";
#else
        string path = Application.persistentDataPath + "/" + fileName + ".txt";
#endif

        var data = JsonUtility.ToJson(dataObject);

        try
        {
            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(fs))
                {
                    writer.Write(data);
                }
            }
        }
        catch (System.Exception ex)
        {
            //todo send error to remote analytic tool such as Crashlytics
        }
    }


}
