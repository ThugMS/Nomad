using System.Collections;
using UnityEngine;

namespace CustomStatePattern
{
    public abstract class StateMachineBase<T> : MonoBehaviour where T : StateMachineBase<T>
    {
        protected StateBase<T> m_currentState;
        protected StateBase<T> m_nullState;

        private void Awake()
        {
            SetStatesTransition();
            SetInitialState();
            SetNullState();
            SetState(m_nullState);
            BaseAwake();
        }

        private void Start()
        {
            BaseStart();
            InitializeStateFunction();
        }

        private void Update()
        {
            if (m_currentState == m_nullState)
                return;

            m_currentState.Update(this as T);
            BaseUpdate();
        }

        protected abstract void BaseUpdate();
        protected abstract void BaseAwake();
        protected abstract void BaseStart();
        protected abstract void SetInitialState();
        protected abstract void SetNullState();
        protected abstract void SetStatesTransition();
        protected void OnCollisionEnter2D(Collision2D collision)
        {
            m_currentState.OnCollistionEnter2D(collision, this as T);
        }
        protected void OnTriggerEnter2D(Collider2D collision)
        {
            m_currentState.OnTriggerEnter2D(collision, this as T);
        }
        protected void OnTriggerExit2D(Collider2D collision)
        {
            m_currentState.OnTriggerExit2D(collision, this as T);
        }
        protected void OnTriggerStay2D(Collider2D collision)
        {
            m_currentState.OnTriggerStay2D(collision, this as T);
        }
        private void InitializeStateFunction()
        {
            m_currentState.OnAwake(this as T);
            m_currentState.OnEnter(this as T);
        }
        public void SetState(StateBase<T> _state)
        {
            if (_state == m_nullState)
            {
                m_currentState = m_nullState;
                return;
            }
            if (!m_currentState.CheckTransitionPossible(_state))
                return;

            m_currentState.OnExit(this as T);
            m_currentState = _state;
            m_currentState.OnAwake(this as T);
            m_currentState.OnEnter(this as T);
        }

    }
}