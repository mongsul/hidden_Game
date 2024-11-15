using System.Collections;
using System.Collections.Generic;
using Core;
using NativeUtil;
using UnityEngine;

public class VibrationLibrary : SimpleManagerBase<VibrationLibrary>
{
    private bool isUseVibration = true;
#if UNITY_EDITOR
#elif UNITY_ANDROID
    private float vibrationTime = 250.0f; // 진동 시간 (m/s) 
#elif UNITY_IOS
    private int vibrationID = 1519; // 진동 사운드 ID
#endif

    protected override void AwakeSetting()
    {
        InitVibrationOption();
    }

    public void InitVibrationOption()
    {
        // 진동 관련 설정 세팅
        isUseVibration = SaveManager.Instance.GetVibrationOptionValue();

#if UNITY_EDITOR
#elif UNITY_ANDROID
        vibrationTime = ClientTableManager.Instance.GetBaseFloatValue("VibrationTime");
#elif UNITY_IOS
        string vibrationSoundName = ClientTableManager.Instance.GetBaseValue("IOSVibrationID");
        vibrationID = ClientTableManager.Instance.GetIOSSoundID(vibrationSoundName);
#endif
    }

    public void ExecuteSimpleVibration()
    {
        if (!isUseVibration)
        {
            return;
        }
        
#if UNITY_EDITOR
#elif UNITY_ANDROID
        AndroidUtil.Vibrate((long)vibrationTime);
#elif UNITY_IOS
        IOSUtil.PlaySystemSound(vibrationID);
#endif
    }
}