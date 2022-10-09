using System;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using CustomStatePattern;
using UnityEngine.UI;
using ExitGames.Client.Photon;
using Unity.VisualScripting;


interface IPossesNickName
{
    public string GetNickName();
}
public interface IPlayerInfoRecordable
{
    public string GetNickName();
    public bool IsDeadState();
    public int[] GetMineralMiningCounts();
}
public interface ISleepable
{
    public void Sleep();
}


public class Player : StateMachineBase<Player>, IUpgradeable, IPossesNickName, IRopeConnectable, IPlayerInfoRecordable,ISleepable
{
    #region PrivateVariable 
    [SerializeField] private VoidEventChannelSO m_playerDeadEventChannelSO;
    [SerializeField] private VoidEventChannelSO m_playerDisconnectEventChannelSO;
    [SerializeField] private BoolEventChannelSO m_onoffToolUpgradeButtonChannelSO;
    [SerializeField] private BoolEventChannelSO m_onoffMineralUpgradeButtonChannelSO;
    [SerializeField] private StringEventChannelSO m_updateUpgradeButtonChannelSO;

    [SerializeField] private TMP_Text m_nickName;
    [SerializeField] private GameObject m_directionToEngineCart;
    [SerializeField] private GameObject[] m_directionToDeadPlayer;
    [SerializeField] private PlayerInvenSO m_invenSO;
    [SerializeField] private UpgradeInfoSO m_upgradeInfoSO;

    [SerializeField] private PlayerRopePhysics m_nomadRope;
    [SerializeField] private PlayerRopePhysics m_playerRope;

    [SerializeField] private Animator m_keyAnimator;
    [SerializeField] private SpriteRenderer m_secondSpriteRenderer;

    private PlayerMineralMoving m_mineralMoving;
    private Dictionary<GameObject, bool> m_otherPlayerDeadState = new Dictionary<GameObject, bool>();
    private Dictionary<GameObject, GameObject> m_arrowTowardToDeadPlayer = new Dictionary<GameObject, GameObject>();
    private Camera m_mainCamera;

    private BoxCollider2D m_boxCollider;
    private CircleCollider2D m_circleTriggerColider;
    private PhotonView m_photonView;
    private Animator m_animator;
    private SpriteRenderer m_spriteRenderer;


    private PlayerInput m_input;
    private PlayerProperty m_property;

    private NomadCartBase m_currentConnectedCart;
    private Transform m_putRequestCartTransform = null;
    private NomadEngineCart m_nomadEngine;
    private MineralBase m_prevMineralOutline;

    private BalloonText m_balloonText;

    private Vector2 m_direction = Vector2.zero;
    private string m_nickNameString;
    private int usedArrowCount = 0;

    private const int m_mineralMask = 1 << 10;

    private bool m_isSendPutMineralEvent = false;
    private MineralInfo m_expectedRemained = new MineralInfo(MineralType.Cazelin, 0);

    private enum InteractionType
    {
       keycodeE, useExtractor, usePickax, toIdle
    }

    #region UI
    [SerializeField] private GameObject m_statePanel;
    [SerializeField] private GameObject m_overlayCanvas;

    [SerializeField] private Image m_oxygenFillImage;
    [SerializeField] private Image[] m_mineralFillImages;
    [SerializeField] private TMP_Text[] m_mineralCountTexts;
    [SerializeField] private Image m_dangerOxygenImage;
    [SerializeField] private GameObject m_deadPanelGameObject;

    private Color m_dangerOxygenTmpColor;
    private int[] m_oxygenChangeTiming = {20, 40};
    private Color[] m_oxygenGageColor = {new Color(1, 0.2f, 0.1f), new Color(1, 0.5f, 0.1f), new Color(0.6f, 0.7f, 1) };
    #endregion
    #endregion

    #region PublicVariable
    public PlayerIdle m_idle = new PlayerIdle();
    public PlayerOnNomad m_onNomad = new PlayerOnNomad();
    public PlayerDead m_dead = new PlayerDead();
    public PlayerConnected m_connected = new PlayerConnected();
    public PlayerNull m_null = new PlayerNull();
    #endregion

