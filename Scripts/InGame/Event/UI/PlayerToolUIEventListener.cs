using UnityEngine;
using UnityEngine.Events;

public class PlayerToolUIEventListener : MonoBehaviour
{
    [SerializeField] private PlayerToolUIChannelSO m_channel;
    [SerializeField] private UnityEvent<ToolBase> m_onEventRaised;

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

    private void Respond(ToolBase _currentTool)
    {
        m_onEventRaised.Invoke(_currentTool);
    }
}