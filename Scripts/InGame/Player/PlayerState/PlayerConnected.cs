using System.Collections;
using UnityEngine;
using CustomStatePattern;
using Photon.Pun;

public class PlayerConnected : StateBase<Player>
{
    private PlayerInput m_playerInput;
    private PlayerController m_playerController;
    private PlayerProperty m_playerProperty;
    private Player m_player;
    private PhotonView m_photonView;

    public override void OnAwake(Player _stateMachine)
    {
        m_playerInput = _stateMachine.GetComponent<PlayerInput>();
        m_playerController = _stateMachine.GetComponent<PlayerController>();
        m_playerProperty = _stateMachine.GetComponent<PlayerProperty>();
        m_photonView = _stateMachine.GetComponent<PhotonView>();
        m_player = _stateMachine;
    }

    public override void OnEnter(Player _stateMachine)
    {
        if (!_stateMachine.IsPhotonViewMine())
            return;

        _stateMachine.OnOffUpgradeButtons(true);

        m_playerProperty.ChangeOxygenSign(PlayerConstants.POSITIVE);

        m_playerInput.AddPlayerIdleInput();
        m_playerController.AddMoveInput();

        InputManager.Instance.AddKeyDownAction(EAction.NomadInteraction, IntreractWithNomad);      
    }

    public override void OnExit(Player _stateMachine)
    {
        if (!_stateMachine.IsPhotonViewMine())
            return;

        m_playerInput.RemovePlayerIdleInput();
        m_playerController.RemoveMoveInput();

        InputManager.Instance.RemoveKeyDownAction(EAction.NomadInteraction, IntreractWithNomad);
    }

    public override void Update(Player _stateMachine)
    {
        if (!_stateMachine.IsPhotonViewMine())
            return;
    }

    public void IntreractWithNomad()
    {
        NomadCartBase cart = m_player.GetConnectedCart();
       
        if (cart is NomadEngineCart)
        {
            NomadEngineCart engineCart = cart as NomadEngineCart;
            engineCart.RemoveAllGetOnAction();
            engineCart.AddGetOnAction(() => m_player.TakeControlOfNomad());
            engineCart.AddGetOnAction(() => m_player.SetState(m_player.m_onNomad));
        }
        cart.OnInteracted(m_player.gameObject);
    }
}