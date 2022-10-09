using UnityEngine;
using UnityEngine.Events;

public class VoidEventListener : MonoBehaviour
{
    [SerializeField] private VoidEventChannelSO m_channel;
    [SerializeField] private UnityEvent m_onEventRaised;

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

    private void Respond()
    {
        m_onEventRaised.Invoke();
    }
}