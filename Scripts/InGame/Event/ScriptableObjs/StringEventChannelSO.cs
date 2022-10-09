using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Events/String Event Channel")]
public class StringEventChannelSO : EventChannelBaseSO
{
    private Action<string> m_onEventRaised;

    public void RaiseEvent(string _str)
    {
        m_onEventRaised.Invoke(_str);
    }

    public void AddEventRaise(Action<string> _action)
    {
        m_onEventRaised += _action;
    }

    public void RemoveEventRaise(Action<string> _action)
    {
        m_onEventRaised -= _action;
    }
}