
using UnityEngine;
using UnityEngine.Events;

public class MonsterPoolListener : MonoBehaviour
{
    [SerializeField] private MonsterPoolChannelSO m_channel;
    [SerializeField] private UnityEvent<MonsterBase> m_onEventRaised;

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

    private void Respond(MonsterBase _monster)
    {
        m_onEventRaised.Invoke(_monster);
    }
}
