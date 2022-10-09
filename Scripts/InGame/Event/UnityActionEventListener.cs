using UnityEngine;
using UnityEngine.Events;

public class UnityActionEventListener : MonoBehaviour
{
    [SerializeField] private UnityActionEventChannelSO m_channel;
    [SerializeField] private UnityEvent<UnityAction> m_onEventRaised;

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

    private void Respond(UnityAction _unityAction)
    {
        m_onEventRaised.Invoke(_unityAction);
    }
}