    #region StateMachineOverride
    protected override void BaseAwake()
    {
        m_photonView = GetComponent<PhotonView>();

        if(m_photonView.IsMine == true)
        PhotonNetwork.NetworkingClient.EventReceived += OnReceivedPutMineralResultEvent;

        FindNomadEngineCart();

        m_property = GetComponent<PlayerProperty>();
        m_input = GetComponent<PlayerInput>();
       
        m_boxCollider = GetComponent<BoxCollider2D>();
        m_circleTriggerColider = GetComponent<CircleCollider2D>();
        m_animator = GetComponent<Animator>();
        m_spriteRenderer = GetComponent<SpriteRenderer>();
        m_balloonText = GetComponentInChildren<BalloonText>();
        m_mineralMoving = Instantiate(Resources.Load<GameObject>("Prefabs/Mineral/Mini/Mineral")).GetComponent<PlayerMineralMoving>();
        m_mineralMoving.SetOwner(transform);
        
        //산소줄 연결/ 연결 해제 이벤트
        m_nomadRope.OnConnected = OnRopeConnect;
        m_nomadRope.OnDisconnected = OnRopeDisconnect;
        m_nomadRope.OnDisconnectedPrevTarget = OnRopeChange;

        m_mainCamera = Camera.main;

        m_photonView.Owner.TagObject = this.gameObject;

        m_dangerOxygenTmpColor = m_dangerOxygenImage.color;

        m_input.InitialToolSetting(m_balloonText, m_invenSO, m_upgradeInfoSO);
    }
    protected override void BaseStart()
    {
        if (m_photonView.IsMine == true)
        {
            m_invenSO.OnChangedMineral += SetMineralAmountGage;
            m_invenSO.Initialize();

            m_overlayCanvas.SetActive(true);
        }

        SetActiveStatePanel(m_photonView.IsMine == true);

        CameraMove();
       
        SetInvenInitialCapacity();
        
        InputManager.Instance.AddMoveAction(SetMovdDirection);
    }
    protected override void BaseUpdate()
    {
        InformWhereIsDeadPlayer();
        InformWhereIsEngineCart();

        if (m_photonView.IsMine == false)
            return;

        m_property.UpdateOxygen();
        SetOxygenStateUI(m_property.GetOxygenPercent());

        GetConnectedCart()?.SetSpriteOutline(true);
        TryToPutMineral();

        DetectMineral();
        
        CameraMove();
    }
    protected override void SetInitialState()
    {
        m_currentState = m_idle;
    }
    protected override void SetNullState()
    {
        m_nullState = m_null;
    }
    protected override void SetStatesTransition()
    {
        m_idle.SetStateTransition(m_connected, m_dead);
        m_onNomad.SetStateTransition(m_connected);
        m_dead.SetStateTransition(m_connected);
        m_connected.SetStateTransition(m_onNomad, m_idle);
        m_null.SetStateTransition(m_idle);
    }
    #endregion

