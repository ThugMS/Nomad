using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Photon.Pun;
using UnityEngine.SceneManagement;


[RequireComponent(typeof(PhotonView))]
public class CommonUI : MonoBehaviour
{
    #region PrivateVariable
    [SerializeField] private GameObject m_commonUIPanel;
    [SerializeField] private TMP_Text m_timeText;
    [SerializeField] private TMP_Text m_waveNumText;
    [SerializeField] private TMP_Text[] m_deadEnemyCountsText;
    [SerializeField] private TMP_Text m_currentLivedEnemyText;
    [SerializeField] private Image m_waveGage;

    [SerializeField] private TMP_Text m_currentCazelinText;
    [SerializeField] private TMP_Text m_currentStarLightText;

    [SerializeField] private Image m_cazelinFillImage;
    [SerializeField] private Image m_starLightFillImage;

    private PhotonView m_photonView;
    private double m_gameStartTime = 0;
    private int m_nextWaveCoolTime = 0;
    private float m_currentWaveCoolTime = 0;

    #region chatPanel
    [SerializeField] private GameObject m_chatBackPanel;
    [SerializeField] private GameObject m_chatSlotGameObject;
    [SerializeField] private Transform m_chatSlotBox;
    [SerializeField] TMP_InputField m_chatInputField;
    [SerializeField] private CustomVerticalScrollViewRect m_chatSlotScrollRect;

    private bool m_isTyping = false;
    private ObjectPoolChatSlot m_chatObjectPool;
    #endregion
    #endregion

    private void Awake()
    {
        m_photonView = gameObject.GetComponent<PhotonView>();
        m_commonUIPanel.SetActive(true);
        AwakeChatPanel();   
    }
    private void FixedUpdate()
    {
        if (m_gameStartTime == 0)
            return;

        UpdateTimeText();

        m_currentWaveCoolTime -= Time.fixedDeltaTime;
        UpdateWaveGage();
    }

    #region PrivateMethod

    private void AwakeChatPanel()
    {
        InputManager.Instance.AddKeyDownAction(EAction.Enter, ChangeFocusChatInputField);

        m_chatObjectPool = m_chatSlotBox.gameObject.AddComponent<ObjectPoolChatSlot>();
        m_chatObjectPool.SetObject(Resources.Load<GameObject>(ConstStringStorage.UI_CHATSLOT_PATH), 10);

        m_chatInputField.onDeselect.AddListener(delegate { SetIsTyping(false); });//선택 해제 될 때
    }
    private void ChangeFocusChatInputField()
    {
        if(m_isTyping == true && m_chatInputField.text.Length != 0)
        {//send
            SendChatMessage2Other(PhotonNetwork.LocalPlayer.NickName, m_chatInputField.text, 1);
            return;
        }
        //포커스 전환
        SetIsTyping(!m_isTyping);

    }
    private void SetIsTyping(bool _isTyping)
    { 
        m_isTyping = _isTyping;

        //배경 on/off
        m_chatBackPanel.SetActive(m_isTyping);
        //다른 Input 상태 설정
        InputManager.Instance.SetStopAllInput(m_isTyping);

        //inputField활성
        if(m_isTyping == true)
            m_chatInputField.ActivateInputField();
        
        //모든 채팅 상태 변경
        SetFadeAllChatSlot(!m_isTyping);
        //스크롤 가능 여부 설정
        m_chatSlotScrollRect.SetScrollable(_isTyping);
    }
    private void SetFadeAllChatSlot(bool _isFade)
    {
        foreach (ChatSlot slot in m_chatObjectPool.GetEnumerator())
        {
            if (_isFade == true)
                slot.SetFade();
            else
                slot.SetActiveTrue();
        }
    }
    private void SetFadeChatSlot(bool _isFade, ChatSlot _chotSlot)
    {
        if (_isFade == true)
            _chotSlot.SetFade();
        else
            _chotSlot.SetActiveTrue();
    }

    private void ClearInputField()
    {
        m_chatInputField.text = "";
    }
    [PunRPC]
    private void RPC_ReceiveChatMessageFromOther(string _nickName, string _content, int _type)
    {   
        ChatSlot chatSlot = CreateNewChatSlot(_nickName, _content, _type);

        UpdateChatBoxUsingNewChatSlot(chatSlot);
    }
    private ChatSlot CreateNewChatSlot(string _name, string _text, int _type)
    {
        ChatSlot chatSlot = m_chatObjectPool.GetObj();
        chatSlot.SetMessage(_name, _text, _type);

        return chatSlot;
    }
    private void UpdateChatBoxUsingNewChatSlot(ChatSlot _chatSlot)
    {
        _chatSlot.transform.SetAsLastSibling();

        m_chatSlotScrollRect.SetContentSize();
        m_chatSlotScrollRect.SetPositionInitialize();

        //이 슬롯을 Fade할 것인지 설정
        SetFadeChatSlot(!m_isTyping, _chatSlot);
        SoundManager.Instance.PlaySFX(SoundManager.SFX_SYSTEM_NOTI);
    }
    private void UpdateTimeText()
    {
        double time = PhotonNetwork.Time - m_gameStartTime;
        string timeText = "";
        for (int i = ConstStringStorage.TEXT_TIMES.Length - 1; i >= 0; i--)
        {
            timeText += (int)(time / Mathf.Pow(60, i));
            timeText += ConstStringStorage.TEXT_TIMES[i];
            time %= Mathf.Pow(60, i);
        }
        m_timeText.text = timeText;
    }
    private void UpdateWaveGage()
    {
        m_waveGage.fillAmount = Mathf.Max(0, m_currentWaveCoolTime / m_nextWaveCoolTime);
    }
    #region RPCMethod


