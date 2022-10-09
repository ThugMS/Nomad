using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MineralStarlight : MineralBase
{
    public override void IndividualStart()
    {
        m_mineralType = MineralType.Starlight;
    }

    #region PublicMethod
    public override void Mine(Player _player, float _toolCapacity)
    {
        int mineValue = (int)(m_mineValue * _toolCapacity);
        _player.GetPlayerInven().AddMineral(m_mineralType, mineValue);
       
        FinishMine();
    }
    #endregion
}
