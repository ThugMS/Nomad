using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Events/int[] Event Channel")]
public class IntArrayEventChannelSO : EventChannelBaseSO
{
    private Action<int[]> m_onEventRaised;

    public void RaiseEvent(int[] _array)
    {
        m_onEventRaised.Invoke(_array);
    }

    public void AddEventRaise(Action<int[]> _action)
    {
        m_onEventRaised += _action;
    }

    public void RemoveEventRaise(Action<int[]> _action)
    {
        m_onEventRaised -= _action;
    }
}
