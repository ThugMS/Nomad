using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;


public struct BulletProperty
{
    public Vector2 pos;
    public float damage;
}
public interface IBulletHitable
{
    public void OnMasterHitBullet(BulletProperty _bulletProperty);      //마스터일때만 적용하게
    public void OnLocalHitBullet(BulletProperty _bulletProperty);       //모든 클라에 적용
}
public class Bullet : MonoBehaviour
{
    #region PrivateVariable
    [SerializeField] private LayerMask m_isLayer;
    [SerializeField] private ParticleSystem m_moveParticle;
    [SerializeField] private Gradient[] m_gradByLevel;

    private Vector2 m_shootDirection = Vector2.zero;
    private Vector2 m_startPos = Vector2.zero;
    private BulletProperty m_property;
    private float m_damage = 0;
    private float m_speed = 50;
    private float m_hitDistance = 0.3f;

    private ObjectPool<Bullet> m_objPoolParent;
    private Gradient m_bulletGrad = new Gradient();
    #endregion

    private void OnEnable()
    {
        m_moveParticle.Play();
        //테스트용으로 3초후 총알이 오브젝트 풀로 돌아가게 설정
        Invoke(nameof(DestroyBullet), 3f);
    }

    private void Update()
    {

        if (m_shootDirection.x > 0)
            transform.Translate(Vector2.right * m_speed * Time.deltaTime);
        else
            transform.Translate(Vector2.left * m_speed * Time.deltaTime);

        CheckCollider();
    }

    #region PrivateMethod
    private bool CheckCollider()
    {
        RaycastHit2D ray = Physics2D.Raycast(transform.position, m_shootDirection, m_hitDistance, m_isLayer);
        if (ray.collider == null)
            return false;

        IBulletHitable onHit = ray.collider.GetComponent<IBulletHitable>();
        if (onHit == null)
            return false;

        m_property.pos = transform.position;
        m_property.damage = m_damage;

        if (PhotonNetwork.IsMasterClient == true)
            onHit.OnMasterHitBullet(m_property);

        onHit.OnLocalHitBullet(m_property);
        DestroyBullet();

        return false;
    }

    private void DestroyBullet()
    {
        if(m_objPoolParent != null)
            m_objPoolParent.ReturnObj(this);
    }
    #endregion

    #region PublicMethod
    public void InitialSetting(Vector2 _shootDirection, Vector2 _startPos, float _damage, ObjectPool<Bullet> _objPoolParent, int _bulletLevel)
    {
        transform.localScale = new Vector3(1, 1, 1);

        this.m_startPos = _startPos;
        this.m_shootDirection = _shootDirection;
        this.m_damage =_damage;
        this.m_objPoolParent = _objPoolParent;

        float angle = Mathf.Acos(Vector2.Dot(_shootDirection, Vector2.left)) * Mathf.Rad2Deg;
        Vector3 crossProduct = Vector3.Cross(Vector3.left, (Vector3)_shootDirection);
        float sign = Mathf.Sign(crossProduct.z);
        float targetEulerAngle = sign * angle;

        if (m_shootDirection.x > 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
            targetEulerAngle += 180;
        }

        transform.localEulerAngles = new Vector3(0, 0, targetEulerAngle);
        m_bulletGrad = m_gradByLevel[Mathf.Min(_bulletLevel - 1, m_gradByLevel.Length - 1)];
        var gradColor = m_moveParticle.colorOverLifetime;
        gradColor.color = m_bulletGrad;
    }
    #endregion
}
