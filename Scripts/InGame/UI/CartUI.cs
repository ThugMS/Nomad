using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CartUI : MonoBehaviour
{
    private MineralCartUI m_mineralCartPanel;
    private WeaponCartUI m_weaponCartPanel;
    private EngineCartUI m_engineCartPanel;

    // Start is called before the first frame update
    void Awake()
    {
        m_mineralCartPanel = GetComponentInChildren<MineralCartUI>();
        m_weaponCartPanel = GetComponentInChildren<WeaponCartUI>();
        m_engineCartPanel = GetComponentInChildren<EngineCartUI>();
    }

    private void Start()
    {
        CloseAllCartUI();
    }
    public void SetUIManagerSpendButton(UIManager _uIManager)
    {
        m_mineralCartPanel.SetSpendButton(_uIManager);
        m_weaponCartPanel.SetSpendButton(_uIManager);
    }

    public void OpenMineralCartUI(NomadCartBase _cart)
    {
        m_weaponCartPanel.Close();
        m_mineralCartPanel.OpenOrClose(_cart);

    }
    public void OpenWeaponCartUI(NomadCartBase _cart)
    {
        m_mineralCartPanel.Close();
        m_weaponCartPanel.OpenOrClose(_cart);
    }
    public void OpenEngineCartUI(NomadCartBase _cart)
    {
        m_weaponCartPanel.Close();
        m_mineralCartPanel.Close();
        m_engineCartPanel.OpenOrClose((NomadEngineCart)_cart);

    }
    public void CloseAllCartUI()
    {
        m_mineralCartPanel.Close();
        m_weaponCartPanel.Close();
        m_engineCartPanel.Close();
    }
    public void CloseSubCartUI()
    {
        m_mineralCartPanel.Close();
        m_weaponCartPanel.Close();
    }
}
