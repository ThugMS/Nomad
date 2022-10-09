using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CustomStatePattern;
using Photon.Pun;
using TMPro;

public class AIPlayer : StateMachineBase<AIPlayer>, IPossesNickName, IPlayerInfoRecordable,ISleepable
{
    #region PrivateVariable
    [SerializeField] private List<ToolBase> m_toolClasses;
    [SerializeField] private PlayerRopePhysics m_nomadRope;
    [SerializeField] private PlayerRopePhysics m_playerRope;
    [SerializeField] private VoidEventChannelSO m_aiDeadEventChannelSO;
    [SerializeField] private TMP_Text m_nickNameText;

    private BoxCollider2D m_boxCollider;
    private CircleCollider2D m_circleTriggerColider;

    private GameObject m_targetObject;
    private PhotonView m_photonView;
    private Animator m_animator;
    private ToolBase m_currentTool;

    private float m_maxOxygen;
    private float m_oxygenChangeSpeed;
    private float m_oxygenSign = -1;
    private float m_currentOxygen;

    private int m_currentCazelin;
    private int m_currentStarlight;
    //TODO<임현준> : 가방 최대칸 정하기 - 20220816
    private const int m_maxCazelin = 300;
    private const int m_maxStarlight = 300;

    private int[] m_minedMineralCounts = { 0, 0 };
    #endregion

    #region PublicMethod
    public AIDecide m_decideState = new AIDecide();
    public AIDead m_deadState = new AIDead();
    public AIOxygenDiffecient m_oxygenDiffecientState = new AIOxygenDiffecient();
    public AIOxygenCharging m_oxygenChargingState = new AIOxygenCharging();
    public AIMove m_moveState = new AIMove();
    public AIMineralFull m_mineralFullState = new AIMineralFull();
    public AIMiningMineral m_miningMIneralState = new AIMiningMineral();
    public AIPutMineral m_putMineralState = new AIPutMineral();
    public AIWaitMineralCarEmpty m_waitMineralCarEmptyState = new AIWaitMineralCarEmpty();
    public AiNull m_null = new AiNull();
    #endregion

    #region Override
    protected override void BaseAwake()
    {
        m_animator = GetComponent<Animator>();
        m_photonView = GetComponent<PhotonView>();
        m_boxCollider = GetComponent<BoxCollider2D>();
        m_circleTriggerColider = GetComponent<CircleCollider2D>();
    }
    protected override void BaseStart()
    {
        m_maxOxygen = PlayerConstants.MAX_OXYGEN;
        m_oxygenChangeSpeed = PlayerConstants.OXYGEN_DECREASE_SPEED;
        m_currentOxygen = m_maxOxygen;

        if (m_photonView.IsMine == true)
        {
            m_nomadRope.CallRPCSetOwnerUsingPhotonViewID(m_photonView.ViewID);
            m_nomadRope.OnConnected += () => ChangeOxygenSign(PlayerConstants.POSITIVE);
            m_nomadRope.OnDisconnected += () => ChangeOxygenSign(PlayerConstants.NEGATIVE);
        }

        SetCurrentTool(EAction.Gun);
    }
    protected override void BaseUpdate()
    {
        SetOxygen(m_oxygenSign);
    }
    protected override void SetInitialState()
    {
        m_currentState = m_decideState;
    }

    protected override void SetNullState()
    {
        m_nullState = m_null;
    }
    protected override void SetStatesTransition()
    {
        m_decideState.SetStateTransition(m_deadState, m_oxygenDiffecientState, m_moveState, 
            m_mineralFullState, m_oxygenChargingState, m_miningMIneralState, m_putMineralState);
        m_deadState.SetStateTransition(m_decideState);
        m_oxygenDiffecientState.SetStateTransition(m_moveState, m_deadState);
        m_oxygenChargingState.SetStateTransition(m_decideState, m_deadState);
        m_moveState.SetStateTransition(m_miningMIneralState, m_putMineralState, m_decideState, m_deadState);
        m_mineralFullState.SetStateTransition(m_decideState, m_moveState, m_deadState);
        m_miningMIneralState.SetStateTransition(m_decideState, m_moveState, m_deadState);
        m_putMineralState.SetStateTransition(m_decideState, m_waitMineralCarEmptyState, m_deadState);
        m_waitMineralCarEmptyState.SetStateTransition(m_decideState, m_deadState);
        m_null.SetStateTransition(m_decideState);
    }
    #endregion

