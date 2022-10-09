using UnityEngine;

public class EventChannelBaseSO : ScriptableObject
{
    [TextArea] public string m_attachedScene;
    [TextArea] public string m_attachedObject;
    [TextArea] public string m_functionDescription;
}
