using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableUI : MonoBehaviour , IBeginDragHandler, IEndDragHandler, IDragHandler
{
    #region PrivateVariable
    private Transform m_canvas;
    public Transform m_previousParent;
    private RectTransform m_rectTransform;
    private CanvasGroup m_canvasGroup;
    #endregion

    #region PrivateMethod
    private void Start()
    {
        m_canvas = GameObject.Find("WeaponCartUICanvas").transform;
        m_rectTransform = GetComponent<RectTransform>();
        m_canvasGroup = GetComponent<CanvasGroup>();
    }
    #endregion

    #region PublicMethod
    public void OnBeginDrag(PointerEventData eventData)
    {
        m_previousParent = transform.parent;

        transform.SetParent(m_canvas);
        transform.SetAsLastSibling();

        m_canvasGroup.alpha = 1.0f;
        m_canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        m_rectTransform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {   
        if(transform.parent == m_canvas || transform.parent == m_previousParent)
            ResetDrag();

        m_canvasGroup.alpha = 1.0f;
        m_canvasGroup.blocksRaycasts = true;
    }

    public void ResetDrag()
    {
        transform.SetParent(m_previousParent);
        m_rectTransform.position = m_previousParent.GetComponent<RectTransform>().position;
    }
    #endregion
}
