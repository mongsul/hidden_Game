using System;
using System.Collections;
using System.Collections.Generic;
using Core.Library;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class TouchCover : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Serializable]
    public class TouchCoverEvent : UnityEvent<RectTransform, PointerEventData, float>{}
    
    [Serializable]
    public class TouchCoverPosEvent : UnityEvent<RectTransform, PointerEventData>{}
    
    [FormerlySerializedAs("OnClickEvent")]
    [SerializeField]
    private TouchCoverPosEvent mOnPress;

    [FormerlySerializedAs("OnReleaseEvent")]
    [SerializeField]
    private TouchCoverEvent mOnRelease;

    private Vector2 pressPos; // 누른 위치 (뗀 위치와의 거리를 Release로 전송)
    
    private RectTransform myRect;
    private bool isPressThis = false;

    /*
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }*/
    
    public void OnPointerDown(PointerEventData eventData)
    {
        pressPos = eventData.position;
        RectTransform rect = GetMyRect();
        if (rect)
        {
            mOnPress?.Invoke(rect, eventData);
        }

        isPressThis = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isPressThis)
        {
            return;
        }
        
        RectTransform rect = GetMyRect();
        if (rect)
        {
            float dist = Vector2.Distance(pressPos, eventData.position);
            mOnRelease?.Invoke(rect, eventData, dist);
        }

        isPressThis = false;
    }

    private RectTransform GetMyRect()
    {
        if (!myRect)
        {
            myRect = CodeUtilLibrary.GetRectTransform(transform);
        }

        return myRect;
    }
}