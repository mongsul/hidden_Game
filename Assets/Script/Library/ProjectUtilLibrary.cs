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
            //Vibrator.Vibrate(time);
        }
    }
    #endregion

    #region Prefab
    public static GameObject LoadStagePrefab(int chapterSort, int stage)
    {
        string prefabName = $"stage{chapterSort}-{stage}";
        string path = "Prefabs/Stage/";
        return ResourceManager.Instance.LoadPrefab<GameObject>(new ResourcePathData(path), prefabName);
    }
    #endregion
}