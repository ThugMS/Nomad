using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class CustomUpgradeButton : CustomIngameButton
{
    private UpgradeInfoSO m_upgradeInfoSO;
    private CartMineralSO m_cartMineralSO;
    private MineralRequestResultEventChannelSO m_spendMineralResultChannel;
    private StringEventChannelSO m_updateUpgradeButtonChannel;

    private TMP_Text[] m_costTexts;

    private string m_recentUpgradeId = "";

    private IUpgradeable m_upgradeable = null;
    private bool isSendSpendMineralRequest = false;

    public delegate void UpButtonHandler();
    public event UpButtonHandler OnUpdated;

    public delegate void UpInfoHandler(string str);
    public event UpInfoHandler OnUpgradeWithInfo;


    protected override void Awake()
    {
        base.Awake();
        m_costTexts = gameObject.GetComponentsInChildren<TMP_Text>();

        onClick.AddListener(TryToSpend);
        m_spendMineralResultChannel = Resources.Load<MineralRequestResultEventChannelSO>(ConstStringStorage.SPENDMINERAL_PATH);
        m_updateUpgradeButtonChannel = Resources.Load<StringEventChannelSO>(ConstStringStorage.UPDATEUPGRADE_EVENTSO_PATH);

        m_updateUpgradeButtonChannel.AddEventRaise(OnReceivedUpdateUpgradeButtonEvent);
        m_spendMineralResultChannel.AddEventRaise(OnReceivedSpendResultEvent);
        PhotonNetwork.NetworkingClient.EventReceived += OnUpdateEvent;
    }
    protected override void Start()
    {
        base.Start();
    }
    protected override void OnDestroy()
    {
        PhotonNetwork.NetworkingClient.EventReceived -= OnUpdateEvent;
        m_spendMineralResultChannel.RemoveEventRaise(OnReceivedSpendResultEvent);
        m_updateUpgradeButtonChannel.RemoveEventRaise(OnReceivedUpdateUpgradeButtonEvent);
    }

    private void OnReceivedSpendResultEvent(int _actorNum, int _buttonId, bool _isSuccess)
    {
        if (_actorNum != PhotonNetwork.LocalPlayer.ActorNumber || _buttonId != m_buttonNum)
            return;

        if (_isSuccess == true)
        {
            UpgradeCurrentObject();
        }
        isSendSpendMineralRequest = false;
    }
    private void OnReceivedUpdateUpgradeButtonEvent(string _upgradeId)
    {
        if (_upgradeId != m_recentUpgradeId)
            return;

        UpdateUsable();
    }
    /// <summary>
    /// ??????????????? 
    /// </summary>
    private void UpgradeCurrentObject()
    {
        if (m_upgradeable == null)
        {
            Debug.Log("????????? ??????????????? ??????????????? ??????");
            return;
        }

        //?????? ???????????????
        m_upgradeable.SetUpgradedInfo(GetUpgradeId());
        SendUpgradeInfo();
        SendRaiseUsableUpdateEvent(GetUpgradeId());
    }
    private void SendRaiseUsableUpdateEvent(string _recentUpgradeId)
    {
        int eventcode = 10;
        object data =  _recentUpgradeId;
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent((byte)eventcode, data, raiseEventOptions, SendOptions.SendReliable);
    }
    private void OnUpdateEvent(EventData photonEvent)
    {
        int code = photonEvent.Code;

        if (code != 10)
            return;

        object data = photonEvent.CustomData;

        if ((string)data == m_recentUpgradeId)
            UpdateUsable();
    }
    private void SendUpgradeInfo()
    {
        string info = PhotonNetwork.LocalPlayer.NickName + "?????? <";
        if (m_upgradeable is NomadCartBase cart)
            info += (cart.GetCartNum() +" ?????? ") ;
        
        info += (GetUpgradeInfoSO().GetUpgradeName(GetUpgradeId())+">??? ?????? "+GetLevel()+"?????? ??????????????? ????????????.");
        OnUpgradeWithInfo.Invoke(info);
    }
    private UpgradeInfoSO GetUpgradeInfoSO()
    {
        if(m_upgradeInfoSO == null)
            m_upgradeInfoSO = Resources.Load<UpgradeInfoSO>(ConstStringStorage.UPGRADEINFOSO_PATH);
        return m_upgradeInfoSO;
    }
    private CartMineralSO GetCartMineralSO()
    {
        if (m_cartMineralSO == null)
            m_cartMineralSO = Resources.Load<CartMineralSO>(ConstStringStorage.CartMineralSO_PATH);
        return m_cartMineralSO;
    }
    private CostInfo GetCostInfo()
    {
        return GetUpgradeInfoSO().GetCostInfo(GetUpgradeId(), GetLevel());
    }
    private bool CanUpgrade()
    {
        return CanSpend(GetCostInfo());
    }

    private bool CanSpend(CostInfo _costInfo)
    {

        if (_costInfo.mineralTypes == null)
        {
            Debug.LogError("null ??? ??????");
            return false;
        }

        return GetCartMineralSO().CanSpendMineral(_costInfo.mineralTypes, _costInfo.costs);
    }

    private void TryToSpend()
    {
        if (isSendSpendMineralRequest == true)
            return;
        isSendSpendMineralRequest = true;
        CostInfo currentCostInfo = GetCostInfo();
        GetCartMineralSO().SendRaiseEventSpendMineralByButton(PhotonNetwork.LocalPlayer.ActorNumber, m_buttonNum, currentCostInfo.mineralTypes, currentCostInfo.costs);
    }
    private void SetUsable(bool _canClick)
    {
        interactable = _canClick;
        m_costTexts[0].color = _canClick ? UIManager.s_defaultColor : UIManager.s_aColor;
    }
    private void SetInfoText()
    {
        CostInfo costInfo = GetUpgradeInfoSO().GetCostInfo(GetUpgradeId(), GetLevel());

        if (m_costTexts == null)
            m_costTexts = gameObject.GetComponentsInChildren<TMP_Text>();

        for(int i=1; i<m_costTexts.Length; i++)
        {
            m_costTexts[i].text = costInfo.costs[i - 1].ToString();
        }
    }
    public int GetLevel()
    {
        if (m_upgradeable == null)
            return 1;
        return m_upgradeable.GetLevel(GetUpgradeId());
    }

    public string GetUpgradeId()
    {
        return m_recentUpgradeId;
    }
    /// <summary>
    /// cart ?????????????????? ???????????? ?????? ????????? ???????????? ?????? ???????????????.
    /// </summary>
    /// <typeparam name="T">Player </typeparam>
    /// <param name="_id"> ??????????????? ????????? </param>
    /// <param name="_upgradeable"> ???????????? </param>
    public void InitializeUpgradeSetting<T>(string _id, T _upgradeable) where T : IUpgradeable
    {
        SetUpgradeID(_id);
        m_upgradeable = _upgradeable;
    }
    public void InitializeUpgradeSetting(string _id)
    {
        SetUpgradeID(_id);
    }
    public void SetUpgradable<T>(T _upgradeable) where T : IUpgradeable
    {
        m_upgradeable = _upgradeable;
    }
    public void SetUpgradeID(string _id)
    {
        m_recentUpgradeId = _id;
        UpdateUsable();
    }

    /// <summary>
    /// ?????? ?????? ???????????? ?????????????????? ??????????????? ????????? ????????????
    /// </summary>
    public void UpdateUsable()
    {
        SetUsable(CanUpgrade());
        SetInfoText();
        OnUpdated?.Invoke();
    }

}
