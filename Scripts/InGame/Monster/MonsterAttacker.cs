using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterAttacker : MonsterBase
{
    public override void InitialOnAwake()
    {
        m_monsterType = MonsterType.Attacker;
        InitialSpec(MonsterConstants.COMMON_HP, MonsterConstants.COMMON_DAMAGE, MonsterConstants.COMMON_ATTACK_COOLTIME, MonsterConstants.COMMON_ATTACK_RANGE,
            MonsterConstants.COMMON_MOVE_SPEED);
    }
}
