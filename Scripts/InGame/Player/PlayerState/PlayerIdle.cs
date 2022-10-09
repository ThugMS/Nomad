using System.Collections;
using UnityEngine;
using CustomStatePattern;

public class PlayerIdle : StateBase<Player>
{
    PlayerInput m_playerInput;
    PlayerController m_playerController;
    PlayerProperty m_playerProperty;

    public override void OnAwake(Player _stateMachine)
    {
        m_playerInput = _stateMachine.GetComponent<PlayerInput>();
        m_playerController = _stateMachine.GetComponent<PlayerController>();
        m_playerProperty = _stateMachine.GetComponent<PlayerProperty>();
    }

    public override void OnEnter(Player _stateMachine)
    {
        if (!_stateMachine.IsPhotonViewMine())
            return;

        m_playerProperty.ChangeOxygenSign(PlayerConstants.NEGATIVE);
        m_playerInput.AddPlayerIdleInput();
        m_playerController.AddMoveInput();

        _stateMachine.OnOffUpgradeButtons(false);
    }

    public override void OnExit(Player _stateMachine)
    {
        if(m_playerInput != null)
            m_playerInput.RemovePlayerIdleInput();

        if (m_playerInput != null)
            m_playerController.RemoveMoveInput();
    }

    public override void Update(Player _stateMachine)
    {
        if (!_stateMachine.IsPhotonViewMine())
            return;

        if (m_playerProperty.GetOxygen() <= 0)
            _stateMachine.SetPlayerDead();
    }
}