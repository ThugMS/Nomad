using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using UnityEngine.UIElements;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    protected static NetworkManager m_instance;
    public static NetworkManager Instance => m_instance;

    [SerializeField] private byte m_roomMaxPlayer = 2;

    [SerializeField] private TMP_InputField m_nickName;
    [SerializeField] private TMP_InputField m_roomName;
    [SerializeField] private TMP_Text m_currentRoomName;
    [SerializeField] private TMP_Text m_enterText;
    [SerializeField] private TMP_Text m_roomPageText;
    [SerializeField] private CustomLobbyButton m_enterCustomButton;
    [SerializeField] private GameObject m_enterButton;
    [SerializeField] private GameObject m_createButton;
    [SerializeField] private GameObject m_readyButton;
    [SerializeField] private GameObject m_startButton;
    [SerializeField] private GameObject m_inputCanvas;

    [SerializeField] private GameObject m_enterPanel;
    [SerializeField] private GameObject m_lobbyPanel;
    [SerializeField] private GameObject m_roomPanel;

    [SerializeField] private List<GameObject> m_playerNetworkInfos = new List<GameObject>();
    [SerializeField] private List<NetworkRoomInfo> m_roomListPanel = new List<NetworkRoomInfo>();

    private List<RoomInfo> m_waitingRoomList = new List<RoomInfo>();
    private PhotonView m_photonView;
    private Hashtable m_props = new Hashtable() { { "IsReady", false } };

    private bool m_isReady;
    private int m_readyPlayer;

    private int m_roomPageIndex = 0;
    private int m_roomPageMin = 0;
    private int m_roomPageMax = 0;

    private const string BASE_ROOM_NAME = "Nomad ";
    private const string BASE_ENTER_TEXT = "생성";
    private const string BASE_ENTERING_TEXT = "입장 중...";

    private void Awake()
    {
        Screen.SetResolution(1920, 1080, true);

        m_photonView = GetComponent<PhotonView>();

        if (m_instance == null)
            m_instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        if (CheckLogin())
        {
            m_enterPanel.SetActive(false);
            m_lobbyPanel.SetActive(true);
        }

        m_nickName.onSubmit.AddListener((st) => m_enterButton.GetComponent<UnityEngine.UI.Button>().onClick.Invoke());
        m_roomName.onSubmit.AddListener((st) => m_createButton.GetComponent<UnityEngine.UI.Button>().onClick.Invoke());

        m_enterText.text = BASE_ENTER_TEXT;
    }

    #region PrivateMethod
    private bool CheckLogin()
    {
        if (PhotonNetwork.NetworkClientState == ClientState.PeerCreated)
            return false;

        return true;      
    }

    private void InactiveButton()
    {
        m_enterText.text = BASE_ENTERING_TEXT;
        m_enterCustomButton.interactable = false;
       
    }

    private void ActiveButton()
    {
        m_enterText.text = BASE_ENTER_TEXT;
        m_enterCustomButton.interactable = true;

    }

    private bool CheckAllPlayerReady()
    {
        for (int i = 0; i < PhotonNetwork.PlayerListOthers.Length; i++)
        {
            if (!(bool)PhotonNetwork.PlayerListOthers[i].CustomProperties[ConstStringStorage.ISREADY])
                return false;
        }

        return true;
    }

    private void ResetRoomList()
    {
        for(int i = 0;  i< m_roomListPanel.Count; i++)
        {
            m_roomListPanel[i].gameObject.SetActive(false);
        }
    }

    private void MakeRoomList()
    {
        int panelCount = m_roomListPanel.Count;
        int startIndex = 0;
        int lastIndex = 0;

        CalMaxPageRoom(panelCount);
        CalIndexNum(ref startIndex, ref lastIndex, panelCount);
    
        int pannelIndex = 0;
        for (int i = startIndex; i <= lastIndex; i++)
        {
            m_roomListPanel[pannelIndex].gameObject.SetActive(true);
            m_roomListPanel[pannelIndex].RoomName = m_waitingRoomList[i].Name;
            m_roomListPanel[pannelIndex].SetPlayerCount(m_waitingRoomList[i].PlayerCount, m_waitingRoomList[i].MaxPlayers);
            pannelIndex++;
        }
       
    }

    private void CalMaxPageRoom(int _panelCount)
    {
        m_roomPageMax = m_waitingRoomList.Count / _panelCount;
        if (m_waitingRoomList.Count >= 1 && m_waitingRoomList.Count % _panelCount == 0)
            m_roomPageMax -= 1;
    }

    private void CalIndexNum(ref int _start, ref int _last, int _panelCount)
    {
        int startIndex = m_roomPageIndex * _panelCount;
        int lastIndex = startIndex + _panelCount - 1;
        if (lastIndex >= m_waitingRoomList.Count - 1)
        {
            lastIndex = m_waitingRoomList.Count - 1;
        }

        _start = startIndex;
        _last = lastIndex;
    }
    private void ResetPlayerInfo()
    {
        Hashtable playerCP = PhotonNetwork.LocalPlayer.CustomProperties;
        if (PhotonNetwork.IsMasterClient == true)
        {
            m_startButton.SetActive(true);
            m_readyButton.SetActive(false);
            playerCP["IsReady"] = true;
            m_isReady = true;
            return;
        }

        m_isReady = false;
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            if (PhotonNetwork.PlayerList[i].IsMasterClient == true)
            {
                PhotonNetwork.PlayerList[i].CustomProperties["IsReady"] = true;
                break;
            }
        }
        m_startButton.SetActive(false);
        m_readyButton.SetActive(true);
        playerCP["IsReady"] = false;

    }

    #endregion

    #region PublicMethod
    public void JoinLobby()
    {
        if(PhotonNetwork.NetworkClientState == ClientState.ConnectingToNameServer)
            return;

        if (string.IsNullOrEmpty(m_nickName.text))
            m_nickName.text = "TempNickName";

        PhotonNetwork.NickName = m_nickName.text;
        PhotonNetwork.ConnectUsingSettings();
        InactiveButton();
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    public void JoinRoom(int _idx)
    {
       
        //버튼에 할당된 idx를 받아서, 방리스트[idx]의 정보대로 방 참가.
        string roomName = m_roomListPanel[_idx].RoomName;
        PhotonNetwork.JoinRoom(roomName);
    }
    
    public void MakeRoom()
    {
        if (string.IsNullOrEmpty(m_roomName.text))
            m_roomName.text = BASE_ROOM_NAME+Random.Range(100,1000);

        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = m_roomMaxPlayer;

        PhotonNetwork.CreateRoom(m_roomName.text, roomOptions, null);
    }


    private bool m_canClickReady = true;
    private float m_readyDelay = 1f;
    public void OnClickReady()
    {
        if (m_canClickReady == false)
            return;

        StartCoroutine(IE_GiveReadyButtonDelay());
        m_isReady = !m_isReady;
        m_props["IsReady"] = m_isReady;
        PhotonNetwork.LocalPlayer.SetCustomProperties(m_props);
    }

    private IEnumerator IE_GiveReadyButtonDelay()
    {
        float delay = 0f;
        m_canClickReady = false;
        UnityEngine.UI.Button button = m_readyButton.GetComponent<UnityEngine.UI.Button>();
        button.interactable = false;
        while (delay < m_readyDelay)
        {
            delay += Time.deltaTime;
            yield return null;
        }
        m_canClickReady = true;
        button.interactable = true;
    }

    public void StartGame()
    {
        if (CheckAllPlayerReady())
        {
            PhotonNetwork.CurrentRoom.IsOpen = false;
            ActiveInputCanvas(false);
            m_photonView.RPC(nameof(RPC_LoadInGame), RpcTarget.All);
        }
    }

    public void ActiveInputCanvas(bool _active)
    {
        m_photonView.RPC(nameof(RPC_ActiveInputCanvas), RpcTarget.All, _active);
    }

    public void RoomPageButton(int _page)
    {
        m_roomPageIndex += _page;

        if (m_roomPageIndex < m_roomPageMin)
            m_roomPageIndex = m_roomPageMax;

        else if (m_roomPageIndex > m_roomPageMax)
            m_roomPageIndex = m_roomPageMin;

        ResetRoomList();
        MakeRoomList();
    }
    #endregion

    #region PhotonOverride
    public override void OnDisconnected(DisconnectCause cause)
    {
        ActiveButton();

    }
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable { { ConstStringStorage.ISREADY, false } });
        PhotonNetwork.JoinLobby();        
    }

    public override void OnJoinedLobby()
    {
        m_lobbyPanel.SetActive(true);

        m_roomPanel.SetActive(false);
        m_enterPanel.SetActive(false); 
      
        m_waitingRoomList.Clear();
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        //로비에서만 호출
        //1. 로비에 입장하는 순간 존재하는 방을 대상으로 진행
        //2. 이후 방에 변화가 있을 때마다 호출

        //유효한 대기방 걸러내기
        for (int i = 0; i < roomList.Count; i++)
        {
            RoomInfo roomInfo = roomList[i];
            if (roomInfo.RemovedFromList)
            {
                m_waitingRoomList.Remove(roomInfo);
                continue;
            }
            if(!roomInfo.IsOpen)
            {
                m_waitingRoomList.Remove(roomInfo);
                continue;
            }

            if (m_waitingRoomList.IndexOf(roomInfo) < 0)
                m_waitingRoomList.Add(roomInfo);
            else
                m_waitingRoomList[m_waitingRoomList.IndexOf(roomInfo)] = roomInfo;

        }

        ResetRoomList();
        MakeRoomList();
    }

    public override void OnJoinedRoom()
    {
        m_roomPanel.SetActive(true);

        m_enterPanel.SetActive(false);
        m_lobbyPanel.SetActive(false);

        m_currentRoomName.text = "방 이름 : " + PhotonNetwork.CurrentRoom.Name;
        
        ResetPlayerInfo();
        RPC_UpdatePlayerInfo();
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        //같은 방제목으로 실패시 번호 바꿔서 생성
        if(returnCode == 32766)
        {
            m_roomName.text = BASE_ROOM_NAME + Random.Range(100, 1000);
            MakeRoom();
        }
    }

    public override void OnLeftRoom()
    {
        m_photonView.TransferOwnership(PhotonNetwork.LocalPlayer);
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        RPC_UpdatePlayerInfo();
    }

    public override void OnPlayerPropertiesUpdate(Photon.Realtime.Player targetPlayer, Hashtable changedProps)
    {
        RPC_UpdatePlayerInfo();
    }
    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        ResetPlayerInfo();
        RPC_UpdatePlayerInfo();
    }
    #endregion

    #region PUNMethod
    [PunRPC]
    private void RPC_LoadInGame()
    {
        StartCoroutine(IE_LoadScene(ConstStringStorage.INGAME));
    }

    [PunRPC]
    private void RPC_ActiveInputCanvas(bool _active)
    {
        m_inputCanvas.SetActive(_active);
    }

    [PunRPC]
    private void RPC_UpdatePlayerInfo()
    {
        for (int i = 0; i < 4; i++)
            m_playerNetworkInfos[i].SetActive(false);

        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            m_playerNetworkInfos[i].SetActive(true);
            NetworkPlayerInfo info = m_playerNetworkInfos[i].GetComponent<NetworkPlayerInfo>();
            info.NickName = PhotonNetwork.PlayerList[i].NickName;
            info.IsReady = (bool)PhotonNetwork.PlayerList[i].CustomProperties["IsReady"];
        }
    }
    #endregion

    #region IEnumerator
    private IEnumerator IE_LoadScene(string _scenename)
    {     
        AsyncOperation loadAsync = SceneManager.LoadSceneAsync(_scenename, LoadSceneMode.Additive);
        while (loadAsync.progress < 1f)
            yield return null;
            
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(_scenename));
    }
    #endregion

}


