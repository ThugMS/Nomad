using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class FloatEventListener : MonoBehaviour
{

    [SerializeField] private FloatEventChannelSO m_channel;
    [SerializeField] private UnityEvent<float> m_onEventRaised;

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

    private void Respond(float _float)
    {
        m_onEventRaised.Invoke(_float);
    }
}
