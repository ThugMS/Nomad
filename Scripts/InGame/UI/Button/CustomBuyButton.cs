using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;
using TMPro;
using Photon.Pun;
using ExitGames.Client.Photon;
using Photon.Realtime;
public class CustomBuyButton : CustomIngameButton
{
    #region PrivateVariable
    private CartMineralSO m_cartMineralSO;
    private MineralRequestResultEventChannelSO m_spendMineralResultChannel;
    private TMP_Text[] m_costTexts;
    private bool isSendSpendMineralRequest = false;
    #endregion

    #region PublicVariable
    public bool m_isBuyWeapon = false;
    public CostInfo m_costInfo;
    #endregion

    public delegate void UpInfoHandler(string str);
    public event UpInfoHandler OnBuyWithInfo;

    protected override void Awake()
    {
        base.Awake();
        onClick.AddListener(BuyWeapon);
        m_costTexts = gameObject.GetComponentsInChildren<TMP_Text>();
        m_spendMineralResultChannel = Resources.Load<MineralRequestResultEventChannelSO>(ConstStringStorage.SPENDMINERAL_PATH);
        m_spendMineralResultChannel.AddEventRaise(OnReceivedSpendResultEvent);
    }
    protected override void Start()
    {
        base.Start();

        List<MineralType> mineralTypes = new List<MineralType>();
        mineralTypes.Add(MineralType.Starlight);
        mineralTypes.Add(MineralType.Cazelin);

        List<int> costs = new List<int>();
        costs.Add(1000);
        costs.Add(1000);

        m_costInfo = new CostInfo(mineralTypes, costs);
        SetInfoText();
    }
    protected override void OnDestroy()
    {
        m_spendMineralResultChannel.RemoveEventRaise(OnReceivedSpendResultEvent);
    }
    private CartMineralSO GetCartMineralSO()
    {
        if (m_cartMineralSO == null)
            m_cartMineralSO = Resources.Load<CartMineralSO>(ConstStringStorage.CartMineralSO_PATH);
        return m_cartMineralSO;
    }
    private void SetInfoText()
    {
        if (m_costTexts == null)
            m_costTexts = gameObject.GetComponentsInChildren<TMP_Text>();

        for (int i = 1; i < m_costTexts.Length; i++)
        {
            m_costTexts[i].text = m_costInfo.costs[i - 1].ToString();
        }
    }
    private void SendSpendCostRequest(CostInfo _costInfo)
    {
        if (isSendSpendMineralRequest == true)
            return;
        isSendSpendMineralRequest = true;
        GetCartMineralSO().SendRaiseEventSpendMineralByButton(PhotonNetwork.LocalPlayer.ActorNumber, m_buttonNum, _costInfo.mineralTypes, _costInfo.costs);
    }
    private void OnReceivedSpendResultEvent(int _actorNum, int _buttonId, bool _isSuccess)
    {
        
        if (_actorNum != PhotonNetwork.LocalPlayer.ActorNumber || _buttonId != m_buttonNum)
            return;

        if(_isSuccess == true)
        {
            SendBuyInfo();
        }
        isSendSpendMineralRequest = false;
    }
    private void SendBuyInfo()
    {
        string info = PhotonNetwork.LocalPlayer.NickName + "님이 <함선 무기>를 구매했습니다.";
        OnBuyWithInfo.Invoke(info);
    }
    private void SetUsable(bool _canClick)
    {
        interactable = _canClick;
        m_costTexts[0].color = _canClick ? UIManager.s_defaultColor : UIManager.s_aColor;
    }
    private bool CanBuy()
    {
        return CanSpend(m_costInfo);
    }
    private bool CanSpend(CostInfo _costInfo)
    {
        if (_costInfo.mineralTypes == null)
        {
            Debug.LogError("null 값 전달");
            return false;
        }

        return GetCartMineralSO().CanSpendMineral(_costInfo.mineralTypes, _costInfo.costs);
    }
    /// <summary>
    /// 사용 가능 상태인지 업데이트하고 업그레이드 정보를 업데이트
    /// </summary>
    public void UpdateUsable()
    {
        SetUsable(CanBuy());
    }
    private void BuyWeapon()
    {  
        m_isBuyWeapon = true;

        SendSpendCostRequest(m_costInfo);
    }
}
