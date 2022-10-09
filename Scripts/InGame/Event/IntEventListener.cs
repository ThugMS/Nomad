using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class IntEventListener : MonoBehaviour
{
    [SerializeField] private IntEventChannelSO m_channel;
    [SerializeField] private UnityEvent<int> m_onEventRaised;

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

    private void Respond(int _amount)
    {
        m_onEventRaised.Invoke(_amount);
    }
}