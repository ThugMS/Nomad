using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;

public class CartEventListener : MonoBehaviour
{
    [SerializeField] private CartEventChannelSO m_channel;
    [SerializeField] private UnityEvent<NomadCartBase> m_onEventRaised;

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

    private void Respond(NomadCartBase _cart)
    {
        m_onEventRaised.Invoke(_cart);
    }
}

