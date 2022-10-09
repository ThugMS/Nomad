using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;

public interface IRopeConnectable
{
    public bool IsConnectable();
}

public class PlayerRopePhysics : MonoBehaviourPun, IPunObservable
{
    private enum ConnectState
    {
        None, UnConnected, Connected, Carrying
    }

    #region PrivateVariables
    [SerializeField] private LineRenderer m_lineRenderer;
    [SerializeField] private ConnectState m_connectState = ConnectState.None;
    [SerializeField] private int m_segmentCount;
    [SerializeField] private int m_constraintLoop;
    [SerializeField] private float m_segmentLength;
    [SerializeField] private float m_lineWidth;
    [SerializeField] private float m_length;
    [SerializeField] private float m_expandLength;
    [SerializeField] private float m_targetDistance = 0;
    [SerializeField] private Vector2 m_tension = Vector2.zero;//장력

    [SerializeField] [Space(10f)] private Transform m_targetTransform = null;
    private Transform m_transformMine = null;
    private Rigidbody2D m_rigid;
    private PhotonView m_photonView;

    [SerializeField] private LayerMask m_targetLayer;

    private List<Segment> m_segments = new List<Segment>();
    private IEnumerator m_connectCoroutine;

    private int m_minSegmentCount = 2;
    #endregion

    #region Protected Variables
    #endregion

    #region PublicVariables

    public Action OnConnected;
    public Action OnDisconnected;
    public Action<Transform> OnDisconnectedPrevTarget;
    #endregion

    private void Awake()
    {
        m_photonView = GetComponent<PhotonView>();
        m_lineRenderer = GetComponent<LineRenderer>();
    }
    private void Update()
    {
        if(m_photonView.IsMine == true)
        {
            if (m_connectState == ConnectState.UnConnected || m_connectState == ConnectState.Connected)
            {
                Transform trans = FindTarget();
                if (trans != null)
                {
                    OnDisconnectedPrevTarget?.Invoke(m_targetTransform);
                    CallRPCConnectRope(trans);
                }
            }
        }

        switch (m_connectState)
        {
            case ConnectState.None:
            case ConnectState.UnConnected:
                DisconnectRope();
                break;
            case ConnectState.Connected:
                if (m_rigid.velocity.magnitude < PlayerConstants.MIN_VELOCITY_SPEED)
                    CallRPCChangeState(ConnectState.Carrying);
                UpdateRope();

                break;
            case ConnectState.Carrying:
                if (m_rigid.velocity.magnitude > PlayerConstants.MIN_VELOCITY_SPEED)
                    CallRPCChangeState(ConnectState.Connected);
                UpdateRope();

                break;
            default:
                Debug.LogError(m_connectState.ToString() + "  Update() : 적절하지 않은 state 에러 ");
                break;
        }

    }

    #region PrivateMethod

    [PunRPC]
    private void RPC_ChangeState(int _connectState)
    {
        ConnectState connectState = (ConnectState)_connectState;

        switch (connectState)
        {
            case ConnectState.None:
                m_transformMine = null;
                break;

            case ConnectState.UnConnected:
                OnDisconnected?.Invoke();
                m_targetDistance = 9999;
                m_targetTransform = null;
                break;
            case ConnectState.Connected:
                OnConnected?.Invoke();
                break;
            case ConnectState.Carrying:
                break;
            default:
                Debug.LogError(connectState.ToString() + " RPC_ChangeState() : 적절하지 않은 state 에러 ");
                break;

        }
        m_connectState = connectState;
    }
    private void UpdateDistance()
    {
        m_targetDistance = (m_targetTransform.position - m_transformMine.position).magnitude;
    }
    private bool IsFar()
    {
        float maxLength = (m_connectState == ConnectState.Carrying) ? m_length + m_expandLength : m_length;
        return m_targetDistance > maxLength;
    }
    private void UpdateRope()
    {
        if (ConnectState.Connected == m_connectState)
            AdjustSegmentsSize();

        UpdateSegments();

        for (int i = 0; i < m_constraintLoop; i++)
        {
            ApplyConstraint();
            AdjustMyPosition();
        }

        DrawRope();

        UpdateDistance();

        if (IsFar())
            CallRPCChangeState(ConnectState.UnConnected);
    }
    private void DrawRope()
    {
        m_lineRenderer.startWidth = m_lineWidth;
        m_lineRenderer.endWidth = m_lineWidth;

        Vector3[] segmentPosition = new Vector3[m_segments.Count];
        for (int i = 0; i < m_segments.Count; i++)
            segmentPosition[i] = m_segments[i].position;

        m_lineRenderer.positionCount = m_segments.Count;
        m_lineRenderer.SetPositions(segmentPosition);
    }
    private void AdjustSegmentsSize()
    {
        int count = (int)(Vector3.Distance(m_targetTransform.position, m_transformMine.position) / m_segmentLength) + m_minSegmentCount;


        int segCnt = m_segments.Count;
        if (segCnt > count)
        {
            for (int i = 0; i < segCnt - count; i++)
                m_segments.RemoveAt(m_segments.Count - 1);
        }
        else
        {
            for (int i = 0; i < count - segCnt; i++)
                m_segments.Add(new Segment(m_segments[m_segments.Count - 1].position));
        }

    }
    private void UpdateSegments()
    {

        for (int i = 0; i < m_segments.Count; i++)
        {

            m_segments[i].velocity = m_segments[i].position - m_segments[i].previousPos;

            m_segments[i].previousPos = m_segments[i].position;

            if (m_connectState == ConnectState.Carrying && i != 0)
                m_segments[i].position += (m_segments[0].velocity) * m_tension.y * Time.fixedDeltaTime * Time.fixedDeltaTime;
            

            //Vector2 vel = (segments[i].velocity + segments[i].previousVelocity) / 2.009f; //보간 버전
            Vector2 vel = m_segments[i].velocity;//
            m_segments[i].position += vel * 1f;  //+ 속도
            m_segments[i].previousVelocity = vel;

        }
    }
    private void ApplyConstraint()
    {
        m_segments[0].position = m_targetTransform.position;
        if (m_connectState == ConnectState.Connected)
            m_segments[m_segments.Count - 1].position = m_transformMine.position;

        for (int i = 0; i < m_segments.Count - 1; i++)
        {
            float distance = (m_segments[i].position - m_segments[i + 1].position).magnitude;//두점의 거리
            float difference = distance - m_segmentLength;
            Vector2 dir = (m_segments[i].position - m_segments[i + 1].position).normalized;//어느 방향으로 이동할지

            Vector2 movement = dir * difference;

            if (i == 0)
                m_segments[i + 1].position += movement;
            else if (m_connectState == ConnectState.Connected && i == m_segments.Count - 2)
                m_segments[i].position -= movement;
            else
            {
                m_segments[i].position -= movement * 0.5f;
                m_segments[i + 1].position += movement * 0.5f;
            }
        }
    }
    private void AdjustMyPosition()
    {
        if (m_connectState == ConnectState.Connected)
            return;

        m_rigid.MovePosition(m_segments[m_segments.Count - 1].position);
        m_segments[m_segments.Count - 1].position = m_rigid.position;
        m_rigid.velocity = m_segments[m_segments.Count - 1].velocity;

    }
    private void ClearRopeRanderer()
    {
        m_lineRenderer.positionCount = 0;
    }
    private Transform FindTarget()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(m_transformMine.position, m_length - 0.7f, m_targetLayer);

