using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class OperationKeyInfo : MonoBehaviour
{
    #region PrivateVariable
    [SerializeField] private GameObject m_operationKeyInfoPanel;
    [SerializeField] private CustomIngameButton m_exitButton;
    [SerializeField] private CustomIngameButton m_keyGuideButton;
    private bool m_activePanel = false;
    #endregion

    private void Awake()
    {
        InputManager.Instance.AddKeyDownAction(EAction.Tab, ShowOperationInfo);
        InputManager.Instance.AddKeyUpAction(EAction.Tab, HideOperationInfo);
        m_exitButton.onClick.AddListener(HideOperationInfo);
        m_keyGuideButton.onClick.AddListener(ShowOperationInfo);
    }

    private void ShowOperationInfo()
    {
        m_activePanel = true;
        m_operationKeyInfoPanel.SetActive(m_activePanel);
    }
    public void HideOperationInfo()
    {
        m_activePanel = false;
        m_operationKeyInfoPanel.SetActive(m_activePanel);
    }
}