    [PunRPC]
    private void RPC_SetCurrentCazelinText(int _cazelin, int _maxAmount)
    {
        m_currentCazelinText.text = "함선 " + ConstStringStorage.TEXT_CAZELIN + " " + _cazelin.ToString() + " / " + _maxAmount;
        m_cazelinFillImage.fillAmount = Mathf.Clamp01((float)(_cazelin) / _maxAmount);
    }

    [PunRPC]
    private void RPC_SetCurrentStarLightText(int _starLight, int _maxAmount)
    {
        m_currentStarLightText.text = "함선 " + ConstStringStorage.TEXT_STARLIGHT + " " + _starLight.ToString() + " / " + _maxAmount;
        m_starLightFillImage.fillAmount = Mathf.Clamp01((float)(_starLight) / _maxAmount);
    }
    [PunRPC]
    private void RPC_SetDeadEnemyCounts(int[] _counts)
    {
        for (int i = 0; i < _counts.Length; i++)
            m_deadEnemyCountsText[i].text = _counts[i].ToString();
    }
    [PunRPC]
    private void RPC_SetLivedEnemyCount(int _count)
    {
        m_currentLivedEnemyText.text = $"{_count}/{MonsterConstants.LIMIT_ENEMY_COUNT}";
    }
    [PunRPC]
    private void RPC_PlayUpgradeSound()
    {
        SoundManager.Instance.PlaySFX(SoundManager.SFX_SYSTEM_UPGRADE);
    }

    #endregion
    #endregion

    #region PublicMethod
    public void SetCommonMineral(MineralType _type, int _amount, int _maxAmount)
    {
        if (_type.Equals(MineralType.Cazelin))
            SetCurrentCazelinBar(_amount, _maxAmount);
        else
            SetCurrentStarLightBar(_amount, _maxAmount);
    }

    public void SetCurrentCazelinBar(int _cazelin, int _maxAmount)
    {
        m_photonView.RPC(nameof(RPC_SetCurrentCazelinText), RpcTarget.All, _cazelin, _maxAmount);
    }

    public void SetCurrentStarLightBar(int _starLight, int _maxAmount)
    {
        m_photonView.RPC(nameof(RPC_SetCurrentStarLightText), RpcTarget.All, _starLight, _maxAmount);
    }

    public void SendChatMessageMySelf(string _name, string _text, int _type)
    {
        ChatSlot chatSlot = CreateNewChatSlot(_name, _text, _type);

        UpdateChatBoxUsingNewChatSlot(chatSlot);
    }
    public void SendChatMessage2Other(string _name ,string _text, int _type)
    {
        if (_type == 1)
        {
            ClearInputField();
            m_chatInputField.ActivateInputField();
        }
        m_photonView.RPC(nameof(RPC_ReceiveChatMessageFromOther), RpcTarget.AllBuffered, _name, _text, _type);
    }
    public void OnReceivedUpgradeNoti()
    {
        m_photonView.RPC(nameof(RPC_PlayUpgradeSound), RpcTarget.All);
    }
    public void CloseUI()
    {
        m_commonUIPanel.SetActive(false);
    }

    public void CloseChatPanel()
    {
        SetIsTyping(false);
    }

    public void SetWaveNumText(int _waveNum)
    {
        m_waveNumText.text = "Wave " + _waveNum;
    }
    public void SetStartTime(double _time)
    {
        m_gameStartTime = _time;
    }
    public void CallRPCSetDeadEnemyCounts(int[] _counts)
    {
        m_photonView.RPC(nameof(RPC_SetDeadEnemyCounts), RpcTarget.AllBuffered, _counts);
    }
    public void CallRPCSetDeadEnemyCounts(int _count)
    {
        m_photonView.RPC(nameof(RPC_SetLivedEnemyCount), RpcTarget.AllBuffered, _count);
    }
    public void SetCoolTime(int _nextWaveCoolTime)
    {
        m_nextWaveCoolTime = _nextWaveCoolTime;
        m_currentWaveCoolTime = m_nextWaveCoolTime;
    }
    #endregion
}
