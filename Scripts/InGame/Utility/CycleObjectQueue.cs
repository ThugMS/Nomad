using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class ObjectPoolChatSlot : CycleObjectPool<ChatSlot>
{
    //제네릭 클래스를 AddComponent하기 위함.
}

//초기에 active된 이후로 새로운 프리팹을 사용할 때 기존 것을 재활용 하는 큐
public class CycleObjectPool<T> : MonoBehaviour where T : MonoBehaviour
{
    #region ProtectedVariables

    [SerializeField] protected int m_count;
    protected GameObject m_prefab;
    protected Queue<T> m_objQueue = new Queue<T>();
    #endregion

    #region PrivateMethod
    private void Initialize()
    {
        for (int i = 0; i < m_count; i++)
            m_objQueue.Enqueue(CreateObj());
    }

    private T CreateObj()
    {
        var newObj = Instantiate(m_prefab.GetComponent<T>(), transform);
        newObj.transform.SetAsLastSibling();
        newObj.gameObject.SetActive(false);
        return newObj;
    }
    #endregion

    #region PublicRegion
    public T GetObj()
    {
        T obj = null;
        if(m_objQueue.Count > 0)
        {
            obj = m_objQueue.Dequeue();
            obj.gameObject.SetActive(true);

            m_objQueue.Enqueue(obj);
        }
        return obj;
    }
    public void SetActiveFalseAll()
    {
        foreach(T obj in m_objQueue)
            obj.gameObject.SetActive(false);
    }
    public void SetObject(GameObject _object, int _cnt)
    {
        m_prefab = _object;
        m_count = _cnt;

        Initialize();
    }
    public int GetSize()
    {
        return m_count;
    }

    public IEnumerable<T> GetEnumerator()
    {
        return m_objQueue;
    }
    #endregion
}
