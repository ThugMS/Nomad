using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class IntArrayEventListener : MonoBehaviour
{
    [SerializeField] private IntArrayEventChannelSO m_channel;
    [SerializeField] private UnityEvent<int[]> m_onEventRaised;

    private void OnEnable()
    {
        if (m_channel == null)
            return;

        m_channel.AddEventRaise(Respond);
    }

    private void OnDisable()
    {
        if (m_channel == null)
            return;

        m_channel.RemoveEventRaise(Respond);
    }

    private void Respond(int[] _array)
    {
        m_onEventRaised.Invoke(_array);
    }
}