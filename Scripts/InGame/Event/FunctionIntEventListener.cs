using UnityEngine;
using UnityEngine.Events;
using System;

public class FunctionIntEventListener : MonoBehaviour
{

    [SerializeField] private FunctionIntEventChannelSO m_channel;
    [SerializeField] private UnityEvent<Func<int>> m_onEventRaised;

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

    private void Respond(Func<int> _nickName)
    {
        m_onEventRaised.Invoke(_nickName);
    }
}