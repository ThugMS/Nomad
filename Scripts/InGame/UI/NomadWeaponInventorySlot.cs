using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class NomadWeaponInventorySlot : MonoBehaviour
{
    #region PrivateVariable
    private NomadWeaponSO m_nomadWeaponSO;
    private TMP_Text m_unequipWeaponText;
    private int m_unequipWeaponTextNum = 0;
    #endregion

    private void Start()
    {
        m_nomadWeaponSO = Resources.Load<NomadWeaponSO>(ConstStringStorage.NOMAD_WEAPONSO_PATH);
        m_unequipWeaponText = GetComponent<TMP_Text>();
        m_unequipWeaponText.text = "사용가능 " + m_unequipWeaponTextNum.ToString() + " 개";
    }

    private void Update()
    {
        if(m_unequipWeaponTextNum != m_nomadWeaponSO.GetUneuipNomadWeapon1Num())
        {
            m_unequipWeaponTextNum = m_nomadWeaponSO.GetUneuipNomadWeapon1Num();
            m_unequipWeaponText.text = "사용가능 " + m_unequipWeaponTextNum.ToString() + " 개";
        }
            
    }
}
