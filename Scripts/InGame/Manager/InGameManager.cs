using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public interface IManageable
{
    //게임에 필요한 오브젝트를 만들고 셋팅
    public void InitializeObject();

    //게임 시작시
    public void StartGame();

    public void StopGame();
}

public class InGameManager : MonoBehaviour
{
    #region PrivateVariables
    [SerializeField] private MapManager m_mapManager;
    [SerializeField] private WaveManager m_waveManager;
    [SerializeField] private PlayerManager m_playerManager;
    [SerializeField] private UIManager m_uiManager;

    private List<IManageable> m_localManageables = new List<IManageable>();         //로컬일때 모두 실행
    private List<IManageable> m_masterManageables = new List<IManageable>();         //마스터일때만 실행

    private bool m_isGameOver = false;
    private PhotonView m_photonView;
    private float m_gameoverDuration = 1.2f;

    private int m_deadReason = 2;
    private RecordPlayerInfo[] m_recordPlayerInfo;
    private RecordWaveInfo m_recordWaveInfo;

    private double m_survivalStartTime = 0;
    #endregion

    private void Awake()
    {
        Resources.Load<UpgradeInfoSO>(ConstStringStorage.UPGRADEINFOSO_PATH).LoadData();
        Resources.Load<CartMineralSO>(ConstStringStorage.CartMineralSO_PATH).Initialize();

        m_photonView = gameObject.GetComponent<PhotonView>();
        m_localManageables.Add(InputManager.Instance);
        m_localManageables.Add(m_uiManager);
        m_localManageables.Add(m_playerManager);


        m_masterManageables.Add(m_mapManager);
        m_masterManageables.Add(m_waveManager);
    }

    private void Start()
    {
        if(PhotonNetwork.IsMasterClient == true)
            StartCoroutine(IE_InitializeGame());
    }

    private void OnDestroy()
    {
        Resources.Load<CartMineralSO>(ConstStringStorage.CartMineralSO_PATH).Reset();
    }
    #region PrivateMethod
    private void InitializeMaster()
    {
        //마스터 클라일 경우만 할 것
        if (PhotonNetwork.IsMasterClient == false)
            return;

        for (int i = 0; i < m_masterManageables.Count; i++)
            m_masterManageables[i].InitializeObject();
    }

    [PunRPC]
    private void InitializeLocal()
    {
        for(int i = 0; i < m_localManageables.Count; i++)
            m_localManageables[i].InitializeObject();
    }

    [PunRPC]
    private void StartGame()
    {
        if(PhotonNetwork.IsMasterClient == true)
        {
            for (int i = 0; i < m_masterManageables.Count; i++)
                m_masterManageables[i].StartGame();
        }
        
        for (int i = 0; i < m_localManageables.Count; i++)
            m_localManageables[i].StartGame();

        if (PhotonNetwork.IsMasterClient == true)
            m_photonView.RPC(nameof(RPC_StartFirstUI), RpcTarget.AllBuffered, PhotonNetwork.Time);
    }

    private void Update()
    {   
        //맵 확장 지속적으로 체크
        if(m_isGameOver == false)
            ExpandMapTrigger();     
    }

    private void ExpandMapTrigger()
    {
        List<Vector2> playerPos = new List<Vector2>();
        List<Vector2> aiPositions = m_playerManager.GetAIPlayerPos();

        playerPos.Add(m_playerManager.GetPlayersPos());
        
        for(int i = 0; i < aiPositions.Count; i++)
            playerPos.Add(aiPositions[i]);
        
        
        for(int i = 0; i < playerPos.Count; i++)
        {
            Vector2 pos = playerPos[i];
            m_mapManager.CheckExapandMapTrigger(pos.x, pos.y, i != 0);
        }

        m_mapManager.SetMapChunk(playerPos);
    }

    [PunRPC]
    private void RPC_SetLoadingUI(float _loadingTime, float _percent)
    {
        m_uiManager.StartLoadingUI(_loadingTime, _percent);
    }
    [PunRPC]
    private void RPC_UIManagerPlayer()
    {
        m_uiManager.SetLocalPlayer(m_playerManager.GetLocalPlayer());
    }
    private IEnumerator IE_InitializeGame()
    {         
        while (!SceneManager.GetActiveScene().name.Equals(ConstStringStorage.INGAME))
            yield return null;
        m_photonView.RPC(nameof(RPC_SetLoadingUI), RpcTarget.AllBuffered, 2f, 0.5f);
        InitializeMaster();
        yield return CoroutineHelper.WaitForSeconds(2f);

        m_photonView.RPC(nameof(RPC_SetLoadingUI), RpcTarget.AllBuffered, 1f, 0.4f);
        m_photonView.RPC(nameof(InitializeLocal), RpcTarget.AllBuffered);
        yield return CoroutineHelper.WaitForSeconds(1f);

        Player[] players = FindObjectsOfType<Player>();
        //플레이어가 모두 소환될 때까지 기다림
        while (players.Length < PhotonNetwork.CurrentRoom.PlayerCount)
        {
            yield return null;
            players = FindObjectsOfType<Player>();
        }

        //남은 게이지 로드
        m_photonView.RPC(nameof(RPC_SetLoadingUI), RpcTarget.AllBuffered, 0.5f, 0.1f);
        yield return CoroutineHelper.WaitForSeconds(0.5f);

        m_photonView.RPC(nameof(RPC_UIManagerPlayer), RpcTarget.AllBuffered);
        m_photonView.RPC(nameof(StartGame), RpcTarget.AllBuffered);
    }

