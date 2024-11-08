using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StageTable : ClientTable
{
    public int stageIndex;
    public int chapter;
    public int stage;
    public int findObjectCount;
    public int touchCount;
}

public class StageTableManager : SingletonTemplate<StageTableManager>
{
    private bool isInitTable = false;
    private Dictionary<int, StageTable> stageTableMap = new Dictionary<int, StageTable>();
    private Dictionary<int, int> chapterKeyIndexMap = new Dictionary<int, int>();
    
    // Start is called before the first frame update
    void Start()
    {
        InitTable();
    }

    /*
    // Update is called once per frame
    void Update()
    {
    }*/
    
    public void InitTable()
    {
        if (isInitTable)
        {
            return;
        }

        InitStageTable();

        isInitTable = true;
    }

    private int GetChapterKey(StageTable stageTable)
    {
        if (stageTable == null)
        {
            return 0;
        }
        
        return stageTable.chapter * 1000 + stageTable.stage;
    }

    private int GetChapterKey(int chapter, int stage)
    {
        return chapter * 1000 + stage;
    }

    private void InitStageTable()
    {
        List<StageTable> stageTableList = ClientTableManager.LoadTable<StageTable>("Stage");
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
        
        // 다음 스테이지 체크
        int chapterKey = GetChapterKey(nowStage.chapter, nowStage.stageIndex + 1);
        StageTable nextStage = GetStageTableByChapterKey(chapterKey);
        if (nextStage != null)
        {
            return nextStage;
        }
        
        // 다음 챕터 체크
        chapterKey = GetChapterKey(nowStage.chapter + 1, 1);
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
}