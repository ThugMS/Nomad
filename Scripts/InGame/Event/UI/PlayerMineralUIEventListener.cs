using UnityEngine;
using UnityEngine.Events;

public class PlayerMineralUIEventListener : MonoBehaviour
{
    [SerializeField] private PlayerMineralUIChannelSO m_channel;
    [SerializeField] private UnityEvent<int, int> m_onEventRaised;

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

    private void Respond(int _amount, int _max)
    {
        m_onEventRaised.Invoke(_amount, _max);
    }
}