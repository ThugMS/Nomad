using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Photon.Pun;

public struct RecordWaveInfo
{
    public double survivalTime;
    public int waveNum;
    public int[] enemyCounts;
    public RecordWaveInfo(double _survivalTime, int _waveNum, int[] _enemyCounts)
    {
        survivalTime = _survivalTime;
        waveNum = _waveNum;
        enemyCounts = _enemyCounts;
    }
}
public struct RecordPlayerInfo
{
    public string nickName;
    public bool isDead;
    public int[] mineralCounts;
    public RecordPlayerInfo(string _nickName, bool _isDead, int[] _mineralCounts)
    {
        nickName = _nickName;
        isDead = _isDead;
        mineralCounts = _mineralCounts;
    }
}
public class GameOverUI : MonoBehaviourPunCallbacks
{
    private PhotonView m_photonView;

    [SerializeField] private GameObject m_gameOverPanel;

    [SerializeField] private Button m_quitButton;
    [SerializeField] private Button m_gotoLobbyButton;
    [SerializeField] private TMP_Text m_reasonText;
    [SerializeField] private TMP_Text m_timeText;
    [SerializeField] private TMP_Text m_waveNumText;
    [SerializeField] private TMP_Text[] m_enemyTexts;
    [SerializeField] private TMP_Text[] m_mineralTexts;

    [SerializeField] private GameOverPlayerSlot[] m_gameoverPlayerSlots;

    private void Awake()
    {
        m_photonView = gameObject.GetComponent<PhotonView>();

        m_quitButton.onClick.AddListener(OnClickQuitButton);
        m_gotoLobbyButton.onClick.AddListener(GoToLobby);

        OffGameOverPanel();
    }

    #region PrivateMethod
    private void GoToLobby()
    {
        Time.timeScale = 1;
        PhotonNetwork.AutomaticallySyncScene = false;
        PhotonNetwork.LeaveRoom();

    }
    private void OnClickQuitButton()
    {
        Application.Quit(0);
    }
    private void OffGameOverPanel()
    {
        m_gameOverPanel.SetActive(false);
    }
    private void SetGameOverInfo(int _deadReason, RecordPlayerInfo[] _recordPlayerInfos, RecordWaveInfo _recordWaveInfo)
    {
        //사망 원인
        m_photonView.RPC(nameof(RPC_SetTextGameOverReason), RpcTarget.AllBuffered, ConstStringStorage.TEXT_DEAD_REASONS[_deadReason]);

        //생존 시간
        string timeText = "";
        for (int i = ConstStringStorage.TEXT_TIMES.Length - 1; i >= 0; i--)
        {
            timeText += (int)(_recordWaveInfo.survivalTime / Mathf.Pow(60, i));
            timeText += ConstStringStorage.TEXT_TIMES[i];
            _recordWaveInfo.survivalTime %= Mathf.Pow(60, i);
        }
        m_photonView.RPC(nameof(RPC_SetTextSurvivalTime), RpcTarget.AllBuffered, timeText);

        //웨이브 숫자
        m_photonView.RPC(nameof(RPC_SetTextWaveNum), RpcTarget.AllBuffered, _recordWaveInfo.waveNum - 1);

        //처치한 적
        m_photonView.RPC(nameof(RPC_TextSetEnemyCounts), RpcTarget.AllBuffered, _recordWaveInfo.enemyCounts);

        int[] mineralCounts = { 0, 0 };
        //플레이어 상태
        for (int i = 0; i < m_gameoverPlayerSlots.Length; i++)
        {
            if (i >= _recordPlayerInfos.Length)
                m_photonView.RPC(nameof(RPC_SetPlayerSlotNull), RpcTarget.AllBuffered, i);
            else
            {
                m_photonView.RPC(nameof(RPC_SetPlayerStateAndMineral), RpcTarget.AllBuffered, i, _recordPlayerInfos[i].nickName, _recordPlayerInfos[i].isDead, _recordPlayerInfos[i].mineralCounts);
                for (int j = 0; j < mineralCounts.Length; j++)
                    mineralCounts[j] += _recordPlayerInfos[i].mineralCounts[j];
            }
        }

        //채굴한 자원
        m_photonView.RPC(nameof(RPC_TextSetMineralCounts), RpcTarget.AllBuffered, mineralCounts);
    }

    [PunRPC]
    private void RPC_SetTextSurvivalTime(string _time)
    {
        m_timeText.text = _time;
    }
    [PunRPC]
    private void RPC_SetTextGameOverReason(string _time)
    {
        m_reasonText.text = _time;
    }
    [PunRPC]
    private void RPC_SetTextWaveNum(int _waveNum)
    {
        m_waveNumText.text = "Wave " + _waveNum;
    }
    [PunRPC]
    private void RPC_TextSetEnemyCounts(int[] _enemyCounts)
    {
        for (int i = 0; i < m_enemyTexts.Length; i++)
            m_enemyTexts[i].text = _enemyCounts[i].ToString();
    }
    [PunRPC]
    private void RPC_SetPlayerStateAndMineral(int _slotIndex, string _nickNames, bool _isDeads, int[] _mineralCounts)
    {
            m_gameoverPlayerSlots[_slotIndex].SetPlayer(_nickNames, _isDeads, _mineralCounts);
    }
    [PunRPC]
    private void RPC_SetPlayerSlotNull(int _slotIndex)
    {

        m_gameoverPlayerSlots[_slotIndex].SetPlayer();

    }
    [PunRPC]
    private void RPC_TextSetMineralCounts(int[] _mineralCounts)
    {
        for (int i = 0; i < m_mineralTexts.Length; i++)
            m_mineralTexts[i].text = _mineralCounts[i].ToString() + "개";
    }
    #endregion

    #region PublicMethod
    public override void OnLeftRoom()
    {
        PhotonNetwork.LoadLevel(ConstStringStorage.ENTER);
    }
    public void OnGameOver(int _deadReason, RecordPlayerInfo[] _recordPlayerInfo, RecordWaveInfo _recordWaveInfo)
    {
        if (PhotonNetwork.IsMasterClient == true)
        {
            SetGameOverInfo(_deadReason,
           _recordPlayerInfo,
           _recordWaveInfo);
        }
        m_gameOverPanel.SetActive(true);
    }
    public void CloseUI()
    {
        m_gameOverPanel.SetActive(false);
    }
    #endregion
}
