using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class MineralBasePrefab : MonoBehaviour
{
    public GameObject mapResource;
    private void Awake()
    {
        mapResource = Instantiate(mapResource, Vector3.zero, Quaternion.identity);
    }

}
