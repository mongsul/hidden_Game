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
}

// _SJ      저장 데이터 관리자
public class SaveManager : SaveBaseManager
{
    private SaveClass saveData;
    
    #region Base
    private static string GetSavePath()
    {
        return GetSaveFolder() + "/SaveData.json";
    }
    
    public void SaveGameOption()
    {
        string json = JsonUtility.ToJson(saveData, true);
        string folder = GetSaveFolder();

        if (!System.IO.Directory.Exists(folder))
        {
            System.IO.Directory.CreateDirectory(folder);
        }
        
        StreamWriter wr = new StreamWriter(GetSavePath(), false);
        wr.WriteLine(json);
        wr.Close();
    }

    public override void Load()
    {
        base.Load();
        
        string savePath = GetSavePath();
        if (!File.Exists(savePath))
        {
            saveData = new SaveClass();
            return;
        }

        //string json = File.ReadAllText(savePath);
        StreamReader rd = new StreamReader(savePath);
        string json = rd.ReadToEnd();
        saveData = JsonUtility.FromJson<SaveClass>(json);
    }
    #endregion
    
    #region InGame
    public void SetClearStage(int stage)
    {
        int nowMaxClearStage = GetClearStage();
        if (nowMaxClearStage < stage)
        {
            saveData.clearStage = stage;

            StageTable nowClearTable = StageTableManager.Instance.GetStageTable(stage);
            StageTable nextStageTable = StageTableManager.Instance.GetNextStageTable(stage);
            bool isEndChapter = false;
            if (nowClearTable != null)
            {
                if (nextStageTable == null)
                {
                    isEndChapter = true;
                }
                else if (nowClearTable.chapter != nextStageTable.chapter)
                {
                    isEndChapter = true;
                }

                if (isEndChapter)
                {
                    ChapterTable chapterTable = StageTableManager.Instance.GetChapterTable(nowClearTable.chapter);
                    if (chapterTable != null)
                    {
                        if (chapterTable.chapterReward != 0)
                        {
                            // 챕터 보상 수급
                            ItemManager.Instance.AddHaveItemCount(chapterTable.chapterReward);
                            SaveFile(SaveKind.Item);
                        }
                    }
                }
            }
            
            SaveGameOption();
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
        SetValue(SaveKind.Record, GetStageRecordTitle(stage, title), nowStageRecord.ToString());
    }

    public void SetStageRecord(int stage, string title, int count)
    {
        SetValue(SaveKind.Record, GetStageRecordTitle(stage, title), count.ToString());
    }

    public int GetStageRecord(int stage, string title)
    {
        return GetIntValue(SaveKind.Record, GetStageRecordTitle(stage, title), 0);
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
        titleList.Add("GameClear");
        titleList.Add("GameOver");

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
            string nowStageRecord = $"{StageTableManager.Instance.GetChapterSort(nowStage)} - {table.stage}";
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
    public void SetLanguageCode(string code)
    {
        SetValue(SaveKind.Option, "language", code);
    }

    public string GetLanguageCode()
    {
        return GetValue(SaveKind.Option, "language");
    }
    
    public void SetOptionValue(string key, string value)
    {
        SetValue(SaveKind.Option, key, value);
    }

    public bool GetOptionValue(string key)
    {
        return GetBoolValue(SaveKind.Option, key, true);
    }

    public bool GetSoundOptionValue(bool isBGM)
    {
        return GetOptionValue(isBGM ? "BGMSound" : "FXSound");
    }

    public bool GetVibrationOptionValue()
    {
        return GetOptionValue("Vibration");
    }
    #endregion
}