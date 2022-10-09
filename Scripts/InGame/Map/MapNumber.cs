using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapNumber : MonoBehaviour
{
    #region PrivateVariable
    [SerializeField] private int m_mapArrayY = 0;
    [SerializeField] private int m_mapArrayX = 0;
    #endregion;

    #region PublicVariable
    public List<GameObject> m_mineralList = new List<GameObject>();
    #endregion

    #region PublicMethod
    public int GetMapArrayX()
    {
        return m_mapArrayX;
    }

    public int GetMapArrayY()
    {
        return m_mapArrayY;
    }

    public void SetMapArrayX(int _x)
    {
        m_mapArrayX = _x;
    }

    public void SetMapArrayY(int _y)
    {
        m_mapArrayY = _y;
    }
    #endregion
}
