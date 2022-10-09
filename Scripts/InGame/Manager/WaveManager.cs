using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

[System.Serializable]
public class WaveMonsterInfo
{
    #region PrivateVariables
    [SerializeField] private int waveNum;
    [SerializeField] private int attacker;
    [SerializeField] private int defender;
    #endregion

    #region PublicMethod
    public int GetAttacker()
    {
        return attacker;
    }
    public int GetDefender()
    {
        return defender;
    }

    public WaveMonsterInfo CreateWaveMonsterInfo()
    {
        WaveMonsterInfo waveMonsterInfo = new WaveMonsterInfo();

        waveMonsterInfo.waveNum = waveNum + 1;
        waveMonsterInfo.attacker = attacker;
        waveMonsterInfo.defender = defender;

        return waveMonsterInfo;
    }
    #endregion

}


public class WaveManager : MonoBehaviourPunCallbacks, IManageable
{
    #region PrivateVariables
  
    [SerializeField] private int m_waveLevel = 1;

    [SerializeField] private IntEventChannelSO m_waveEventChannelSO;
    [SerializeField] private MonsterManager m_monsterManager;
    private NomadCartManager m_cartManager;

    private double m_startTime = 0f;
    private float curTime = 0f;

    private int WAVE_COOL_TIME = 60;
    
    private List<WaveMonsterInfo> m_waveMonster;
    [SerializeField] private TextAsset m_waveMonsterInfoJson;

    private bool isGameStart = false;
    #endregion


#if UNITY_EDITOR
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
            StartWave();
    }
#endif


    public void CreateWaveSetting()
    {
        m_waveMonster = new List<WaveMonsterInfo>(JsonParser.ParseToTArray<WaveMonsterInfo>(m_waveMonsterInfoJson.text));
        if (PhotonNetwork.IsMasterClient)
            Invoke(nameof(WaveInitialSetting), 2f);
    }
    #region PrivateMethod
    private void FixedUpdate()
    {
        if (PhotonNetwork.IsMasterClient == true && isGameStart == true)
        {
            curTime += Time.fixedDeltaTime;

            if ((int)(curTime / WAVE_COOL_TIME) == m_waveLevel)
            {
                if (WAVE_COOL_TIME < 180)
                {
                    WAVE_COOL_TIME += 9;
                    curTime += (9 * m_waveLevel);
                }
                StartWave();
            }    
        }
    }

    private void WaveInitialSetting()
    {
        m_monsterManager.WaveSetting(GetNomadCartManager());
    }
    private void UpdateWaveMonsterInfo()
    {
        WaveMonsterInfo updateWaveMonster = m_waveMonster[m_waveLevel - 2].CreateWaveMonsterInfo();
        m_waveMonster.Add(updateWaveMonster);
    }
    #endregion

    #region PublicMethod
    public void StartWave()
    {
        if (m_waveLevel > 5)
            UpdateWaveMonsterInfo();

        if (m_waveLevel % 2 == 0)
            m_monsterManager.UpgradeSpec();

        m_waveEventChannelSO.RaiseEvent(m_waveLevel);
        m_monsterManager.WaveSpawner(m_waveMonster[m_waveLevel - 1], GetNomadCartManager());
        m_waveLevel += 1;

    }
    private NomadCartManager GetNomadCartManager()
    {
        if (m_cartManager == null)
            m_cartManager = FindObjectOfType<NomadCartManager>();
        return m_cartManager;
    }

    public NomadCartBase GetEngineCart()
    {
        return GetNomadCartManager().GetCart(0);
    }


    public RecordWaveInfo GetWaveInfo()
    {
        return new RecordWaveInfo(0, m_waveLevel, m_monsterManager.GetDeadEnemyCounts());
    }
    public void InitializeObject()
    {
        CreateWaveSetting();
        GetNomadCartManager().CreateShip();
    }

    public void StartGame()
    {
        isGameStart = true;
        m_startTime = PhotonNetwork.Time;
    }
    public int GetWaveCooltime()
    {
        return WAVE_COOL_TIME;
    }

    public void StopGame()
    {
        isGameStart = false;
        m_monsterManager.StopMonsterManager();
    }
    #endregion
}