    #region PrivateMethod
    private void SetOxygen(float _sign)
    {
        if (m_photonView.IsMine == false)
            return;

        if (_sign > 0)
            m_oxygenChangeSpeed = PlayerConstants.OXYGEN_INCREASE_SPEED;
        else
            m_oxygenChangeSpeed = PlayerConstants.OXYGEN_DECREASE_SPEED;

        m_currentOxygen += m_oxygenChangeSpeed * _sign * Time.deltaTime;

        if (m_currentOxygen < 0)
        {
            m_currentOxygen = 0;

            if (!IsDeadState())
            {
                m_targetObject = null;
                SetState(m_deadState);
            }
        }

        if (m_currentOxygen > m_maxOxygen)
            m_currentOxygen = m_maxOxygen;
    }
    private void NotifyAiDead()
    {
        m_photonView.RPC(nameof(RPC_NotifyAiDead), RpcTarget.AllBuffered);
    }
    private void NotifyAiAlive()
    {
        m_photonView.RPC(nameof(RPC_NotifyAiAlive), RpcTarget.AllBuffered);
    }
    private int GetMineralPlusDelta(int _prevAmount, int _newAmount)
    {
        return _newAmount - _prevAmount;
    }
    [PunRPC]
    private void RPC_AddCumulativeMineralAmounts(int _intMineralType, int _amount)
    {
        m_minedMineralCounts[_intMineralType] += _amount;
    }
    [PunRPC]
    private void RPC_SetCurrentTool(int _toolType)
    {
        m_currentTool?.gameObject.SetActive(false);
        m_currentTool = m_toolClasses[_toolType - 1];
        m_currentTool.gameObject.SetActive(true);

        m_animator.SetInteger(ConstStringStorage.PLAYER_ANIM_CHANGE_TOOL_TYPE, _toolType - 1);
    }
    [PunRPC]
    private void RPC_NotifyAiDead()
    {
        Photon.Realtime.Player[] list = PhotonNetwork.PlayerList;
        for (int i = 0; i < list.Length; i++)
        {
            GameObject playerObj = list[i].TagObject as GameObject;
            Player player = playerObj.GetComponent<Player>();
            player.AddDeadPlayer(this.gameObject);
        }
    }
    [PunRPC]
    private void RPC_NotifyAiAlive()
    {
        Photon.Realtime.Player[] list = PhotonNetwork.PlayerList;
        for (int i = 0; i < list.Length; i++)
        {
            GameObject playerObj = list[i].TagObject as GameObject;
            Player player = playerObj.GetComponent<Player>();
            player.RemoveDeadPlayer(this.gameObject);
        }
    }

    [PunRPC]
    private void RPC_SetNickName(string _name)
    {
        m_nickNameText.text = _name;
    }

    [PunRPC]
    private void RPC_TurnOnOffBoxCol(bool _onoff)
    {
        m_boxCollider.enabled = _onoff;
    }
    #endregion

