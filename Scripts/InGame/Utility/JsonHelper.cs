using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class JsonHelper : MonoBehaviour
{
    [System.Serializable]
    private class Wrapper<T> { public T[] Items; }

    public static T[] FromJson<T>(string _json)
    {
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(_json);
        return wrapper.Items;
    }

    public static string ToJson<T>(T[] _array, bool _prettyPrint = true)
    {
        Wrapper<T> wrapper = new Wrapper<T>();
        wrapper.Items = _array;
        return JsonUtility.ToJson(wrapper, _prettyPrint);
    }

    public static int ItemCount(string json)
    {
        return JArray.Parse(json).Count;
    }
}
