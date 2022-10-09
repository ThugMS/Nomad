using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CustomDelButton : Button
{
    protected override void Awake()
    {
        base.Awake();
        onClick.AddListener(PlayClickSound);
    }
    private void PlayClickSound()
    {
        SoundManager.Instance.PlaySFX(SoundManager.SFX_SYSTEM_DEL);
    }
}
