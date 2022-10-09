using System.Collections;
using UnityEngine;
using CustomStatePattern;


public class AIWaitMineralCarEmpty : StateBase<AIPlayer>
{
    public override void OnAwake(AIPlayer _stateMachine)
    {
        
    }

    public override void OnEnter(AIPlayer _stateMachine)
    {
        
    }

    public override void OnExit(AIPlayer _stateMachine)
    {
        
    }

    public override void Update(AIPlayer _stateMachine)
    {
        if (_stateMachine.IsAtLeastOneMineralFull())
            return;
        _stateMachine.SetState(_stateMachine.m_decideState);
    }
}