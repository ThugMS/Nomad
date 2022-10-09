using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingUIFront : MonoBehaviour
{
    [SerializeField] private GameObject m_loadingUIPanel;
    [SerializeField] private Image progressBar;

    private void Awake()
    {
        m_loadingUIPanel.SetActive(true);
        progressBar.fillAmount = 0;
    }

    public void HideLoadingUI()
    {
        m_loadingUIPanel.SetActive(false);
    }

    private IEnumerator UpdateProgressBar(float _time, float _percent)
    {
        float  prevAmount = progressBar.fillAmount;

        float timer = 0f;

        while(timer < _time)
        {
            yield return null;
            timer += Time.unscaledDeltaTime;
            progressBar.fillAmount = Mathf.Lerp(prevAmount, prevAmount + _percent, timer/ _time);
        }
    }
    public void StartLoading(float _loadBaseTime, float _percent)
    {
        StartCoroutine(UpdateProgressBar(_loadBaseTime, _percent));
    }

}
