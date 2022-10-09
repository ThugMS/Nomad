using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;

public abstract class NomadCartBase : MonoBehaviour, IPunObservable, IUpgradeable, IRopeConnectable
{
    public enum Type
    {
        None, EngineCart, StarlightCart, CazelinCart, WeaponCart
    }
    public enum State
    {
        Alive, NotPicked, Broken
    }

    #region PrivateVariables
    [SerializeField] private State m_state;//어떤 상태인지
    private float m_space = NomadConstants.SPACE;//칸 사이 간격

    [SerializeField] private TotalMineraUIChannelSO m_totalMineralUIChannelSO;//미네랄이 바뀔때 전달 이벤트
    [SerializeField] private StringEventChannelSO m_updateUpgradeButtonChannel;
    [SerializeField] private float m_maxHp = 0;

    [SerializeField] private Canvas m_canvas;
    [SerializeField] private TMP_Text m_cartName;
    [SerializeField] private GameObject m_barrier;

    [SerializeField] private ParticleSystem m_fuelParticle;

    private BarcodeHPBar m_barcodeHPBar;
    private UpgradeInfoSO m_upgradeInfoSO;

    private SpriteRenderer m_spriteRencerer;
    private SpriteOutline m_spriteOutline;

    private Dictionary<string, Action<float>> m_upgradeIDActionDict = new();
    private Dictionary<string, int> m_upgradeIDLevelDict = new();

    private Color m_darkColor = new Color(0.2f, 0.2f, 0.2f);
    private Color m_defultColor = new Color(1, 1, 1);
    private int[] m_dangerousHpPercent = new int[3] { 10, 30, 50 };
    private int m_dangerLevel = 3;
    private Vector3 m_canvaseBasicAngle = new Vector3(0, 0, 0);

    private Animator m_animator;
    private int m_animationCool = 90;
    private int m_animationdir = 1;
    #endregion

    #region Protected Variables
    protected CartMineralSO m_cartMineralSO;
    //동기화 변수
    [SerializeField] protected float m_currentHp = 0;
    [SerializeField] Type m_type;
    protected NomadCartBase m_parentCart;
    protected float m_speed = NomadConstants.SPEED;
    protected Vector2 m_curPos;
    protected int m_cartNum = 0;
    protected Rigidbody2D m_rigid;
    protected PhotonView m_photonView;
    protected IRPCCartAddable m_RPCCartAddable;
    #endregion

    #region PublicVariables
    #endregion

    public delegate void MaxHpHandler();
    public event MaxHpHandler OnChangedMaxHp;

    public delegate void CartNotiHandler(int value);
    public event CartNotiHandler OnDangerHp;

    void Awake()
    {
        m_RPCCartAddable = FindObjectOfType<NomadCartManager>();
        m_cartMineralSO = Resources.Load<CartMineralSO>(ConstStringStorage.CartMineralSO_PATH); 
        m_photonView = GetComponent<PhotonView>();
        m_rigid = GetComponent<Rigidbody2D>();
        m_barcodeHPBar = m_canvas.GetComponentInChildren<BarcodeHPBar>();
        m_spriteRencerer = GetComponent<SpriteRenderer>();
        m_animator = gameObject.GetComponent<Animator>();
        m_spriteOutline = GetComponent<SpriteOutline>();

        SetActiveHpBar(false);

        ChangeState(State.NotPicked);

        Init();
    }
    private void FixedUpdate()
    {
        if (IsNotPicked())
        {
            PlayNotPickedAnimation();
            return;
        }

        FollowParentCart();

        AdjustHpbarAngles();

        #if UNITY_EDITOR
        TestCheatMineral();
        #endif
    }
    
    #region PrivateMethod

    #if UNITY_EDITOR
    public virtual void TestCheatMineral()
    {
    }
    #endif

