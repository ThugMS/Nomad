using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerMineralMoving : MonoBehaviour
{
    [SerializeField] private GameObject m_starlightObj;
    [SerializeField] private GameObject m_cazelinObj;
    private const int m_count = 5;
    private const float m_speed = 5f;
    private const float m_plusSpeed = 1.5f;

    private const double m_cazelinTime = 0.4f;
    private const double m_starlightTime = 0.4f;

    private double m_cazelinStartTime = 0;
    private double m_starlightStartTime = 0;

    private Transform[] m_starlightTrans = new Transform[m_count];
    private Transform[] m_cazelinTrans = new Transform[m_count];

    private Transform m_starlightTarget = null;
    private Transform m_cazelinTarget = null;
    private Transform m_myTransform = null;
    void Start()
    {
        Create();
    }
    void Update()
    {
        if (m_starlightTarget != null)
            MoveToStarlightTarget();

        if (m_cazelinTarget != null)
            MoveToCazelineTarget();
        
    }
    private void Create()
    {
        for(int i=0; i< m_count; i++)
        {
            m_starlightTrans[i] = Instantiate(m_starlightObj,transform).transform;
            m_starlightTrans[i].gameObject.SetActive(false);

            m_cazelinTrans[i] = Instantiate(m_cazelinObj, transform).transform;
            m_cazelinTrans[i].gameObject.SetActive(false);
        }
    }
    private void MoveToCazelineTarget()
    {
        int count = 0;
        for(int i=0; i<m_count; i++)
        {
            if ((m_cazelinTarget.position - m_cazelinTrans[i].position).magnitude < 0.1f)
            {
                count++;
                continue;
            } 
            if(PhotonNetwork.Time - m_cazelinStartTime < m_cazelinTime)
                m_cazelinTrans[i].position += (m_cazelinTarget.position - m_cazelinTrans[i].position).normalized * (m_speed + (m_plusSpeed * i)) * Time.deltaTime;
            else
                m_cazelinTrans[i].position += (m_cazelinTarget.position - m_cazelinTrans[i].position).normalized * m_speed * Time.deltaTime;
        }
        if (count == m_count)
            ResetCazeline();

    }
    private void MoveToStarlightTarget()
    {
        int count = 0;
        for (int i = 0; i < m_count; i++)
        {
     
            if ((m_starlightTarget.position - m_starlightTrans[i].position).magnitude < 0.1f)
            {
                count++;
                continue;
            }
            if (PhotonNetwork.Time - m_starlightStartTime < m_starlightTime)
                m_starlightTrans[i].position += (m_starlightTarget.position - m_starlightTrans[i].position).normalized * (m_speed + (m_plusSpeed * i)) * Time.deltaTime;
            else
                m_starlightTrans[i].position += (m_starlightTarget.position - m_starlightTrans[i].position).normalized * m_speed * Time.deltaTime;
        }
        if (count == m_count)
            ResetStarlight();

    }
    private void ResetCazeline()
    {
        for (int i = 0; i < m_count; i++)
            m_cazelinTrans[i].gameObject.SetActive(false);
        m_cazelinTarget = null;
        m_cazelinStartTime = 0;
    }
    private void ResetStarlight()
    {
        for (int i = 0; i < m_count; i++)
            m_starlightTrans[i].gameObject.SetActive(false);

        m_starlightTarget = null;
        m_starlightStartTime = 0;
    }
    private void SetFirstMineralPosition(MineralType _mineralType)
    {
        if(_mineralType == MineralType.Cazelin)
        {
            m_cazelinStartTime = PhotonNetwork.Time;
            for (int i = 0; i < m_count; i++)
            {
                m_cazelinTrans[i].gameObject.SetActive(true);
                m_cazelinTrans[i].position = m_myTransform.position;
            }
        }
        else
        {
            m_starlightStartTime = PhotonNetwork.Time;
            for (int i = 0; i < m_count; i++)
            {
                m_starlightTrans[i].gameObject.SetActive(true);
                m_starlightTrans[i].position = m_myTransform.position;
            }
        }
    }
    public void SetStarlightTarget(Transform _targetTran)
    {
        if (m_starlightTarget != null)
            return;
        m_starlightTarget = _targetTran;

        SetFirstMineralPosition(MineralType.Starlight);

    }
    public void SetCazelinTarget(Transform _targetTran)
    {
        if (m_cazelinTarget != null)
            return;
        m_cazelinTarget = _targetTran;

        SetFirstMineralPosition(MineralType.Cazelin);
    }

    public void SetOwner(Transform _transform)
    {
        m_myTransform = _transform;
    }
}
