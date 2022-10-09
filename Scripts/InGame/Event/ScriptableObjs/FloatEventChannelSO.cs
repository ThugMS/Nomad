using System.Collections;
using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Events/Float Event Channel")]
public class FloatEventChannelSO : EventChannelBaseSO
{
    private Action<float> m_onEventRaised;

    public void RaiseEvent(float _float)
    {
        m_onEventRaised.Invoke(_float);
    }

    public void AddEventRaise(Action<float> _action)
    {
        m_onEventRaised += _action;
    }

    public void RemoveEventRaise(Action<float> _action)
    {
        m_onEventRaised -= _action;
    }
}