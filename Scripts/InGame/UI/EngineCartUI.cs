using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EngineCartUI : MonoBehaviour
{

    [SerializeField] private GameObject m_panel;
    [SerializeField] private TMP_Text m_cazelinInfoText;
    [SerializeField] private TMP_Text m_currentCazelinText;
    [SerializeField] private Image m_currentCazelinGageImage;

    [SerializeField] private CartMineralSO m_cartMineralSO;

    private NomadEngineCart m_cart;

    private float m_cazelinWeight;
    private float m_currentCazelin;
    private float m_currentMaxCazelin;


    private void FixedUpdate()
    {
        if (m_cart == null)
            return;

        if (m_cazelinWeight != m_cart.GetCazelinWeight())
            ChangeCazelinWeight(m_cart.GetCazelinWeight());

        if (m_currentCazelin != m_cartMineralSO.GetCountOfMineral(MineralType.Cazelin) || m_currentCazelin != m_cartMineralSO.GetCountOfMaxMineral(MineralType.Cazelin))
            ChangeCurrentCazelinAmount();

    }
    private void ChangeCazelinWeight(float _cazelinWeight)
    {
        m_cazelinInfoText.text = _cazelinWeight.ToString();
    }
    private void ChangeCurrentCazelinAmount()
    {
        m_currentMaxCazelin = m_cartMineralSO.GetCountOfMaxMineral(MineralType.Cazelin);
        m_currentCazelin = m_cartMineralSO.GetCountOfMineral(MineralType.Cazelin);

        m_currentCazelinGageImage.fillAmount = (float)m_currentCazelin / m_currentMaxCazelin;
        m_currentCazelinText.text = m_currentCazelin.ToString();
    }

    public void OpenOrClose(NomadEngineCart _enginCart)
    {
        if (m_cart == _enginCart)
        {
            Close();
            return;
        }
        m_cart = _enginCart;
        m_panel.SetActive(true);

    }
    public void Close()
    {
        m_cart = null;
        m_panel.SetActive(false);
    }
}
