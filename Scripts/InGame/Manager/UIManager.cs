using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using TMPro;
using UnityEngine.UI;
using System;

using ListUpgradeButton = System.Collections.Generic.List<CustomUpgradeButton>;
using ListBuyButton = System.Collections.Generic.List<CustomBuyButton>;

[RequireComponent(typeof(RectTransform))]
public class UIManager : MonoBehaviour, IManageable
{
    #region PrivateVariables
    #region PlayerUI
    [SerializeField] private GameObject m_playerUICanvas;
    [SerializeField] private GameObject m_invenMaxUpgradeButtonsPanel;
    [SerializeField] private GameObject m_toolUpgradeButtonPanel;

    [SerializeField] private TMP_Text m_currentToolLevel;
    [SerializeField] private TMP_Text m_currenttToolKoreanName;

    [SerializeField] private Image m_currentToolImage;
    [SerializeField] private Image m_currentToolCoolTimeImage;

    [SerializeField] private CustomUpgradeButton m_toolUpgradeButton;
    [SerializeField] private CustomUpgradeButton m_maxCazelinUpgradeButton;
    [SerializeField] private CustomUpgradeButton m_maxStarlightUpgradeButton;

    [SerializeField] private GameOverUI m_gameOverPanel;
    [SerializeField] private OperationKeyInfo m_guideUI;
    [SerializeField] private GameObject m_toolSlotBox;
    [SerializeField] private LoadingUIFront m_loadingUI;

    private CommonUI m_commonUI;
    private CartUI m_cartUI;

    private Player m_localPlayer;

    private ListUpgradeButton m_upgradeButtons = new ListUpgradeButton();
    private ListBuyButton m_buyButtons = new ListBuyButton();
    private Outline[] m_toolSlotOutlines;
    private int m_currentSelectedToolIndex = -1;
    #endregion

    private int m_guideCooltime = 30;
    private float m_currentGuideCooltime = 0;
    private System.Random m_random= new System.Random();
    private Color m_tmpColor;

    private bool m_isGameStart = false;
    #endregion

    #region PublicVariables
    static public Color s_aColor = new Color(1, 1, 1, 0.3f); //비활성화 상태일 때 투명도 색상
    static public Color s_defaultColor = new Color(1, 1, 1);
    #endregion

    private void Awake()
    {
        m_toolSlotOutlines = m_toolSlotBox.GetComponentsInChildren<Outline>();

        m_commonUI = GetComponentInChildren<CommonUI>();
        m_cartUI = GetComponentInChildren<CartUI>();

        m_gameOverPanel.CloseUI();
        InputManager.Instance.AddKeyDownAction(EAction.Esc, CloaseSubCartUI);
        
    }

    private void FixedUpdate()
    {
        if (!IsGameRun())
            return;

        m_currentGuideCooltime += Time.fixedDeltaTime;
        if (m_currentGuideCooltime >= m_guideCooltime)
        {
            SendRandomGuideText();
            m_currentGuideCooltime = 0;
        }
    }

    #region PrivateMethod
    private bool IsGameRun()
    {
        return m_isGameStart == true;
    }
    IEnumerator SendFirstGuideText()
    {
        for (int i = 0; i < ConstStringStorage.TEXT_INFO_FIRST_GUIDE.Length; i++)
        {
            SendInfoChat(ConstStringStorage.TEXT_INFO_FIRST_GUIDE[i]);
            yield return CoroutineHelper.WaitForSeconds(3f);
        }
    }
    private void SendRandomGuideText()
    {
        int index = m_random.Next(ConstStringStorage.TEXT_INFO_GUIDES.Length);
        SendInfoChat(ConstStringStorage.TEXT_INFO_GUIDES[index]);
    }
    private void UpdateToolLevel()
    {
        m_currentToolLevel.text = "Lv. " + m_toolUpgradeButton.GetLevel();
    }
    private void SetNextWaveCoolTime(int _nextWaveTime)
    {
        m_commonUI.SetCoolTime(_nextWaveTime);
    }
    private void OnDestroy()
    {
        m_toolUpgradeButton.OnUpdated -= UpdateToolLevel;
    }
    private void CloseAllGameUI()
    {
        m_cartUI.CloseAllCartUI();
        m_guideUI.HideOperationInfo();
        m_commonUI.CloseChatPanel();
    }
    private void CloaseSubCartUI()
    {
        m_cartUI.CloseSubCartUI();
        m_guideUI.HideOperationInfo();
        m_commonUI.CloseChatPanel();
    }
    private void SetToolSlotOutline(int _slotIndex, bool _isEnabled)
    {
        if (_slotIndex < 0  || _slotIndex >= m_toolSlotOutlines.Length)
            return;

        m_toolSlotOutlines[_slotIndex].enabled = _isEnabled;
    }
    #endregion

