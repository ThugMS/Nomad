using UnityEngine;
using TMPro;

public class NetworkRoomInfo : MonoBehaviour
{
    #region PrivateValues
    [SerializeField] private TMP_Text m_roomName;
    [SerializeField] private TMP_Text m_curCount;
    private string nickName;
    #endregion

    #region PrivateMethod
    private void SetRoomName(string _nickName)
    {
        nickName = _nickName;
        m_roomName.text = _nickName;
    }
    #endregion

    #region PublicMethod
    public string RoomName { get => nickName; set => SetRoomName(value); }

    public void SetPlayerCount(int _playerCount, int _maxCount)
    {
        m_curCount.text = _playerCount.ToString() + " / " + _maxCount.ToString();
    }
    #endregion
}
