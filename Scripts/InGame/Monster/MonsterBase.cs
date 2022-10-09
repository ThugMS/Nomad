using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Photon.Pun;

public enum MonsterType
{
    Attacker, Defender
}

public class MonsterBase : MonoBehaviour, IBulletHitable, IPunObservable
{
    #region PrivateVariables
    [SerializeField] private CircleCollider2D m_detectCollider;
    [SerializeField] private CapsuleCollider2D m_hitCollider;
    [SerializeField] private BarcodeHPBar m_barcodeHPBar;
    [SerializeField] private GameObject m_LockOn;
    private float m_flashTime = MonsterConstants.HIT_FLASH_TIME;
    private const float MIN_DISTANCE = 0.15f;
    private const float MIN_MOVE_DISTANCE = 0.05f;

    private Vector2 m_masterDirection;
    private Vector3 m_masterPos;
    private Vector2 m_masterScale;

    private bool m_masterIsMove = false;
    private bool m_masterIsAttack = false;
    #endregion

    #region ProtectedVariables
    [SerializeField] protected MonsterPoolChannelSO m_poolChannelSO;
    [SerializeField] protected float m_maxHp;
    [SerializeField] protected float m_curHp;
    [SerializeField] protected float m_damage;
    protected float m_range;
    protected float m_coolTime;
    protected float m_currentCoolTime;
    protected float m_speed;
  
    protected bool m_isReadyToAttack = false;
    protected bool m_isWithInRange = false;
    protected bool m_isMove = false;
    protected bool m_lastIsMove = false;
    protected bool m_isAttack = false;
    protected bool m_lastIsAttack = false;
    protected bool m_isDead = false;
    protected bool m_isSleep = false;

    [SerializeField] protected MonsterType m_monsterType;
    protected NomadCartBase m_mainTarget;
    protected NomadCartBase m_traceTarget;
    protected NomadCartBase m_attackTarget;
    protected NomadCartBase m_currentTarget;

    [SerializeField] protected Animator m_animator;
    [SerializeField] protected Material m_whiteMaterial;
    [SerializeField] protected SpriteRenderer m_spriteRenderer;

    protected Material m_baseMaterial;
    
    #endregion

    #region PublicVariables
    [SerializeField] public PhotonView m_photonView;
    #endregion

    private void Awake()
    {
        m_photonView = GetComponent<PhotonView>();
        if (m_detectCollider == null)
            Debug.LogError("컬라이더 미할당");

        m_animator = GetComponent<Animator>();
        m_baseMaterial = m_spriteRenderer.material;
        InitialOnAwake();
    }

    private void Update()
    {
        if (m_isSleep == true)
            return;

        PhotonMasterUpdate();
      
    }

    private void FixedUpdate()
    {
        if (PhotonNetwork.IsMasterClient == true)
            return;

        if (m_isDead == true)
            return;

        LocalMove();
        UpdateHpBarScale();
        LocalAnimation();
    }

    private void OnEnable()
    {
        if(PhotonNetwork.IsMasterClient == true)
            m_photonView.RPC(nameof(RPC_SynchronizeStat), RpcTarget.AllBuffered, m_maxHp, m_damage);
     
        IndividualOnEnable();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        NomadCartBase nomadCartBase = collision.gameObject.GetComponentInParent<NomadCartBase>();

        if (nomadCartBase == null)
            return;

        if (!nomadCartBase.IsAlive())
            return;

        m_attackTarget = nomadCartBase;
        SetDetect(false);
        ResetCurrentTarget();

    }

    #region PrivateMethod
    private void PhotonMasterUpdate()
    {
        if (PhotonNetwork.IsMasterClient == false)
            return;

        if (m_isDead == true)
            return;

        UpdateCoolTime();
        CalculateRange();
        MoveToTarget();

        if (CanAttack())
            AttackNomad();

        UpdateHpBarScale();
        UpdateAnimation();
    }


    private void ToIdle()
    {
        m_isAttack = false;
    }

    private void UpdateCoolTime()
    {
        if (m_isAttack == true)
            return;

        if (m_isReadyToAttack == true)
            return;

        m_currentCoolTime += Time.deltaTime;

        if (m_currentCoolTime >= m_coolTime)
            m_isReadyToAttack = true;
    }

    private void CalculateRange()
    {
        Vector2 goal = (m_currentTarget.transform.position - m_detectCollider.transform.position);
        float sqrDistanceToTarget = (m_currentTarget.transform.position - m_detectCollider.transform.position).sqrMagnitude;
        float sqrAttackRange = Mathf.Pow(m_range, 2);

        LookAt(goal);
        if (sqrDistanceToTarget < sqrAttackRange)
        {
            m_isWithInRange = true;
            return;
        }
           
        m_isWithInRange = false;

    }

