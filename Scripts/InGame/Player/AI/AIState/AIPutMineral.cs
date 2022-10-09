using System.Collections;
using UnityEngine;
using CustomStatePattern;
using ExitGames.Client.Photon;
using Photon.Pun;
using System;
public class AIPutMineral : StateBase<AIPlayer>
{
    private NomadMineralCart m_target;
    private int m_photonViewId = 0;
    private Action<int> m_changeCazelinAction;
    private Action<int> m_changeStarlightAction;

    private bool m_isSendPutMineralEvent = false;
    private MineralInfo m_expectedRemained = new MineralInfo(MineralType.Cazelin, 0);
    public override void OnAwake(AIPlayer _stateMachine)
    {
        m_photonViewId = _stateMachine.GetPhotonViewId();
        m_changeCazelinAction = _stateMachine.ChangeCurrentCazelin;
        m_changeStarlightAction = _stateMachine.ChangeCurrentStarLight;
        PhotonNetwork.NetworkingClient.EventReceived += OnReceivedPutMineralResultEvent;
        m_target = _stateMachine.GetTarget().GetComponent<NomadMineralCart>();
    }

    public override void OnEnter(AIPlayer _stateMachine)
    {
        if (m_target.GetCartType() == NomadCartBase.Type.StarlightCart && m_isSendPutMineralEvent == false)
        {
            m_isSendPutMineralEvent = true;
            int current = _stateMachine.UseAllCurrentStarLight();
            m_expectedRemained.mineralType = MineralType.Starlight;
            m_expectedRemained.amount = m_target.PutMineral(m_photonViewId, MineralType.Starlight, current);
        }
        else if(m_target.GetCartType() == NomadCartBase.Type.CazelinCart && m_isSendPutMineralEvent == false)
        {
            m_isSendPutMineralEvent = true;
            int current = _stateMachine.UseAllCurrentCazelin();
            m_expectedRemained.mineralType = MineralType.Cazelin;
            m_expectedRemained.amount = m_target.PutMineral(m_photonViewId, MineralType.Cazelin, current);
        }
        _stateMachine.SetState(_stateMachine.m_decideState);
    }

    public override void OnExit(AIPlayer _stateMachine)
    {
        m_photonViewId = 0;
        m_changeCazelinAction = null;
        m_changeStarlightAction = null;
        PhotonNetwork.NetworkingClient.EventReceived -= OnReceivedPutMineralResultEvent;
        _stateMachine.SetTarget(null);
    }

    public override void Update(AIPlayer _stateMachine)
    {
        if(m_isSendPutMineralEvent == false)
            _stateMachine.SetState(_stateMachine.m_decideState);
    }
    private void OnReceivedPutMineralResultEvent(EventData photonEvent)
    {
        if (PhotonNetwork.IsMasterClient == false)
            return;

        int code = photonEvent.Code;

        if (code != 20)
            return;

        object[] data = (object[])photonEvent.CustomData;
        if ((int)data[1] == m_photonViewId)
        {
            if ((bool)data[2] == true)
            {
                m_isSendPutMineralEvent = false;
                if (m_expectedRemained.mineralType == MineralType.Cazelin)
                    m_changeCazelinAction.Invoke(m_expectedRemained.amount);
                else
                    m_changeStarlightAction.Invoke(m_expectedRemained.amount);

                m_expectedRemained.amount = 0;
                m_isSendPutMineralEvent = false;
            }
        }
    }
}