    #region PublicMethod

    public void SetPlayerCurrentToolUI(ToolBase _currentTool)
    {
        int newToolIndex = (int)_currentTool.GetToolType();
        if (m_currentSelectedToolIndex == newToolIndex)
            return;

        SetToolSlotOutline(m_currentSelectedToolIndex, false);
        m_currentSelectedToolIndex = newToolIndex;
        SetToolSlotOutline(m_currentSelectedToolIndex, true);

        Tuple<Sprite, int, string> _currentToolInfo = _currentTool.GetToolInfo();
        m_currentToolImage.sprite = _currentToolInfo.Item1;
        m_currentToolCoolTimeImage.sprite = _currentToolInfo.Item1;
        SetPlayerCurrentToolCoolTimeUI(0);

        m_currenttToolKoreanName.text = _currentToolInfo.Item3;

        m_toolUpgradeButton.SetUpgradeID(_currentTool.GetUpgradeID());
    }

    public void SetPlayerCurrentToolCoolTimeUI(float _amount)
    {
        m_currentToolCoolTimeImage.fillAmount = _amount;
    }

    /// <summary>
    /// 공용 자원이 바뀔 때마다 이벤트 호출로 불려짐.
    /// 모든 버튼의 상태를 업데이트
    /// </summary>
    public void CheckOnOffSpendButton()
    {
        //모든 업그레이드 버튼을 확인하고 사용 가능 여부 업데이트
        foreach(CustomUpgradeButton upgradebutton in m_upgradeButtons)
            upgradebutton.UpdateUsable();
        foreach (CustomBuyButton buybutton in m_buyButtons)
            buybutton.UpdateUsable();
    }

    public void OnOffMineralMaxUpgradeButtonPanel(bool _onoff)
    {
        m_invenMaxUpgradeButtonsPanel.SetActive(_onoff);
    }

    public void OnOffToolUpgradeButtonPanel(bool _onoff)
    {
        m_toolUpgradeButtonPanel.SetActive(_onoff);
    }

    public void OnOffToolUpgradeButton(bool _onoff)
    {
        m_toolUpgradeButton.gameObject.SetActive(_onoff);
    }

    public void OnOffCazelinMaxUpgradeButton(bool _onoff)
    {
        m_maxCazelinUpgradeButton.gameObject.SetActive(_onoff);
    }
    
    public void OnOffStarlightMaxUpgradeButton(bool _onoff)
    {
        m_maxStarlightUpgradeButton.gameObject.SetActive(_onoff);
    }

    public void OnGameOver(int _deadReason, RecordPlayerInfo[] _recordPlayerInfo, RecordWaveInfo _recordWaveInfo)
    {
        m_gameOverPanel.OnGameOver(_deadReason,
            _recordPlayerInfo,
            _recordWaveInfo);
    }

    public void AddUpgradeButton<T>(string _id, CustomUpgradeButton _button, T _upgradable) where T : IUpgradeable
    {
        m_upgradeButtons.Add(_button);
        _button.InitializeUpgradeSetting(_id,_upgradable);
        _button.OnUpgradeWithInfo += SendUpgradeNotiChat;
    }
    public void AddUpgradeButton(string _id, CustomUpgradeButton _button) 
    {
        m_upgradeButtons.Add(_button);
        _button.InitializeUpgradeSetting(_id);
        _button.OnUpgradeWithInfo += SendUpgradeNotiChat;
    }
    public void AddBuyButton( CustomBuyButton _button)
    {
        m_buyButtons.Add(_button);
        _button.OnBuyWithInfo += SendNotiChat;
    }

