using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Events/UI/Player Oxygen UI Event Channel")]
public class PlayerOxygenUIChannelSO : EventChannelBaseSO
{
    private Action<float, float> m_onEventRaised;

    public void RaiseEvent(float _float1, float _float2)
    {
        m_onEventRaised.Invoke(_float1, _float2);
    }

    public void AddEventRaise(Action<float, float> _action)
    {
        m_onEventRaised += _action;
    }

    public void RemoveEventRaise(Action<float, float> _action)
    {
        m_onEventRaised -= _action;
    }
}
