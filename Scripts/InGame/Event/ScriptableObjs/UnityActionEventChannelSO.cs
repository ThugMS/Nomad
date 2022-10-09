using UnityEngine;
using UnityEngine.Events;
using System;

[CreateAssetMenu(menuName = "Events/UnityAction Event Channel")]
public class UnityActionEventChannelSO : EventChannelBaseSO
{
    private Action<UnityAction> m_onEventRaised;

    public void RaiseEvent(UnityAction _action)
    {
        m_onEventRaised.Invoke(_action);
    }

    public void AddEventRaise(Action<UnityAction> _action)
    {
        m_onEventRaised += _action;
    }

    public void RemoveEventRaise(Action<UnityAction> _action)
    {
        m_onEventRaised -= _action;
    }
}