    private IEnumerator IE_MoveCamera2EngineCart(Vector3 _endPos)
    {
        float runTime = 0;
        Vector3 startPos = Camera.main.transform.position;
        _endPos.z = Camera.main.transform.position.z;
        while (runTime < m_gameoverDuration)
        {
            runTime += Time.deltaTime;
            Camera.main.transform.position = Vector3.Lerp(startPos, _endPos, runTime / m_gameoverDuration);
            yield return null;
        }

        yield return CoroutineHelper.WaitForSeconds(0.5f);

        OpenGameOverPanel();
    }
    private void StopGame(int _deadReson)
    {
        m_isGameOver = true;
        //상태 저장
        if (PhotonNetwork.IsMasterClient == true)
        {
            m_recordPlayerInfo = m_playerManager.GetPlayerInfos();
            m_recordWaveInfo = m_waveManager.GetWaveInfo();
            m_recordWaveInfo.survivalTime = PhotonNetwork.Time - m_survivalStartTime;
        }

        for (int i = 0; i < m_masterManageables.Count; i++)
        {
            m_masterManageables[i].StopGame();
        }

        for (int i = 0; i < m_localManageables.Count; i++)
        {
            m_localManageables[i].StopGame();
        }
    }
    private void OpenGameOverPanel()
    {
        m_uiManager.OnGameOver(
            m_deadReason,
            m_recordPlayerInfo,
            m_recordWaveInfo);
    }
    [PunRPC]
    private void RPC_WaveStart(int _waveNum,int _cool)
    {
        SoundManager.Instance.PlaySFX(SoundManager.SFX_WARNING);
        m_uiManager.OnWaveStart(_waveNum, _cool);
    }
    [PunRPC]
    private void RPC_StartFirstUI(double _startTime)
    {
        m_survivalStartTime = _startTime;
        m_uiManager.SetStartTimeAndWaveCoolTime(m_survivalStartTime, m_waveManager.GetWaveCooltime());
    }
    #endregion

    #region PublicMethod
    //플레이어가 죽었을때 모든 죽은 수 확인
    public void ObservePlayersDeath()
    {
        if (m_playerManager.CheckAllPlayersDeath())
            GameOver();
    }

    /// <summary>
    /// 게임 오버 
    /// </summary>
    /// <param name="_type">원인 
    /// 0. 충돌
    /// 1. 공격
    /// 2. 팀원 사망    </param>
    public void GameOver(int _type = 2)
    {
        m_deadReason = _type;
        m_photonView.RPC(nameof(RPC_PlayGameOver), RpcTarget.AllBuffered, m_deadReason);
    }
    [PunRPC]
    public void RPC_PlayGameOver(int _deadReason)
    {
        if (m_isGameOver == true) 
            return;

        m_deadReason = _deadReason;
        SoundManager.Instance.PlaySFX(SoundManager.SFX_NOMAD_DIE, 0.7f);

        StopGame(_deadReason);

        PlayGameOverAnim();
    }
    public void PlayGameOverAnim()
    {
        NomadCartBase engineCart = m_waveManager.GetEngineCart();
        m_mapManager.GameOverMapChunk(engineCart.GetPosition(), m_playerManager.GetPlayersPos());
        engineCart.PlayBrokenAnim(true);
        StartCoroutine(IE_MoveCamera2EngineCart(engineCart.GetPosition()));
    }
    
    public void OnNewWaveStart(int _waveNum)
    {
        m_photonView.RPC(nameof(RPC_WaveStart), RpcTarget.AllBuffered, _waveNum, m_waveManager.GetWaveCooltime());
    }
    public void OnBeDangerousHp(int _percent)
    {
        m_uiManager.SendDangeousHpChat(_percent);
    }
    public void OnMonsterPowerUp()
    {
        m_uiManager.SendMonsterPowerUpChat();
    }
    #endregion
}
