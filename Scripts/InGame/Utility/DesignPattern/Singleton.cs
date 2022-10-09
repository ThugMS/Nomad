using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
        //간략히
        protected static T m_instance;

        public static T Instance
        {
            get
            {
                return GetInstance();
            }
        }
        public static T GetInstance()
        {
            if (m_instance == null)
            {
                m_instance = (T)FindObjectOfType(typeof(T));

            }
            return m_instance;
        }
}

