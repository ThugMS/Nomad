using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToolMine : ToolBase
{
    private MineralBase m_targetMineral;
    private float fillAmount = 1;

    public override void Init()
    {
        if (m_toolType == ToolType.Pickax)
        {
            SetUpgradeID(ConstStringStorage.UPGRADE_ID_PICK_MINING);

            m_workTime = ToolConstants.TOOL_PICKAX_WORKTIME;
            m_range = ToolConstants.TOOL_PICKAX_RANGE;
            m_isCompleteWork = false;
            return;
        }
        
        if(m_toolType == ToolType.Extractor)
        {
            SetUpgradeID(ConstStringStorage.UPGRADE_ID_EXTRACTOR_MINING);

            m_workTime = ToolConstants.TOOL_EXTRACTOR_WORKTIME;
            m_range = ToolConstants.TOOL_EXTRACTOR_RANGE;

            m_isCompleteWork = false;
        }
    }

    private void OnDisable()
    {
        ResetWorkTime();
        ResetMineralSize();
    }

    #region PrivateMethod
    private MineralBase RayToMineral(Player _player)
    {
        RaycastHit2D rayHit = Physics2D.Raycast(transform.position, _player.GetForward().normalized, m_range, m_mineralMask);

        if (rayHit.collider == null)
            return null;

        if(m_toolType == ToolBase.ToolType.Pickax)
        {
            MineralStarlight starlight = rayHit.collider.GetComponent<MineralStarlight>();
            if (starlight != null)
                return starlight;

            MineralStone stone = rayHit.collider.GetComponent<MineralStone>();
            if (stone != null)
                return stone;
        }
        
        if(m_toolType == ToolType.Extractor)
        {
            MineralCazelin cazelin = rayHit.collider.GetComponent<MineralCazelin>();
            if (cazelin != null)
                return cazelin;
        }

        return null;
    }

    private bool CheckEqualMineral(MineralBase _targetMineral)
    {
        if (_targetMineral == m_targetMineral)
            return true;

        return false;

    }

    private bool CanMine()
    {
        if (m_isCompleteWork == false)
            return false;

        return true;
    }

    private void MineMineral()
    {
        float amount = m_targetMineral.GainMineral() * m_capability;
        MineralType mineral = m_targetMineral.GetMineralType();

        if(mineral == MineralType.Starlight || mineral == MineralType.Cazelin)
        {
            float ableGain = m_playerInvenSO.AddMineral(mineral, (int)amount);
            m_balloonText.ShowBalloon(mineral, (int)ableGain);
        }
        
        m_targetMineral.FinishMine();
        m_targetMineral = null;
        ResetWorkTime();
    }

    private void ResetMineralSize()
    {
        if (m_targetMineral == null)
            return;
        if (m_targetMineral.gameObject.activeSelf == false)
            return;

            m_targetMineral.ResetSize();
    }

    private void ResizeMineral()
    {
        fillAmount = m_currentWorkTime / m_workTime;
        fillAmount = 1 - fillAmount;
        if (fillAmount > 0)
            m_targetMineral.DisplayStep(fillAmount);

    }
    #endregion

    #region PublicMethod
    public override void Function(Player _player)
    {
        _player.UseToolAnimator();
        MineralBase targetMineral = RayToMineral(_player);
        if (targetMineral == null)
        {
           
            ResetMineralSize();
            m_targetMineral = null;
            ResetWorkTime();
            return;
        }

        if (!CheckEqualMineral(targetMineral))
        {
            ResetMineralSize();
            m_targetMineral = targetMineral;
            ResetWorkTime();
        }

        UpdateWorkTime();
        ResizeMineral(); 
        if (CanMine())
            MineMineral();
    }

    public override void CancleTool()
    {
        ResetWorkTime();
        ResetMineralSize();
    }

    public bool HasTargetMineral()
    {
        return m_targetMineral != null;
    }
        #endregion


}
