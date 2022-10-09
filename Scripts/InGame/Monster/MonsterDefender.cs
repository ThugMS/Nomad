using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class MonsterDefender : MonsterBase
{
    #region PrivateValues
    [SerializeField] private GameObject m_shieldGameObject;
    [SerializeField] private SpriteRenderer m_shieldRender;

    private int m_shieldCount = MonsterConstants.DEFESNSE_SHIELD_COUNT;
    private float m_originAlpha = 0;

    #endregion

    #region PrivateMethod
    
    #endregion

    #region PublicMethod
    public override void InitialOnAwake()
    {
        InitialSpec(MonsterConstants.COMMON_HP, MonsterConstants.COMMON_DAMAGE, MonsterConstants.COMMON_ATTACK_COOLTIME, MonsterConstants.COMMON_ATTACK_RANGE,
                    MonsterConstants.DEFENSE_SPEED);

        m_shieldRender = m_shieldGameObject.GetComponent<SpriteRenderer>();
        m_originAlpha = m_shieldRender.color.a;
    }

    public override void IndividualOnEnable()
    {
        //쉴드 재생 
        m_shieldGameObject.SetActive(true);
        m_shieldCount = MonsterConstants.DEFESNSE_SHIELD_COUNT;
    }

    public override float PriorPartApplyDamage(float _originDamage)
    {
        float applyDamage = _originDamage;
        if (m_shieldCount == 0)
            return applyDamage;
        
        applyDamage = 0;
        m_photonView.RPC(nameof(RPC_PlayShieldEffect), RpcTarget.All);

        if (m_shieldCount == 0)
            m_photonView.RPC(nameof(RPC_SwitchOffShield), RpcTarget.All);

        return applyDamage;
    }

    public void BreakShield()
    {
        Color shieldAlpha = m_shieldRender.color;
        shieldAlpha.a = m_shieldCount * m_originAlpha / MonsterConstants.DEFESNSE_SHIELD_COUNT;
        m_shieldRender.color = shieldAlpha;
    }
    #endregion

    #region RPCMethod
    [PunRPC]
    public void RPC_SwitchOffShield()
    {
        m_shieldGameObject.SetActive(false);
    }

    [PunRPC]
    public void RPC_PlayShieldEffect()
    {
        m_shieldCount--;
        BreakShield();
    }
    #endregion
}
