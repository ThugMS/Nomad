using System.Collections;
using UnityEngine;

    public static class CoroutineHelper
    {
        public static IEnumerator WaitForSeconds(float _time)
        {
            float endTime = Time.time + _time;
            while (Time.time < endTime)
                yield return null;
        }

        public static IEnumerator WaitForUntil(System.Func<bool> _func)
        {
            while (!_func())
                yield return null;
        }
    }

