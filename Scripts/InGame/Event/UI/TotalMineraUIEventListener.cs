using UnityEngine;
using UnityEngine.Events;

public class TotalMineraUIEventListener : MonoBehaviour
{
    [SerializeField] private TotalMineraUIChannelSO m_channel;
    [SerializeField] private UnityEvent<MineralType, int, int> m_onEventRaised;

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

    private void Respond(MineralType _type, int _amount, int _maxAmount)
    {
        m_onEventRaised.Invoke(_type, _amount, _maxAmount);
    }
}
