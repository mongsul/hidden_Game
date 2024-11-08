using System;
using Core.Library;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

// _SJ      세이프존 체크해주는 컴포넌트
public class SafeZoneChecker : UIBehaviour
{
    // 세이프존 정보 적용 방법 타입
    private enum ApplyHowToSafeZoneType
    {
        DefaultTypeApplyHowToSymmetry, // 양 측면에서 최대값을 써서 대칭으로 적용
        ApplyHowToBaseValue, // 세이프존 기본 값 사용
    }

    private enum ApplyAxisToType
    {
        DefaultTypeAllAxis, // 모든 측면에 값 적용
        ApplyAxisToX, // X축에만 적용
        ApplyAxisToY, // Y축에만 적용
    }

    [Tooltip("세이프존 정보 적용 방법 타입\n\n"
        + "Default type apply how to symmetry\n"
        + " - 양 측면에서 최대값을 써서 대칭으로 적용\n\n"
        + "Apply how to base value\n"
        + " - 세이프존 기본 값 사용")]
    [FormerlySerializedAs("ApplyHowToSafeZoneType")] [SerializeField] private ApplyHowToSafeZoneType applyHowToSafeZoneType;
    
    [Tooltip("세이프존 정보 적용 축 타입\n\n"
             + "Default type all axis\n"
             + " - 모든 측면에 값 적용\n\n"
             + "Apply axis to x\n"
             + " - X축에만 적용"
             + "Apply axis to y\n"
             + " - Y축에만 적용")]
    [FormerlySerializedAs("ApplyAxisToType")] [SerializeField] private ApplyAxisToType applyAxisToType;
    
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        RefreshSafeZoneInfo();
    }

    // Update is called once per frame
    /*protected override void Update()
    {
        base.Update();
    }*/
    
    /*
#if UNITY_EDITOR
    override protected void OnValidate()
    {
        base.OnValidate();
        
        RefreshSafeZoneInfo();
    }

    override protected void Reset()
    {
        base.Reset();

        RefreshSafeZoneInfo();
    }
#endif*/

    protected override void OnRectTransformDimensionsChange()
    {
        base.OnRectTransformDimensionsChange();

        RefreshSafeZoneInfo();
    }

    private void RefreshSafeZoneInfo()
    {
        ApplySafeZone(CodeUtilLibrary.GetRectTransform(gameObject));
        /*
        switch (applyWhichToSafeZoneType)
        {
            case ApplyWhichToSafeZoneType.DefaultTypeApplyWhichToThisObject:
            {
                ApplySafeZone(this.gameObject.GetRectTransform());
            }
                break;
            case ApplyWhichToSafeZoneType.ApplyWhichToChildObjectList:
            {
                int endCount = this.gameObject.transform.childCount;
                for (int i = 0; i < endCount; i++)
                {
                    Transform childTransform = this.gameObject.transform.GetChild(i);
                    if (childTransform)
                    {
                        ApplySafeZone(childTransform.GetRectTransform());
                    }
                }
            }
                break;
        }*/
    }

    private void ApplySafeZone(RectTransform applyRect)
    {
        // 세이프존 정보 적용
        if (!applyRect)
        {
            return;
        }
        
        //Screen.currentResolution
        Rect deviceSafeArea = Screen.safeArea;
        Vector2 screenSize = new Vector2(Screen.width, Screen.height);
        Vector2 anchorMin = deviceSafeArea.position;
        Vector2 anchorMax = new Vector2(0.0f, 0.0f);

        switch (applyHowToSafeZoneType)
        {
            case ApplyHowToSafeZoneType.DefaultTypeApplyHowToSymmetry:
            {
                Vector2 remainScreenSize = screenSize - deviceSafeArea.position - deviceSafeArea.size;
                Vector2 newSafeZoneSize;
                newSafeZoneSize.x = Math.Max(remainScreenSize.x, anchorMin.x);
                newSafeZoneSize.y = Math.Max(remainScreenSize.y, anchorMin.y);
                
                anchorMin = newSafeZoneSize;
                anchorMax = screenSize - newSafeZoneSize;
            }
                break;
            case ApplyHowToSafeZoneType.ApplyHowToBaseValue:
            {
                anchorMax = deviceSafeArea.position + deviceSafeArea.size;
            }
                break;
        }

        bool isApplyToX = true;
        bool isApplyToY = true;
        switch (applyAxisToType)
        {
            case ApplyAxisToType.DefaultTypeAllAxis:
                break;
            case ApplyAxisToType.ApplyAxisToX:
            {
                isApplyToY = false;
            }
                break;
            case ApplyAxisToType.ApplyAxisToY:
            {
                isApplyToX = false;
            }
                break;
        }

        if (isApplyToX)
        {
            anchorMin.x /= screenSize.x;
            anchorMax.x /= screenSize.x;
        }
        
        if (isApplyToY)
        {
            anchorMin.y /= screenSize.y;
            anchorMax.y /= screenSize.y;
        }
                
        applyRect.anchoredPosition = Vector2.zero;
        applyRect.sizeDelta = Vector2.zero;

        applyRect.anchorMin = anchorMin;
        applyRect.anchorMax = anchorMax;
    }

    public static Vector2 GetScreenSizeWithSafeZone()
    {
        Rect deviceSafeArea = Screen.safeArea;
        //Vector2 screenSize = new Vector2(Screen.width, Screen.height);
        //Vector2 anchorMin = deviceSafeArea.position;
        //Vector2 anchorMax = new Vector2(0.0f, 0.0f);
       // Vector2 remainScreenSize = screenSize - deviceSafeArea.position - deviceSafeArea.size;
        return deviceSafeArea.size;
    }
}