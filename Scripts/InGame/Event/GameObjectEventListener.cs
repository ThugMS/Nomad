using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class GameObjectEventListener : MonoBehaviour
{
    [SerializeField] private GameObjectEventChannelSO m_channel;
    [SerializeField] private UnityEvent<GameObject> m_onEventRaised;

    private void OnEnable()
    {
        if (m_channel == null)
            return;

        m_channel.AddEventRaise(Respond);
    }

    private void OnDisable()
    {
        if (m_channel == null)
            return;

        m_channel.RemoveEventRaise(Respond);
    }

    private void Respond(GameObject _obj)
    {
        m_onEventRaised.Invoke(_obj);
    }
}