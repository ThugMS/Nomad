//#region PrivateVariables
//[SerializeField] private MonsterBase m_targetMonster;
//private List<MonsterBase> m_inRangedMonsters = new List<MonsterBase>();
//[SerializeField] private CircleCollider2D m_circlecollider2D;
//[SerializeField] private PhotonView m_photonView;
//private LineRenderer m_lineRenderer;
//private Animator m_animator;
//private int m_bulletSize = 10;
//#endregion

//private void Awake()
//{
//    m_upgradeID = ConstStringStorage.UPGRADE_ID_GUN_ATTACK;

//    m_photonView = GetComponent<PhotonView>();
//    m_lineRenderer = GetComponent<LineRenderer>();
//    m_animator = GetComponent<Animator>();

//    if (m_photonView.IsMine == false)
//        m_circlecollider2D.enabled = false;
//}

//private void Start()

//    m_capability = ToolConstants.TOOL_GUN_CAPABILITY;
//    m_coolTime = ToolConstants.TOOL_GUN_COOLTIME;
//    m_range = ToolConstants.TOOL_GUN_RANGE;
//    m_currentCoolTime = 0f;
//    m_circlecollider2D.radius = m_range;
//    m_isReadyToUse = false;
//}

//private void Update()
//{
//    if (m_photonView.IsMine == true)
//    {
//        UpdateCoolTime();
//        SelectTarget();

//        if (IsReadyToAttack())
//            AttackMonster();
//    }

//}

//private void OnEnable()
//{
//    InputManager.Instance.AddMoveAction(SetGunAnim);
//}

//private void OnDisable()
//{
//    ResetCoolTime();
//    InputManager.Instance.RemoveMoveAction(SetGunAnim);
//}


//#region PrivateMethod

//private void SelectTarget()
//{
//    int max_cnt = m_inRangedMonsters.Count;
//    if (max_cnt == 0)
//    {
//        m_targetMonster = null;
//        return;
//    }

//    float originalDistance = (m_inRangedMonsters[0].transform.position - transform.position).sqrMagnitude;
//    m_targetMonster = m_inRangedMonsters[0];

//    for (int i = 1; i < max_cnt; i++)
//    {

//        float newDistance = (m_inRangedMonsters[i].transform.position - transform.position).sqrMagnitude; //몬스터 까지의 거리

//        if (newDistance < originalDistance)
//        {
//            originalDistance = newDistance;
//            m_targetMonster = m_inRangedMonsters[i];
//        }
//    }
//}

//private bool IsReadyToAttack()
//{
//    if (m_isReadyToUse == false)
//        return false;

//    if (m_targetMonster == null)
//        return false;

//    return true;
//}

//private void AttackMonster()
//{
//    m_photonView.RPC(nameof(RPC_ShowGunBullet), RpcTarget.AllBuffered, m_targetMonster.m_photonView.ViewID);


//    ResetCoolTime();
//}

//private void MonsterHpDown()
//{
//    m_targetMonster.HP -= m_capability;

//    if (m_targetMonster.IsDead())
//        m_targetMonster = null;
//}

//private Vector3[] MakeBulletPathFunction(Vector3 _targetPos)
//{
//    float playerX = transform.position.x;
//    float playerY = transform.position.y;
//    float targetX = _targetPos.x;
//    float targetY = _targetPos.y;

//    float gradient = (targetY - playerY) / (targetX - playerX);

//    float distance = Vector3.Distance(transform.position, _targetPos);
//    int bulletMoveCount = (int)(distance * 10);


//    Vector3[] bulletPosArray = new Vector3[bulletMoveCount + 1];

//    for (int i = 0; i <= bulletMoveCount; i++)
//    {
//        Vector3 bulletPos = Vector3.zero;

//        bulletPos.x = playerX < targetX ? playerX + ((Mathf.Abs(playerX - targetX) / bulletMoveCount) * i) : playerX - ((Mathf.Abs(playerX - targetX) / bulletMoveCount) * i);
//        bulletPos.y = gradient * (bulletPos.x - playerX) + playerY;

//        bulletPosArray[i] = bulletPos;
//    }
//    return bulletPosArray;
//}
//private void SetGunAnim(float _x, float _y)
//{
//    if (m_photonView.IsMine == false)
//        return;

//    if (_x == 0 && _y == 0)
//        return;

//    m_animator.SetFloat(ConstStringStorage.PLAYER_ANIM_MOVE_X, _x);
//    m_animator.SetFloat(ConstStringStorage.PLAYER_ANIM_MOVE_Y, _y);
//}

//[PunRPC]
//private void RPC_ShowGunBullet(int _targetViewID)
//{
//    //애니메이션 적용
//    m_animator.SetTrigger(ConstStringStorage.GUN_ANIM_FIRE);

//    MonsterBase targetmonster = PhotonView.Find(_targetViewID).GetComponent<MonsterBase>();
//    Vector3 targetMonsterPosition = targetmonster.transform.position;

//    m_lineRenderer.positionCount = 2;
//    m_lineRenderer.startWidth = 0.05f;
//    m_lineRenderer.endWidth = 0.05f;

//    Vector3[] bulletPosArray = MakeBulletPathFunction(targetMonsterPosition);

//    if (bulletPosArray.Length < m_bulletSize)
//    {
//        MonsterHpDown();
//        return;
//    }


//    m_lineRenderer.SetPosition(0, bulletPosArray[0]);
//    m_lineRenderer.SetPosition(1, bulletPosArray[m_bulletSize]);

//    m_lineRenderer.enabled = true;
//    StartCoroutine(DrawBullet(bulletPosArray, targetmonster));
//}

//IEnumerator DrawBullet(Vector3[] _bulletPosArray, MonsterBase targetmonster)
//{
//    int i = 1;
//    while (i < _bulletPosArray.Length - m_bulletSize)
//    {
//        yield return null;
//        if (targetmonster.IsDead())
//        {
//            m_lineRenderer.positionCount = 0;
//            yield break;
//        }

//        m_lineRenderer.SetPosition(0, _bulletPosArray[i]);
//        m_lineRenderer.SetPosition(1, _bulletPosArray[i + m_bulletSize]);
//        i++;
//    }
//    MonsterHpDown();
//    m_lineRenderer.enabled = false;
//}
//private void OnTriggerEnter2D(Collider2D collision)
//{
//    if (collision.GetComponent<MonsterBase>() != null)
//        m_inRangedMonsters.Add(collision.GetComponent<MonsterBase>());

//}

//private void OnTriggerExit2D(Collider2D collision)
//{
//    if (collision.GetComponent<MonsterBase>() != null)
//        m_inRangedMonsters.Remove(collision.GetComponent<MonsterBase>());

//}
//#endregion