    private bool CanAttack()
    {
        if (m_isReadyToAttack == false)
            return false;

        if (m_attackTarget == null)
            return false;

        if(!m_attackTarget.IsAlive())
        {
            ResetAttackTarget();
            return false;
        }

        if (m_isWithInRange == false)
            return false;

       
        return true;
    }

    private void ResetAttackTarget()
    {
        m_attackTarget = null;
        SetDetect(true);
        ResetCurrentTarget();
    }
   
    private void MoveToTarget()
    {
        if (m_isWithInRange == true || m_isAttack == true)
        {
            m_isMove = false;
            return;
        }

        Vector2 direction = (m_currentTarget.transform.position - m_detectCollider.transform.position).normalized;
        transform.Translate(direction * Time.deltaTime * m_speed);
        m_isMove = true;
    }

    private void AttackNomad()
    {
        m_isAttack = true;
        m_isReadyToAttack = false;
        m_currentCoolTime = 0f;
    }
    private void UpdateAnimation()
    {
        m_animator.SetBool(ConstStringStorage.MONSTER_ANIM_ISMOVE, m_isMove);
        m_animator.SetBool(ConstStringStorage.MONSTER_ANIM_ISATTACK, m_isAttack);
    }

    private void LookAt(Vector2 _goal)
    {
        if (Math.Abs(_goal.x) < MIN_DISTANCE)
             return;

        Vector3 lookDirection = transform.localScale;

        if(_goal.x > 0)
        {
            lookDirection.x = Math.Abs(lookDirection.x);
        }
        else if(_goal.x < 0)
        {
            lookDirection.x = -Math.Abs(lookDirection.x);
        }
        transform.localScale = lookDirection;
    }

    private void LocalMove()
    {
        transform.localScale = m_masterScale;

        if (Vector2.Distance(m_masterPos, transform.position) < MIN_MOVE_DISTANCE)
            return; 

        m_masterDirection = (m_masterPos - transform.position).normalized;
        transform.Translate(m_masterDirection * Time.deltaTime * m_speed);
    }

    private void LocalAnimation()
    {
        m_animator.SetBool(ConstStringStorage.MONSTER_ANIM_ISMOVE, m_masterIsMove);
        m_animator.SetBool(ConstStringStorage.MONSTER_ANIM_ISATTACK, m_masterIsAttack);
    }

    private void SetDetect(bool _on)
    {
        m_detectCollider.enabled = _on;
    }

    private void DieByAniEvent()
    {
        if(PhotonNetwork.IsMasterClient == true)
            m_photonView.RPC(nameof(RPC_InsertPool), RpcTarget.AllBuffered);
    }

    private void ToBaseMaterial()
    {
      m_spriteRenderer.material = m_baseMaterial;
    }

    private void ApplyDamage(float _damage)
    {
        m_curHp = Math.Max(m_curHp - _damage, 0);
        SynchronizeHp(_damage);

        if (m_curHp == 0)
        {
            Funeral();
            SetTargetUI(false);
        }
    }

    private void ApplyHPBar(float _damage)
    {
        m_barcodeHPBar.DisplayHurt(_damage);
    }

    private void UpdateHpBarScale()
    {
        Vector3 lookDirection = transform.localScale;
        Vector3 barDirection = m_barcodeHPBar.transform.localScale;

        if (lookDirection.x > 0)
        {
            barDirection.x = Math.Abs(barDirection.x);
        }
        else if (lookDirection.x < 0)
        {
            barDirection.x = -Math.Abs(barDirection.x);
        }
        m_barcodeHPBar.transform.localScale = barDirection;
    }
    #endregion

    #region ProtectedMethod
    public virtual void InitialOnAwake()
    {
       
    }

    public virtual void IndividualOnEnable()
    {

    }

    public virtual float PriorPartApplyDamage(float _originDamage)
    {
        return _originDamage;
    }
    
    public Vector2 GetPosition()
    {
        return transform.position;
    }

    public void ResetCurrentTarget()
    {
        m_isWithInRange = false; 

        if (m_attackTarget != null)
        {
            m_currentTarget = m_attackTarget;
            return;
        }

        if (m_traceTarget.IsAlive())
        {
            m_currentTarget = m_traceTarget;
            return;
        }

        m_traceTarget = m_mainTarget;
        m_currentTarget = m_traceTarget;


    }

    public void SetMainTarget(NomadCartBase _nomadCartBase)
    {
        m_mainTarget = _nomadCartBase;
    }
    
    public void SetTraceTarget(NomadCartBase _nomadCartBase)
    {
        m_traceTarget = _nomadCartBase;
    }
    
    public void ApplyAttack(float _damage)
    {
        //모든 클라이언트에서 진행
        float appliedDamage = _damage;
        appliedDamage = PriorPartApplyDamage(appliedDamage);

        if(appliedDamage >0)
            m_photonView.RPC(nameof(RPC_FlashWhite), RpcTarget.AllBuffered);

        ApplyDamage(appliedDamage);
    }

