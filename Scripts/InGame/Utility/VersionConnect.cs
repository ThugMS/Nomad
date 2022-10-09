using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VersionConnect : MonoBehaviour
{
    private TMP_Text m_versionText;

    private void Awake()
    {
        m_versionText = GetComponent<TMP_Text>();
    }
    private void Start()
    {
        m_versionText.text = "v " + Application.version;
    }
}
