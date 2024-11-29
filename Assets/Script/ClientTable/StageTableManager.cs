using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ChapterTable : ClientTable
{
    public int chapterIndex;
    public int chapterSort;
    public string episodeIcon;
    public int chapterReward;
}

[System.Serializable]
public class StageTable : ClientTable
{
    public int stageIndex;
    public int chapter;
    public int stage;
    public int findObjectCount;
    public int touchCount;
    public bool afterAd;
    public string episodeText;
    public string episodeImagePath;
    public string episodeImageName;
}

public class StageTableManager : SingletonTemplate<StageTableManager>
{
    private Dictionary<int, ChapterTable> chapterTableMap = new Dictionary<int, ChapterTable>();
    private Dictionary<int, int> chapterSortIndexMap = new Dictionary<int, int>();
    private Dictionary<int, StageTable> stageTableMap = new Dictionary<int, StageTable>();
    private Dictionary<int, int> chapterKeyIndexMap = new Dictionary<int, int>();
    private Dictionary<int, int> chapterRewardItemMap = new Dictionary<int, int>(); // 아이템 번호 - 챕터 키값 맵 
    private int maxStageChapterKey;
    
    // Start is called before th
    /*e first frame update
    void Start()
    {
        //InitTable();
    }

    // Update is called once per frame
    void Update()
    {
    }*/

    private void OnDestroy()
    {
        chapterTableMap.Clear();
        chapterSortIndexMap.Clear();
        stageTableMap.Clear();
        chapterKeyIndexMap.Clear();
    }

    public void InitTable()
    {
        InitChapterTable();
        InitStageTable();
    }

    private int GetChapterKey(StageTable stageTable)
    {
        if (stageTable == null)
        {
            return 0;
        }

        int sort = GetChapterSort(stageTable.chapter);
        
        return sort * 1000 + stageTable.stage;
    }

    private int GetChapterKey(int chapterSort, int stage)
    {
        return chapterSort * 1000 + stage;
    }

    private void InitChapterTable()
    {
        List<ChapterTable> chapterTableList = ClientTableManager.LoadTable<ChapterTable>("Chapter");
        chapterTableMap.Clear();
        chapterSortIndexMap.Clear();
        chapterRewardItemMap.Clear();
        maxStageChapterKey = 0;
        for (int i = 0; i < chapterTableList.Count; i++)
        {
            int key = chapterTableList[i].chapterIndex;
            int sort = chapterTableList[i].chapterSort;
            int reward = chapterTableList[i].chapterReward;
            if (!chapterTableMap.ContainsKey(key))
            {
                chapterTableMap.Add(key, chapterTableList[i]);
            }

            if (!chapterSortIndexMap.ContainsKey(sort))
            {
                chapterSortIndexMap.Add(sort, key);
            }

            if (reward > 0)
            {
                if (!chapterRewardItemMap.ContainsKey(reward))
                {
                    chapterRewardItemMap.Add(reward, key);
                }
            }
        }
    }

    private void InitStageTable()
    {
        List<StageTable> stageTableList = ClientTableManager.LoadTable<StageTable>("Stage");
        stageTableMap = new Dictionary<int, StageTable>();
        chapterKeyIndexMap = new Dictionary<int, int>();
        for (int i = 0; i < stageTableList.Count; i++)
        {
            int key = stageTableList[i].stageIndex;
            int chapterKey = GetChapterKey(stageTableList[i]);
            if (!stageTableMap.ContainsKey(key))
            {
                stageTableMap.Add(key, stageTableList[i]);
            }

            if (!chapterKeyIndexMap.ContainsKey(chapterKey))
            {
                chapterKeyIndexMap.Add(chapterKey, key);
                if (maxStageChapterKey < chapterKey)
                {
                    maxStageChapterKey = chapterKey; // 최대 챕터 키 기록
                }
            }
        }
    }

    public StageTable GetStageTable(int stageIndex)
    {
        if (stageTableMap.ContainsKey(stageIndex))
        {
            return stageTableMap[stageIndex];
        }

        return null;
    }

    public StageTable GetStageTableByChapterInfo(int chapter, int stage)
    {
        return GetStageTableByChapterKey(GetChapterKey(chapter, stage));
    }

    public StageTable GetStageTableByChapterKey(int chapterKey)
    {
        if (chapterKeyIndexMap.ContainsKey(chapterKey))
        {
            int stageKey = chapterKeyIndexMap[chapterKey];
            return GetStageTable(stageKey);
        }

        return null;
    }

    public StageTable GetNextStageTable(int stageIndex)
    {
        StageTable nowStage = GetStageTable(stageIndex);
        if (nowStage == null)
        {
            return GetMinStageTable();
        }

        int nowChapterSort = GetChapterSort(nowStage.chapter);
        
        // 다음 스테이지 체크
        int chapterKey = GetChapterKey(nowChapterSort, nowStage.stage + 1);
        StageTable nextStage = GetStageTableByChapterKey(chapterKey);
        if (nextStage != null)
        {
            return nextStage;
        }
        
        // 다음 챕터 체크
        chapterKey = GetChapterKey(nowChapterSort + 1, 1);
        return GetStageTableByChapterKey(chapterKey);
    }

    public int GetNextStageTableIndex(int stageIndex)
    {
        StageTable nextStage = GetNextStageTable(stageIndex);
        return (nextStage != null) ? nextStage.stageIndex : 0;
    }

    public bool IsValidStage(int stageIndex)
    {
        return stageTableMap.ContainsKey(stageIndex);
    }

    public StageTable GetMinStageTable()
    {
        int chapterKey = GetChapterKey(1, 1);
        return GetStageTableByChapterKey(chapterKey);
    }

    public int GetMinStageIndex()
    {
        StageTable minStage = GetMinStageTable();
        return (minStage != null) ? minStage.stageIndex : 0;
    }

    public StageTable GetMaxStageTable()
    {
        return GetStageTableByChapterKey(maxStageChapterKey);
    }

    public int GetMaxStageIndex()
    {
        StageTable maxStage = GetMaxStageTable();
        return (maxStage != null) ? maxStage.stageIndex : 0;
    }

    public ChapterTable GetChapterTable(int chapterIndex)
    {
        if (chapterTableMap.ContainsKey(chapterIndex))
        {
            return chapterTableMap[chapterIndex];
        }

        return null;
    }

    public int GetChapterSort(int chapterIndex)
    {
        ChapterTable chapterTable = GetChapterTable(chapterIndex);
        return (chapterTable != null) ? chapterTable.chapterSort : 0;
    }

    public int GetChapterBySort(int chapterSort)
    {
        if (chapterSortIndexMap.ContainsKey(chapterSort))
        {
            return chapterSortIndexMap[chapterSort];
        }

        return 0;
    }

    public int GetChapterByRewardItem(int itemIndex)
    {
        if (chapterRewardItemMap.ContainsKey(itemIndex))
        {
            return chapterRewardItemMap[itemIndex];
        }

        return 0;
    }
}