    public void SynchronizeHp(float _damage)
    {
        m_photonView.RPC(nameof(RPC_SetHp), RpcTarget.AllBuffered, m_curHp, _damage);
    }

    public void InsertPool()
    {
        m_photonView.RPC(nameof(RPC_InsertPool), RpcTarget.AllBuffered);
    }

    public bool IsDead()
    {
        return m_isDead;
    }

    public virtual void AttackNomadByAniEvent()
    {
        if (PhotonNetwork.IsMasterClient == false)
            return;

        ToIdle();
        if(m_isWithInRange == true)
            m_attackTarget.CallRPCTakeDamage(m_damage);

    }

    public MonsterType GetMonsterType()
    {
        return m_monsterType;
    }

    public void Respawn()
    {
        m_photonView.RPC(nameof(RPC_Revive), RpcTarget.AllBuffered, transform.position);
    }

    public void UpgradeSpec(float _maxHp, float _damage)
    {
        m_maxHp = _maxHp;
        m_damage = _damage;
    }

    public void InitialSpec(float _maxHp, float _damage, float _cool, float _range, float _speed)
    {
        m_maxHp = _maxHp;
        m_curHp = m_maxHp;
        m_damage = _damage;
        m_coolTime = _cool;
        m_range = _range;
        m_speed = _speed;
    }

    public void Funeral()
    {
        m_photonView.RPC(nameof(RPC_Dead), RpcTarget.AllBuffered);
    }

    public void SetTargetUI(bool _vaule)
    {
        m_LockOn.SetActive(_vaule);
    }

    public void OnMasterHitBullet(BulletProperty _bulletProperty)
    {
        //마스터 불렛으로만 호출됨
       ApplyAttack(_bulletProperty.damage);
    }

    public void OnLocalHitBullet(BulletProperty _bulletProperty)
    {
        
    }
    public void Sleep()
    {
        m_isSleep = true;
    }
    public void PowerUp(bool _on)
    {
        m_photonView.RPC(nameof(RPC_Power), RpcTarget.AllBuffered, _on);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext((Vector2)transform.position);
            stream.SendNext((Vector2)transform.localScale);
            stream.SendNext(m_isMove);
            stream.SendNext(m_isAttack);
        }
        else
        {
           m_masterPos = (Vector2)stream.ReceiveNext();
            m_masterScale = (Vector2)stream.ReceiveNext();
            m_masterIsMove = (bool)stream.ReceiveNext();
            m_masterIsAttack = (bool)stream.ReceiveNext();

        }
    }
    #endregion

    #region RPCMethod
    [PunRPC]
    public void RPC_SetHp(float _curHp, float _damage)
    {
        ApplyHPBar(_damage);

        m_curHp = _curHp;
        if (m_curHp == 0)
            m_isDead = true;
    }

    [PunRPC]
    public void RPC_FlashWhite()
    {
        m_spriteRenderer.material = m_whiteMaterial;
        Invoke(nameof(ToBaseMaterial), m_flashTime);
    }

    [PunRPC]
    public void RPC_Dead()
    {
        m_isDead = true;
        m_hitCollider.enabled = false;
        m_animator.SetBool(ConstStringStorage.MONSTER_ANIM_ISDEAD, m_isDead);

    }

    [PunRPC]
    public void RPC_InsertPool()
    {
        gameObject.SetActive(false);

        if (PhotonNetwork.IsMasterClient == true)
            m_poolChannelSO.RaiseEvent(this);
    }

    [PunRPC]
    public void RPC_Revive(Vector3 _pos)
    {
        gameObject.SetActive(true);
        m_hitCollider.enabled = true;
        transform.position = _pos;
        m_animator.SetBool(ConstStringStorage.MONSTER_ANIM_ISDEAD, m_isDead);
    }

    [PunRPC]
    public void RPC_SynchronizeStat(float _maxHp, float _damage)
    {
        m_currentCoolTime = 0f;
        m_maxHp = _maxHp;
        m_curHp = _maxHp;
        m_damage = _damage;
        m_currentTarget = null;
        m_isDead = false;
        m_isMove = false;
        m_isWithInRange = false;

        m_barcodeHPBar.ResetHpImage(m_curHp, m_maxHp);
    }

    [PunRPC]
    public void RPC_Power(bool _on)
    {
        if (_on)
        {
            m_damage *= MonsterConstants.POWERUP_DAMAGE;
            m_speed *= MonsterConstants.POWERUP_SPEED;
            return;
        }

        m_damage /= MonsterConstants.POWERUP_DAMAGE;
        m_speed /= MonsterConstants.POWERUP_SPEED;
    }
    #endregion
}
