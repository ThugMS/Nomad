using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Events/IntBool Event Channel")]
public class MineralRequestResultEventChannelSO : EventChannelBaseSO
{
    private Action<int,int,  bool> m_onEventRaised;
    public void RaiseEvent(int _actorName, int _objectId, bool _isSuccess)
    {
        m_onEventRaised.Invoke(_actorName, _objectId, _isSuccess);
    }

    public void AddEventRaise(Action<int,int, bool> _action)
    {
        m_onEventRaised += _action;
    }

    public void RemoveEventRaise(Action<int, int, bool> _action)
    {
        m_onEventRaised -= _action;
    }
}
