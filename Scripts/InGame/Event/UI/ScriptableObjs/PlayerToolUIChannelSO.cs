using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Events/UI/Player Tool UI Event Channel")]
public class PlayerToolUIChannelSO : EventChannelBaseSO
{
    private Action<ToolBase> m_onEventRaised;

    public void RaiseEvent(ToolBase _toolBase)
    {
        m_onEventRaised.Invoke(_toolBase);
    }

    public void AddEventRaise(Action<ToolBase> _action)
    {
        m_onEventRaised += _action;
    }

    public void RemoveEventRaise(Action<ToolBase> _action)
    {
        m_onEventRaised -= _action;
    }
}