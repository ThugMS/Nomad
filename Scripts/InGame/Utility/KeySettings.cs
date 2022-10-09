using System;
using System.Collections.Generic;
using UnityEngine;

public enum EAction 
{
    Gun = 1, Pick, Extractor, Repair, NomadInteraction, UsingTool, Enter, Tab, Esc
}

public class KeySettings
{
    public static Dictionary<EAction, KeyCode> m_defaultKey = new Dictionary<EAction, KeyCode>();
    public static Dictionary<KeyCode, Action> m_keyAction = new Dictionary<KeyCode, Action>();

    public static Dictionary<EAction, int> m_defaultMouse = new Dictionary<EAction, int>();
    public static Dictionary<KeyCode, int> m_MouseAction = new Dictionary<KeyCode, int>();

    public static void ChangeKeyCode(EAction _action, KeyCode _newKey)
    {
        if (m_defaultKey.ContainsKey(_action))
            m_defaultKey[_action] = _newKey;
    }

    public static void AddKey(EAction _action, KeyCode _keyCode)
    {
        if (!m_defaultKey.ContainsKey(_action) && !m_defaultKey.ContainsValue(_keyCode))
            m_defaultKey.Add(_action, _keyCode);
    }
    public static void AddMouse(EAction _action, int _mouseNum)
    {
        if (!m_defaultMouse.ContainsKey(_action) && !m_defaultMouse.ContainsValue(_mouseNum))
            m_defaultMouse.Add(_action, _mouseNum);
    }

    public static void AddAction(KeyCode _keyCode, Action _act)
    {
        if(!m_keyAction.ContainsKey(_keyCode))
            m_keyAction.Add(_keyCode, _act);
        else
            m_keyAction[_keyCode] = _act;
    }

    public static Action GetAction(EAction _action)
    {
        return m_keyAction[m_defaultKey[_action]];
    }
}