        Transform newTarget = null;
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].transform == m_targetTransform) 
                continue;

            IRopeConnectable connectable = hits[i].GetComponent<IRopeConnectable>();
            Transform hitTrans = hits[i].transform;

            if (connectable == null)
                continue;

            if (!connectable.IsConnectable())
                continue;

            if (m_targetDistance <= ((Vector2)hitTrans.position - (Vector2)m_transformMine.position).magnitude)
                continue;

            newTarget = hitTrans;
        }

        return newTarget;
    }
    [PunRPC]
    private void RPC_StartToConnect(int _viewID)
    {
        m_targetTransform = PhotonView.Find(_viewID).transform;

        Vector2 segmentPos = m_targetTransform.position;
        Vector2 dir = (m_transformMine.position - m_targetTransform.position).normalized;
        for (int i = 0; i < m_segmentCount; i++)
        {
            m_segments.Add(new Segment(segmentPos));
            segmentPos += m_segmentLength * dir;
        }
        CallRPCChangeState(ConnectState.Connected);
    }
    private void CallRPCChangeState(ConnectState _connectStateint)
    {
        if (m_photonView.IsMine == true)
            m_photonView.RPC(nameof(RPC_ChangeState), RpcTarget.AllBuffered, (int)_connectStateint);
    }
    private void DisconnectRope()
    {
        m_segments.Clear();
        m_lineRenderer.positionCount = 0;

        m_targetTransform = null;

    }
    private void CallRPCConnectRope(Transform _transform)
    {
        if (m_photonView.IsMine == true)
            m_photonView.RPC(nameof(RPC_StartToConnect), RpcTarget.AllBuffered , _transform.GetComponent<PhotonView>().ViewID);

    }
    [PunRPC]
    private void RPC_SetOwnerUsingPhotonViewID(int _viewID)
    {
        m_transformMine = PhotonView.Find(_viewID).transform;
        m_rigid = m_transformMine.GetComponent<Rigidbody2D>();

        transform.SetParent(m_transformMine);
        transform.localPosition = Vector2.zero;
        CallRPCChangeState(ConnectState.UnConnected);
    }
    [PunRPC]
    private void RPC_RemoveMyTransformPhotonViewID()
    {
        m_transformMine = null;
        m_rigid = null;
        transform.SetParent(m_transformMine);
        CallRPCChangeState(ConnectState.None);
    }
    [PunRPC]
    private void RPC_MakeShortRope()
    {
        while(m_segments.Count > m_minSegmentCount)
            m_segments.RemoveAt(m_segments.Count - 1);
    }
    #endregion

    #region PublicMethod
    public NomadCartBase GetCart()
    {
        if (m_connectState == ConnectState.Connected || m_connectState == ConnectState.Carrying )
            return m_targetTransform?.GetComponent<NomadCartBase>();
       
        return null;
    }
    public void CallRPCSetOwnerUsingPhotonViewID(int _photonViewID)
    {
        if (m_photonView.IsMine == true)
            m_photonView.RPC(nameof(RPC_SetOwnerUsingPhotonViewID), RpcTarget.AllBuffered, _photonViewID);
    }
    public void CallRPCRemoveOwner()
    {
        if (m_photonView.IsMine == true)
            m_photonView.RPC(nameof(RPC_RemoveMyTransformPhotonViewID), RpcTarget.AllBuffered);
    }
    public void CallRPCUpdateRopeSize()
    {
        if (m_photonView.IsMine == false)
            return;
        if(m_segments.Count > m_minSegmentCount)
            m_photonView.RPC(nameof(RPC_MakeShortRope), RpcTarget.AllBuffered);
    }
    #endregion

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {

    }
    public class Segment
    {
        public Vector2 previousPos;
        public Vector2 position;
        public Vector2 velocity;

        public Vector2 previousVelocity;

        public Segment(Vector2 _position)
        {
            previousPos = _position;
            position = _position;
            velocity = Vector2.zero;
        }
    }

}
