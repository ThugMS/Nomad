using UnityEngine;
using UnityEngine.Events;
using Photon.Pun;

public class PlayerProperty : MonoBehaviour
{
    #region PrivateVariable
    [SerializeField] private PlayerOxygenUIChannelSO m_oxygenUIChannelSO;

    private float m_maxOxygen;
    private float m_oxygenSign;
    private float m_currentOxygen;
    private float m_speed;
    #endregion

    private void Awake()
    {
        m_maxOxygen = PlayerConstants.MAX_OXYGEN;
        m_oxygenSign = PlayerConstants.NEGATIVE;

        m_currentOxygen = m_maxOxygen;
    }

    #region PrivateMethod
    private void SetOxygen()
    {
        if(m_oxygenSign > 0)
            m_speed =  PlayerConstants.OXYGEN_INCREASE_SPEED;
        else
            m_speed = PlayerConstants.OXYGEN_DECREASE_SPEED;

        m_currentOxygen += m_speed * m_oxygenSign * Time.deltaTime;

        if (m_currentOxygen < 0)
            m_currentOxygen = 0;

        if(m_currentOxygen > m_maxOxygen)
            m_currentOxygen = m_maxOxygen;
    }
    #endregion

    #region PublicMethod
    public void UpdateOxygen()
    {
        SetOxygen();
    }
    public float GetOxygen()
    {
        return m_currentOxygen;
    }
    public float GetOxygenPercent()
    {
        return m_currentOxygen / m_maxOxygen;
    }
    public void ChangeOxygenSign(int _sign)
    {
        m_oxygenSign = _sign;
    }
    #endregion
}
