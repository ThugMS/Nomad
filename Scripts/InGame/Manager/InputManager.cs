using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InputManager : Singleton<InputManager>, IManageable
{
    #region PrivateVariables
    private Action<float, float> m_moveAction;

    private Dictionary<EAction, Action> m_mousePressActionDics = new Dictionary<EAction, Action>();
    private Dictionary<EAction, Action> m_mouseDownActionDics = new Dictionary<EAction, Action>();
    private Dictionary<EAction, Action> m_mouseUpActionDics = new Dictionary<EAction, Action>();

    private Dictionary<EAction, Action> m_keyPressActionDics = new Dictionary<EAction, Action>();
    private Dictionary<EAction, Action> m_keyDownActionDics = new Dictionary<EAction, Action>();
    private Dictionary<EAction, Action> m_keyUpActionDics = new Dictionary<EAction, Action>();

    private List<EAction> m_gameInputs = new List<EAction>();
    private List<EAction> m_otherInputs = new List<EAction>();

    [SerializeField] LayerMask m_canClickLayer;

    private bool m_isChating;
    private bool m_isGameStop = true;
    private bool m_isMouseUIClicking = false;


    #endregion

    void Awake()
    {
        AddKeySetting(EAction.NomadInteraction, KeyCode.E);
        AddMouseSetting(EAction.UsingTool, 0);

        AddKeySetting(EAction.Gun, KeyCode.Alpha1);
        AddKeySetting(EAction.Pick, KeyCode.Alpha2);
        AddKeySetting(EAction.Extractor, KeyCode.Alpha3);
        AddKeySetting(EAction.Repair, KeyCode.Alpha4);

        AddKeySetting(EAction.Enter, KeyCode.Return, false);
        AddKeySetting(EAction.Tab, KeyCode.Tab, false);
        AddKeySetting(EAction.Esc, KeyCode.Escape, false);
}
    void Update()
    {
        if (m_isGameStop == true)
            return;

        if (m_isChating == false)
        {
            OnKeyboard(m_gameInputs);
            OnMouseInput(m_gameInputs);
        }
        
        OnKeyboard(m_otherInputs);

    }

    void FixedUpdate()
    {
        if (m_isGameStop == true)
            return;

        OnMoveKeyboard(m_isChating == false);
    }

    #region PrivateMethod
    private void AddKeySetting(EAction _action, KeyCode _newKey, bool isGameInput = true)
    {
        KeySettings.AddKey(_action, _newKey);

        if (isGameInput)
            m_gameInputs.Add(_action);
        else
            m_otherInputs.Add(_action);
    }
    private void AddMouseSetting(EAction _action, int _mouseNum, bool isGameInput = true)
    {
        KeySettings.AddMouse(_action, _mouseNum);

        if (isGameInput)
            m_gameInputs.Add(_action);
        else
            m_otherInputs.Add(_action);
    }

    private void OnMoveKeyboard(bool isActive)
    {
        if (m_moveAction == null)
        {
            //Debug.LogError("move 키에 등록된 Action이 없음");
            return;
        }
        float moveX = Input.GetAxisRaw(ConstStringStorage.HORIZONTAL);
        float moveY = Input.GetAxisRaw(ConstStringStorage.VERTICAL);

        if (Mathf.Abs(moveX) < 0.01)
            moveX = 0;
        if (Mathf.Abs(moveY) < 0.01)
            moveY = 0;

        if (isActive == false)
        {
            moveX = 0; 
            moveY = 0;
        }
        
        m_moveAction.Invoke(moveX, moveY);
    }
    private void OnKeyboard(List<EAction> _inputs)
    {
        foreach(EAction _keyAction in _inputs)
        {
            if (!KeySettings.m_defaultKey.ContainsKey(_keyAction))
                continue;

            KeyCode keyCode = KeySettings.m_defaultKey[_keyAction];

            if (Input.GetKey(keyCode))
            {
                if (m_keyPressActionDics.ContainsKey(_keyAction) && m_keyPressActionDics[_keyAction] != null)
                    m_keyPressActionDics[_keyAction].Invoke();

            }

            if (Input.GetKeyDown(keyCode))
            {
                if (m_keyDownActionDics.ContainsKey(_keyAction) && m_keyDownActionDics[_keyAction] != null)
                    m_keyDownActionDics[_keyAction].Invoke();
            }

            if (Input.GetKeyUp(keyCode))
            {
                if (m_keyUpActionDics.ContainsKey(_keyAction) && m_keyUpActionDics[_keyAction] != null)
                    m_keyUpActionDics[_keyAction].Invoke();
            }
        }
    }

    private void OnMouseInput(List<EAction> _inputs)
    {
        foreach (EAction _keyAction in _inputs)
        {
            if (!KeySettings.m_defaultMouse.ContainsKey(_keyAction))
                continue;

            int mouseNum = KeySettings.m_defaultMouse[_keyAction];

            if (Input.GetMouseButtonUp(mouseNum))
            {
                m_isMouseUIClicking = false;
                if (m_mouseUpActionDics.ContainsKey(_keyAction) && m_mouseUpActionDics[_keyAction] != null)
                    m_mouseUpActionDics[_keyAction].Invoke();
            }
            if (m_isMouseUIClicking == true)
                continue;

            if (Input.GetMouseButtonDown(mouseNum))
            {
                if (EventSystem.current.IsPointerOverGameObject())
                {
                    m_isMouseUIClicking = true;
                    continue;
                }

                if (m_mouseDownActionDics.ContainsKey(_keyAction) && m_mouseDownActionDics[_keyAction] != null)
                    m_mouseDownActionDics[_keyAction].Invoke();
            }
            if (Input.GetMouseButton(mouseNum))
            {
                if(m_mousePressActionDics.ContainsKey(_keyAction) && m_mousePressActionDics[_keyAction] != null)
                    m_mousePressActionDics[_keyAction].Invoke();
            }
        }
    }
    #endregion

    #region PublicVariables

    public void SetStopAllInput(bool _stop)
    {
        m_isChating = _stop;
    }

    public void AddKeyDownAction(EAction _eAction, Action _action)
    {
        if (!m_keyDownActionDics.ContainsKey(_eAction))
        {
            m_keyDownActionDics.Add(_eAction, _action);
            return;
        }

        m_keyDownActionDics[_eAction] += _action;
    }
    public void RemoveKeyDownAction(EAction _eAction, Action _action)
    {
        if (!m_keyDownActionDics.ContainsKey(_eAction))
        {
            Debug.LogError("AddKeyAction : " + _eAction + " 키가 포함되어 있지 않음.");
            return;
        }
        m_keyDownActionDics[_eAction] -= _action;
    }

    public void AddKeyPressAction(EAction _eAction, Action _action)
    {
        if (!m_keyPressActionDics.ContainsKey(_eAction))
        {
            m_keyPressActionDics.Add(_eAction, _action);
            return;
        }

        m_keyPressActionDics[_eAction] += _action;
    }
    public void RemoveKeyPressAction(EAction _eAction, Action _action)
    {
        if (!m_keyPressActionDics.ContainsKey(_eAction))
        {
            Debug.LogError("AddKeyAction : " + _eAction + " 키가 포함되어 있지 않음.");
            return;
        }
        m_keyPressActionDics[_eAction] -= _action;
    }

    public void AddKeyUpAction(EAction _eAction, Action _action)
    {
        if (!m_keyUpActionDics.ContainsKey(_eAction))
        {
            m_keyUpActionDics.Add(_eAction, _action);
            return;
        }

        m_keyUpActionDics[_eAction] += _action;
    }
    public void RemoveKeyUpAction(EAction _eAction, Action _action)
    {
        if (!m_keyUpActionDics.ContainsKey(_eAction))
        {
            Debug.LogError("AddKeyAction : " + _eAction + " 키가 포함되어 있지 않음.");
            return;
        }
        m_keyUpActionDics[_eAction] -= _action;
    }

    public void AddMouseDownAction(EAction _eAction, Action _action)
    {
        if (!m_mouseDownActionDics.ContainsKey(_eAction))
        {
            m_mouseDownActionDics.Add(_eAction, _action);
            return;
        }

        m_mouseDownActionDics[_eAction] += _action;
    }
    public void RemoveMouseDownAction(EAction _eAction, Action _action)
    {
        if (!m_mouseDownActionDics.ContainsKey(_eAction))
        {
            Debug.LogError("AddmouseAction : " + _eAction + " 키가 포함되어 있지 않음.");
            return;
        }
        m_mouseDownActionDics[_eAction] -= _action;
    }

    public void AddMousePressAction(EAction _eAction, Action _action)
    {
        if (!m_mousePressActionDics.ContainsKey(_eAction))
        {
            m_mousePressActionDics.Add(_eAction, _action);
            return;
        }

        m_mousePressActionDics[_eAction] += _action;
    }
    public void RemoveMousePressAction(EAction _eAction, Action _action)
    {
        if (!m_mousePressActionDics.ContainsKey(_eAction))
        {
            Debug.LogError("AddmouseAction : " + _eAction + " 키가 포함되어 있지 않음.");
            return;
        }
        m_mousePressActionDics[_eAction] -= _action;
    }

    public void AddMouseUpAction(EAction _eAction, Action _action)
    {
        if (!m_mouseUpActionDics.ContainsKey(_eAction))
        {
            m_mouseUpActionDics.Add(_eAction, _action);
            return;
        }

        m_mouseUpActionDics[_eAction] += _action;
    }
    public void RemoveMouseUpAction(EAction _eAction, Action _action)
    {
        if (!m_mouseUpActionDics.ContainsKey(_eAction))
        {
            Debug.LogError("AddmouseAction : " + _eAction + " 키가 포함되어 있지 않음.");
            return;
        }
        m_mouseUpActionDics[_eAction] -= _action;
    }

    public void AddMoveAction(Action<float, float> _moveAction)
    {
        m_moveAction += _moveAction;
    }
    public void RemoveMoveAction(Action<float, float> _moveAction)
    {
        m_moveAction -= _moveAction;
    }

    public void InitializeObject()
    {

    }

    public void StartGame()
    {
        m_isGameStop = false;
    }

    public void StopGame()
    {
        m_isGameStop = true;
    }
    #endregion
}
