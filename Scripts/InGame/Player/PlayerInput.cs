using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;

public class PlayerInput : MonoBehaviour
{
    #region PrivateVariables
    [SerializeField] private List<ToolBase> m_toolClasses;
    [SerializeField] private PlayerToolUIChannelSO m_playerToolUIChannelSO;
    [SerializeField] private UnityActionEventChannelSO m_playerToolUpgradeButtonChannelSO;

    private ToolBase m_currentTool;
    private Player m_player;
    private Animator m_animator;
    private PlayerController m_controller;
    private PhotonView m_photonView;
    #endregion

    private void Awake()
    {
        m_currentTool = m_toolClasses[0];

        m_player = GetComponent<Player>();
        m_animator = GetComponent<Animator>();
        m_controller = GetComponent<PlayerController>();
        m_photonView = GetComponent<PhotonView>();
        SetCurrentTool(EAction.Gun);
        m_animator.SetFloat(ConstStringStorage.PLAYER_ANIM_MOVE_Y, -1f);
    }

    #region PrivateMethod
    #region IndleInput
    private void GetKeyUpUsingTool()
    {
        m_animator.SetBool(ConstStringStorage.PLAYER_ANIM_USE_TOOL, false);
        SoundManager.Instance.StopLoopSFX();
        m_currentTool.CancleTool();
    }
    private void GetKeyDownUsingTool()
    {
        
    }

    private void GetKeyUsingTool()
    {
        m_currentTool.Use(m_player);

        Vector2 dir = m_player.GetForward();

        if (!dir.x.Equals(0) || !dir.y.Equals(0))
        {
            m_controller.SetAnimatorFloatValue(ConstStringStorage.PLAYER_ANIM_MOVE_X, dir.x);
            m_controller.SetAnimatorFloatValue(ConstStringStorage.PLAYER_ANIM_MOVE_Y, dir.y);
        }
    }
    private void GetKeyDownExtractor()
    {
        SetCurrentTool(EAction.Extractor);
    }
    private void GetKeyDownGun()
    {
        SetCurrentTool(EAction.Gun);
    }
    private void GetKeyDownPick()
    {
        SetCurrentTool(EAction.Pick);
    }
    private void GetKeyDownRepair()
    {
        SetCurrentTool(EAction.Repair);
    }
    #endregion
    private void SetCurrentTool(EAction _eAction)
    {
        if (m_photonView.IsMine == true)
        {
            m_photonView.RPC(nameof(RPC_SetCurrentTool), RpcTarget.AllBuffered, (int)_eAction);
            SetPlayerToolUI();
        }
    }
    [PunRPC]
    private void RPC_SetCurrentTool(int _toolType)
    {
        m_currentTool?.gameObject.SetActive(false);
        m_currentTool = m_toolClasses[_toolType - 1];
        m_currentTool.gameObject.SetActive(true);

        m_controller.SetAnimatorIntValue(ConstStringStorage.PLAYER_ANIM_CHANGE_TOOL_TYPE, _toolType - 1);
    }
    private void SetPlayerToolUI()
    {
        m_playerToolUIChannelSO.RaiseEvent(m_currentTool);
    }
    #endregion
    public void AddPlayerIdleInput()
    { 

        InputManager.Instance.AddKeyDownAction(EAction.Gun, GetKeyDownGun);
        InputManager.Instance.AddKeyDownAction(EAction.Pick, GetKeyDownPick);
        InputManager.Instance.AddKeyDownAction(EAction.Extractor, GetKeyDownExtractor);
        InputManager.Instance.AddKeyDownAction(EAction.Repair, GetKeyDownRepair);

        InputManager.Instance.AddMouseDownAction(EAction.UsingTool, GetKeyDownUsingTool);

        InputManager.Instance.AddMousePressAction(EAction.UsingTool, GetKeyUsingTool);
        InputManager.Instance.AddMouseUpAction(EAction.UsingTool, GetKeyUpUsingTool);
    }
    public void RemovePlayerIdleInput()
    {
        InputManager.Instance.RemoveKeyDownAction(EAction.Gun, GetKeyDownGun);
        InputManager.Instance.RemoveKeyDownAction(EAction.Pick, GetKeyDownPick);
        InputManager.Instance.RemoveKeyDownAction(EAction.Extractor, GetKeyDownExtractor);
        InputManager.Instance.RemoveKeyDownAction(EAction.Repair, GetKeyDownRepair);

        InputManager.Instance.RemoveMouseDownAction(EAction.UsingTool, GetKeyDownUsingTool);

        InputManager.Instance.RemoveMousePressAction(EAction.UsingTool, GetKeyUsingTool);
        InputManager.Instance.RemoveMouseUpAction(EAction.UsingTool, GetKeyUpUsingTool);
    }
    public ToolBase GetCurrentTool() => m_currentTool;

    public void InitialToolSetting(BalloonText _balloonText, PlayerInvenSO _invenSO, UpgradeInfoSO _upgradeInfoSO)
    {
        for (int i = 0; i < m_toolClasses.Count; i++)
        {
            ToolBase currentTool = m_toolClasses[i];
            currentTool.Init();
            currentTool.GiveComponent(_balloonText, _invenSO);
            currentTool.SetInitialInfo(_upgradeInfoSO.GetUpgradeValue(currentTool.GetUpgradeID(), currentTool.GetLevel()));
        }
     
    }
}