    private void ChangeState(State _state)
    {
        if (_state == m_state)
            return;
        switch (_state)
        {
            case State.Alive:
                if (IsBroken())
                {
                    m_state = State.Alive;
                    RPC_RecoverHp(GetMaxHp());
                    OnPostRestore();
                }
                else
                    SetCurrentHp(GetMaxHp());
                UpdateCartNameByStateAndKind(_state);
                break;
            case State.NotPicked:
                SetCurrentHp(0);
                break;
            case State.Broken:
                SetCurrentHp(0);
                UpdateCartNameByStateAndKind(_state);
                break;
            default:
                Debug.LogError(_state.ToString() + "ChangeState(State _state) : 적절하지 않은 state값");
                break;
        }
        m_state = _state;

        PlayBrokenAnim(m_state == State.Broken);
    }
    private void UpdateCartNameByStateAndKind(State _newState)
    {
        if(m_type == Type.CazelinCart)
        {
            SetCartName(_newState == State.Broken ?
                NomadConstants.CARTNAME_BROKEN_CAZELINCART : NomadConstants.CARTNAME_CAZELINCART);
        }
        else if(m_type == Type.StarlightCart)
        {
            SetCartName(_newState == State.Broken ?
                NomadConstants.CARTNAME_BROKEN_STARLIGHTCART : NomadConstants.CARTNAME_STARLIGHTCART);
        }
        else if (m_type == Type.WeaponCart)
        {
            SetCartName(_newState == State.Broken ?
                NomadConstants.CARTNAME_BROKEN_WEAPONCART : NomadConstants.CARTNAME_ENGINECART);
        }
    }

    private void PlayNotPickedAnimation()
    {
        if (!IsNotPicked())
            return;

        transform.position = new Vector2(transform.position.x, transform.position.y + (m_animationdir * 0.003f));

        m_animationCool++;
        if(m_animationCool % 180 == 0)
        {
            m_animationCool = 0;
            m_animationdir *= -1;
        }
    }
    /* 
     * Follow()
     * m_parentCart를 바라보는 방향으로 방향을 변경하고 
     * m_parentCart와의 거리가 m_space보다 크다면 위치 변경(따라가기)
     */
    private void FollowParentCart()
    {
        if (m_parentCart == null)
            return;

        if (IsNotPicked())
            return;

        float angle = Mathf.Atan2(GetPosition().y - m_parentCart.GetPosition().y, GetPosition().x - m_parentCart.GetPosition().x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle + 90, Vector3.forward);

        if (Vector3.Distance(GetPosition(), m_parentCart.GetPosition()) > m_space)
            SetPosition(GetPosition() + (m_parentCart.GetPosition() - GetPosition()).normalized * m_speed * Time.fixedDeltaTime);
    }
    private void AdjustHpbarAngles()
    {
        if (m_canvas == null || m_canvas.gameObject.activeSelf == false)
            return;

        m_canvas.transform.eulerAngles = m_canvaseBasicAngle;

    }
    private UpgradeInfoSO GetUpgradeInfoSO()
    {
        if (m_upgradeInfoSO == null)
            m_upgradeInfoSO = Resources.Load<UpgradeInfoSO>(ConstStringStorage.UPGRADEINFOSO_PATH);
        return m_upgradeInfoSO;
    }
    private bool FindNextDangerHp()
    {
        for (int i = 0; i < m_dangerousHpPercent.Length; i++)
        {
            if (m_dangerLevel > i && (GetCurrentHp() / GetMaxHp()) <= (m_dangerousHpPercent[i] / (float)100))
            {
                m_dangerLevel = i;
                return true;
            }
        }
        return false;
    }
    private bool CanRecoverDangerHp()
    {
        for (int i = m_dangerousHpPercent.Length-1; i >= 0; i--)
        {
            if (m_dangerLevel <= i && (GetCurrentHp() / GetMaxHp()) > (m_dangerousHpPercent[i] / (float)100))
            {
                m_dangerLevel = i+1;
                return true;
            }
        }
        return false;
    }
    private void UpdateDangerHpOnTaken()
    {
        if (!FindNextDangerHp())
            return;

        OnDangerHp?.Invoke(m_dangerousHpPercent[m_dangerLevel]);
        m_barcodeHPBar.ChanageBarColor(m_dangerLevel);

    }
    private void UpdateDangerHpOnRecovered()
    {
        if (!CanRecoverDangerHp())
            return;

        m_barcodeHPBar.ChanageBarColor(m_dangerLevel);
    }
    private void InitUpgradablevalue()
    {
        foreach(string id in m_upgradeIDActionDict.Keys)
            ApplyUpgradeValue(id, GetUpgradeInfoSO().GetUpgradeValue(id, 1));
    }
    #endregion

