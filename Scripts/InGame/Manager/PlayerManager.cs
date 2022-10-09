using UnityEngine;
using System.Collections.Generic;
using Photon.Pun;

public class PlayerManager : MonoBehaviour, IManageable
{
#region PrivateVariables
    [SerializeField] private List<Vector2> m_playerSpawnPos;

    private List<AIPlayer> m_AiList = new List<AIPlayer>();
    private Player m_player;
    private const int m_maxPlayer = 4;
#endregion

    private void StopAllAIPlayer()
    {
        foreach(ISleepable sleepable in m_AiList)
            sleepable.Sleep();
    }

    private void StartAllAIPlayer()
    {
        foreach (AIPlayer ai in m_AiList)
            ai.StartGame();
    }
#region PublicMethod

    public void Spawn(Vector2 _pos)
    {
        m_player = PhotonNetwork.Instantiate(ConstStringStorage.PLAYER_PATH, _pos, Quaternion.identity).GetComponent<Player>();
        m_player.SetNickName(PhotonNetwork.LocalPlayer.NickName);
    }

    public void SpawnAI(Vector2 _pos)
    {
        AIPlayer ai = PhotonNetwork.Instantiate(ConstStringStorage.AI_PLAYER_PATH, _pos, Quaternion.identity).GetComponent<AIPlayer>();
        ai.SetNickName("AI" + m_AiList.Count);
        m_AiList.Add(ai);
    }

    public bool CheckAllPlayersDeath()
    {
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            GameObject playerObject = PhotonNetwork.PlayerList[i].TagObject as GameObject;
            Player player = playerObject.GetComponent<Player>();

            if (!player.CheckPlayerDeadState())
                return false;
        }
        return true;
    }
    public RecordPlayerInfo[] GetPlayerInfos()
    {
        List<IPlayerInfoRecordable> stateRecordables = new List<IPlayerInfoRecordable>();
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            GameObject playerObject = PhotonNetwork.PlayerList[i].TagObject as GameObject;
            Player player = playerObject.GetComponent<Player>();

            stateRecordables.Add(player);
        }
        stateRecordables.AddRange(m_AiList);

        RecordPlayerInfo[] recordPlayerInfos = new RecordPlayerInfo[stateRecordables.Count];

        for (int i = 0; i < recordPlayerInfos.Length; i++)
        {
            recordPlayerInfos[i].nickName = stateRecordables[i].GetNickName();
            recordPlayerInfos[i].isDead = stateRecordables[i].IsDeadState();
            recordPlayerInfos[i].mineralCounts = stateRecordables[i].GetMineralMiningCounts();
        }
        return recordPlayerInfos;
    }
    public Vector2 GetPlayersPos()
    {
        if (m_player == null)
            return Vector2.zero;

        return m_player.transform.position;
    }
    public List<Vector2> GetAIPlayerPos()
    {
        List<Vector2> posList = new List<Vector2>();
        for(int i = 0; i < m_AiList.Count; i++)
        {
            Vector2 aiPos = m_AiList[i].transform.position;
            posList.Add(aiPos);
        }
        return posList;
    }
    public Player GetLocalPlayer()
    {
        return m_player;
    }
    public void InitializeObject()
    {
        Random.InitState(PhotonNetwork.LocalPlayer.ActorNumber);
        Vector2 pos = CustomMath.CircleCoord(Vector2.zero, 7f, Random.Range(0, 360));

        pos = CustomMath.CircleCoord(Vector2.zero, 7f, Random.Range(0, 360));
        Spawn(pos);

        if (PhotonNetwork.IsMasterClient == true)
        {
            int currentPlayerCount = PhotonNetwork.CurrentRoom.PlayerCount;
            Debug.Log($"현재 플레이어 수 : {currentPlayerCount})");
            if (currentPlayerCount == m_maxPlayer)
                return;

#if UNITY_EDITOR
            SpawnAI(pos);
#else

            for (int i = m_maxPlayer - 1; i >= currentPlayerCount; i--)
            {
                pos = CustomMath.CircleCoord(Vector2.zero, 7f, Random.Range(0, 360));
                SpawnAI(pos);
            }
#endif
        }
    }
    public void StartGame()
    {
        m_player.AttachNomadRope();
        m_player.StartGame();
        StartAllAIPlayer();
    }

    public void StopGame()
    {
        m_player.Sleep();
        StopAllAIPlayer();
    }
#endregion
}
