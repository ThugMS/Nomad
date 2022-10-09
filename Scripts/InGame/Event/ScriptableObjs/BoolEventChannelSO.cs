using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Events/Void Event Channel")]
public class BoolEventChannelSO : EventChannelBaseSO
{
    private Action<bool> m_onEventRaised;
    public void RaiseEvent(bool _bool)
    {
        m_onEventRaised.Invoke(_bool);
    }

    public void AddEventRaise(Action<bool> _action)
    {
        m_onEventRaised += _action;
    }

    public void RemoveEventRaise(Action<bool> _action)
    {
        m_onEventRaised -= _action;
    }
}
