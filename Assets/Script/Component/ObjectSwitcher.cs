using System.Collections;
using System.Collections.Generic;
using Core.Library;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

// _SJ      자식 오브젝트 활성화 스위쳐
public class ObjectSwitcher : UIBehaviour
{
    // 활성화 번호
    [FormerlySerializedAs("ActiveIndex")] [SerializeField] private int activeIndex = 0;
    
    // Start is called before the first frame update
    protected override void Start()
    {
        Refresh();
    }

    /*
    // Update is called once per frame
    void Update()
    {
    }*/
    
#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        Refresh();
    }

    protected override void Reset()
    {
        base.Reset();
    }
#endif

    private void Refresh()
    {
        RectTransform rect = CodeUtilLibrary.GetRectTransform(transform);
        if (rect == null)
        {
            return;
        }

        for (int i = 0; i < rect.childCount; i++)
        {
            Transform childTransform = rect.GetChild(i);
            GameObject childObject = childTransform ? childTransform.gameObject : null;
            if (childObject)
            {
                childObject.SetActive(i == activeIndex);
            }
        }
    }

    public void SetActiveByChildIndex(int index)
    {
        RectTransform rect = CodeUtilLibrary.GetRectTransform(transform);
        if (rect == null)
        {
            return;
        }

        Transform childTransform;
        GameObject childObject;
        
        // 기존 오브젝트 비활성화
        if ((activeIndex >= 0) && (activeIndex < rect.childCount))
        {
            childTransform = rect.GetChild(activeIndex);
            childObject = GetActiveObject();
            if (childObject)
            {
                childObject.SetActive(false);
            }
        }

        // 새로운 오브젝트 활성화
        activeIndex = index;
        if ((activeIndex >= 0) && (activeIndex < rect.childCount))
        {
            childTransform = rect.GetChild(activeIndex);
            childObject = childTransform ? childTransform.gameObject : null;
            if (childObject)
            {
                childObject.SetActive(true);
            }
        }
    }

    public GameObject GetActiveObject()
    {
        RectTransform rect = CodeUtilLibrary.GetRectTransform(transform);
        if (rect == null)
        {
            return null;
        }

        if ((activeIndex < 0) || (rect.childCount <= activeIndex))
        {
            return null;
        }
        
        Transform childTransform = rect.GetChild(activeIndex);
        return childTransform ? childTransform.gameObject : null;
    }

    public T GetActivePrefab<T>() where T : Component
    {
        RectTransform rect = CodeUtilLibrary.GetRectTransform(transform);
        if (rect == null)
        {
            return null;
        }

        if ((activeIndex < 0) || (rect.childCount <= activeIndex))
        {
            return null;
        }
        
        Transform childTransform = rect.GetChild(activeIndex);
        if (childTransform)
        {
            return childTransform.GetComponentInChildren<T>();
        }

        return null;
    }

    public int GetActiveIndex()
    {
        return activeIndex;
    }
}