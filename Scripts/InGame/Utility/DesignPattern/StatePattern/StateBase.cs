using System.Collections.Generic;
using UnityEngine;

namespace CustomStatePattern
{
    public abstract class StateBase<T> where T : StateMachineBase<T>
    {
        protected List<StateBase<T>> m_transition = new List<StateBase<T>>();

        public abstract void OnAwake(T _stateMachine);
        public abstract void OnEnter(T _stateMachine);
        public abstract void Update(T _stateMachine);
        public abstract void OnExit(T _stateMachine);

        public virtual void OnCollistionEnter2D(Collision2D _collision, T _stateMachine)
        {

        }
        public virtual void OnTriggerEnter2D(Collider2D _collsion, T _stateMachine)
        {

        }

        public virtual void OnTriggerExit2D(Collider2D _collsion, T _stateMachine)
        {

        }

        public virtual void OnTriggerStay2D(Collider2D _collsion, T _stateMachine)
        {

        }

        public void SetStateTransition(params StateBase<T>[] _states)
        {
            m_transition.Clear();

            for(int i = 0; i < _states.Length; i++)
            {
                m_transition.Add(_states[i]);
            }
        }

        public bool CheckTransitionPossible(StateBase<T> _targetState)
        {
            return m_transition.Contains(_targetState);
        }
    }
}