using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


//스크롤 뷰의 크기를 맞춰주는 클래스 content에다 넣기
public class CustomVerticalScrollViewRect : ScrollRect
{
    #region PrivateVariable
    // 스크롤 뷰와 관련된 수정을 하기 위해 가지고 있는 변수 
    private VerticalLayoutGroup m_verticalLayoutGroup;


    private Vector3 m_initPosition;
    #endregion

    protected override void Start()
    {
        base.Start();

        m_verticalLayoutGroup = GetComponentInChildren<VerticalLayoutGroup>();

        SetContentSize();
        m_initPosition = new Vector3(content.anchoredPosition.x, 0, 0);
    }

    #region PublicMethod
    public void SetScrollable(bool _canScroll)
    {
        horizontal = _canScroll;
        vertical = _canScroll;
    }
    public void SetContentSize(float height = 5)
    {

        int cnt = content.childCount;
        Canvas.ForceUpdateCanvases();

        for (int i = 0; i < cnt; i++)
        {
            GameObject obj = content.GetChild(i).gameObject;
            if (obj.activeSelf == false)
                continue;

            height += obj.GetComponent<RectTransform>().sizeDelta.y + m_verticalLayoutGroup.spacing;
        }
        

        // scrollRect.content를 통해서 Hierachy 뷰에서 봤던 Viewport 밑의 Content 게임 오브젝트에 접근
        // Content의 높이와 넓이를 수정
        content.sizeDelta = new Vector2(content.sizeDelta.x, height);
    }
    public void SetPositionInitialize()
    {
        content.anchoredPosition = new Vector3(content.anchoredPosition.x, 0, 0);
    }
    #endregion
}
