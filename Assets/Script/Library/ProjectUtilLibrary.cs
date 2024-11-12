using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectUtilLibrary : MonoBehaviour
{
    #region vibration
    public static void VibrateByBaseValue()
    {
        bool isUseVibration = SaveManager.Instance.GetVibrationOptionValue();
        if (isUseVibration)
        {
            int time = ClientTableManager.Instance.GetBaseIntValue("VibrationTime");
            Vibrator.Vibrate(time);
        }
    }
    #endregion
}