    #region PrivateMethod
    private void TryToPutMineral()
    {
        NomadCartBase cart = GetConnectedCart();

        if (cart == null)
            return;

        foreach (MineralType mineralType in typeof(MineralType).GetEnumValues())
        {
            if (m_invenSO.GetCountOfMineral(mineralType) > 0 && !cart.IsFull(mineralType))
                PutMyMineralToCart(mineralType, cart);
        }
        
    }
    private void PutMyMineralToCart(MineralType _mineralType, NomadCartBase _cart)
    {
        if (m_isSendPutMineralEvent == true)
            return;
        m_isSendPutMineralEvent = true;

        m_putRequestCartTransform = _cart.transform;

        m_expectedRemained.mineralType = _mineralType;
        m_expectedRemained.amount = _cart.PutMineral(m_photonView.ViewID, _mineralType, m_invenSO.GetCountOfMineral(_mineralType));
    }
    public void OnReceivedPutMineralResultEvent(EventData photonEvent)
    {
        int code = photonEvent.Code;

        if (code != 20)
            return;

        object[] data = (object[])photonEvent.CustomData;
        if ((int)data[0] == PhotonNetwork.LocalPlayer.ActorNumber && m_photonView.IsMine == true && (int)data[1] == m_photonView.ViewID) 
        {
            if((bool)data[2] == true)
            {
                SoundManager.Instance.PlaySFX(SoundManager.SFX_PUT_MINERAL);
                m_invenSO.RemoveMineral(m_expectedRemained.mineralType, 
                    m_invenSO.GetCountOfMineral(m_expectedRemained.mineralType) - m_expectedRemained.amount);

                if(m_expectedRemained.mineralType == MineralType.Cazelin)
                    m_mineralMoving.SetCazelinTarget(m_putRequestCartTransform);
                else
                    m_mineralMoving.SetStarlightTarget(m_putRequestCartTransform);

                m_putRequestCartTransform = null;
                m_expectedRemained.amount = 0;
            }
        }
        m_isSendPutMineralEvent = false;
    }
    private void DetectMineral()
    {
        MineralBase targetMineral = RayToFront();

        if(targetMineral != null)
        {
            if (m_prevMineralOutline == null)
                m_prevMineralOutline = targetMineral;

            if (targetMineral != m_prevMineralOutline)
                m_prevMineralOutline.SwitchOutLine(false);

            m_prevMineralOutline = targetMineral;
            targetMineral.SwitchOutLine(true);
            return;
        }

        if(m_prevMineralOutline != null)
            m_prevMineralOutline.SwitchOutLine(false);

        PopUpIcon(InteractionType.toIdle);
    }
    private MineralBase RayToFront()
    {
        RaycastHit2D rayHit = Physics2D.Raycast(transform.position, GetForward().normalized, 1, m_mineralMask);

        if (rayHit.collider == null)
            return null;

        MineralStarlight starlight = rayHit.collider.GetComponent<MineralStarlight>();
        if (starlight != null)
        {
            PopUpIcon(InteractionType.usePickax);
            return starlight;
        }

        MineralStone stone = rayHit.collider.GetComponent<MineralStone>();
        if (stone != null)
        {
            PopUpIcon(InteractionType.usePickax);
            return stone;
        }

        MineralCazelin cazelin = rayHit.collider.GetComponent<MineralCazelin>();
        if (cazelin != null)
        {
            PopUpIcon(InteractionType.useExtractor);
            return cazelin;
        }

        return null;
    }
    private void PopUpIcon(InteractionType _interaction, bool on)
    {
            m_keyAnimator.SetBool(_interaction.ToString(), on);
    }
    private void PopUpIcon(InteractionType _interaction)
    {
        m_keyAnimator.SetTrigger(_interaction.ToString());
    }
    private bool IsGameObjectInCamera(Vector2 _objPos)
    {
        Vector2 viewPoint = m_mainCamera.WorldToViewportPoint(_objPos);
        if (viewPoint.x < 0 || viewPoint.x > 1 || viewPoint.y < 0 || viewPoint.y > 1)
            return false;

        return true;
    }
    private float CalculateAngleBetweenTarget(Vector2 _targetPos)
    {
        Vector2 playerPos = transform.position;

        Vector2 directionToEngine = (_targetPos - playerPos).normalized;
        Vector2 standardVector = Vector2.right;

        //엔진카와 이미지와의 내적을 통한 각도 구하기 -> 나온 각도는 0 ~ 180도
        float angle = Mathf.Acos(Vector2.Dot(directionToEngine, standardVector)) * Mathf.Rad2Deg;

        //오른쪽인지 왼쪽인지 부호 판별 -> 외적은 3차원에서 가능 + 순서 중요
        Vector3 crossProduct = Vector3.Cross((Vector3)standardVector, (Vector3)directionToEngine);

        //cross의 z값이 0보다 크면 왼쪽 아니면 오른쪽
        float sign = Mathf.Sign(crossProduct.z);
        float targetEulerAngle = sign * angle;

        return targetEulerAngle;
    }
    private void InformWhereIsEngineCart()
    {
        if (m_photonView.IsMine == false)
            return;

        Vector2 enginePos = m_nomadEngine.transform.position;

        //카메라 안에 있으면 리턴
        if (IsGameObjectInCamera(enginePos))
        {
            m_directionToEngineCart.SetActive(false);
            return;
        }
        m_directionToEngineCart.gameObject.SetActive(true);
        float angle = CalculateAngleBetweenTarget(enginePos);
        Vector3 angleBorderPos = CalculateBordePos(transform.position, enginePos);
        m_directionToEngineCart.GetComponent<RectTransform>().localEulerAngles = new Vector3(0, 0, angle);
        m_directionToEngineCart.transform.localPosition = angleBorderPos;
    }
    private void InformWhereIsDeadPlayer()
    {
        if (m_photonView.IsMine == false)
            return;

        foreach (var playerDic in m_otherPlayerDeadState)
        {
            GameObject player = playerDic.Key;

            bool deadState = playerDic.Value;
            if (deadState == false)
            {
                if (m_arrowTowardToDeadPlayer.ContainsKey(player))
                    m_arrowTowardToDeadPlayer[player].SetActive(false);
                continue;
            }

            if (IsGameObjectInCamera(player.transform.position))
            {
                if(m_arrowTowardToDeadPlayer.ContainsKey(player))
                    m_arrowTowardToDeadPlayer[player].SetActive(false);
                continue;
            }
            GameObject arrow;

            if (m_arrowTowardToDeadPlayer.ContainsKey(player))
                arrow = m_arrowTowardToDeadPlayer[player];
            else
            {
                arrow = m_directionToDeadPlayer[usedArrowCount++];
                IPossesNickName possesNickName = player.GetComponent<IPossesNickName>();
                if (possesNickName != null)
                {
                    string nickName = possesNickName.GetNickName();
                    TMP_Text nickText = arrow.transform.GetChild(1).GetComponent<TMP_Text>();
                    nickText.text = nickName;
                }
                m_arrowTowardToDeadPlayer.Add(player, arrow);
            }
            RectTransform deadArrowImage = arrow.transform.GetChild(0).GetComponent<RectTransform>();
            Vector3 angleBorderPos = CalculateBordePos(transform.position, player.transform.position);
            arrow.transform.localPosition = angleBorderPos;
            float angle = CalculateAngleBetweenTarget(player.transform.position);
            deadArrowImage.eulerAngles = new Vector3(0, 0, angle);
            arrow.SetActive(true);
        }
    }
    private Vector3 CalculateBordePos(Vector2 _standardPos, Vector2 _targetPos)
    {
        float cameraYSize = m_mainCamera.orthographicSize;
        float cameraXSize = cameraYSize * ((float)Screen.width / (float)Screen.height);

        cameraXSize -= 1.5f;
        cameraYSize -= 1.5f;

        Vector2 targetVector = _targetPos - _standardPos;
        float targetAngle = Mathf.Atan2(targetVector.y, targetVector.x);
        Vector3 targetUIPos = Vector3.zero;

        if (Mathf.Abs(targetVector.x * (float)Screen.height / (float)Screen.width) > Mathf.Abs(targetVector.y))
        {
            float sign = Mathf.Sign(targetVector.x);
            targetUIPos.x = cameraXSize * sign;
            targetUIPos.y = Mathf.Tan(targetAngle) * targetUIPos.x;
        }
        else
        {
            float sign = Mathf.Sign(targetVector.y);
            targetUIPos.y = cameraYSize * sign;
            targetUIPos.x = targetUIPos.y  / Mathf.Tan(targetAngle);            
        }

        targetUIPos.z = -1;
        return targetUIPos;
    }
    private void FindNomadEngineCart()
    {
        m_nomadEngine = FindObjectOfType<NomadEngineCart>();
    }
    private void OnRopeConnect()
    {
        if (m_currentState != m_onNomad)
            SetState(m_connected);

        if (m_photonView.IsMine == true)
            PopUpIcon(InteractionType.keycodeE, true);

        if (m_currentState != m_onNomad)
            SetNomadCart();
    }
    private void OnRopeDisconnect()
    {
        SetState(m_idle);
        GetConnectedCart()?.SetSpriteOutline(false);

        m_currentConnectedCart = null;
      
        if (m_photonView.IsMine == true)
        {
            PopUpIcon(InteractionType.keycodeE, false);
            m_playerDisconnectEventChannelSO.RaiseEvent();
        }
    }

