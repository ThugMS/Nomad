using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

public abstract class CartUIBase<T> : MonoBehaviour where T : NomadCartBase
{
    #region PrivateVariables
    [SerializeField] private CartMineralSO m_cartMineralSO;
    [SerializeField] private GameObject m_cartUIPanel;
    [SerializeField] private Button m_exitButton;
    private bool m_isUsable = true;
    #endregion

    #region ProtectedVariables
    [SerializeField] protected PlayerInvenSO m_inven;
    [SerializeField] protected TMP_Text m_cartInfoText;
    [SerializeField] protected GameObject m_cartBrokenStatePanel;
    [SerializeField] protected Button m_restoreButton;
    [SerializeField] protected TMP_Text m_restoreText;

    private MineralRequestResultEventChannelSO m_spendMineralResultChannel;
    private bool isSendSpendMineralRequest = false;
    private int[] m_tmpCost = { 0, 0 };
    private int m_requestedViewID = -int.MaxValue;

    protected NomadCartBase m_requestedCart;
    protected T m_cart;
    #endregion

    void Awake()
    {
        m_spendMineralResultChannel = Resources.Load<MineralRequestResultEventChannelSO>(ConstStringStorage.SPENDMINERAL_PATH);
        m_spendMineralResultChannel.AddEventRaise(OnReceivedSpendResultEvent);

        m_exitButton.onClick.AddListener(Close);
        m_restoreButton.onClick.AddListener(SendSpendMineral);
        m_restoreButton.interactable = true;
        SetCartUIUsable(true);

        Init();
    }
    private void Update()
    {
        if (m_cart == null)
            return;

        if (m_cart.IsBroken() && m_isUsable == true)
            SetCartUIUsable(false);
        else if (!m_cart.IsBroken() && m_isUsable == false)
            SetCartUIUsable(true);

        BaseUpdate();
    }
    private void OnDestroy()
    {
        m_spendMineralResultChannel.RemoveEventRaise(OnReceivedSpendResultEvent);
    }
    #region PrivateMethod
    private void OnReceivedSpendResultEvent(int _actorNum, int _buttonId, bool _isSuccess)
    {
        if (_actorNum != PhotonNetwork.LocalPlayer.ActorNumber || _buttonId != m_requestedViewID)
            return;

        if (_isSuccess == true)
        {
            m_requestedCart = PhotonView.Find(_buttonId).gameObject.GetComponent<NomadCartBase>();
            RestoreCart();
            m_inven.RemoveMineral(MineralType.Cazelin, m_tmpCost[0]);
            m_inven.RemoveMineral(MineralType.Starlight, m_tmpCost[1]);
        }
        m_tmpCost[0] = 0;
        m_tmpCost[1] = 0;
        m_restoreText.text = "<sprite=4>   300\n<sprite=5>   300\n복구 시도하기";
        isSendSpendMineralRequest = false;
        m_restoreButton.interactable = true;
        m_requestedViewID = -int.MaxValue;
    }
    private void SendSpendMineral()
    {
        if (isSendSpendMineralRequest == true)
            return;
        m_requestedCart = (NomadCartBase)m_cart;
        m_requestedViewID = m_requestedCart.GetPhotonView().ViewID;
        if (m_inven.GetCountOfMineral(MineralType.Cazelin) >= NomadConstants.RECOVERY_COST
            && m_inven.GetCountOfMineral(MineralType.Starlight) >= NomadConstants.RECOVERY_COST)
        {
            RestoreCart();
        }
        else
        {
            isSendSpendMineralRequest = true;
            m_restoreButton.interactable = false;
            m_restoreText.text = "복구가능한지 확인중..";

            m_tmpCost[0] = Mathf.Min(NomadConstants.RECOVERY_COST, m_inven.GetCountOfMineral(MineralType.Cazelin));
            m_tmpCost[1] = Mathf.Min(NomadConstants.RECOVERY_COST, m_inven.GetCountOfMineral(MineralType.Starlight));

            GetCartMineralSO().SendRaiseEventSpendMineralByButton(PhotonNetwork.LocalPlayer.ActorNumber, m_cart.GetPhotonView().ViewID,
                new List<MineralType>() { MineralType.Cazelin, MineralType.Starlight }, new List<int>() { NomadConstants.RECOVERY_COST- m_tmpCost[0], NomadConstants.RECOVERY_COST - m_tmpCost[1] });
        }
    }
    private CartMineralSO GetCartMineralSO()
    {
        if (m_cartMineralSO == null)
            m_cartMineralSO = Resources.Load<CartMineralSO>(ConstStringStorage.CartMineralSO_PATH);
        return m_cartMineralSO;
    }
    private void RestoreCart()
    {
        if (!m_requestedCart.TryRestoreCart())
            return;
        
        m_inven.RemoveMineral(MineralType.Cazelin, NomadConstants.RECOVERY_COST);
        m_inven.RemoveMineral(MineralType.Starlight, NomadConstants.RECOVERY_COST);

    }
    public void SetCartUIUsable(bool _usable)
    {
        m_isUsable = _usable;
        m_cartBrokenStatePanel.SetActive(!m_isUsable);


        if (m_isUsable == false)
            OnBroken();
    }
    #endregion

    #region ProtectedMethod

    protected abstract void BaseUpdate();
    protected abstract void Init();
    protected abstract void SetInfo();

    /// <summary>
    /// 각 클래스에 있는 m_cart 초기화
    /// </summary>
    protected abstract void Reset();

    /// <summary>
    /// 카트 타입을 해당 클래스에 맞는 타입으로 다운캐스팅하여 멤버변수에 저장
    /// </summary>
    /// <param name="_cart"></param>
    protected abstract void TransferCartChild(NomadCartBase _cart);

    protected virtual void OnBroken()
    {

    }
    #endregion

    #region PublicMethod
    public abstract void SetSpendButton(UIManager _uIManager);

    public void OpenOrClose(NomadCartBase _cart)
    {
        if (m_cartUIPanel.activeSelf == true && m_cart == _cart)
        {
            Close();
            return;
        }
        TransferCartChild(_cart);
        m_cartUIPanel.SetActive(true);
        SetInfo();

        SoundManager.Instance.PlaySFX(SoundManager.SFX_SYSTEM_OPEN);
    }

    public void Close()
    {
        m_cartUIPanel.SetActive(false);

        m_cart = null;
        Reset();
    }
    #endregion
}