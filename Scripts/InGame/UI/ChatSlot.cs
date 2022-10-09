using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChatSlot : MonoBehaviour
{
    private enum State
    {
        ActiveTrue, ActiveFalse, Fade
    }

    #region PrivateVariable
    [SerializeField] private TMP_Text m_contentText;
    private Color m_textColor;

    private State m_activeState = State.ActiveTrue;
    private float m_currentStayTime = 0;
    private float m_stayTime = 10;

    private Color m_greyColor = new Color(0.7f, 0.7f, 0.7f);
    #endregion

    private void Awake()
    {
        m_textColor = m_contentText.color;
    }

    private void FixedUpdate()
    {
        if (m_activeState != State.Fade)
            return;

        m_currentStayTime += Time.fixedDeltaTime;
        SetTextAlphaColor((m_stayTime - m_currentStayTime) / m_stayTime);
        if (m_currentStayTime >= m_stayTime)
        {
            ChangeState(State.ActiveFalse);
        }
    }

    #region PrivateMethod
    private void SetContentText(string _contentText)
    {
        m_contentText.text = _contentText;
    }
    private void SetTextColor(Color _color)
    {
        m_contentText.color = _color;
    }
    private void SetTextAlphaColor(float _alpha)
    {
        m_textColor.a = _alpha;
        SetTextColor(m_textColor);
    }
    private void ChangeState(State _state)
    {
        switch (_state)
        {
            case State.ActiveTrue:
                SetTextAlphaColor(1);
                break;
            case State.ActiveFalse:
                SetTextAlphaColor(0);
                break;
            case State.Fade:
                SetTextAlphaColor(1);
                m_currentStayTime = 0;
                break;
        }

        m_activeState = _state;
    }
    #endregion

    #region PublicMethod
    public void SetMessage(string _nickName, string _content, int _chatType)
    {
        SetActiveTrue();
        SetContentText(_nickName + " : " + _content);

        if (_chatType == 0) //danger
            m_textColor = Color.red;
        else if (_chatType == 1) // 채팅
            m_textColor = Color.white;
        else if (_chatType == 2) // 가이드
            m_textColor = Color.cyan;
        else // 알림
            m_textColor = m_greyColor;
    }

    //TODO : chat info 스타일로 수정
    public void SetUserChat(string _nickName, string _content)
    {
        SetActiveTrue();
        SetContentText(_nickName + " : " + _content);
    }
    public void SetSystemChat(string _content)
    {
        SetActiveTrue();
        SetContentText("[시스템] " + _content);
    }
    public void SetActiveTrue()
    {
        ChangeState(State.ActiveTrue);
    }
    public void SetFade()
    {
        ChangeState(State.Fade);
    }
    #endregion
}
