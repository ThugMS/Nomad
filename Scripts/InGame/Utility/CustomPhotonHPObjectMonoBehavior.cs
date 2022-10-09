using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Photon.Pun;

public abstract class CustomPhotonHPObjectMonoBehavior : MonoBehaviour
{
    protected PhotonView m_photonView;

    private float m_maxHp;
    protected float m_currentHp;

    Action OnDie;
    Action OnDamageTaken;
    Action OnDamageRecovered;

    private void Awake()
    {
        m_photonView = GetComponent<PhotonView>();
        BaseInit();
        Init();
    }
    protected abstract void BaseInit();
    protected virtual void Init() { }

    
    [PunRPC]
    protected void RPC_SetMaxHp(float _hp)
    {
        m_maxHp = _hp;
    }
    protected void SetCurrentHp(float _hp)
    {
        m_currentHp = _hp;
    }

    [PunRPC]
    private void RPC_TakeDamage(float _damage)
    {
        SetCurrentHp(Math.Max(GetCurrentHp() - _damage, 0));

        if (m_currentHp <= 0)
            OnDie?.Invoke();

        OnDamageTaken?.Invoke();
    }

    [PunRPC]
    private void RPC_RecoverHp(float _hp)
    {
        SetCurrentHp(Math.Min(GetCurrentHp() + _hp, m_maxHp));

        OnDamageRecovered?.Invoke();
    }


    /// <summary>
    /// OnDie이벤트가 호출되었을 때 실행할 메서드를 넣는 메서드
    /// </summary>
    /// <param name="_action"></param>
    protected void AddOnDieAction(Action _action)
    {
        OnDie += _action;
    }
    protected void RemoveOnDieAction(Action _action)
    {
        OnDie -= _action;
    }



    public float GetMaxHp()
    {
        return m_maxHp;
    }
    public float GetCurrentHp()
    {
        return m_currentHp;
    }

    public void CallRPCTakeDamage(float _damage)
    {
        m_photonView.RPC(nameof(RPC_TakeDamage), RpcTarget.AllBuffered, _damage);
    }
    public void CallRPCRecoverHp(float _hp)
    {
        m_photonView.RPC(nameof(RPC_RecoverHp), RpcTarget.AllBuffered, _hp);
    }
    public void CallRPCSetMaxHp(float _maxHp)
    {
        m_photonView.RPC(nameof(RPC_SetMaxHp), RpcTarget.AllBuffered, _maxHp);
    }

}
