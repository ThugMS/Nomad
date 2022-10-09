using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolRepair : ToolBase
{

    public override void Init()
    {
        SetUpgradeID(ConstStringStorage.UPGRADE_ID_REPAIR_EFFICIENCY);
        m_coolTime = ToolConstants.TOOL_REPAIR_COOLTIME;
    }

    private void OnDisable()
    {
        ResetCurrentCoolTime();
        SoundManager.Instance.StopLoopSFX();
    }

    #region PrivateMethod
    private bool CanRepair(NomadCartBase _brokenCart)
    {
        if (_brokenCart == null)
            return false;

        if (!_brokenCart.IsAlive())
            return false;

        return true;
    }

    private void Repair(NomadCartBase _brokenCart)
    {
        SoundManager.Instance.PlayLoopSFX(SoundManager.SFX_REPAIR, 0.6f);
        _brokenCart.CallRPCRecoverHp(m_capability);
        ResetCurrentCoolTime();
    }
    #endregion

    #region PublicMethod
    public override void Function(Player _player)
    {
        NomadCartBase brokenCart = _player.GetConnectedCart();
        _player.UseToolAnimator();
        if (CanRepair(brokenCart))
            Repair(brokenCart);
        else
            SoundManager.Instance.StopLoopSFX();
    }
    #endregion
}
