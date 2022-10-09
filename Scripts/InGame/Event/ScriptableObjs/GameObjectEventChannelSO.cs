using System.Collections;
using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Events/GameObject Event Channel")]
public class GameObjectEventChannelSO : EventChannelBaseSO
{
    private Action<GameObject> m_onEventRaised;

    public void RaiseEvent(GameObject _obj)
    {
        m_onEventRaised.Invoke(_obj);
    }

    public void AddEventRaise(Action<GameObject> _action)
    {
        m_onEventRaised += _action;
    }

    public void RemoveEventRaise(Action<GameObject> _action)
    {
        m_onEventRaised -= _action;
    }
}