using UnityEngine;
using UnityEngine.Events;

public class PlayerOxygenUIEventListener : MonoBehaviour
{
    [SerializeField] private PlayerOxygenUIChannelSO m_channel;
    [SerializeField] private UnityEvent<float, float> m_onEventRaised;

    private void OnEnable()
    {
        if(m_channel == null)
            return;

        m_channel.AddEventRaise(Respond);
    }

    private void OnDisable()
    {
        if(m_channel == null)
            return;

        m_channel.RemoveEventRaise(Respond);
    }

    private void Respond(float _curretOxygen, float _maxOxygen)
    {
        m_onEventRaised.Invoke(_curretOxygen, _maxOxygen);
    }
}