    private void OnRopeChange(Transform _prevCart)
    {
        if (_prevCart == null)
            return;

        NomadCartBase sub =  _prevCart.gameObject.GetComponent<NomadCartBase>();
        sub.SetSpriteOutline(false);
    }
    private void SetMovdDirection(float _x, float _y)
    {
        if (_x == 0 && _y == 0)
            return;
        m_direction.x = _x;
        m_direction.y = _y;
    }
    private void SetInvenInitialCapacity()
    {
        string _id = ConstStringStorage.UPGRADE_ID_INVEN_CAZELIN_CAPACITY;
        m_invenSO.SetInitialCapacity(m_upgradeInfoSO.GetUpgradeValue(_id, GetLevel(_id)), _id);

        _id = ConstStringStorage.UPGRADE_ID_INVEN_STARLIGHT_CAPACITY;
        m_invenSO.SetInitialCapacity(m_upgradeInfoSO.GetUpgradeValue(_id, GetLevel(_id)), _id);
       
    }
    private void NotifyPlayerDead()
    {
        m_photonView.RPC(nameof(RPC_NotifyPlayerDead), RpcTarget.AllBuffered);
    }
    private void NotifyPlayerAlive()
    {
        m_photonView.RPC(nameof(RPC_NotifyPlayerAlive), RpcTarget.AllBuffered);
    }
    private void RPC_TurnOnOffCol(bool _onoff)
    {
        m_boxCollider.enabled = _onoff;
        m_circleTriggerColider.enabled = _onoff;
    }
    private void SetOxygenStateUI(float _percent)
    {
        //위험 상황 주변 빨간색 이미지
        m_dangerOxygenTmpColor.a = Mathf.Max(1 - _percent * 3, 0f);
        m_dangerOxygenImage.color = m_dangerOxygenTmpColor;

        //산소 게이지
        m_oxygenFillImage.fillAmount = _percent;
        ChangeOxygenColorByDanger(_percent);

        SetActivePlayerDeadUI(CheckPlayerDeadState());
    }
    private void ChangeOxygenColorByDanger(float _currentOxygenPercent)
    {
        for (int i=0; i<m_oxygenChangeTiming.Length; i++)
        {
            if(_currentOxygenPercent * 100 <= m_oxygenChangeTiming[i])
            {
                m_oxygenFillImage.color = m_oxygenGageColor[i];
                return;
            }

        }
        m_oxygenFillImage.color = m_oxygenGageColor[m_oxygenGageColor.Length - 1];
    }
    public void SetActivePlayerDeadUI(bool _active)
    {
        if (_active == m_deadPanelGameObject.activeSelf)
            return;

        m_deadPanelGameObject.SetActive(_active);
    }
    private void SetMineralAmountGage(MineralType _mineralType, int _amount, int _maxAmount)
    {
        int mineralIndex = (int)_mineralType;
        StartCoroutine(IE_MineralUICoroutine(mineralIndex, _amount, _maxAmount));
    }

