using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
[SerializeField]
public enum MineralType
{
    Cazelin, Starlight, Stone
}
public class MineralBase : MonoBehaviour, IBulletHitable
{
    #region PrivateVariables
    private float m_initialSizeX = 0;
    private float m_initialSizeY = 0;
    private float m_curSizeX = 0;
    private float m_curSizeY = 0;
    private int m_mapResourceIndex = 0;
    private int m_mineralIndex = 0;

    private const int MINERAL_INITIAL_AMOUNT = 50;
    #endregion

    #region ProtectedVariables
    protected PhotonView m_photonView;
    protected int m_mineValue = MINERAL_INITIAL_AMOUNT;
    protected SpriteOutline m_spriteOutline;
    [SerializeField] GameObject m_ChangeObject;
    [SerializeField] protected MineralType m_mineralType;
    [SerializeField] protected ParticleSystem m_fragmentParticle;
    
    #endregion


    public delegate void MineralHandler(int r,int c);
    public event MineralHandler OnFinished;
    public MineralHandler m_onFinishedAction;

    protected void Awake()
    {
        m_photonView = GetComponent<PhotonView>();
        m_spriteOutline = GetComponent<SpriteOutline>();
    }

    protected void Start()
    {
        m_initialSizeX = m_ChangeObject.transform.localScale.x;
        m_initialSizeY = m_ChangeObject.transform.localScale.y;
        m_curSizeX = m_initialSizeX;
        m_curSizeY = m_initialSizeY;
        IndividualStart();
    }

    protected void OnEnable()
    {
        OnFinished = m_onFinishedAction;
    }

    protected void OnDisable()
    {
        OnFinished = null;
    }

    #region PublicMethod

    public virtual void IndividualStart()
    {

    }

    public virtual void Mine(Player _player, float _toolCapacity)
    {
        FinishMine();
    }
    
    public void SwitchOutLine(bool _on)
    {
        m_spriteOutline.enabled = _on;
    }

    public MineralType GetMineralType()
    {
        return m_mineralType;
    }

    public float GainMineral()
    {
        return m_mineValue;
    }

    public float AIMine(float _toolCapacity)
    {
        FinishMine();
        return m_mineValue * _toolCapacity;
    }

    public void FinishMine()
    {
        OnFinished.Invoke(m_mapResourceIndex, m_mineralIndex);
    }

    public void DisplayStep(float _step)
    {
        m_curSizeX = m_initialSizeX * _step;
        m_curSizeY = m_initialSizeY * _step;
        m_ChangeObject.transform.localScale = new Vector2(m_curSizeX, m_curSizeY);
    }
    public void ResetSize()
    {
        m_ChangeObject.transform.localScale = new Vector2(m_initialSizeX, m_initialSizeY);
    }

    public void SetIndex(int _mapResourceIndex, int _mineralIndex)
    {
        m_mapResourceIndex = _mapResourceIndex;
        m_mineralIndex = _mineralIndex;
    }
    #endregion

    #region RPCmethod
    
    #endregion

    public void OnMasterHitBullet(BulletProperty _bulletProperty)
    {

    }

    public void OnLocalHitBullet(BulletProperty _bulletProperty)
    {
        Vector3 pos = _bulletProperty.pos;
        pos.x = _bulletProperty.pos.x;
        pos.y = _bulletProperty.pos.y;
        pos.z = -1;
        m_fragmentParticle.transform.position = pos;
        m_fragmentParticle.Play();
    }
}
