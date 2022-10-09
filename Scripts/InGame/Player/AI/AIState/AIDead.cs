using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CustomStatePattern;

public class AIDead : StateBase<AIPlayer>
{
    private float m_space = 2;
    private Player m_followPlayer;
    private Rigidbody2D m_rigidBody;
    private Animator m_animator;

    public override void OnAwake(AIPlayer _stateMachine)
    {
        m_rigidBody = _stateMachine.GetComponent<Rigidbody2D>();
        m_animator = _stateMachine.GetComponent<Animator>();
    }

    public override void OnEnter(AIPlayer _stateMachine)
    {
        m_animator.SetBool(ConstStringStorage.PLAYER_ANIM_ISDEAD, true);
        _stateMachine.RaiseDeadEvent();
    }

    public override void OnExit(AIPlayer _stateMachine)
    {
        m_followPlayer = null;
        m_animator.SetBool(ConstStringStorage.PLAYER_ANIM_ISDEAD, false);
        _stateMachine.RaiseAliveEvent();
        _stateMachine.TurnOnOffBoxColRPC(true);
    }

    public override void Update(AIPlayer _stateMachine)
    {
        if (_stateMachine.GetOxygen() > 0)
            _stateMachine.SetState(_stateMachine.m_decideState);

        FollowPlayer(_stateMachine);

        if (m_followPlayer != null && m_followPlayer.CheckPlayerDeadState())
        {
            _stateMachine.TurnOnOffBoxColRPC(true);
            m_followPlayer = null;
        }
    }

    private void FollowPlayer(AIPlayer _stateMachine)
    {
        if (m_followPlayer == null)
            return;

        Vector2 curPos = _stateMachine.transform.position;
        Vector2 targetPos = m_followPlayer.transform.position;


        if (Vector2.SqrMagnitude(curPos - targetPos) > m_space * m_space)
            m_rigidBody.velocity = m_followPlayer.GetComponent<Rigidbody2D>().velocity;
    }

    private void SetFollowPlayer(Collider2D _collision, AIPlayer _stateMachine)
    {
        if (m_followPlayer != null)
            return;

        Player player = _collision.GetComponent<Player>();

        if (player == null)
            return;

        m_followPlayer = player;
        _stateMachine.TurnOnOffBoxColRPC(false);
    }

    public override void OnTriggerEnter2D(Collider2D _collsion, AIPlayer _stateMachine)
    {
        SetFollowPlayer(_collsion, _stateMachine);
    }
}
