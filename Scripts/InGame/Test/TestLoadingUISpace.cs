using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingUISpace : MonoBehaviour
{

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            LoadingUI.Instance.LoadScene("InGame");
    }
}
