using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Library;
using UnityEngine;
using UnityEngine.Windows;
using File = System.IO.File;

[System.Serializable]
public class SaveClass
{
    public int clearStage;

    public string languageCode;

    public string[] stringMapKeyList;
    public string[] stringMapValueList;

    public int GetStringMapCount()
    {
        int keyCount = (stringMapKeyList != null) ? stringMapKeyList.Length : 0;
        int valueCount = (stringMapValueList != null) ? stringMapValueList.Length : 0;
        return Math.Min(keyCount, valueCount);
    }
}

// _SJ      저장 데이터 관리자
public class SaveManager : SingletonTemplate<SaveManager>
{
    private SaveClass saveData;
    
    private Dictionary<string, string> stringMapValue = new Dictionary<string, string>();

    #region Base
    private static string GetSaveFolder()
    {
        return Application.dataPath + "/Save";
    }
    
    private static string GetSavePath()
    {
        return GetSaveFolder() + "/SaveData.json";
    }
    
    public void Save()
    {
        saveData.stringMapKeyList = stringMapValue.Keys.ToArray();
        saveData.stringMapValueList = stringMapValue.Values.ToArray();
        
        string json = JsonUtility.ToJson(saveData, true);
        string folder = GetSaveFolder();
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }
        
        File.WriteAllText(GetSavePath(), json);
    }

    public void Load()
    {
        string savePath = GetSavePath();
        if (!File.Exists(savePath))
        {
            saveData = new SaveClass();
            stringMapValue = new Dictionary<string, string>();
            return;
        }

        string json = File.ReadAllText(savePath);
        saveData = JsonUtility.FromJson<SaveClass>(json);
        
        stringMapValue = new Dictionary<string, string>();
        int count = saveData.GetStringMapCount();
        for (int i = 0; i < count; i++)
        {
            stringMapValue.Add(saveData.stringMapKeyList[i], saveData.stringMapValueList[i]);
        }
    }
    #endregion

    #region Dictionary
    public string GetStringMapValue(string stringKey)
    {
        if (stringMapValue != null)
        {
            if (stringMapValue.ContainsKey(stringKey))
            {
                return stringMapValue[stringKey];
            }
        }

        return "";
    }

    public void SetStringMapValue(string stringKey, string value)
    {
        if (stringMapValue == null)
        {
            stringMapValue = new Dictionary<string, string>();
        }

        if (stringMapValue.ContainsKey(stringKey))
        {
            stringMapValue[stringKey] = value;
        }
        else
        {
            stringMapValue.Add(stringKey, value);
        }
    }
    #endregion

    #region Option
    public void SetLanguageCode(string code)
    {
        saveData.languageCode = code;
    }

    public string GetLanguageCode()
    {
        return saveData.languageCode;
    }
    #endregion
    
    #region InGame
    public void SetClearStage(int stage)
    {
        int nowMaxClearStage = GetClearStage();
        if (nowMaxClearStage < stage)
        {
            saveData.clearStage = stage;
            Save();
        }
    }

    public int GetClearStage()
    {
        return saveData.clearStage;
    }
    #endregion

    #region Option
    public void SetOptionValue(string key, string value)
    {
        SetStringMapValue("option_" + key, value);
    }

    public string GetOptionValue(string key)
    {
        string value = GetStringMapValue("option_" + key);
        return string.IsNullOrEmpty(value) ? "true" : value;
    }

    public bool GetSoundOptionValue(bool isBGM)
    {
        string value = GetOptionValue(isBGM ? "BGMSound" : "FXSound");
        return Convert.ToBoolean(value);
    }

    public bool GetVibrationOptionValue()
    {
        string value = GetOptionValue("Vibration");
        return Convert.ToBoolean(value);
    }
    #endregion
}