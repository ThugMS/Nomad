using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class NomadMineralCart : NomadCartBase
{
    #region PrivateVariables

    private MineralType m_mineralType;
    [SerializeField] private CartEventChannelSO m_openMineralCartChannelSO;

    private int m_maxAmount = 0;
    private int m_prevMaxAmount = 0;
    #endregion

    #region Protected Variables
    #endregion

    #region PublicVariables

    #endregion

    #region PrivateMethod
    #endregion

    #region ProtectedMethod

    protected override void Init()
    {
        switch (GetCartType())
        {
            case Type.StarlightCart:
                m_mineralType = MineralType.Starlight;
                SetCartName(NomadConstants.CARTNAME_STARLIGHTCART);
                break;
            case Type.CazelinCart:
                m_mineralType = MineralType.Cazelin;
                SetCartName(NomadConstants.CARTNAME_CAZELINCART);
                break;
            default:
                Debug.Log(GetCartType().ToString() + "NomadMineralCart 적절하지 않은 state");
                break;
        }

        SetUpgradeIDAndAction(ConstStringStorage.UPGRADE_ID_MINERAL_CART_HP, CallRPCSetMaxHp);
        SetUpgradeIDAndAction(ConstStringStorage.UPGRADE_ID_MINERAL_CART_CAPACITY, CallRPCSetMaxAmount);
    }

    protected override void StopFuntion()
    {
        m_photonView.RPC(nameof(RPC_SetMaxAmount), RpcTarget.AllBuffered, 0);
    }
    protected override void OnPostRestore()
    {
        RPC_SetMaxAmount(m_prevMaxAmount);
    }

    public override void OnInteracted(GameObject _player)
    {
        m_openMineralCartChannelSO.RaiseEvent(this);
    }

    protected override void OnPostSetParent()
    {
    }
    #endregion

    #region PublicMetho

    public MineralType GetMineralType()
    {
        return m_mineralType;
    }

    [PunRPC]
    public void RPC_SetMaxAmount(int _maxAmount)
    {
        if (_maxAmount < 0)
            _maxAmount = int.MaxValue;

        m_cartMineralSO.ChangeMaxMineral(m_mineralType, _maxAmount - GetMaxAmount());
        if(_maxAmount != 0)
            m_prevMaxAmount = m_maxAmount;
        m_maxAmount = _maxAmount;
    }

    public void CallRPCSetMaxAmount(float _maxAmount)
    {
        m_photonView.RPC(nameof(RPC_SetMaxAmount), RpcTarget.AllBuffered, (int)_maxAmount);
    }

    public int GetMaxAmount()
    {
        return m_maxAmount;
    }

    #endregion
}
