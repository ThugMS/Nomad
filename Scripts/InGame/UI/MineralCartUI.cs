using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

[RequireComponent(typeof(RectTransform))]
public class MineralCartUI : CartUIBase<NomadMineralCart>
{
    #region PrivateVariables

    [SerializeField] private TMP_Text m_hpUpgradeInfoText;
    [SerializeField] private TMP_Text m_capacityUpgradeInfoText;

    [SerializeField] private CustomUpgradeButton m_hpUpgradeButton;
    [SerializeField] private CustomUpgradeButton m_capacityUpgradeButton;


    private Image m_putMineralButtonImage;

    #endregion

    #region PrivateMethod
    private void SetCurrentHpText()
    {
        if (m_cart == null)
            return;

        m_hpUpgradeInfoText.text = "함선 최대 체력\n" + m_cart.GetMaxHp();
    }
    private void SetCurrentCapacityText()
    {
        if (m_cart == null)
            return;

        m_capacityUpgradeInfoText.text = (m_cart.GetMineralType() == MineralType.Cazelin ? ConstStringStorage.TEXT_CAZELIN : ConstStringStorage.TEXT_STARLIGHT) 
            +" 최대 보관량\n" + m_cart.GetMaxAmount();
    }

    #endregion

    #region ProtectedMethod
    protected override void BaseUpdate()
    {
    }
    protected override void Init()
    {
    }
    protected override void SetInfo()
    {
        m_cartInfoText.text = $"{m_cart.GetCartNum()}번째 칸 : {(m_cart.GetMineralType() == MineralType.Cazelin ? ConstStringStorage.TEXT_CAZELIN : ConstStringStorage.TEXT_STARLIGHT)} 적재 칸의 정보 ";
        SetCurrentHpText();
        SetCurrentCapacityText();

        m_hpUpgradeButton.UpdateUsable();
        m_capacityUpgradeButton.UpdateUsable();
    }

    protected override void Reset()
    {
        //TODO : 정보 클리어
        //
    }
    protected override void TransferCartChild(NomadCartBase _cart)
    {
        m_cart = (NomadMineralCart)_cart;
        m_hpUpgradeButton.SetUpgradable(m_cart);
        m_capacityUpgradeButton.SetUpgradable(m_cart);
    }
    #endregion
 
    #region PublicMethod

    public override void SetSpendButton(UIManager _uIManager)
    {
        _uIManager.AddUpgradeButton(ConstStringStorage.UPGRADE_ID_MINERAL_CART_HP, m_hpUpgradeButton);
        _uIManager.AddUpgradeButton(ConstStringStorage.UPGRADE_ID_MINERAL_CART_CAPACITY, m_capacityUpgradeButton);

        m_hpUpgradeButton.OnUpdated += SetCurrentHpText;
        m_capacityUpgradeButton.OnUpdated += SetCurrentCapacityText;
    }

    #endregion

}
