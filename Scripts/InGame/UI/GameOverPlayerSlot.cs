using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameOverPlayerSlot : MonoBehaviour
{
    [SerializeField] private Image m_playerImage;
    [SerializeField] private TMP_Text m_nickNameText;
    [SerializeField] private TMP_Text m_survivalText;

    [SerializeField] private TMP_Text[] m_mineralCountTexts;

    public void SetPlayer(string _nickName, bool _isDead, int[] _mineralCounts)
    {
        m_nickNameText.text = _nickName;
        
        if(_isDead == true)
        {
            m_survivalText.text = ConstStringStorage.TEXT_DEAD;
            m_survivalText.color = Color.red;
            m_playerImage.sprite = Resources.Load<Sprite>(ConstStringStorage.PLAYER_DEAD_IMAGE);
        }
        else
        {
            m_survivalText.text = ConstStringStorage.TEXT_ALIVE;
            m_survivalText.color = Color.green;
            m_playerImage.sprite = Resources.Load<Sprite>(ConstStringStorage.PLAYER_ALIVE_IMAGE);
        } 

        for(int i=0; i<m_mineralCountTexts.Length; i++)
            m_mineralCountTexts[i].text = _mineralCounts[i].ToString() + "개";
    }
    public void SetPlayer()
    {
        m_nickNameText.text = "";
        m_survivalText.text = "없음";
        m_survivalText.color = Color.black;
        for (int i = 0; i < m_mineralCountTexts.Length; i++)
            m_mineralCountTexts[i].text = "";
    }
}