    private System.Collections.IEnumerator IE_MineralUICoroutine(int _mineralIndex, int _amount, int _maxAmount)
    {
        
        float value = float.Parse(m_mineralCountTexts[_mineralIndex].text);
        float diffvalue = _amount - value;

        if(diffvalue > 0)
        {
            while (value <= _amount)
            {
                value += diffvalue / 50;
                FillMineralUI(_mineralIndex, value, _maxAmount);
                yield return null;
            }
        }
        else if(diffvalue < 0)
        {
            diffvalue *= -1;

            while (value >= _amount)
            {
                FillMineralUI(_mineralIndex, value, _maxAmount);
                value -= diffvalue / 100;
                yield return null;
            }
        }
        else
            FillMineralUI(_mineralIndex, value, _maxAmount);

        m_mineralCountTexts[_mineralIndex].text = (_amount).ToString();

        yield break;
    }

    private void FillMineralUI(int _mineralIndex, float _value, int _maxAmount)
    {
        m_mineralCountTexts[_mineralIndex].text = ((int)_value).ToString();
        m_mineralFillImages[_mineralIndex].fillAmount = (float)_value / _maxAmount;
    }

    private void SetActiveStatePanel(bool _isActive)
    {
        m_statePanel.SetActive(_isActive);
    }
    private void OnDestroy()
    {
        m_invenSO.OnChangedMineral -= SetMineralAmountGage;
    }
    #region RPC
    [PunRPC]
    private void RPC_TakeControlOfNomad()
    {
        RPC_TurnOnOffCol(false);
        m_spriteRenderer.enabled = false;
        m_secondSpriteRenderer.enabled = false;
        GetCurrentTool().gameObject.SetActive(false);
        //if(m_photonView.IsMine == true)
        //m_playerDisconnectEventChannelSO.RaiseEvent();
    }
    [PunRPC]
    private void RPC_LoseControlOfNomad()
    {
        RPC_TurnOnOffCol(true);
        m_spriteRenderer.enabled = true;
        m_secondSpriteRenderer.enabled = true;
        GetCurrentTool().gameObject.SetActive(true);
    }
    [PunRPC]
    private void RPC_SetNickName(string _nickName)
    {
        //닉네임 UI
        m_nickNameString = _nickName;
        m_nickName.text = m_nickNameString;
    }
    [PunRPC]
    private void RPC_SetDead()
    {
        SetState(m_dead);
    }
    [PunRPC]
    private void RPC_SetConnected()
    {
        SetState(m_connected);
    }
    [PunRPC]
    private void RPC_NotifyPlayerDead()
    {
        Photon.Realtime.Player[] list = PhotonNetwork.PlayerList;
        for (int i = 0; i < list.Length; i++)
        {
            GameObject playerObj = list[i].TagObject as GameObject;
            if (playerObj.GetInstanceID() == this.gameObject.GetInstanceID())
                continue;

            Player player = playerObj.GetComponent<Player>();
            player.AddDeadPlayer(this.gameObject);
        }
    }
    [PunRPC]
    private void RPC_NotifyPlayerAlive()
    {
        Photon.Realtime.Player[] list = PhotonNetwork.PlayerList;
        for (int i = 0; i < list.Length; i++)
        {
            GameObject playerObj = list[i].TagObject as GameObject;
            if (playerObj.GetInstanceID() == this.gameObject.GetInstanceID())
                continue;

            Player player = playerObj.GetComponent<Player>();
            player.RemoveDeadPlayer(this.gameObject);
        }
    }
    [PunRPC]
    private void RPC_TurnOnOffBoxCol(bool _onoff)
    {
        m_boxCollider.enabled = _onoff;
    }
    #endregion
    #endregion

