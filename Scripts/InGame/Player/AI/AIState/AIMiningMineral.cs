using System.Collections;
using UnityEngine;
using CustomStatePattern;

public class AIMiningMineral : StateBase<AIPlayer>
{
    private MineralBase m_targetMineral;
    private ToolBase m_currentTool;
    private Animator m_animator;
    private float m_workTime;
    private float m_time;
   
    public override void OnAwake(AIPlayer _stateMachine)
    {
        m_currentTool = _stateMachine.GetCurrentTool();
        m_targetMineral = _stateMachine.GetTarget().GetComponent<MineralBase>();
        m_animator = _stateMachine.GetComponent<Animator>();
    }

    public override void OnEnter(AIPlayer _stateMachine)
    {
        m_time = 0f;
        ChooseTool(_stateMachine);
    }

    public override void OnExit(AIPlayer _stateMachine)
    {
        _stateMachine.SetTarget(null);
    }

    public override void Update(AIPlayer _stateMachine)
    {

        if (_stateMachine.GetTarget() == null)
            return;
        if (!UseTool())
            return;
        float amount = MineMineral(_stateMachine);
        AddMineral(_stateMachine, (int)amount);
        if(_stateMachine.IsAtLeastOneMineralFull())
        {
            _stateMachine.SetState(_stateMachine.m_decideState);
            return;
        }
        FindMineral(_stateMachine);
    }

    private bool UseTool()
    {
        m_time += Time.deltaTime;
        m_animator.SetBool(ConstStringStorage.PLAYER_ANIM_USE_TOOL, true);
        float fillAmount = m_time / m_workTime;
        fillAmount = 1 - fillAmount;
        if (fillAmount > 0)
        {
            m_targetMineral.DisplayStep(fillAmount);
            return false;
        }
        m_animator.SetBool(ConstStringStorage.PLAYER_ANIM_USE_TOOL, false);
        return true;
    }

    private void ChooseTool(AIPlayer _stateMachine)
    {
        if (m_targetMineral is MineralCazelin)
        {
            _stateMachine.SetCurrentTool(EAction.Extractor);
            m_workTime = ToolConstants.TOOL_EXTRACTOR_WORKTIME;
            return;
        }

        _stateMachine.SetCurrentTool(EAction.Pick);
        m_workTime = ToolConstants.TOOL_PICKAX_WORKTIME;
    }

    private float MineMineral(AIPlayer _stateMachine)
    {
        m_currentTool = _stateMachine.GetCurrentTool();
        _stateMachine.SetTarget(null);
        return m_targetMineral.AIMine(m_currentTool.GetCapability());
    }

    private void AddMineral(AIPlayer _stateMachine, int _amount)
    {
        if (m_targetMineral is MineralStarlight)
            _stateMachine.ChangeCurrentStarLight(_amount);
        else if (m_targetMineral is MineralCazelin)
            _stateMachine.ChangeCurrentCazelin(_amount);
        
    }

    public void FindMineral(AIPlayer _stateMachine)
    {
        m_targetMineral = null;

        Transform trans = _stateMachine.transform;
        RaycastHit2D hit = Physics2D.CircleCast((Vector2)trans.position, 5f,  Vector2.zero, 0f, 1 << 10);
        if(hit.collider != null)
        {
            MineralBase mineral = hit.collider.GetComponent<MineralBase>();
            _stateMachine.SetTarget(mineral.gameObject);
        }

        _stateMachine.SetState(_stateMachine.m_decideState);
    }
}