    #region ProtectedMethod
    protected abstract void Init();
    protected virtual void OnPostRestore()
    {

    }
    [PunRPC] 
    protected virtual void RPC_TakeDamage(float _damage)
    {
        SetCurrentHp(Math.Max(GetCurrentHp() - _damage, 0));

        UpdateDangerHpOnTaken();

        if (GetCurrentHp() <= 0)
            OnMalFuntioned();
    }

    [PunRPC]
    protected void RPC_RecoverHp(float _hp)
    {
        if (IsAlive() == false)
        {
            Debug.Log("Log : 함선 회복할 수 없는데 회복중");
            return;
        }
        SetCurrentHp(Math.Min(GetCurrentHp() + _hp, GetMaxHp()));

        UpdateDangerHpOnRecovered();
    }
    [PunRPC]
    protected void RPC_SetMaxHp(float _hp)
    {
        if (_hp < 0)
            _hp = float.MaxValue;

        float delta = _hp - GetMaxHp();

        m_maxHp = _hp;
        SetCurrentHp(Math.Min(GetCurrentHp() + delta, GetMaxHp()));
        m_barcodeHPBar.ResetHpImage(GetCurrentHp(), m_maxHp);
        UpdateDangerHpOnRecovered();
        OnChangedMaxHp?.Invoke();
    }

    protected void SetCartName(string _cartName)
    {
        m_cartName.text = _cartName;
    }

    /// <summary>
    /// 함선 체력이 0이될 때 호출
    /// </summary> 
    protected void OnMalFuntioned()
    {
        ChangeState(State.Broken);
        StopFuntion();
    }

    /// <summary>
    /// 해당 칸의 기능을 제거하는 메서드
    /// </summary>
    protected abstract void StopFuntion();

    protected abstract void OnPostSetParent();

    [PunRPC]
    protected void RPC_RestartFuntion()
    {
        ChangeState(State.Alive);
    }
    protected void SetCurrentHp(float _hp)
    {
        float delta = _hp - GetCurrentHp();
        if(delta < 0)
            m_barcodeHPBar.DisplayHurt(-delta);
        else if(delta > 0)
            m_barcodeHPBar.DisplayRepair(delta);

        m_currentHp = _hp;
    }
    protected void ApplyUpgradeValue(string _id, float _value)
    {
        m_upgradeIDActionDict[_id].Invoke(_value);
    }
    protected void SetUpgradeIDAndAction(string _id, Action<float> _action)
    {
        m_upgradeIDActionDict.Add(_id, _action);
        m_upgradeIDLevelDict.Add(_id, 1);
    }
    [PunRPC]
    protected void RPC_UpgradeLevel(string _id)
    {
        //아이디에 해당하는 요소 레벨 +1 
        m_upgradeIDLevelDict[_id] += 1;
        float upgradeValue = GetUpgradeInfoSO().GetUpgradeValue(_id, GetLevel(_id));

        m_updateUpgradeButtonChannel.RaiseEvent(_id);
        
        if (PhotonNetwork.IsMasterClient == true)
            ApplyUpgradeValue(_id, upgradeValue);

    }
    #endregion

    #region PublicMethod
    public abstract void OnInteracted(GameObject _player);
    /// <summary>
    /// 고장 상태 복구
    /// </summary>
    public bool TryRestoreCart()
    {
        if (m_state != State.Broken)//이미 복구 되었을 때
            return false;

        m_photonView.RPC(nameof(RPC_RestartFuntion), RpcTarget.AllBuffered);
        return true;
    }
    public Vector3 GetPosition()
    {
        return transform.position;
    }
    public void SetPosition(Vector3 _pos)
    {
        transform.position = _pos;
    }
    public bool IsAlive()
    {
        return m_state == State.Alive;
    }

