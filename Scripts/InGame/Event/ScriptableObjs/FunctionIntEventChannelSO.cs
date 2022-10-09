using System.Collections;
using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Events/Function Int Event Channel")]
public class FunctionIntEventChannelSO : EventChannelBaseSO
{
    private Action<Func<int>> m_onEventRaised;
     public void RaiseEvent(Func<int> _funcInt)
    {
        m_onEventRaised.Invoke(_funcInt);
    }

    public void AddEventRaise(Action<Func<int>> _action)
    {
        m_onEventRaised += _action;
    }

    public void RemoveEventRaise(Action<Func<int>> _action)
    {
        m_onEventRaised -= _action;
    }
}