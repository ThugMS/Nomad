using System.Collections.Generic;
using UnityEngine;
using CustomStatePattern;


public class AIMove : StateBase<AIPlayer>
{
    private Rigidbody2D m_rigidBody;
    private GameObject m_target;
    private Vector2 m_directionTowardTarget;

    private Animator m_animator;

    private float m_acceleration;

    private PathFinder m_findPath = new PathFinder();
    private List<Node> m_path;
    private Vector2 m_targetPos;
    private int m_index;

    public override void OnAwake(AIPlayer _stateMachine)
    {
        m_rigidBody = _stateMachine.GetComponent<Rigidbody2D>();
        m_animator = _stateMachine.GetComponent<Animator>();
        m_target = _stateMachine.GetTarget();
    }

    public override void OnEnter(AIPlayer _stateMachine)
    {
        m_acceleration = PlayerConstants.ACCELERATION;

        //자유 탐색
        if(m_target == null)
        {
            SetRandomSeed(_stateMachine);
            InitializeRandomDirection();
            return;
        }

        InitializePath(_stateMachine);
    }
    public override void OnExit(AIPlayer _stateMachine)
    {
        if (m_target == null)
            return;

        m_directionTowardTarget = m_target.transform.position - _stateMachine.transform.position;
        m_rigidBody.velocity = Vector2.zero;
        MoveAnimator(m_directionTowardTarget.x, m_directionTowardTarget.y);
    }
    public override void Update(AIPlayer _stateMachine)
    {
        SetPathDirection(_stateMachine);

        MoveAnimator(m_directionTowardTarget.x, m_directionTowardTarget.y);
        _stateMachine.transform.Translate(m_directionTowardTarget * m_acceleration * Time.deltaTime);


        if (_stateMachine.GetOxygen() < PlayerConstants.MIN_OXYGEN)
        {
            if (m_target == null)
            {
                _stateMachine.SetState(_stateMachine.m_decideState);
                return;
            }

            NomadEngineCart engineType = m_target.GetComponent<NomadEngineCart>();
            if (engineType != null)
                return;

            _stateMachine.SetState(_stateMachine.m_decideState);
        }
    }
    private void InitializePath(AIPlayer _stateMachine)
    {
        m_path = m_findPath.CalculatePath(_stateMachine.transform.position, m_target.transform.position);
        m_index = 0;

        if (m_path.Count > 0)
            m_targetPos = m_path[m_index].pos;
        else
            m_targetPos = m_target.transform.position;
    }
    private void MoveAnimator(float _x, float _y)
    {
        if (!_x.Equals(0f) || !_y.Equals(0f))
        {
            m_animator.SetFloat(ConstStringStorage.PLAYER_ANIM_MOVE_X, _x);
            m_animator.SetFloat(ConstStringStorage.PLAYER_ANIM_MOVE_Y, _y);
        }
    }
    private void SetPathDirection(AIPlayer _stateMachine)
    {
        if (m_target == null)
            return;
  
        Vector2 currentPos = _stateMachine.transform.position;
        m_directionTowardTarget = (m_targetPos - currentPos).normalized;

        if (Vector2.SqrMagnitude(currentPos - m_targetPos) > 0.1f)
            return;

        m_index++;

        if (m_path.Count <= m_index)
            InitializePath(_stateMachine);

        try
        {
            m_targetPos = m_path[m_index].pos;
        }
        catch(System.ArgumentOutOfRangeException _e)
        {
            Debug.Log($"index가 범위 밖 {m_index}, {_e}");
        }

    }
    private void SetRandomSeed(AIPlayer _stateMachine)
    {
        float time = System.DateTime.Now.Millisecond;
        float x = _stateMachine.transform.position.x;
        float y = _stateMachine.transform.position.y;
        float randomMultiplier = 1000.7f;
        Random.InitState((int)(time * x * y * randomMultiplier));
    }
    private void InitializeRandomDirection()
    {
        m_directionTowardTarget.x = UnityEngine.Random.Range(-1f, 1f);
        m_directionTowardTarget.y = UnityEngine.Random.Range(-1f, 1f);
        m_directionTowardTarget = m_directionTowardTarget.normalized;
    }

    public override void OnCollistionEnter2D(Collision2D _collision, AIPlayer _stateMachine)
    {
        MineralBase mineral = _collision.transform.GetComponent<MineralBase>();
  
        if (m_target == null)
        {
            if (mineral == null)
            {
                InitializeRandomDirection();
                return;
            }

            _stateMachine.SetTarget(mineral.gameObject);
            _stateMachine.SetState(_stateMachine.m_decideState);
            return;
        }
    }
    public override void OnTriggerStay2D(Collider2D _collsion, AIPlayer _stateMachine)
    {
        //타겟을 찾았으면 다시 결정 단계로
        if (m_target == null)
            return;

        if (m_target.GetHashCode() == _collsion.gameObject.GetHashCode())
        {
            _stateMachine.SetState(_stateMachine.m_decideState);
            return;
        }

        if(_collsion.GetType().Equals(m_target.GetType()))
        {
            _stateMachine.SetTarget(_collsion.gameObject);
            _stateMachine.SetState(_stateMachine.m_decideState);
            return;
        }

        InitializePath(_stateMachine);

    }
}