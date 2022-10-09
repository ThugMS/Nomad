using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool<T> : MonoBehaviour where T : MonoBehaviour
{
    #region ProtectedVariables
    [SerializeField] protected int m_count;
    [SerializeField] protected GameObject m_prefab;
    protected Queue<T> m_objQueue = new Queue<T>();
    #endregion

    protected virtual void Awake()
    {
        Initialize();
    }


    #region PrivateMethod
    private void Initialize()
    {
        for (int i = 0; i < m_count; i++)
            m_objQueue.Enqueue(CreateObj());
    }

    private T CreateObj()
    {
        var newObj = Instantiate(m_prefab.GetComponent<T>(), transform);
        newObj.transform.localPosition = Vector3.zero;
        newObj.gameObject.SetActive(false);
        return newObj;
    }
    #endregion

    #region PublicRegion
    public T GetObj()
    {
        if (m_objQueue.Count > 0)
        {
            var obj = m_objQueue.Dequeue();
            obj.transform.SetParent(null);
            obj.gameObject.SetActive(true);
            return obj;
        }
        else
            return CreateObj();
    }

    public void ReturnObj(T _obj)
    {
        _obj.gameObject.SetActive(false);
        _obj.gameObject.transform.SetParent(transform);
        _obj.gameObject.transform.localPosition = Vector2.zero;
        m_objQueue.Enqueue(_obj);
    }
    #endregion
}


