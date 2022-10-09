using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public  class JsonParser
{    
    public static T[] ParseToTArray<T>(string _json)
    {
        try
        {
            var dataArray = JsonHelper.FromJson<T>("{\"Items\":" + _json + "}");
            return dataArray;
        }
        catch (System.Exception _e)
        {
            T[] failed = null;
            Debug.LogError(_e);
            return failed;
        }
    }

    //Item이 이미 있는 json 자료형
    public static T[] ParseToTArrayContainsItem<T>(string _json)
    {
        try
        {
            var dataArray = JsonHelper.FromJson<T>(_json);
            return dataArray;
        }
        catch (System.Exception _e)
        {
            T[] failed = null;
            Debug.LogError(_e);
            return failed;
        }
    }

    public static void SaveDataAsJson<T>(T[] _dataArray, string _fileName)
    {
        string data = JsonHelper.ToJson<T>(_dataArray);
        File.WriteAllText(Application.dataPath + "/Resources/Json/" + _fileName + ".json", data);
    }
}
