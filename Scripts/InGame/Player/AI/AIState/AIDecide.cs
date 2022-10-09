using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CustomStatePattern;

//무얼 할지 결정
public class AIDecide : StateBase<AIPlayer>
{
    private float m_currentOxygen;
    private GameObject m_target;
    public override void OnAwake(AIPlayer _stateMachine)
    {
        m_target = _stateMachine.GetTarget();
    }

    public override void OnEnter(AIPlayer _stateMachine)
    {
        
    }

    public override void OnExit(AIPlayer _stateMachine)
    {
        
    }

    public override void Update(AIPlayer _stateMachine)
    {

        if (OxygenCondition(_stateMachine))
            return;
        if (BehaveAccordingToTarget(_stateMachine))
            return;
        if (MineralCondition(_stateMachine))
            return;

        _stateMachine.SetState(_stateMachine.m_moveState);

    }

    private bool OxygenCondition(AIPlayer _stateMachine)
    {
        m_currentOxygen = _stateMachine.GetOxygen();

        if (m_currentOxygen < PlayerConstants.MIN_OXYGEN)
        {
            _stateMachine.SetState(_stateMachine.m_oxygenDiffecientState);
            return true;
        }

        return false;
    }
    private bool MineralCondition(AIPlayer _stateMachine)
    {
        if (_stateMachine.IsAtLeastOneMineralFull())
        {
            _stateMachine.SetState(_stateMachine.m_mineralFullState);
            return true;
        }

        return false;
    }
    private bool BehaveAccordingToTarget(AIPlayer _stateMachine)
    {
        if (m_target == null)
            return false;

        NomadMineralCart mineralCart = m_target.GetComponent<NomadMineralCart>();
        NomadEngineCart engineCart = m_target.GetComponent<NomadEngineCart>();
        MineralBase mineral = m_target.GetComponent<MineralBase>();

        if (engineCart != null)
        {
            _stateMachine.SetState(_stateMachine.m_oxygenChargingState);
            return true;
        }

        if (_stateMachine.GetOxygen() < PlayerConstants.MIN_OXYGEN)
            return false;

        if (mineralCart != null)
        {
            _stateMachine.SetState(_stateMachine.m_putMineralState);
            return true;
        }

        if (mineral != null)
        {
            _stateMachine.SetState(_stateMachine.m_miningMIneralState);
            return true;
        }

        return false;
    }
}
