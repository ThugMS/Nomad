using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Events/Monster/Monster Pool Event Channel")]
public class MonsterPoolChannelSO : EventChannelBaseSO
{
    private Action<MonsterBase> m_onEventRaised;

    public void RaiseEvent(MonsterBase _monster)
    {
        m_onEventRaised?.Invoke(_monster);
    }

    public void AddEventRaise(Action<MonsterBase> _action)
    {
        m_onEventRaised += _action;
    }

    public void RemoveEventRaise(Action<MonsterBase> _action)
    {
        m_onEventRaised -= _action;
    }
}