    #region PublicMethod
    public float GetOxygen()
    {
        return m_currentOxygen;
    }
    public void ChangeOxygenSign(int _sign)
    {
        m_oxygenSign = _sign;
    }
    public void SetTarget(GameObject _obj)
    {
        m_targetObject = _obj;
    }
    public GameObject GetTarget()
    {
        return m_targetObject;
    }
    public ToolBase GetCurrentTool()
    {
        return m_currentTool;
    }
    public void SetCurrentTool(EAction _eAction)
    {
        if (m_photonView.IsMine == true)
            m_photonView.RPC(nameof(RPC_SetCurrentTool), RpcTarget.AllBuffered, (int)_eAction);
    }
    public void ChangeCurrentCazelin(int _amount)
    {
        int prevCazelin = m_currentCazelin;

        m_currentCazelin += _amount;
        if (m_currentCazelin > m_maxCazelin)
            m_currentCazelin = m_maxCazelin;

        
        if(_amount > 0)
            m_photonView.RPC(nameof(RPC_AddCumulativeMineralAmounts), RpcTarget.AllBuffered, 0, GetMineralPlusDelta(prevCazelin, m_currentCazelin));
        

        if (m_currentCazelin < 0)
            m_currentCazelin = 0;
    }
    public void ChangeCurrentStarLight(int _amount)
    {
        int prevStarlight= m_currentStarlight;

        m_currentStarlight += _amount;
        if (m_currentStarlight >= m_maxStarlight)
            m_currentStarlight = m_maxStarlight;

        if (_amount > 0)
            m_photonView.RPC(nameof(RPC_AddCumulativeMineralAmounts), RpcTarget.AllBuffered, 1, GetMineralPlusDelta(prevStarlight, m_currentStarlight));

        if (m_currentStarlight < 0)
            m_currentStarlight = 0;
    }
    public int UseAllCurrentStarLight()
    {
        int current = m_currentStarlight;
        m_currentStarlight = 0;

        return current;
    } 
    public int UseAllCurrentCazelin()
    {
        int current = m_currentCazelin;
        m_currentCazelin = 0;
        return current;
    }
    public bool IsAtLeastOneMineralFull()
    {
        return IsCazelinFull() || IsStarlightFull();
    }
    public bool IsCazelinFull()
    {
        return m_currentCazelin == m_maxCazelin;
    }
    public bool IsStarlightFull()
    {
        return m_currentStarlight == m_maxStarlight;
    }
    public void RaiseDeadEvent()
    {
        NotifyAiDead();
        m_playerRope.CallRPCSetOwnerUsingPhotonViewID(m_photonView.ViewID);
        m_aiDeadEventChannelSO.RaiseEvent();
    }
    public void RaiseAliveEvent()
    {
        m_playerRope.CallRPCRemoveOwner();
        NotifyAiAlive();
    }
    public bool IsDeadState()
    {
        return m_currentState == m_deadState;
    }
    public string GetNickName()
    {
        return m_nickNameText.text;
    }
    public void SetNickName(string _nickName)
    {
        m_photonView.RPC(nameof(RPC_SetNickName), RpcTarget.All, _nickName);
    }
    public int[] GetMineralMiningCounts()
    {
        return m_minedMineralCounts;
    }
    public void Sleep()
    {
        m_nomadRope.CallRPCRemoveOwner();
        m_playerRope.CallRPCRemoveOwner();

        m_boxCollider.enabled = false;
        m_circleTriggerColider.enabled = false;
        SetState(m_null);
    }

    public void StartGame()
    {
        SetState(m_decideState);
    }

    public void TurnOnOffBoxColRPC(bool _enable)
    {
        m_photonView.RPC(nameof(RPC_TurnOnOffBoxCol), RpcTarget.All, _enable);
    }
    public int GetPhotonViewId()
    {
        return m_photonView.ViewID;
    }
    public void PlayToolSfx()
    {
        ToolBase currentTool = GetCurrentTool();
        if (currentTool.GetToolType().Equals(ToolBase.ToolType.Pickax))
        {
            ToolMine mine = currentTool as ToolMine;
           // if (mine.HasTargetMineral())
                //SoundManager.Instance.PlaySFX(SoundManager.SFX_PICKAXE);
        }
    }
    #endregion
}
