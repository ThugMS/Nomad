using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class MonsterManager : MonoBehaviour
{

    #region PrivateVariables

    [SerializeField] private int m_xDistanceFromEngincart = 35;
    [SerializeField] private int m_yDistanceFromEngincart = 30;
    [SerializeField] private PhotonView m_photonView;
    [SerializeField] private IntArrayEventChannelSO m_enemyDeadCountsChannelSO;
    [SerializeField] private IntEventChannelSO m_livedEnemyCountChannelSO;
    [SerializeField] private VoidEventChannelSO m_enemyPowerUpChannelSO;

    private List<MonsterBase> dequeueMonsters = new List<MonsterBase>();
    private Dictionary<MonsterType, Queue<MonsterBase>> m_poolDiction = new Dictionary<MonsterType, Queue<MonsterBase>>();
    private int m_monsterPoolObjectViewID;

    private float m_attackerHp = MonsterConstants.COMMON_HP;
    private float m_attackerDamage = MonsterConstants.COMMON_DAMAGE;
    private float m_defenderHp = MonsterConstants.COMMON_HP;
    private float m_defenderDamage = MonsterConstants.COMMON_DAMAGE;
    private float m_supporterHp = MonsterConstants.COMMON_HP;
    private float m_supporterDamage = MonsterConstants.COMMON_DAMAGE;

    private int[] m_enemyDeadCounts = { -MonsterConstants.ATTACKER_MONSTER_POOL_COUNT, -MonsterConstants.DEFENDER_MONSTER_POOL_COUNT };
    private List<MonsterBase> m_livedMonsters = new List<MonsterBase>();
    private bool m_isPowerUpMode = false;
    #endregion


    #region PrivateMethod
    private void CommonCreatePart(MonsterBase _monster, NomadCartBase _enginCart)
    {
        _monster.SetMainTarget(_enginCart);
        _monster.InsertPool();

        if (PhotonNetwork.IsMasterClient)
        {
            int monsterViewID = _monster.gameObject.GetPhotonView().ViewID;
            m_photonView.RPC(nameof(RPC_PutInParentObject), RpcTarget.AllBuffered, monsterViewID, m_monsterPoolObjectViewID);
        }
    }

    private void MakePoolGameObject()
    {
       m_monsterPoolObjectViewID = PhotonNetwork.Instantiate(ConstStringStorage.MONSTER_POOL_PATH, Vector3.zero, Quaternion.identity).GetPhotonView().ViewID;
    }

    private void MakePoolDiction()
    {
        int monsterTypeCount = System.Enum.GetValues(typeof(MonsterType)).Length;
        for (int i = 0; i < monsterTypeCount; i++)
        {
            Queue<MonsterBase> monsterQueue = new Queue<MonsterBase>();
            m_poolDiction.Add((MonsterType)i, monsterQueue);
        }
    }

    private void CreateMonster(NomadCartManager _nomadCartManager)
    {
        NomadCartBase enginCart = _nomadCartManager.GetCart(0);
        //몬스터 생성 마스터 클라이언트에서 최초 1번 roomObj로 몬스터 생성
        for (int i = 0; i < MonsterConstants.ATTACKER_MONSTER_POOL_COUNT; i++)
        {
            MonsterAttacker monster = PhotonNetwork.Instantiate(ConstStringStorage.MONSTER_ATTACKER_PATH, new Vector3(255, 255, -100), Quaternion.identity).GetComponent<MonsterAttacker>(); 
            CommonCreatePart(monster, enginCart);
        }
        for (int i = 0; i < MonsterConstants.DEFENDER_MONSTER_POOL_COUNT; i++)
        {
            MonsterDefender monster = PhotonNetwork.Instantiate(ConstStringStorage.MONSTER_DEFENDER_PATH, new Vector3(255, 255, -100), Quaternion.identity).GetComponent<MonsterDefender>();
            CommonCreatePart(monster, enginCart);
        }
    }

    private void MoreCreateMonster(NomadCartManager _nomadCartManager, MonsterType _monsterType)
    {
        NomadCartBase enginCart = _nomadCartManager.GetCart(0);
        if(_monsterType == MonsterType.Attacker)
        {
            MonsterAttacker monster = PhotonNetwork.Instantiate(ConstStringStorage.MONSTER_ATTACKER_PATH, new Vector3(255, 255, -100), Quaternion.identity).GetComponent<MonsterAttacker>();
            CommonCreatePart(monster, enginCart);
            return;
        }
        if (_monsterType == MonsterType.Defender)
        {
            MonsterDefender monster = PhotonNetwork.Instantiate(ConstStringStorage.MONSTER_DEFENDER_PATH, new Vector3(255, 255, -100), Quaternion.identity).GetComponent<MonsterDefender>();
            CommonCreatePart(monster, enginCart);
            return;
        }
    }

    private void MakeMonster(MonsterType _monsterType, int _monsterCount, NomadCartManager _cartManager)
    {
        for (int j = 0; j < _monsterCount; j++)
        {
            if (m_poolDiction[_monsterType].Count == 0)
            {
                MoreCreateMonster(_cartManager, _monsterType);
            }
            MonsterBase monster = m_poolDiction[_monsterType].Dequeue();

            ChangeCount(monster, 1);
            dequeueMonsters.Add(monster);
        }

    }

    private void UpgradeMonster()
    {
        MonsterType monsterType;

        for (int i = 0; i < dequeueMonsters.Count; i++)
        {
            MonsterBase monster = dequeueMonsters[i];
            monsterType = monster.GetMonsterType();

            switch (monsterType)
            {
                case MonsterType.Attacker:
                    monster.UpgradeSpec(m_attackerHp, m_attackerDamage);
                    break;
                case MonsterType.Defender:
                    monster.UpgradeSpec(m_defenderHp, m_defenderDamage);
                    break;
                default:
                    Debug.Log("없는 타입의 몬스터");
                    break;

            }
        }
    }

    private void TraceTarget(NomadCartManager _nomadCartManager)
    {
        int cartCount = _nomadCartManager.GetNomadSize();
        int cartIndex;
        for (int i = 0; i < dequeueMonsters.Count; i++)
        {
            cartIndex = i % cartCount;
            MonsterBase monster = dequeueMonsters[i];
            monster.SetTraceTarget(_nomadCartManager.GetCart(cartIndex));
        }
    }

    private void RelocateMonster(NomadCartManager _nomadCartManager)
    {
        int monsterCount = dequeueMonsters.Count;
        float degree = Random.Range(0, 360);
        float differ = Mathf.PI * 2 * Mathf.Rad2Deg / monsterCount;
        NomadCartBase engineCart = _nomadCartManager.GetCart(0);
        Vector2 engineCartPos = engineCart.GetPosition();
        for (int i = 0; i < monsterCount; i++)
        {
            int randomNum = Random.Range(0, dequeueMonsters.Count);
            MonsterBase monster = dequeueMonsters[randomNum];
            dequeueMonsters.RemoveAt(randomNum);
            monster.transform.position = CustomMath.GetCenteredEllipseCoord(engineCartPos, m_xDistanceFromEngincart, m_yDistanceFromEngincart, degree);
            monster.Respawn();
            monster.ResetCurrentTarget();
            degree += differ;
        }
    }

    private void ChangeCount(MonsterBase _monsterBase, int _delta)
    {
        if(_delta > 0)
            m_livedMonsters.Add(_monsterBase);
        else
            m_livedMonsters.Remove(_monsterBase);

        m_livedEnemyCountChannelSO.RaiseEvent(m_livedMonsters.Count);

        if (CanTurnOnPowerUpMode())
            PowerUpLivedMonsters();
        else if (CanTurnOffPowerUpMode())
            ResetLivedMonstersPower();
    }

    private bool CanTurnOnPowerUpMode()
    {
        return m_livedMonsters.Count >= MonsterConstants.LIMIT_ENEMY_COUNT && m_isPowerUpMode == false;
    }

    private bool CanTurnOffPowerUpMode()
    {
        return m_livedMonsters.Count < MonsterConstants.LIMIT_ENEMY_COUNT && m_isPowerUpMode == true;
    }
    
    private void PowerUpLivedMonsters()
    {
        m_isPowerUpMode = true;
        foreach (MonsterBase monster in m_livedMonsters)
            monster.PowerUp(true);
        m_enemyPowerUpChannelSO.RaiseEvent();
    }
    
    private void ResetLivedMonstersPower()
    {
        m_isPowerUpMode = false;
        foreach (MonsterBase monster in m_livedMonsters)
            monster.PowerUp(false);
    }

    #endregion

    #region PublicMethod
    public void WaveSetting(NomadCartManager _cartManager)
    {
        MakePoolGameObject();
        MakePoolDiction();
        CreateMonster(_cartManager);
    }
    public void WaveSpawner(WaveMonsterInfo _waveMonsterInfo, NomadCartManager _cartManager)
    {
        dequeueMonsters.Clear();
        MakeMonster(MonsterType.Attacker, _waveMonsterInfo.GetAttacker(), _cartManager);
        MakeMonster(MonsterType.Defender, _waveMonsterInfo.GetDefender(), _cartManager);
        UpgradeMonster();
        TraceTarget(_cartManager);
        RelocateMonster(_cartManager);
  
    }


    public void InsertPoolEvent(MonsterBase _monster)
    {
        //생존 몬스터 빼기
        ChangeCount(_monster, -1);
        //죽은 몬스터 수 추가
        m_enemyDeadCounts[(int)_monster.GetMonsterType()] += 1;
        m_enemyDeadCountsChannelSO.RaiseEvent(m_enemyDeadCounts);
        //Pool에 추가
        m_poolDiction[_monster.GetMonsterType()].Enqueue(_monster);
    }
    public int[] GetDeadEnemyCounts()
    {
        return m_enemyDeadCounts;
    }

    public void StopMonsterManager()
    {
        foreach(MonsterBase monster in m_livedMonsters)
            monster.Sleep();
        
    }
    #endregion

    #region RPCMethod
    [PunRPC]
    void RPC_PutInParentObject(int _monster, int _parentGO)
    {
        PhotonView.Find(_monster).gameObject.transform.parent = PhotonView.Find(_parentGO).gameObject.transform;
    }
    
    public void UpgradeSpec()
    {
        m_attackerHp *= 1.2f;
        m_attackerDamage *= 1.2f;
        m_defenderHp *= 1.2f;
        m_defenderDamage *= 1.2f;
        m_supporterHp *= 1.2f;
        m_supporterDamage *= 1.2f;
    }
    #endregion
}