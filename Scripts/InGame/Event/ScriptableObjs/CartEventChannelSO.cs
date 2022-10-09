using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
[CreateAssetMenu(menuName = "Events/Cart Event Channel")]
public class CartEventChannelSO : EventChannelBaseSO
{
    private Action<NomadCartBase> m_onEventRaised;

    public void RaiseEvent(NomadCartBase _cart)
    {
        m_onEventRaised.Invoke(_cart);
    }

    public void AddEventRaise(Action<NomadCartBase> _action)
    {
        m_onEventRaised += _action;
    }

    public void RemoveEventRaise(Action<NomadCartBase> _action)
    {
        m_onEventRaised -= _action;
    }
}
