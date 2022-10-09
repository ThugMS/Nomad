using System.Collections.Generic;
using UnityEngine;
using CustomStatePattern;


public class AIMineralFull : StateBase<AIPlayer>
{
    NomadCartManager m_nomadCartManager;
    List<NomadMineralCart> m_allMineralCarts = new List<NomadMineralCart>();

    public override void OnAwake(AIPlayer _stateMachine)
    {
        if(m_nomadCartManager == null)  
            m_nomadCartManager = GameObject.FindObjectOfType<NomadCartManager>();

        int length = m_nomadCartManager.GetNomadSize();
        
        for (int i = 0; i < length; i++)
        {
            NomadCartBase cart = m_nomadCartManager.GetCart(i);
            if (cart is not NomadMineralCart)
                continue;

            NomadMineralCart mineralCart = cart as NomadMineralCart;
            m_allMineralCarts.Add(mineralCart);
        }
    }

    public override void OnEnter(AIPlayer _stateMachine)
    {
        SetRandomCartTarget(_stateMachine);
    }

    public override void OnExit(AIPlayer _stateMachine)
    {

    }

    public override void Update(AIPlayer _stateMachine)
    {

    }

    private void SetRandomCartTarget(AIPlayer _stateMachine)
    {
        int rand = Random.Range(0, m_allMineralCarts.Count);
        _stateMachine.SetTarget(m_allMineralCarts[rand].gameObject);
        _stateMachine.SetState(_stateMachine.m_moveState);
    }
}