using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

public class BalloonText : MonoBehaviour
{
    #region PrivateValues
    [SerializeField] private PhotonView m_photonView;
    [SerializeField] private Sprite[] m_mineralIcons;
    [SerializeField] private Image m_icon;
    [SerializeField] private TMP_Text m_info;
    [SerializeField] private const float PERSIST_TIME = 1f;
    [SerializeField] private GameObject m_textBox;
    private bool m_isActive = false;
    private Vector2 m_originPosition;


    private int[] m_mineralCumulativeAmounts = {0,0};
    #endregion
    private void Start()
    {
        m_originPosition = transform.localPosition;
    }

    private void Update()
    {
        if(m_isActive == true)
            m_textBox.transform.Translate(Vector2.up * Time.deltaTime * 0.5f);
    
    }

    #region PrivateMethod
    private void SetInfo(Sprite _mineralIcon, int _value)
    {
        m_icon.sprite = _mineralIcon;
        if(_value == 0)
            m_info.text = ConstStringStorage.TEXT_FULL_INVEN;
        else
            m_info.text = "+" + _value.ToString();
    }

    private void ShowUp()
    {
        m_textBox.SetActive(true);
        m_isActive = true;
        m_textBox.transform.localPosition = m_originPosition;
    }

    private void ShowDown()
    {
        m_textBox.SetActive(false);
        m_isActive = false;
    }
    [PunRPC]
    private void RecordMineral(int _mineralType, int _value)
    {
        m_mineralCumulativeAmounts[_mineralType] += _value;
    }
    #endregion

    #region PublicMethod
    public void ShowBalloon(MineralType _mineralType, int _value)
    {
        int mineralTypeInt = (int)_mineralType;

        SetInfo(m_mineralIcons[mineralTypeInt], _value);
        m_photonView.RPC(nameof(RecordMineral), RpcTarget.AllBuffered, mineralTypeInt, _value);

        ShowUp();
        Invoke("ShowDown", PERSIST_TIME);
    }

    public int[] GetCumulativeAmounts()
    {
        return m_mineralCumulativeAmounts;
    }
    #endregion
}
