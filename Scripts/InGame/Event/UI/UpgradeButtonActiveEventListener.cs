using UnityEngine;
using UnityEngine.Events;

public class UpgradeButtonActiveEventListener : MonoBehaviour
{
    [SerializeField] private UpgradeButtonActiveChannelSO m_channel;
    [SerializeField] private UnityEvent<string, bool> m_onEventRaised;

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

    private void Respond(string _id, bool _upgradable)
    {
        m_onEventRaised.Invoke(_id, _upgradable);
    }
}