    #region PublicMethod

    #region Get
    public NomadEngineCart GetEngineCart()
    {
        return m_nomadEngine;
    }
    public PlayerInvenSO GetPlayerInven()
    {
        return m_invenSO;
    }
    public Vector2 GetForward()
    {
        return m_direction;
    }
    public NomadCartBase GetConnectedCart()
    {
        return m_nomadRope.GetCart();
    }
    public ToolBase GetCurrentTool()
    {
        return m_input.GetCurrentTool();
    }
    public string GetNickName()
    {
        return m_nickNameString;
    }
    #endregion

    public void CameraMove()
    {
        Camera.main.transform.position = new Vector3(transform.position.x, transform.position.y, -10f);
    }
    public void OnOffUpgradeButtons(bool _onoff)
    {
        m_onoffMineralUpgradeButtonChannelSO.RaiseEvent(_onoff);
        m_onoffToolUpgradeButtonChannelSO.RaiseEvent(_onoff);
    }
    public bool IsPhotonViewMine()
    {
        return m_photonView.IsMine;
    }
    public void NomadMove(float _x, float _y)
    {
        if (m_nomadEngine == null)
            return;
        m_nomadEngine.Move(_x, _y);
        transform.position = m_nomadEngine.GetPosition();
        m_nomadRope.CallRPCUpdateRopeSize();
    }
    public void RaiseDeadEvent()
    {
        NotifyPlayerDead();
        m_playerRope.CallRPCSetOwnerUsingPhotonViewID(m_photonView.ViewID);
        m_playerDeadEventChannelSO.RaiseEvent();
    }
    public void TurnOnOffBoxColRPC(bool _enable)
    {
        m_photonView.RPC(nameof(RPC_TurnOnOffBoxCol), RpcTarget.All, _enable);
    }
    public void RaiseAliveEvent()
    {
        NotifyPlayerAlive();
        m_playerRope.CallRPCRemoveOwner();
    }
    public int[] GetMineralCounts()
    {
        return m_balloonText.GetCumulativeAmounts();
    }
    public void SetNomadCart()
    {
        m_currentConnectedCart = m_nomadRope.GetCart();
    }
    public void SetPlayerDead()
    {
        m_photonView.RPC(nameof(RPC_SetDead), RpcTarget.AllBuffered);
    }
    public void SetPlayerConnected()
    {
        m_photonView.RPC(nameof(RPC_SetConnected), RpcTarget.AllBuffered);

    }
    public bool CheckPlayerDeadState()
    {
        return m_currentState == m_dead;
    }
    public void TakeControlOfNomad()
    {
        m_photonView.RPC(nameof(RPC_TakeControlOfNomad), RpcTarget.AllBuffered);
    }
    public void LoseControlOfNomad()
    {
        m_photonView.RPC(nameof(RPC_LoseControlOfNomad), RpcTarget.AllBuffered);
    }
    public void SetNickName(string _nickName)
    {
        m_photonView.RPC(nameof(RPC_SetNickName), RpcTarget.AllBuffered, _nickName);
    }

