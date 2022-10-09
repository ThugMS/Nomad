using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Events/Void Event Channel")]
public class VoidEventChannelSO : EventChannelBaseSO
{
    private Action m_onEventRaised;
    public void RaiseEvent()
    {
        m_onEventRaised?.Invoke();
    }

    public void AddEventRaise(Action _action)
    {
        m_onEventRaised += _action;
    }

    public void RemoveEventRaise(Action _action)
    {
        m_onEventRaised -= _action;
    }
}
