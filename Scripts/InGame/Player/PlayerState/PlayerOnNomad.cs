using System.Collections;
using UnityEngine;
using CustomStatePattern;

public class PlayerOnNomad : StateBase<Player>
{
    private Player m_player;
    private PlayerProperty m_playerProperty;
    private PlayerController m_playerController;
    private Rigidbody2D m_playerRigid;
    private ToolBase m_currentTool;
    private int m_previousToolIndex;

    public override void OnAwake(Player _stateMachine)
    {
        m_playerRigid = _stateMachine.GetComponent<Rigidbody2D>();
        m_playerProperty = _stateMachine.GetComponent<PlayerProperty>();
        m_playerController = _stateMachine.GetComponent<PlayerController>();
        m_player = _stateMachine;
        m_currentTool = _stateMachine.GetCurrentTool();
        _stateMachine.SetNomadCart();
    }

    public override void OnEnter(Player _stateMachine)
    {
        _stateMachine.OnOffUpgradeButtons(false);
        m_playerRigid.velocity = Vector2.zero;

        m_previousToolIndex = (int)m_currentTool.GetToolType();
        m_playerController.SetAnimatorBoolValue(ConstStringStorage.PLAYER_ANIM_USE_TOOL, false);
        //m_playerController.SetAnimatorIntValue(ConstStringStorage.PLAYER_ANIM_CHANGE_TOOL_TYPE, -1);

        InputManager.Instance.AddKeyDownAction(EAction.NomadInteraction, IntreractWithNomad);
        InputManager.Instance.AddMoveAction(_stateMachine.NomadMove);
    }

    public override void OnExit(Player _stateMachine)
    {
        m_playerController.SetAnimatorIntValue(ConstStringStorage.PLAYER_ANIM_CHANGE_TOOL_TYPE, m_previousToolIndex);

        InputManager.Instance.RemoveKeyDownAction(EAction.NomadInteraction, IntreractWithNomad);
        InputManager.Instance.RemoveMoveAction(_stateMachine.NomadMove);
    }

    public override void Update(Player _stateMachine)
    {
        if (!_stateMachine.IsPhotonViewMine())
            return;
        m_playerRigid.velocity = Vector2.zero;
        m_playerProperty.ChangeOxygenSign(PlayerConstants.POSITIVE);
    }

    public void IntreractWithNomad()
    {
        NomadEngineCart engineCart = m_player.GetEngineCart();
        engineCart.RemoveAllGetOffAction();
        engineCart.AddGetOffAction(() => m_player.LoseControlOfNomad());
        engineCart.AddGetOffAction(() => m_player.SetState(m_player.m_connected));
        engineCart.OnInteracted(m_player.gameObject);
    }
}