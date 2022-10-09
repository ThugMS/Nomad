using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class WeaponCartUI : CartUIBase<NomadWeaponCart>
{
    #region PrivateVariables
    [SerializeField] private DroppableUI[] m_DroppableUI = new DroppableUI[4];

    [SerializeField] private TMP_Text m_hpUpgradeInfoText;
    [SerializeField] private CustomUpgradeButton m_hpUpgradeButton;

    [SerializeField] private TMP_Text m_attackPowerUpgradeInfoText;
    [SerializeField] private CustomUpgradeButton m_attackPowerUpgradeButton;

    private CustomBuyButton[] m_buyButtons;
    #endregion

    #region PublicVariables
    public int m_currentUINum = 0;
    #endregion

    private void SettingWeapon(int _slotNum)
    {
        m_cart.AddWeapon(NomadWeapon.Type.NomadWeapon1, _slotNum);
    }

    private void SetCurrentHpText()
    {
        if (m_cart == null)
            return;

        m_hpUpgradeInfoText.text = "함선 최대 체력\n" + m_cart.GetMaxHp();
    }
    private void SetCurrentAttackPowerText()
    {
        if (m_cart == null)
            return;

        m_attackPowerUpgradeInfoText.text = "추가 공격력 계수\n" + m_cart.GetAttackPower();
    }
    private void RemoveWeapon(int _slotNum)
    {
        m_cart.RemoveWeapon(_slotNum);
    }

    private void ResetUINumber()
    {
        for(int i = 0; i < m_DroppableUI.Length; i++)
        {
            m_DroppableUI[i].SetCurrentUINum(m_currentUINum);
        }
    }

    #region ProtectedMethod
    protected override void BaseUpdate()
    {
        for (int i = 0; i < m_DroppableUI.Length; i++)
        {
            if (m_DroppableUI[i].wantRemoveWeapon == true)
            {
                RemoveWeapon(i);
                m_DroppableUI[i].wantRemoveWeapon = false;
            }

            if (m_DroppableUI[i].wantSettingWeapon == true)
            {
                SettingWeapon(i);
                m_DroppableUI[i].wantSettingWeapon = false;
            }
        }
    }
    protected override void SetInfo()
    {
        m_currentUINum = m_cart.GetCartNum();
        ResetUINumber();
        m_cartInfoText.text = $"{m_cart.GetCartNum()}번째 칸 : 무기 칸의 정보 ";

        SetCurrentHpText();
        SetCurrentAttackPowerText();

        m_hpUpgradeButton.UpdateUsable();
        m_attackPowerUpgradeButton.UpdateUsable();

        DroppableUI[] clone = transform.GetComponentsInChildren<DroppableUI>();

        for(int i = 0; i < clone.Length; i++)
        {
            if (clone[i].name.Contains("Inventory"))
                continue;

            Transform[] weapon = clone[i].GetComponentsInChildren<Transform>();

            for(int j = 0; j < weapon.Length; j++) {
                if (weapon[j].name.Contains("NomadWeapon"))
                    clone[i].RemoveUIWeapon(weapon[j].gameObject);
            }
        }
        for(int i = 0; i < m_cart.m_currentEquipWeaponCount.Length; i++)
        {
            if (m_cart.m_currentEquipWeaponCount[i])
                m_DroppableUI[i].UISlotReset();
        }
        //-------------------------------------
        //테스트 코드 빌드 전 푸쉬
        //for(int i = 0; i < m_cart.m_currentEquipWeaponCount.Length; i++)
        //{
        //    if (m_cart.m_currentEquipWeaponCount[i] != m_DroppableUI[i].m_isEquipWeapon)
        //    {
        //        if(m_cart.m_currentEquipWeaponCount[i])
        //    }
        //}
    }
    protected override void Init()
    {
        m_buyButtons = gameObject.GetComponentsInChildren<CustomBuyButton>();
    }

    protected override void Reset()
    {
    }

    protected override void TransferCartChild(NomadCartBase _cart)
    {
        m_cart = (NomadWeaponCart)_cart;
        m_hpUpgradeButton.SetUpgradable(m_cart);
        m_attackPowerUpgradeButton.SetUpgradable(m_cart);
    }

    protected override void OnBroken()
    {
        DroppableUI[] clone = transform.GetComponentsInChildren<DroppableUI>();

        for (int i = 0; i < clone.Length; i++)
        {
            if (clone[i].name.Contains("Inventory"))
                continue;

            Transform[] weapon = clone[i].GetComponentsInChildren<Transform>();

            for (int j = 0; j < weapon.Length; j++)
            {
                if (weapon[j].name.Contains("NomadWeapon"))
                    clone[i].RemoveUIWeapon(weapon[j].gameObject);
            }
        }

    }
    #endregion

    #region PublicMethod
    public override void SetSpendButton(UIManager _uIManager)
    {
        _uIManager.AddUpgradeButton(ConstStringStorage.UPGRADE_ID_WEAPON_CART_HP, m_hpUpgradeButton);
        _uIManager.AddUpgradeButton(ConstStringStorage.UPGRADE_ID_WEAPON_CART_ATTACK, m_attackPowerUpgradeButton);
        m_hpUpgradeButton.OnUpdated += SetCurrentHpText;
        m_attackPowerUpgradeButton.OnUpdated += SetCurrentAttackPowerText;

        for (int i = 0; i < m_buyButtons.Length; i++) 
            _uIManager.AddBuyButton(m_buyButtons[i]);
    }
    #endregion
}
