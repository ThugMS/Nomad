using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Events/UI/Total Mineral UI Event Channel")]
public class TotalMineraUIChannelSO : EventChannelBaseSO
{
    private Action<MineralType, int, int> m_onEventRaised;

    public void RaiseEvent(MineralType _type, int _amount, int _maxAmount)
    {
        m_onEventRaised.Invoke(_type, _amount, _maxAmount);
    }

    public void AddEventRaise(Action<MineralType, int, int> _action)
    {
        m_onEventRaised += _action;
    }

    public void RemoveEventRaise(Action<MineralType, int, int> _action)
    {
        m_onEventRaised -= _action;
    }
}
