using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScriptableObjectSingletonSO<T> : ScriptableObject where T : ScriptableObjectSingletonSO<T>
{
    private static T m_instance;
    public static T Instance
    {
        get
        {
            if (m_instance != null)
                return m_instance;

            T[] assets = Resources.LoadAll<T>("ScriptableObject/Singleton");

            if (assets == null || assets.Length < 1)
                throw new System.Exception("싱글톤을 찾을 수 없습니다");

            m_instance = assets[0];
            return m_instance;
        }
    }
}
