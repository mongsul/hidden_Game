using System;
using System.Collections;
using System.Collections.Generic;
using Core.Library;
using Script.Component;
using UI.Common.Base;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Vector2 = System.Numerics.Vector2;

public class Level : MonoBehaviour, IPointerClickHandler
{
    //Public은 외부
    //이미지 배열 (Inspector에 볼 수 있음)
    public Sprite[] chageImage;
    public Button button;

    // Private 은 내부
    // 이미지
    Image image;
    //이미지 배열번호 변수 
    int imageNumber = 0;
    //gameManager gameManager;
    
    [Serializable]
    public class FindObjectEvent : UnityEvent<Level>{}
    
    [Serializable]
    public class TouchPosEvent : UnityEvent<RectTransform, PointerEventData>{}

    [FormerlySerializedAs("OnFindObjectEvent")]
    [SerializeField]
    public FindObjectEvent mOnFindObject;

    // 포장지 찾음
    [FormerlySerializedAs("OnFindWrabObjectEvent")]
    [SerializeField]
    public FindObjectEvent mOnFindWrabObject;
    
    [FormerlySerializedAs("OnClickEvent")]
    [SerializeField]
    public TouchPosEvent mOnPress;

    private ObjectChanger wrap; // 감춰져있는 오브젝트 포장지(?)
    private bool isWrapped = false; // 감춰져있던 오브젝트인가 여부

    public int index;
    private BaseSimplePrefab hint;

    private RectTransform myRect;

    // Start is called before the first frame update
    void Start()
    {
        //이 스크립트를 가진 오브젝트의 Image
        image = this.gameObject.GetComponent<Image>();
        //이 스크립트를 가진 오브젝트의 button
        button = this.gameObject.GetComponent<Button>();

        //gameManager = GameObject.Find("GameManager").GetComponent<gameManager>();

        //Button 로직 추가 클릭할 때 hideObject 함수를 호출한다.
        //Button btn = button.GetComponent<Button>();
        //btn.onClick.AddListener(hideObject);
    }

    /*
    // Update is called once per frame
    void Update()
    {
    }*/

    private void OnEnable()
    {
        if (wrap)
        {
            mOnFindWrabObject?.Invoke(this);
            wrap = null; // 없앰
        }
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        FindObject();
        
        RectTransform rect = GetMyRect();
        if (rect)
        {
            mOnPress?.Invoke(rect, eventData);
        }
    }

    public void SetLinkedWrapObject(ObjectChanger objectChanger)
    {
        wrap = objectChanger;
        isWrapped = true;

        if (wrap)
        {
            wrap.mOnPress.AddListener(OnPressWrapping);
        }
    }

    private void FindObject()
    {
        mOnFindObject?.Invoke(this);
        if (IsEndFindObject())
        {
            return;
        }
        
        imageNumber++; //imageNumber 1추가

        //만약 imageNumber가 넣은 이미지보다 수가 적으면 해당 배열 이미지 출력 넘으면 0부터 다시 시작
        if(imageNumber < chageImage.Length)
        {
            image.sprite = chageImage[imageNumber];
            //gameManager.finded();
        }
        else
        {
            button.interactable = false;
        }
    }

    public bool IsEndFindObject()
    {
        return (imageNumber >= 1);
    }

    public void SetHint(BaseSimplePrefab hintPrefab = null)
    {
        if (!hintPrefab)
        {
            hint = null; // 초기화 처리
            return;
        }
        
        hint = hintPrefab;
        GameObject standardObj = wrap ? wrap.gameObject : gameObject;
        if (hint)
        {
            hint.transform.SetParent(standardObj.transform);
            RectTransform hintRect = CodeUtilLibrary.GetRectTransform(hint.transform);
            if (hintRect)
            {
                hintRect.anchorMax = new UnityEngine.Vector2(0.5f, 0.5f);
                hintRect.anchorMin = new UnityEngine.Vector2(0.5f, 0.5f);
                hintRect.localPosition = Vector3.zero;
            }
            
            SpineAnimPlayer anim = hint.GetBasePrefab<SpineAnimPlayer>();
            if (anim)
            {
                anim.PlayAnim();
            }
        }
    }

    public BaseSimplePrefab GetHint()
    {
        return hint;
    }

    public bool IsWraped()
    {
        return (wrap != null);
    }

    public bool GetIsWrapped()
    {
        return isWrapped;
    }

    private void OnPressWrapping(RectTransform parentRect, PointerEventData eventData)
    {
        mOnPress?.Invoke(parentRect, eventData);
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