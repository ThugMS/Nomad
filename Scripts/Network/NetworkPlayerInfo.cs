using UnityEngine;
using TMPro;

public class NetworkPlayerInfo : MonoBehaviour
{
    [SerializeField] private TMP_Text nickname;
    [SerializeField] private TMP_Text ready;

    private bool isReady;
    public bool IsReady { get => isReady; set => SetIsReady(value); }

    private string nickName;
    public string NickName { get => nickName; set => SetNickName(value); }

    #region PrivateMethod
    private void SetIsReady(bool _isReady)
    {
        isReady = _isReady;
        ready.text = isReady? ConstStringStorage.TEXT_READYTRUE : ConstStringStorage.TEXT_READYFALSE;
    }

    private void SetNickName(string _nickName)
    {
        nickName = _nickName;
        nickname.text = _nickName;
    }
    #endregion
}
