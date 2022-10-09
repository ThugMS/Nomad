using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Events/UI/Upgrade Button Active UI Event Channel")]
public class UpgradeButtonActiveChannelSO : EventChannelBaseSO
{
    private Action<string, bool> m_onEventRaised;

    public void RaiseEvent(string _id, bool _upgradable)
    {
        m_onEventRaised.Invoke(_id, _upgradable);
    }

    public void AddEventRaise(Action<string, bool> _action)
    {
        m_onEventRaised += _action;
    }

    public void RemoveEventRaise(Action<string, bool> _action)
    {
        m_onEventRaised -= _action;
    }
}
