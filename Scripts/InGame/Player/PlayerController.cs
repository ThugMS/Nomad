using UnityEngine;
using Photon.Pun;

public class PlayerController : MonoBehaviour, IPunObservable
{
    #region PrivateVariable
    private float m_acceleration;
    private float m_networkAcceleration;
    private float m_maxVelocity;
    private float m_lag;
    private float m_currentTime;
    private float m_maxDistance = 10f;
    private Rigidbody2D m_playerRigid;
    private Animator m_animator;
    private PhotonView m_photonView;

    private Vector2 m_remoteTransformPos;
    private Vector2 m_remoteVelocity;
    private Vector2 m_targetTransformPos;
    private Vector2 m_offset;
    #endregion

    private void Awake()
    {
        m_playerRigid = GetComponent<Rigidbody2D>();
        m_animator = GetComponent<Animator>();
        m_photonView = GetComponent<PhotonView>();

        m_acceleration = PlayerConstants.ACCELERATION;
        m_networkAcceleration = m_acceleration * 2f;
        m_maxVelocity = PlayerConstants.MAX_VELOCITY_SPEED;

        PhotonNetwork.SerializationRate = 10;
    }

    private void FixedUpdate()
    {
        if (m_photonView.IsMine == true)
            return;
        m_playerRigid.velocity = m_remoteVelocity;
        transform.position = Vector2.Lerp(transform.position, m_targetTransformPos, Time.fixedDeltaTime * m_networkAcceleration);
        m_currentTime += Time.fixedDeltaTime;

        if (Vector2.SqrMagnitude((Vector2)transform.position - m_targetTransformPos) > m_maxDistance * m_maxDistance)
            transform.position = m_targetTransformPos;

        if (m_currentTime > m_lag)
        {
            m_currentTime = 0;
            m_targetTransformPos += m_offset;
        }
    }

    #region PrivateMethod
    private void Move(float _x, float _y)
    {
        if (m_photonView.IsMine == false)
            return;

        MoveAnimator(_x, _y);

        Vector2 direction = new Vector2(_x, _y).normalized;


        m_playerRigid.velocity += direction * m_acceleration * Time.deltaTime;
        float theta = Mathf.Acos(Vector2.Dot(direction, m_playerRigid.velocity.normalized)) * Mathf.Rad2Deg;

        if (theta < 0.01f && m_playerRigid.velocity.magnitude > m_maxVelocity)
            m_playerRigid.velocity = direction * m_maxVelocity;

        //밧줄 때문에 밧줄에서 벗어나게 하는 것
        if (direction != Vector2.zero)
        {
            if (m_playerRigid.velocity.magnitude < PlayerConstants.MIN_VELOCITY_SPEED)
                m_playerRigid.velocity = direction * (PlayerConstants.MIN_VELOCITY_SPEED + 0.01f);
        }


    }
    private void MoveAnimator(float _x, float _y)
    {
        if (!_x.Equals(0f) || !_y.Equals(0f))
        {
            SetAnimatorFloatValue(ConstStringStorage.PLAYER_ANIM_MOVE_X, _x);
            SetAnimatorFloatValue(ConstStringStorage.PLAYER_ANIM_MOVE_Y, _y);
        }
    }
    #endregion
    #region PublicMethod

    public void SetAnimatorFloatValue(string _parameter, float _value)
    {
        if (m_photonView.IsMine == false)
            return;
        m_animator.SetFloat(_parameter, _value);
    }
    public void SetAnimatorBoolValue(string _parameter, bool _value)
    {
        if (m_photonView.IsMine == false)
            return;
        m_animator.SetBool(_parameter, _value);
    }
    public void SetAnimatorIntValue(string _parameter, int _value)
    {
        if (m_photonView.IsMine == false)
            return;
        m_animator.SetInteger(_parameter, _value);
    }
    public void AddMoveInput()
    {
        InputManager.Instance.AddMoveAction(Move);
    }
    public void RemoveMoveInput()
    {
        InputManager.Instance.RemoveMoveAction(Move);
    }
    #endregion

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext((Vector2)transform.position);
            stream.SendNext((Vector2)m_playerRigid.velocity);
        }
        else
        {
            m_remoteTransformPos = (Vector2)stream.ReceiveNext();
            m_remoteVelocity = (Vector2)stream.ReceiveNext();

            m_currentTime = 0f;
            m_lag = Mathf.Abs((float)(PhotonNetwork.Time - info.SentServerTime));
            m_offset = m_lag * m_remoteVelocity;
            m_targetTransformPos = m_remoteTransformPos + m_offset;
        }
    }
}
