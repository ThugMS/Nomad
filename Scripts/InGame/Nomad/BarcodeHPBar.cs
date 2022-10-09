using UnityEngine;
using UnityEngine.UI;

public class BarcodeHPBar : MonoBehaviour
{
    #region PrivateValues;
    [SerializeField] private Image m_image;
    private float m_maxHealthPoints;
    private float m_healthBarStepsLength;
    private float m_damagesDecreaseRate = 10;
    private float m_healDecreaseRate = 10;
    private float m_currentHealthPoints;
    private RectTransform m_imageRectTransform;
    private float m_damages;
    private float m_heal;
    private Color m_originColor;
    #endregion

    private void Awake()
    {
        m_healthBarStepsLength = NomadConstants.BARCORD_HP_DIVIDE;
        m_image.material = Instantiate(m_image.material);
        m_originColor = m_image.material.GetColor("_Color");

    }

    private void Update()
    {
        if (Damages > 0)
        {
            Damages -= m_damagesDecreaseRate * Time.deltaTime*10f;
        }
        if (Heal > 0)
        {
            Heal -= m_healDecreaseRate * Time.deltaTime * 10f;
        }
    }

    #region PrivateMethod
    private float Health
    {   //_Percent - 메인 색깔 부분 비율 정하는 부분
        get { return m_currentHealthPoints; }
        set
        {
            //Clamp - value의 범위를 0, MaxHealth 사이로 고정
            m_currentHealthPoints = Mathf.Clamp(value, 0, MaxHealthPoints);
            m_image.material.SetFloat("_Percent", m_currentHealthPoints / MaxHealthPoints);

            //Epsilon 매우 조그마한 float point
            if (m_currentHealthPoints < Mathf.Epsilon)
                Damages = 0;
        }
    }

    private float Damages
    {
        get { return m_damages; }
        set
        {
            m_damages = Mathf.Clamp(value, 0, MaxHealthPoints);
            m_image.material.SetFloat("_DamagesPercent", m_damages / MaxHealthPoints);
        }
    }

    private float Heal
    {
        get { return m_heal; }
        set
        {
            m_heal = Mathf.Clamp(value, 0, MaxHealthPoints);
            m_image.material.SetFloat("_HealPercent", m_heal / MaxHealthPoints);
        }
    }

    private float MaxHealthPoints
    {   //검은 줄을 긋는 부분
        get { return m_maxHealthPoints; }
        set
        {
            m_maxHealthPoints = value;
            m_image.material.SetFloat("_Steps", MaxHealthPoints / m_healthBarStepsLength);
        }
    }
    #endregion

    #region PublicMethod
    public void ResetHpImage(float _curHp, float _maxHp)
    {
        m_currentHealthPoints = _curHp;
        MaxHealthPoints = _maxHp;

        m_imageRectTransform = m_image.GetComponent<RectTransform>();
        m_image.material = Instantiate(m_image.material); 

        m_image.material.SetFloat("_Percent", m_currentHealthPoints / MaxHealthPoints);


        m_image.material.SetVector("_ImageSize", new Vector4(m_imageRectTransform.rect.size.x, m_imageRectTransform.rect.size.y, 0, 0));

        MaxHealthPoints = MaxHealthPoints; 
    }

    public void DisplayHurt(float _damagesPoint)
    {
        Damages = _damagesPoint;
        Health -= Damages;
    }

    public void DisplayRepair(float _healPoint)
    {
        Heal = _healPoint;
        Health += Heal;
    }

    public void ChanageBarColor(int _danger)
    {
        if (_danger <= 2)
        {
            m_image.material.SetColor("_Color", Color.red);
            return;
        }
        m_image.material.SetColor("_Color", m_originColor);

    }
    #endregion;
}
