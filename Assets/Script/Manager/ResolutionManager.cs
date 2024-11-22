using System;
using System.Collections;
using System.Collections.Generic;
using Core;
using UnityEngine;

public class ResolutionManager : SimpleManagerBase<ResolutionManager>
{
    private bool isInit = false;
    private Vector2 safeZonePos = new Vector2(); // 기본 세이프존 영역 위치 = 왼쪽, 아래쪽
    private Vector2 safeZoneSize = new Vector2(); // 기본 세이프존 영역 크기 = 오른쪽, 위쪽
    private Vector2 maxSafeZone = new Vector2(); // 양옆 대칭을 맞춘 세이프존 최대 영역
    
    /*
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }*/

    private void InitThis()
    {
        if (isInit)
        {
            return;
        }
        
        Rect deviceSafeArea = Screen.safeArea;
        Vector2 screenSize = new Vector2(Screen.width, Screen.height);
        
        safeZonePos = deviceSafeArea.position;
        safeZoneSize = screenSize - deviceSafeArea.position - deviceSafeArea.size;

        maxSafeZone.x = Math.Max(safeZonePos.x, safeZoneSize.x);
        maxSafeZone.y = Math.Max(safeZonePos.y, safeZoneSize.y);

        isInit = false;
    }

    public Vector2 GetSafeZonePos()
    {
        if (!isInit)
        {
            InitThis();
        }

        return safeZonePos;
    }

    public Vector2 GetSafeZoneSize()
    {
        if (!isInit)
        {
            InitThis();
        }

        return safeZoneSize;
    }

    public Vector2 GetMaxSafeZone()
    {
        if (!isInit)
        {
            InitThis();
        }

        return maxSafeZone;
    }
}