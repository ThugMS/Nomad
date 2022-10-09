using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Events/UI/Player Mineral UI Event Channel")]
public class PlayerMineralUIChannelSO : EventChannelBaseSO
{
    private Action<int, int> m_onEventRaised;

    public void RaiseEvent(int _int1, int _int2)
    {
        m_onEventRaised?.Invoke(_int1, _int2);
    }

    public void AddEventRaise(Action<int, int> _action)
    {
        m_onEventRaised += _action;
    }

    public void RemoveEventRaise(Action<int, int> _action)
    {
        m_onEventRaised -= _action;
    }
}
