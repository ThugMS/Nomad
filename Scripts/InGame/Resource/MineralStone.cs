using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MineralStone : MineralBase
{
    public override void IndividualStart()
    {
        m_mineralType = MineralType.Stone;
    }
}
