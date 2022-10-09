using System.Collections;
using UnityEngine;
using CustomStatePattern;

public class PlayerDead : StateBase<Player>
{
    private Player m_followPlayer;      //죽었을때 따라가는 플레이어
    private PlayerController m_playerController;
    private PlayerProperty m_playerProperty;
    private Rigidbody2D m_rigidBody;

    private float m_space = 2;

    public override void OnAwake(Player _stateMachine)
    {
        m_rigidBody = _stateMachine.GetComponent<Rigidbody2D>();
        m_playerController = _stateMachine.GetComponent<PlayerController>();
        m_playerProperty = _stateMachine.GetComponent<PlayerProperty>();
    }

    public override void OnEnter(Player _stateMachine)
    {
        if (!_stateMachine.IsPhotonViewMine())
            return;
        m_followPlayer = null;
        m_playerProperty.ChangeOxygenSign(PlayerConstants.ZERO);
        m_playerController.SetAnimatorBoolValue(ConstStringStorage.PLAYER_ANIM_ISDEAD, true);
        _stateMachine.RaiseDeadEvent();
    }

    public override void OnExit(Player _stateMachine)
    {
        if (!_stateMachine.IsPhotonViewMine())
            return;

        m_followPlayer = null;
        m_playerController.SetAnimatorBoolValue(ConstStringStorage.PLAYER_ANIM_ISDEAD, false);
        _stateMachine.RaiseAliveEvent();
        _stateMachine.TurnOnOffBoxColRPC(true);
    }

    public override void Update(Player _stateMachine)
    {
        if (!_stateMachine.IsPhotonViewMine())
            return;

        if (m_playerProperty.GetOxygen() > 0)
            _stateMachine.SetPlayerConnected();

        if (m_followPlayer != null && m_followPlayer.CheckPlayerDeadState())
        {
            _stateMachine.TurnOnOffBoxColRPC(true);
            m_followPlayer = null;
        }

        FollowPlayer(_stateMachine);
    }

    private void FollowPlayer(Player _stateMachine)
    {
        if (m_followPlayer == null)
            return;

        Vector2 curPos = _stateMachine.transform.position;
        Vector2 targetPos = m_followPlayer.transform.position;

       
        if (Vector2.SqrMagnitude(curPos - targetPos) > m_space * m_space)
            m_rigidBody.velocity = m_followPlayer.GetComponent<Rigidbody2D>().velocity;
    }

    private void SetFollowPlayer(Collider2D _collision, Player _stateMachine)
    {

        if (m_followPlayer != null)
            return;

        Player player = _collision.GetComponent<Player>();

        if (player == null)
            return;

        if (player.CheckPlayerDeadState())
            return;

        m_followPlayer = player;
        _stateMachine.TurnOnOffBoxColRPC(false);
    }

    public override void OnTriggerEnter2D(Collider2D collision, Player _stateMachine)
    {
        SetFollowPlayer(collision, _stateMachine);
    }
}