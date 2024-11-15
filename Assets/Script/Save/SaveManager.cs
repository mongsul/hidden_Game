using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Core.Library;
using UI.Common;
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
#if UNITY_EDITOR
        return Application.dataPath + "/Save";
#else
        return Application.persistentDataPath + "/Save";
#endif
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

        if (!System.IO.Directory.Exists(folder))
        {
            System.IO.Directory.CreateDirectory(folder);
        }
        
        /*
#if UNITY_EDITOR
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }
#endif*/
        
        //File.WriteAllText(GetSavePath(), json);
        StreamWriter wr = new StreamWriter(GetSavePath(), false);
        wr.WriteLine(json);
        wr.Close();
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

        //string json = File.ReadAllText(savePath);
        StreamReader rd = new StreamReader(savePath);
        string json = rd.ReadToEnd();
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
    public string GetStringMapValue(string stringKey, string defaultValue = "")
    {
        if (stringMapValue != null)
        {
            if (stringMapValue.ContainsKey(stringKey))
            {
                return stringMapValue[stringKey];
            }
        }

        return defaultValue;
    }

    public int GetStringMapIntValue(string stringKey, int defaultValue = 0)
    {
        return Convert.ToInt32(GetStringMapValue(stringKey, defaultValue.ToString()));
    }

    public bool GetStringMapBoolValue(string stringKey, bool defaultValue = false)
    {
        return Convert.ToBoolean(GetStringMapValue(stringKey, defaultValue.ToString()));
    }

    public float GetStringMapFloatValue(string stringKey, float defaultValue = 0)
    {
        return (float)Convert.ToDouble(GetStringMapValue(stringKey, defaultValue.ToString()));
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

    public string GetStageRecordTitle(int stage, string title)
    {
        return $"{stage}_{title}";
    }

    public void AddStageRecord(int stage, string title)
    {
        int nowStageRecord = GetStageRecord(stage, title) + 1;
        SetStringMapValue(GetStageRecordTitle(stage, title), nowStageRecord.ToString());
    }

    public void SetStageRecord(int stage, string title, int count)
    {
        SetStringMapValue(GetStageRecordTitle(stage, title), count.ToString());
    }

    public int GetStageRecord(int stage, string title)
    {
        return GetStringMapIntValue(GetStageRecordTitle(stage, title), 0);
    }

    public string GetAllStageRecord()
    {
        int minStage = StageTableManager.Instance.GetMinStageIndex();
        int maxStage = StageTableManager.Instance.GetMinStageIndex();
        int nowStage = minStage;
        string record = "";
        List<string> titleList = new List<string>();
        titleList.Add("TouchToCollect");
        titleList.Add("TouchToWrong");
        titleList.Add("UseHint");

        List<int> countList = new List<int>();
        int i;
        for (i = 0; i < titleList.Count; i++)
        {
            countList.Add(0);
        }

        LocalizeTextField.LocalizeInfo localizeInfo = new LocalizeTextField.LocalizeInfo();
        
        while (true)
        {
            StageTable table = StageTableManager.Instance.GetStageTable(nowStage);
            if (table == null)
            {
                break;
            }

            bool isCounting = false;
            string nowStageRecord = "";
            for (i = 0; i < titleList.Count; i++)
            {
                int count = GetStageRecord(nowStage, titleList[i]);
                if (count > 0)
                {
                    localizeInfo.localizeKey = titleList[i];
                    localizeInfo.SetContent(count.ToString());
                    nowStageRecord += LocalizeTextField.GetFormatStringByLocalizeInfo(localizeInfo) + "\n";
                    isCounting = true;
                }
            }

            if (isCounting)
            {
                record += nowStageRecord;
            }

            nowStage = StageTableManager.Instance.GetNextStageTableIndex(nowStage);
        }
        
        return record;
    }
    #endregion

    #region Option
    public void SetOptionValue(string key, string value)
    {
        SetStringMapValue("option_" + key, value);
    }

    public string GetOptionValue(string key)
    {
        string value = GetStringMapValue("option_" + key, true.ToString());
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