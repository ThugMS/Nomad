using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

/*
 * 무기칸 구현을 위해 임시로 함선무기 클래스 생성
 */
[System.Serializable]
public class NomadWeapon : MonoBehaviour
{
    #region PrivateVariable
    [SerializeField] private MonsterBase m_targetMonster;
    [SerializeField] private ParticleSystem m_particleSystem;

    private List<MonsterBase> m_inRangedMonsters = new List<MonsterBase>();
    private PhotonView m_photonView;
    private LineRenderer m_lineRenderer;
    private CircleCollider2D m_circleCollider;
    private int m_bulletSize = 10;
    private int m_damage = 50;
    private float m_damageExtraMulValue = 1;
    private float m_currentCoolTime = 0f;
    private float m_coolTime = 1f;
    private float m_range = 10;
    private bool m_isReadyToUse = false;
    #endregion

    #region PublicVariable
    public enum Type
    {
        None, NomadWeapon1, NomadWeapon2
    }
    public Type type;
    #endregion

    #region PrivateMethod
    private void Start()
    {   
        m_photonView = GetComponent<PhotonView>();
        m_lineRenderer = GetComponent<LineRenderer>();
        m_circleCollider = GetComponent<CircleCollider2D>();
        m_circleCollider.radius = m_range;
        m_isReadyToUse = false;
    }

    private void Update()
    {
        UpdateCoolTime();
        SelectTarget();

        if (!IsReadyToAttack())
            return;

        AttackMonster();
    }

    private void SelectTarget()
    {
        int max_cnt = m_inRangedMonsters.Count;
        if (max_cnt == 0)
        {
            m_targetMonster = null;
            return;
        }

        float originalDistance = (m_inRangedMonsters[0].transform.position - transform.position).sqrMagnitude;
        m_targetMonster = m_inRangedMonsters[0];

        for (int i = 1; i < max_cnt; i++)
        {
            float newDistance = (m_inRangedMonsters[i].transform.position - transform.position).sqrMagnitude; //몬스터 까지의 거리

            if (newDistance < originalDistance)
            {
                originalDistance = newDistance;
                m_targetMonster = m_inRangedMonsters[i];
            }
        }
    }

    private bool IsReadyToAttack()
    {
        if (m_isReadyToUse == false)
            return false;

        if (m_targetMonster == null)
            return false;

        return true;
    }

    private void AttackMonster()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        m_photonView.RPC(nameof(RPC_ShowGunBullet), RpcTarget.AllBuffered, m_targetMonster.m_photonView.ViewID);

        if (m_targetMonster.IsDead())
            m_targetMonster = null;

        ResetCurrentCoolTime();
    }

    private Vector3[] MakeBulletPathFunction(Vector3 _targetPos)
    {
        float playerX = transform.position.x;
        float playerY = transform.position.y;
        float targetX = _targetPos.x;
        float targetY = _targetPos.y;

        float gradient = (targetY - playerY) / (targetX - playerX);
        float distance = Vector3.Distance(transform.position, _targetPos);
        int bulletMoveCount = (int)(distance * 10);

        Vector3[] bulletPosArray = new Vector3[bulletMoveCount + 1];

        for (int i = 0; i <= bulletMoveCount; i++)
        {
            Vector3 bulletPos = Vector3.zero;

            bulletPos.x = playerX < targetX ? playerX + ((Mathf.Abs(playerX - targetX) / bulletMoveCount) * i) : playerX - ((Mathf.Abs(playerX - targetX) / bulletMoveCount) * i);
            bulletPos.y = gradient * (bulletPos.x - playerX) + playerY;

            bulletPosArray[i] = bulletPos;
        }
        return bulletPosArray;
    }

    [PunRPC]
    private void RPC_ShowGunBullet(int _targetViewID)
    {
        if (m_targetMonster == null)
            return;

        SoundManager.Instance.PlaySFXPos(SoundManager.SFX_NOMAD_WEAPON1,transform.position, 0.5f);
        Vector3 new_ = m_targetMonster.GetPosition();
        Vector3 targetVector = new_ - transform.position;

        float zEulerAngle = Quaternion.FromToRotation(Vector3.up, targetVector).eulerAngles.z;

        transform.eulerAngles = new Vector3(0, 0, zEulerAngle);

        MonsterBase targetmonster = PhotonView.Find(_targetViewID).GetComponent<MonsterBase>();
        Vector3 targetMonsterPosition = targetmonster.GetPosition();

        m_lineRenderer.positionCount = 2;
        //m_lineRenderer.startWidth = 0.05f;
        //m_lineRenderer.endWidth = 0.05f;

        Vector3[] bulletPosArray = MakeBulletPathFunction(targetMonsterPosition);

        if (bulletPosArray.Length <= m_bulletSize)
            return;

        m_lineRenderer.SetPosition(0, bulletPosArray[0]);
        m_lineRenderer.SetPosition(1, bulletPosArray[m_bulletSize]);

        m_lineRenderer.enabled = true;
        StartCoroutine(DrawBullet(bulletPosArray, targetmonster));
    }

    IEnumerator DrawBullet(Vector3[] _bulletPosArray, MonsterBase targetmonster)
    {
        int i = 1;

        while (i < _bulletPosArray.Length - m_bulletSize)
        {
            yield return 0.01f;
            if (targetmonster.IsDead())
            {
                m_lineRenderer.positionCount = 0;
                yield break;
            }

            m_lineRenderer.startWidth = 1f;
            m_lineRenderer.endWidth = 1f;

            m_lineRenderer.SetPosition(0, _bulletPosArray[i]);
            m_lineRenderer.SetPosition(1, _bulletPosArray[i + m_bulletSize]);

            i++;
        }
        if(PhotonNetwork.IsMasterClient)
            m_targetMonster.ApplyAttack(m_damage * m_damageExtraMulValue);

        m_particleSystem.transform.position = targetmonster.GetPosition();
        m_particleSystem.Play();

        m_lineRenderer.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<MonsterBase>() != null)
            m_inRangedMonsters.Add(collision.GetComponent<MonsterBase>());

    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.GetComponent<MonsterBase>() != null)
            m_inRangedMonsters.Remove(collision.GetComponent<MonsterBase>());

    }
    #endregion
    public void UpdateCoolTime()
    {
        if (m_isReadyToUse)
            return;

        m_currentCoolTime += Time.deltaTime;

        if (m_currentCoolTime >= m_coolTime)
            m_isReadyToUse = true;
    }

    public void ResetCurrentCoolTime()
    {
        m_currentCoolTime = 0f;
        m_isReadyToUse = false;
    }
    public void SetDamageExtraMulValue(float _newValue)
    {
        m_damageExtraMulValue = _newValue;
    }
}
