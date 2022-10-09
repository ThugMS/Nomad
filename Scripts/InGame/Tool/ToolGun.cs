using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ToolGun : ToolBase
{
    #region PrivateVairable
    [SerializeField] private Transform m_bulletStartPos;
    private GunMagazine m_gunMagazine;
    private MonsterBase m_targetMonster;
    private const float m_hitDegree = 55f;
    private float m_initDamage = 25f;

    #endregion

    public override void Init()
    {
        SetUpgradeID(ConstStringStorage.UPGRADE_ID_GUN_ATTACK);

        m_range = ToolConstants.TOOL_GUN_RANGE;
        m_coolTime = ToolConstants.TOOL_GUN_COOLTIME;

        m_gunMagazine = transform.GetChild(0).GetComponent<GunMagazine>();
    }

    protected override void SetUpgradeID(string _id)
    {
        base.SetUpgradeID(_id);

    }

    #region PrivateMethod
    private void Fire(Vector2 _dir)
    {
        m_photonView.RPC(nameof(RPC_Fire), RpcTarget.AllBuffered, _dir);
    }

    [PunRPC]
    private void RPC_Fire(Vector2 _dir)
    {
        Bullet bullet = m_gunMagazine.GetObj();
        SoundManager.Instance.PlaySFXPos(SoundManager.SFX_SHOOT1, transform.position);
        bullet.InitialSetting(_dir, transform.position, m_initDamage * m_capability, m_gunMagazine, GetLevel());

        //남은 시간을 최대로 한다
        ResetCurrentCoolTime();
    }

    private MonsterBase FindTargetMonster(Vector2 _dir)
    {
        RaycastHit2D[] circleHits = Physics2D.CircleCastAll(transform.position, 4f, _dir, m_range, 1 << 11);

        if (circleHits.Length < 1)
            return null;

        float theta = Mathf.Acos(Vector2.Dot(_dir.normalized, (Vector2)(-transform.position + circleHits[0].transform.position).normalized)) * Mathf.Rad2Deg;

        if (theta > m_hitDegree)
            return null;

        MonsterBase monster = circleHits[0].transform.GetComponent<MonsterBase>();
        monster.SetTargetUI(true);


        return monster;
    }
    #endregion

    #region PublicMethod
    public override void Function(Player _player)
    {
        Vector2 direction = ((Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition) - (Vector2)transform.position).normalized;

        if (m_targetMonster != null)
            m_targetMonster.SetTargetUI(false);

        m_targetMonster = FindTargetMonster(direction);

        if (m_targetMonster != null)
        {
            Vector2 targetPos = m_targetMonster.GetPosition();
            direction = targetPos - (Vector2)transform.position;
        }

        _player.UseToolAnimator();
        Fire(direction.normalized);
    }
    #endregion
}
