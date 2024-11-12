using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class TouchCover : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Serializable]
    public class TouchCoverEvent : UnityEvent<float>{}

    // Event delegates triggered on click.
    [FormerlySerializedAs("OnClickEvent")]
    [SerializeField]
    private TouchCoverEvent mOnRelease;

    private Vector2 pressPos; // 누른 위치 (뗀 위치와의 거리를 Release로 전송)

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
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        float dist = Vector2.Distance(pressPos, eventData.position);
        mOnRelease?.Invoke(dist);
    }
}