    public void InitializeObject()
    {

    }

    public void StartGame()
    {
        m_isGameStart = true;

        AddUpgradeButton(ConstStringStorage.UPGRADE_ID_GUN_ATTACK, m_toolUpgradeButton, m_localPlayer);
        m_toolUpgradeButton.OnUpdated += UpdateToolLevel;

        AddUpgradeButton(ConstStringStorage.UPGRADE_ID_INVEN_CAZELIN_CAPACITY, m_maxCazelinUpgradeButton, m_localPlayer);
        AddUpgradeButton(ConstStringStorage.UPGRADE_ID_INVEN_STARLIGHT_CAPACITY, m_maxStarlightUpgradeButton, m_localPlayer);

        m_cartUI.SetUIManagerSpendButton(this);

        CheckOnOffSpendButton();

        StartCoroutine(SendFirstGuideText());
    }
    public void OnWaveStart(int _waveNum, int _coolTime)
    {
        m_commonUI.SendChatMessageMySelf(ConstStringStorage.TEXT_DANGER_CHAT_NAME, $"Wave {_waveNum} 이 시작됩니다.\n함선을 지켜내세요.", 0);
        m_commonUI.SetWaveNumText(_waveNum);
        SetNextWaveCoolTime(_coolTime);
    }
    public void OnGetNewCart(int _cartType)
    {
        m_commonUI.SendChatMessageMySelf(ConstStringStorage.TEXT_NOTI_CHAT_NAME, $"누군가 새로운 <{ConstStringStorage.TEXT_CARTS[_cartType]}>을 얻었습니다.", 3);
    }
    public void SendMonsterPowerUpChat()
    {
        m_commonUI.SendChatMessageMySelf(ConstStringStorage.TEXT_DANGER_CHAT_NAME, $"생존한 적이 한계치를 초과하여 광폭화 상태가 되었습니다!!!!!", 0);
    }
    public void SendDangeousHpChat(int _percent)
    {
        m_commonUI.SendChatMessageMySelf(ConstStringStorage.TEXT_DANGER_CHAT_NAME, $"함선이 공격받는 중입니다! 함선의 체력이{_percent}% 만큼 남았습니다. 함선을 수리하세요.", 0);
    }
    public void SendInfoChat( string _str)
    {
        m_commonUI.SendChatMessageMySelf(ConstStringStorage.TEXT_GUIDE_CHAT_NAME, _str, 2);
    }
    public void SendNotiChat(string _str)
    {
        m_commonUI.SendChatMessage2Other(ConstStringStorage.TEXT_NOTI_CHAT_NAME, _str, 3);
    }
    public void SendUpgradeNotiChat(string _str)
    {
        m_commonUI.OnReceivedUpgradeNoti();
        m_commonUI.SendChatMessage2Other(ConstStringStorage.TEXT_NOTI_CHAT_NAME, _str, 3);
    }
    public void SetLocalPlayer(Player _player)
    {
        m_localPlayer = _player;
    }
    public void SetStartTimeAndWaveCoolTime(double _time, int _nextWaveCoolTime)
    {
        m_commonUI.SetStartTime(_time);
        SetNextWaveCoolTime(_nextWaveCoolTime);
        m_loadingUI.HideLoadingUI();
    }

    /// <summary>
    /// MonsterManager에서 몬스터가 소환될 때 바뀜
    /// </summary>
    /// <param name="_counts"></param>
    public void SetDeadEnemyCounts(int[] _counts)
    {
        m_commonUI.CallRPCSetDeadEnemyCounts(_counts);
    }
    /// <summary>
    /// MonsterManager에서 생존한 몬스터가 바뀔 때 바뀜 
    /// </summary>
    /// <param name="_counts"></param>
    public void SetLivedEnemyCount(int _count)
    {
        m_commonUI.CallRPCSetDeadEnemyCounts(_count);
    }
    public void StartLoadingUI(float _baseLoadingTime, float _percent)
    {
        m_loadingUI.StartLoading(_baseLoadingTime, _percent);
    }
    public void StopGame()
    {
        m_isGameStart = false;

        m_cartUI.CloseAllCartUI();
        m_commonUI.CloseUI();
    }

    #endregion
}
