using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

[CreateAssetMenu(menuName = "Datas/NomadWeapon")]
public class NomadWeaponSO : ScriptableObject
{
    private int m_totalNomadWeapon1Num = 0;
    private int m_equipNomadWeapon1Num = 0;
    private int m_unequipNomadWeapon1Num = 0;
     
    private int m_weaponLevel = 1;

    public void BuyWeapon()
    {
        m_totalNomadWeapon1Num++;
        m_unequipNomadWeapon1Num++;
    }

    public void EquipWeapon()
    {
        m_equipNomadWeapon1Num++;
        m_unequipNomadWeapon1Num--;
    }

    public void UnequipWeapon()
    {
        m_equipNomadWeapon1Num--;
        m_unequipNomadWeapon1Num++;
    }

    public void UpgradeWeapon()
    {
        m_weaponLevel++;
    }

    public int GetUneuipNomadWeapon1Num()
    {
        return m_unequipNomadWeapon1Num;
    }
}