    public bool IsBroken()
    {
        return m_state == State.Broken;
    }
    public bool IsNotPicked()
    {
        return m_state == State.NotPicked;
    }
    public void InitCarConfig(Transform _nomadTransform, int _carNum, NomadCartBase _parentCart = null)
    {
        transform.SetParent(_nomadTransform);

        //칸 초기설정
        this.m_cartNum = _carNum;

        InitUpgradablevalue();

        SetParent(_parentCart);

    }
    public void SetActiveHpBar(bool _on)
    {
        m_canvas.gameObject.SetActive(_on);
    }
    public void SetSpeed(float _speed)
    {
        m_speed = _speed;
    }
    public void SetParent(NomadCartBase _parentCart = null)
    {
        //함선에 칸 연결
        ChangeState(State.Alive);

        m_barrier.SetActive(false);
        SetActiveHpBar(true);

        if (_parentCart == null)
            return;

        m_parentCart = _parentCart;

        SetPosition(_parentCart.GetPosition() - (_parentCart.transform.up * m_space));

        OnPostSetParent();
    }

    public PhotonView GetPhotonView()
    {
        return m_photonView;
    }
    public virtual void CallRPCTakeDamage(float _damage)
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
    public float GetMaxHp()
    {
        return m_maxHp;
    }
    public float GetCurrentHp()
    {
        return m_currentHp;
    }
    public int GetCartNum()
    {
        return m_cartNum;
    }
    public Type GetCartType()
    {
        return m_type;
    }
    public SpriteRenderer GetSpriteRenderer()
    {
        return m_spriteRencerer;
    }
    public int GetLevel(string _id)
    {
        return m_upgradeIDLevelDict[_id];
    }
    public void SetUpgradedInfo(string _id)
    {
        m_photonView.RPC(nameof(RPC_UpgradeLevel), RpcTarget.AllBuffered, _id);
    }
    public void PlayBrokenAnim(bool _isBroken)
    {
        if (m_animator.GetBool("IsBroken") == false && _isBroken == true)
            SoundManager.Instance.PlaySFXPos(SoundManager.SFX_NOMAD_EXPLOSION,GetPosition(), 0.3f);
        m_animator.SetBool("IsBroken", _isBroken);
    }
    public void SetSpriteOutline(bool _value)
    {
        m_spriteOutline.enabled = _value;
    }
    public void UseFuelParticle()
    {
        m_fuelParticle.Play();
    }
    /// <summary>
    /// 자원을 적재하고 남은 자원을 돌려줌.
    /// </summary>
    /// <param name="_amount"> 적재량 </param>
    /// <returns> 적재하고 남은 자원 </returns>
    public int PutMineral(int _senderPhotonViewId, MineralType _mineralType, int _amount)
    {
        int newAmount = _amount + m_cartMineralSO.GetCountOfMineral(_mineralType);
        int remained = 0;
        if (newAmount <= m_cartMineralSO.GetCountOfMaxMineral(_mineralType))
            m_cartMineralSO.SendRaiseEventChangeMineral(PhotonNetwork.LocalPlayer.ActorNumber,_senderPhotonViewId, _mineralType, _amount);
        else
        {
            remained = newAmount - m_cartMineralSO.GetCountOfMaxMineral(_mineralType);
            m_cartMineralSO.SendRaiseEventChangeMineral(PhotonNetwork.LocalPlayer.ActorNumber, _senderPhotonViewId, _mineralType, _amount - remained);
        }
        return remained;
    }
    public bool IsFull(MineralType _mineralType)
    {
        return m_cartMineralSO.GetCountOfMineral(_mineralType) >= m_cartMineralSO.GetCountOfMaxMineral(_mineralType);
    }

    #endregion

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (IsNotPicked() && collision.CompareTag(ConstStringStorage.TAG_PLAYER))
        {
            //줍기
            SoundManager.Instance.PlaySFXPos(SoundManager.SFX_SHUU, GetPosition());
            m_RPCCartAddable.CallRPCAddCart(gameObject);
        }
    }
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
    }
    public bool IsConnectable()
    {
        return !IsNotPicked();
    }


}
