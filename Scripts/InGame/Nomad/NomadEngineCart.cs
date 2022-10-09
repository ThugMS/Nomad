using System.Collections;
using UnityEngine;
using Photon.Pun;
using TMPro;

public class NomadEngineCart : NomadCartBase
{
    #region PrivateVariables
    private float m_turnSpeed;
    private IAllCartUsable m_allCartUsable;
    private bool m_isCooldown = false;
    private IEnumerator m_CazelinCooldownCoroutine;
    private int m_cazelinUsageWeight;
    private bool m_isSendSpendMineralRequest = false;
    private bool m_canMove = false;
    private bool m_canRide = true;
    private int m_spendObjectId = 0;

    private System.Action m_getOnAction;
    private System.Action m_getOffAction;

    [SerializeField] private int m_currentPlayerId;

    [SerializeField] private TMP_Text m_infoText;

    private Color m_tmpColor;
    private MineralRequestResultEventChannelSO m_spendMineralResultChannel;
    [SerializeField] private CartEventChannelSO m_openEngineCartChannelSO;
    #endregion

    #region Protected Variables

    #endregion

    #region PublicVariables

    public delegate void EngineCartHandler(int reason);
    public event EngineCartHandler OnDie;

    #endregion

    #region PrivateMethod
    [PunRPC]
    private void RPC_SetCurrentPlayerID(int _id)
    {
        m_currentPlayerId = _id;
    }
    private IEnumerator StartCazelinCooltime()
    {
        m_isCooldown = true;
        m_allCartUsable.PlayCazelinParticle();

        yield return CoroutineHelper.WaitForSeconds(1f);

        m_isCooldown = false;
    }
    private IEnumerator ShowInfoText()
    {
        float alpha = 1;
        SetTextAlpha(alpha);

        yield return CoroutineHelper.WaitForSeconds(0.5f);

        while (alpha > 0)
        {
            alpha -= 0.1f;
            SetTextAlpha(alpha);
            yield return CoroutineHelper.WaitForSeconds(0.1f);
        }
    }
    private void CheckFrontCollide()
    {
        RaycastHit2D hit = Physics2D.Raycast(GetPosition(), transform.up, GetSpriteRenderer().bounds.size.y/2, LayerMask.GetMask("Mineral"));

        if (hit)
            OnDie?.Invoke(0);
    }
    private void SetTextAlpha(float _a)
    {
        m_tmpColor.a = _a;
        m_infoText.color = m_tmpColor;
    }
    private void TurnCartToTarget(Vector3 _currentDir, Vector3 _targetDir)
    {
        Vector3 referenceRight = Vector3.Cross(Vector3.forward, _currentDir);//외적
        float angle = Vector3.Angle(_targetDir, _currentDir);
        float sign = Mathf.Sign(Vector3.Dot(_targetDir, referenceRight));
        float finalAngle = sign * angle;

        transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, transform.eulerAngles.z + finalAngle);

    }
    public void SendSpendCazeline( int _cost)
    {
        if (m_isSendSpendMineralRequest == true)
            return;
        m_isSendSpendMineralRequest = true;
        m_cartMineralSO.SendRaiseEventChangeMineral(PhotonNetwork.LocalPlayer.ActorNumber, m_spendObjectId, MineralType.Cazelin, -_cost);
    }
    private void OnReceivedSpendResultEvent(int _actorNum, int _viewId, bool _isSuccess)
    {
        if (_actorNum != PhotonNetwork.LocalPlayer.ActorNumber || _viewId != m_spendObjectId)
            return;

        if (_isSuccess == true)
        {
            m_canMove = true;
            SetTextAlpha(0);
        }
        else
        {
            m_canMove = false;
            if (m_infoText.color.a <= 0)
                StartCoroutine(ShowInfoText());
        }

        m_isSendSpendMineralRequest = false;
    }
    [PunRPC]
    private void RPC_GetOn(int _actorNum, int _viewID)
    {
        if (m_currentPlayerId != _viewID)
            return;

        //탑승하면 제어권 가짐
        m_getOnAction?.Invoke();
        m_allCartUsable.TransferOwnerShip(_actorNum);
        m_openEngineCartChannelSO.RaiseEvent(this);
    }

    [PunRPC]
    private void RPC_GetOff(int _actorNum, int _viewID)
    {
        if (m_currentPlayerId != _viewID)
            return;

        m_openEngineCartChannelSO.RaiseEvent(this);
        m_getOffAction?.Invoke();
    }

    [PunRPC]
    private void RPC_MasterDecideGetOn(int _actorNum, int _viewID)
    {
        if (m_currentPlayerId > 0)
        {
            //하차
            if(m_currentPlayerId == _viewID)
            {
                m_currentPlayerId = -1;
                m_photonView.RPC(nameof(RPC_GetOff), RpcTarget.Others, _actorNum, _viewID);
                return;
            }

            m_photonView.RPC(nameof(RPC_SetCanGetOn), RpcTarget.AllBuffered, false);
            return;
        }
        m_photonView.RPC(nameof(RPC_SetCanGetOn), RpcTarget.AllBuffered, true);
        m_photonView.RPC(nameof(RPC_GetOn), RpcTarget.Others, _actorNum, _viewID);
        m_currentPlayerId = _viewID;
    }

    [PunRPC]
    private void RPC_SetCanGetOn(bool _canRide)
    {
        m_canRide = _canRide;
    }

    #endregion

    #region ProtectedMethod
    protected override void Init()
    {
        m_isCooldown = false;
        m_currentPlayerId = -1;
        m_tmpColor = m_infoText.color;
        SetTextAlpha(0);

        SetCartName(NomadConstants.CARTNAME_ENGINECART);

        m_spendMineralResultChannel = Resources.Load<MineralRequestResultEventChannelSO>(ConstStringStorage.SPENDMINERAL_PATH);
        m_spendMineralResultChannel.AddEventRaise(OnReceivedSpendResultEvent);
    }

    protected override void StopFuntion()
    {
        //죽음
        OnDie?.Invoke(1);
    }
    protected override void OnPostSetParent()
    {
        //엔진칸은 사용안함 
        //이유 : parent가 없음
    }
    #endregion

    #region PublicMethod

    public override void OnInteracted(GameObject _player)
    {
        PhotonView pv = _player.GetComponent<PhotonView>();

        if (PhotonNetwork.IsMasterClient == false)
        {
            m_currentPlayerId = pv.ViewID;
            m_photonView.RPC(nameof(RPC_MasterDecideGetOn), RpcTarget.MasterClient, pv.OwnerActorNr, pv.ViewID);
            return;
        }

        if (m_currentPlayerId > 0)
        {
            //하차
            if (m_currentPlayerId == pv.ViewID)
            {
                RPC_GetOff(pv.OwnerActorNr, pv.ViewID);
                m_currentPlayerId = -1;
                return;
            }
            m_photonView.RPC(nameof(RPC_SetCanGetOn), RpcTarget.AllBuffered, false);
        }
        else
        {
            m_photonView.RPC(nameof(RPC_SetCanGetOn), RpcTarget.AllBuffered, true);
            m_currentPlayerId = pv.ViewID;
            RPC_GetOn(pv.OwnerActorNr, pv.ViewID);
        }
    }
    public void AddGetOnAction(System.Action _action)
    {
        m_getOnAction += _action;
    }
    public void AddGetOffAction(System.Action _action)
    {
        m_getOffAction += _action;
    }
    public void RemoveAllGetOnAction()
    {
        m_getOnAction = null;
    }

    public void RemoveAllGetOffAction()
    {
        m_getOffAction = null;
    }

    /*
     * 엔진 칸  이동 메서드
     * 플레이어가 함선 탑승 상태일 때, 호출하여 사용할 수 있게
     */
    public void Move(float _h, float _v)
    {
        if (m_currentPlayerId == -1)
            Debug.LogError("탑승중인 플레이어가 없는데 엔진 이동");
        
        Vector3 dir = new Vector3(_h, _v, 0);

        if (dir == Vector3.zero)
            return;

        if(m_isCooldown == false)
            SendSpendCazeline(GetCazelinWeight());

        if(m_canMove == false)
            return;

        if (m_isCooldown == false)
        {
            m_CazelinCooldownCoroutine = StartCazelinCooltime();
            StartCoroutine(m_CazelinCooldownCoroutine);
        }

        Vector3 currentDir = transform.up.normalized;//기준 벡터
        Vector3 targetDir = Vector3.Lerp(currentDir, dir, Time.deltaTime * m_turnSpeed);//타겟 벡터(보간 적용)

        TurnCartToTarget(currentDir, targetDir);

        dir = transform.up;
        transform.Translate(dir * m_speed * Time.deltaTime, Space.World);

        CheckFrontCollide();
    }

    public void SetCazelinUseWeight(int _value)
    {
        m_cazelinUsageWeight = _value * NomadConstants.ENGINE_CAZELIN_COEFFICIENT;
    }
    public void SetEngineCartConfig(IAllCartUsable _manager, float _turnSpeed)
    {
        m_allCartUsable = _manager;
        m_turnSpeed = _turnSpeed;
    }
    public int GetCazelinWeight()
    {
        return m_cazelinUsageWeight;
    }
    #endregion
}