    /// IStateRecordable
    public bool IsDeadState()
    {
        return CheckPlayerDeadState();
    }
    public int[] GetMineralMiningCounts()
    {
        return GetMineralCounts();
    }

    public int GetLevel(string _id)
    {
        ToolBase currentTool = GetCurrentTool();
        string toolUpgradId = currentTool.GetUpgradeID();
        if (_id == toolUpgradId)
            return currentTool.GetLevel();

        if (_id == ConstStringStorage.UPGRADE_ID_INVEN_CAZELIN_CAPACITY || _id == ConstStringStorage.UPGRADE_ID_INVEN_STARLIGHT_CAPACITY)
            return m_invenSO.GetLevel(_id);

        Debug.LogError($"{_id}에 해당하는 업그레이드 요소가 없습니다");
        return -1;
    }
    public void SetUpgradedInfo(string _id)
    {
        ToolBase currentTool = GetCurrentTool();
        string toolUpgradId = currentTool.GetUpgradeID();

        if (_id == toolUpgradId)
        {
            //도구 업그레이드?
            currentTool.SetUpgradedInfo(m_upgradeInfoSO.GetUpgradeValue(_id, GetLevel(_id)+1));
            m_updateUpgradeButtonChannelSO.RaiseEvent(_id);
            return;
        }

        if (_id == ConstStringStorage.UPGRADE_ID_INVEN_CAZELIN_CAPACITY || _id == ConstStringStorage.UPGRADE_ID_INVEN_STARLIGHT_CAPACITY)
        {
            //인벤 업그레이드?
            m_invenSO.SetUpgradedInfo(m_upgradeInfoSO.GetUpgradeValue(_id, GetLevel(_id)+1), _id);
            m_updateUpgradeButtonChannelSO.RaiseEvent(_id);
            return;
        }

        Debug.LogError($"{_id}에 해당하는 업그레이드 요소가 없습니다");
        return;
    }
    public void UseToolAnimator()
    {
        m_animator.SetBool(ConstStringStorage.PLAYER_ANIM_USE_TOOL, true);
    }
    public void AddDeadPlayer(GameObject _deadPlayer)
    {
        if (m_otherPlayerDeadState.ContainsKey(_deadPlayer))
        {
            m_otherPlayerDeadState[_deadPlayer] = true;
            return;
        }
        m_otherPlayerDeadState.Add(_deadPlayer, true);
    }
    public void RemoveDeadPlayer(GameObject _alivePlayer)
    {
        m_otherPlayerDeadState[_alivePlayer] = false;
    }
    public bool IsConnectable()
    {
        return m_currentState != m_dead;
    }
    public void Sleep()
    {
        PhotonNetwork.NetworkingClient.EventReceived -= OnReceivedPutMineralResultEvent;

        m_nomadRope.CallRPCRemoveOwner();
        m_playerRope.CallRPCRemoveOwner();

        RPC_TurnOnOffCol(false);
        SetState(m_null);

        m_statePanel.SetActive(false);
    }
    public void AttachNomadRope()
    {
        m_nomadRope.CallRPCSetOwnerUsingPhotonViewID(m_photonView.ViewID);
    }

    public void StartGame()
    {
        SetState(m_idle);
        FindNomadEngineCart();
    }

    public void PlayToolSfx()
    {
        ToolBase currentTool = GetCurrentTool();
        if(currentTool.GetToolType().Equals(ToolBase.ToolType.Pickax))
        {
            ToolMine mine = currentTool as ToolMine;
            if(mine.HasTargetMineral())
                SoundManager.Instance.PlaySFX(SoundManager.SFX_PICKAXE, 0.15f);
        }
        else if(currentTool.GetToolType().Equals(ToolBase.ToolType.Extractor))
        {
            ToolMine mine = currentTool as ToolMine;
            if (mine.HasTargetMineral())
                SoundManager.Instance.PlaySFX(SoundManager.SFX_EXTRACTOR);
        }
    }
    #endregion
}
