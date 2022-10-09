using UnityEngine;
using UnityEngine.Events;

public class BoolEventListener : MonoBehaviour
{
    [SerializeField] private BoolEventChannelSO m_channel;
    [SerializeField] private UnityEvent<bool> m_onEventRaised;

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

    private void Respond(bool _bool)
    {
        m_onEventRaised.Invoke(_bool);
    }
}
