using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class NomadWeaponCart : NomadCartBase
{
    #region PrivateVariables
    [SerializeField] private NomadWeapon[] m_shipWeapons;
    [SerializeField] private Transform[] m_weaponTransforms;
    [SerializeField] private int m_currentSlotCount;
    [SerializeField] private int m_maxSlotCount;
    [SerializeField] private float m_attackPower = 1;

    [SerializeField] CartEventChannelSO m_openMineralCartChannelSO;
    #endregion

    #region Protected Variables
    #endregion

    #region PublicVariables
    public bool[] m_currentEquipWeaponCount = new bool[4];
    #endregion

    #region PrivateMethod
    private void ApplyCurrentAttackPowerToWeapons()
    {
        for(int i=0; i<m_shipWeapons.Length; i++)
        {
            if (m_shipWeapons[i] == null)
                continue;
            m_shipWeapons[i].SetDamageExtraMulValue(m_attackPower);
        }
    }
    private void ApplyNewAttackPowerToWeapon(NomadWeapon _nomadWeapon, float _newPower = 1)
    {
        _nomadWeapon.SetDamageExtraMulValue(_newPower);
    }
    [PunRPC]
    private void RPC_AddWeaponSetting(int _viewID, int _locationIdx)
    {
        GameObject obj = PhotonView.Find(_viewID).gameObject;

        m_shipWeapons[_locationIdx] = obj.GetComponent<NomadWeapon>();
        obj.transform.SetParent(m_weaponTransforms[_locationIdx]);
        obj.transform.localPosition = Vector3.zero;

        if (_locationIdx % 2 == 0)
            obj.transform.localEulerAngles = new Vector3(0, 0, 90);
        else
            obj.transform.localEulerAngles = new Vector3(0, 0, 270);

        m_currentEquipWeaponCount[_locationIdx] = true;
        ApplyNewAttackPowerToWeapon(m_shipWeapons[_locationIdx], m_attackPower);
    }
    [PunRPC]
    private void RPC_RemoveWeaponSetting(int _locationIdx)
    {
        Destroy(m_shipWeapons[_locationIdx].gameObject);
        m_currentEquipWeaponCount[_locationIdx] = false;
        ApplyNewAttackPowerToWeapon(m_shipWeapons[_locationIdx]);
    }
    #endregion

    #region ProtectedMethod
    protected override void Init()
    {
        m_maxSlotCount = NomadConstants.WEAPONCART_MAX_SLOTCOUNT;

        m_shipWeapons = new NomadWeapon[m_maxSlotCount];
        m_weaponTransforms = new Transform[m_maxSlotCount];
        for (int i = 0; i < m_maxSlotCount; i++)
            m_weaponTransforms[i] = transform.GetChild(i);

        SetUpgradeIDAndAction(ConstStringStorage.UPGRADE_ID_WEAPON_CART_HP, CallRPCSetMaxHp);
        SetUpgradeIDAndAction(ConstStringStorage.UPGRADE_ID_WEAPON_CART_ATTACK, CallRPCSetAttackPower);

        SetCartName(NomadConstants.CARTNAME_WEAPONCART);
    }

    public override void OnInteracted(GameObject _player)
    {
        //UI오픈 이벤트 호출
        m_openMineralCartChannelSO.RaiseEvent(this);
    }

    protected override void StopFuntion()
    {
        //모든 무기 소멸시키기
        for (int i = 0; i < m_maxSlotCount; i++)
        {
            if (m_shipWeapons == null) 
                break;
            if (m_shipWeapons[i] == null)
                continue;

            Destroy(m_shipWeapons[i].gameObject);
            m_shipWeapons[i] = null;
        }
    }

    protected override void OnPostSetParent()
    {
        m_currentSlotCount = NomadConstants.WEAPONCART_INIT_SLOTCOUNT;
    }

    [PunRPC]
    protected void ExpandSlot()
    {
        if (m_currentSlotCount >= m_maxSlotCount)
            return;

        m_currentSlotCount++;
    }
    #endregion

    #region PublicMethod
    public void AddWeapon(NomadWeapon.Type _nomadType, int _locationIdx)
    {
        if (_locationIdx >= m_maxSlotCount)
            Debug.LogError("오류 : 잘못된 슬롯 설정 " + _locationIdx);

        GameObject obj = PhotonNetwork.Instantiate(ConstStringStorage.WEAPON_FOLDER_PATH + _nomadType.ToString(),Vector3.zero,Quaternion.identity);

        m_photonView.RPC(nameof(RPC_AddWeaponSetting), RpcTarget.AllBuffered, obj.GetPhotonView().ViewID, _locationIdx);
    }

    public NomadWeapon.Type RemoveWeapon(int _locationIdx)
    {
        m_photonView.RPC(nameof(RPC_RemoveWeaponSetting), RpcTarget.AllBuffered, _locationIdx);
        NomadWeapon.Type st = m_shipWeapons[_locationIdx].type;
        return st;
    }

    public void CallRPCExpandSlot(float _value)
    {
        m_photonView.RPC(nameof(ExpandSlot), RpcTarget.AllBuffered, (int)_value);
    }
    [PunRPC]
    public void RPC_SetAttackPower(float _newPower)
    {
        if (_newPower < 0)
            _newPower = float.MaxValue;

        m_attackPower = _newPower;
        ApplyCurrentAttackPowerToWeapons();
    }

    public void CallRPCSetAttackPower(float _newPower)
    {
        m_photonView.RPC(nameof(RPC_SetAttackPower), RpcTarget.AllBuffered, _newPower);
    }
    public float GetAttackPower()
    {
        return m_attackPower;
    }


    #endregion

}
