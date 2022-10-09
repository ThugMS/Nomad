using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Photon.Pun;

public class DroppableUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IDropHandler
{
    #region PrivateVariable
    private Image m_image;
    private RectTransform m_rectTransform;
    private NomadWeaponSO m_nomadWeaponSO;
    private CustomBuyButton m_customBuyButton;
    private PhotonView m_photonView;
    private int m_switchTargetSlotNum = 0;
    private int m_currentUINum = 0;

    [SerializeField] private int m_slotNum = 0;
    #endregion

    #region PublicVariable
    public bool m_isEquipWeapon = false;
    public bool wantSettingWeapon = false;
    public bool wantSwitchingWeapon = false;
    public bool wantRemoveWeapon = false;
    #endregion

    #region PrivateVariable
    private void Start()
    {
        m_image = GetComponent<Image>();
        m_rectTransform = GetComponent<RectTransform>();
        m_nomadWeaponSO = Resources.Load<NomadWeaponSO>(ConstStringStorage.NOMAD_WEAPONSO_PATH);
        m_customBuyButton = GetComponentInChildren<CustomBuyButton>();
        m_photonView = GetComponent<PhotonView>();
    }

    private void Update()
    {
        if (m_customBuyButton == null)
            return;

        if (m_customBuyButton.m_isBuyWeapon == true)
        {
            GameObject clone = PhotonNetwork.Instantiate(ConstStringStorage.NOMAD_WEAPON1UI_PATH, Vector3.zero, Quaternion.identity);
            m_photonView.RPC(nameof(RPC_BuyWeapon), RpcTarget.AllBuffered, clone.GetPhotonView().ViewID);
            m_customBuyButton.m_isBuyWeapon = false;
        }
    }

    [PunRPC]
    private void RPC_BuyWeapon(int _viewID)
    {
        GameObject clone = PhotonView.Find(_viewID).gameObject;
        clone.transform.SetParent(transform, false);
        m_nomadWeaponSO.BuyWeapon();
    }

    [PunRPC]
    private void RPC_ResetUIWeapon(int _viewID)
    {   
        GameObject clone = PhotonView.Find(_viewID).gameObject;
        clone.transform.SetParent(transform, false);
        m_isEquipWeapon = true;
    }


    [PunRPC]
    private void RPC_EquipWeapon(int _num)
    {
        if (_num != m_currentUINum)
            return;

        m_nomadWeaponSO.EquipWeapon();
        m_isEquipWeapon = true;
    } 

    [PunRPC]
    private void RPC_UnequipWeapon(int _num)
    {
        if (_num != m_currentUINum)
            return;

        m_nomadWeaponSO.UnequipWeapon();
        m_isEquipWeapon = false;
    }

    [PunRPC]
    private void RPC_UpdateIsEquipWeapon(int _num)
    {
        if (_num != m_currentUINum)
            return;

        m_isEquipWeapon = true;
    }

    [PunRPC]
    private void RPC_UpdateIsUnequipWeapon(int _num)
    {
  
        if (_num != m_currentUINum)
            return;

        m_isEquipWeapon = false;
    }

    [PunRPC]
    private void RPC_DropItemUI(int _viewID, int _num)
    {
        if (_num != m_currentUINum)
            return;
        GameObject dragItem = PhotonView.Find(_viewID).gameObject;
        dragItem.transform.SetParent(transform);
        dragItem.GetComponent<RectTransform>().position = m_rectTransform.position;
        RectTransform itemRect = dragItem.GetComponent<RectTransform>();
        itemRect.offsetMin = new Vector2(10, 10);
        itemRect.offsetMax = new Vector2(-10, -10);
    }

    private void RPC_RemoveWeapon(int _viewID)
    {
        GameObject weapon = PhotonView.Find(_viewID).gameObject;
        Destroy(weapon);
        m_isEquipWeapon = false;
    }
    #endregion

    #region PublicMethod
    public void OnPointerEnter(PointerEventData eventData)
    {
        m_image.color = Color.yellow;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        m_image.color = Color.white;
    }

    public void RemoveUIWeapon(GameObject weapon)
    {
        Destroy(weapon);
        m_isEquipWeapon = false;
        //m_photonView.RPC(nameof(RPC_RemoveWeapon), RpcTarget.AllBuffered, viewID);
    }

    public void UISlotReset()
    {
        GameObject clone = PhotonNetwork.Instantiate(ConstStringStorage.NOMAD_WEAPON1UI_PATH, Vector3.zero, Quaternion.identity);
        m_isEquipWeapon = true;
        clone.transform.SetParent(transform, false);
        //m_photonView.RPC(nameof(RPC_ResetUIWeapon), RpcTarget.AllBuffered, clone.GetPhotonView().ViewID);
    }

    public void RemoveWeapon()
    {
        wantRemoveWeapon = true;
        m_photonView.RPC(nameof(RPC_UpdateIsUnequipWeapon), RpcTarget.AllBuffered, m_currentUINum);
    }

    public int GetSlotNum()
    {
        return m_slotNum;
    }

    public void OnDrop(PointerEventData eventData)
    {   
        if(eventData.pointerDrag.name.Contains("NomadWeapon"))
        {
            DraggableUI prevParent = eventData.pointerDrag.GetComponent<DraggableUI>();

            if (prevParent.m_previousParent.name.Contains("WeaponSlot"))
            {
                if (m_isEquipWeapon == true)
                {
                    eventData.pointerDrag.GetComponent<DraggableUI>().ResetDrag();
                    return;
                }

                prevParent.m_previousParent.GetComponent<DroppableUI>().RemoveWeapon();

                if (transform.name.Contains("InventorySlot"))
                    m_photonView.RPC(nameof(RPC_UnequipWeapon), RpcTarget.AllBuffered, m_currentUINum);
                else
                    m_photonView.RPC(nameof(RPC_UpdateIsEquipWeapon), RpcTarget.AllBuffered, m_currentUINum);
            }

            else
            {
                if (!transform.name.Contains("InventorySlot"))
                {
                    if (m_isEquipWeapon == true)
                    {
                        eventData.pointerDrag.GetComponent<DraggableUI>().ResetDrag();
                        return;
                    }
                    m_photonView.RPC(nameof(RPC_EquipWeapon), RpcTarget.AllBuffered, m_currentUINum);
                }
                
            }

            m_photonView.RPC(nameof(RPC_DropItemUI), RpcTarget.AllBuffered, eventData.pointerDrag.GetPhotonView().ViewID, m_currentUINum);
            

            if (transform.name.Contains("WeaponSlot"))
            {
                wantSettingWeapon = true;
            }
        }
    }

    public void SetCurrentUINum(int _num)
    {
        m_currentUINum = _num;
    }

    public bool CheckSameUINum(int _num)
    {
        return _num == m_currentUINum;
    }
    #endregion
}
