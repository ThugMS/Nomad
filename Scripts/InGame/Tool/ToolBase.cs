using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Photon.Pun;

public class ToolBase : MonoBehaviour
{
    public enum ToolType
    {
        Gun, Pickax, Extractor, Repair
    }
    #region ProtectedVariable
    [SerializeField] protected Sprite m_toolSprite;
    [Multiline][SerializeField] protected string m_toolKoreanName;
    [SerializeField] protected FloatEventChannelSO m_coolTImeUIChannelSO;
    [SerializeField] protected ToolType m_toolType;

    protected float m_range = 1f;
    protected float m_coolTime = 2f;                        //동작하는데 필요한 대기 시간
    protected float m_workTime = 2f;                       //동작하는데 필요한 작업 시간
    protected float m_currentCoolTime = 0f;              //현재 충전된 시간 
    protected float m_currentWorkTime = 0f;             //현재 충전된 시간 
    protected float m_capability = 1f;
    protected int m_level = 1;
    protected bool m_isReadyToUse = true;
    protected bool m_isCompleteWork = false;
    protected PhotonView m_photonView;
    protected const int m_mineralMask = 1 << 10;

    protected Tuple<Sprite, int, string> m_info;
    protected string m_upgradeID = "";

    protected BalloonText m_balloonText;
    protected PlayerInvenSO m_playerInvenSO;
    #endregion

    private void Awake()
    {
        m_photonView = GetComponent<PhotonView>();
    }

    private void OnEnable()
    {
        SetToolInfo();
    }

    private void Update()
    {
        if (m_photonView.IsMine == false)
            return;

        UpdateCoolTime();
    }

    #region ProtectedMethod
    protected virtual void SetUpgradeID(string _id)
    {
        m_upgradeID = _id;
    }

    protected void UpdateCoolTime()
    {
        if (m_isReadyToUse)
            return;

        m_currentCoolTime += Time.deltaTime;
        m_coolTImeUIChannelSO.RaiseEvent(1 - m_currentCoolTime / m_coolTime);
        if (m_currentCoolTime >= m_coolTime)
            m_isReadyToUse = true;
    }

    protected void ResetCurrentCoolTime()
    {
        m_currentCoolTime = 0f;
        m_isReadyToUse = false;
    }

    protected void UpdateWorkTime()
    {
        if (m_isCompleteWork)
            return;

        m_currentWorkTime += Time.deltaTime;
        if (m_currentWorkTime >= m_workTime)
            m_isCompleteWork = true;
    }

    protected void ResetWorkTime()
    {
        m_isCompleteWork = false;
        m_currentWorkTime = 0;
    }

    [PunRPC]
    protected void RPC_SetUpgradeValue(float _value)
    {
        m_capability = _value;

        if (m_upgradeID == "gun_attack")
            m_coolTime = m_coolTime - (m_capability / (m_capability + 500));

        m_level++;
    }
    #endregion

    #region PublicMethod
    public void GiveComponent(BalloonText _balloonText, PlayerInvenSO _invenSO)
    {
        m_balloonText = _balloonText;
        m_playerInvenSO = _invenSO;
    }

    public void SetToolInfo()
    {
        m_info = new Tuple<Sprite, int, string>(m_toolSprite, m_level, m_toolKoreanName);
    }

    public ToolType GetToolType()
    {
        return m_toolType;
    }

    public Tuple<Sprite, int, string> GetToolInfo()
    {
        if (m_info == null)
            SetToolInfo();

        return m_info;
    }

    public virtual void Init()
    {

    }

    public virtual void Function(Player _player)
    {

    }

    public void Use(Player _player)
    {
        if (!m_isReadyToUse)
            return;


        Function(_player);
    }
    public virtual void CancleTool()
    {

    }

    public string GetUpgradeID()
    {
        return m_upgradeID;
    }

    public int GetLevel()
    {
        return m_level;
    }

    public void SetUpgradedInfo(float _value)
    {
        if (_value < 0)
            _value = float.MaxValue;

        m_capability = _value;

        m_photonView.RPC(nameof(RPC_SetUpgradeValue), RpcTarget.AllBuffered, _value);
        SetToolInfo();
    }

    public void SetInitialInfo(float _value)
    {
        m_capability = _value;

        if (m_upgradeID == "gun_attack")
            m_coolTime = m_coolTime - (m_capability / (m_capability + 500));
    }

    public float GetCapability()
    {
        return m_capability;
    }
    #endregion

}
