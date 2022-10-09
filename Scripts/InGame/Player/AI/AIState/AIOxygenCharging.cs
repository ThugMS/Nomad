using System.Collections;
using UnityEngine;
using CustomStatePattern;


public class AIOxygenCharging : StateBase<AIPlayer>
{
    private float m_currentOxygen;

    public override void OnAwake(AIPlayer _stateMachine)
    {
        
    }

    public override void OnEnter(AIPlayer _stateMachine)
    {
        
    }

    public override void OnExit(AIPlayer _stateMachine)
    {
        _stateMachine.SetTarget(null);
    }

    public override void Update(AIPlayer _stateMachine)
    {
        m_currentOxygen = _stateMachine.GetOxygen();

        if (m_currentOxygen < PlayerConstants.SUFFICIENT_OXYGEN)
            return;

        _stateMachine.SetState(_stateMachine.m_decideState);
    }
}