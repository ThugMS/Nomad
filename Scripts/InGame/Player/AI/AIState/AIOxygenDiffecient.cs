using System.Collections;
using UnityEngine;
using CustomStatePattern;

public class AIOxygenDiffecient : StateBase<AIPlayer>
{
    private NomadEngineCart m_engineCart;

    public override void OnAwake(AIPlayer _stateMachine)
    {
        m_engineCart = GameObject.FindObjectOfType<NomadEngineCart>();
        _stateMachine.SetTarget(m_engineCart.gameObject);
    }

    public override void OnEnter(AIPlayer _stateMachine)
    {
        _stateMachine.SetState(_stateMachine.m_moveState);
    }

    public override void OnExit(AIPlayer _stateMachine)
    {

    }

    public override void Update(AIPlayer _stateMachine)
    {
        
    }
}

