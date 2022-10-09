using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CustomIngameButton : Button
{
    //버튼Num이 -1부터 음수로지정됨.
    private static int m_totalButtonNum = 0;
    protected int m_buttonNum = 0;
    protected override void Awake()
    {
        m_buttonNum = --m_totalButtonNum;
        base.Awake();
        onClick.AddListener(PlayClickSound);
    }
    private void PlayClickSound()
    {
        SoundManager.Instance.PlaySFX(SoundManager.SFX_SYSTEM_CLICK);
    }
}
