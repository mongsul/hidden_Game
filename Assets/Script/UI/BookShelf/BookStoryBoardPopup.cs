using System;
using System.Collections;
using System.Collections.Generic;
using Core.Library;
using UI.Common;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class BookStoryBoardPopup : MonoBehaviour
{
    [SerializeField] private Image storyImage;
    [SerializeField] private LocalizeTextField storyDescField;
    
    [Serializable]
    public class RetryEvent : UnityEvent<int>{}
    
    [FormerlySerializedAs("OnRetryEvent")]
    [SerializeField]
    public RetryEvent mOnRetry = new RetryEvent();

    private StageTable storyTable;

    /*
    // Start is called before the first frame updater
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }*/

    public void SetStage(StageTable stage)
    {
        storyTable = stage;
        if (storyTable == null)
        {
            return;
        }

        if (storyImage)
        {
            Sprite image = CodeUtilLibrary.LoadSprite(storyTable.episodeImagePath, storyTable.episodeImageName);
            storyImage.sprite = image;
        }

        if (storyDescField)
        {
            storyDescField.SetText(storyTable.episodeText);
        }
    }

    public void OnClickRetry()
    {
        if (storyTable != null)
        {
            CallRetryEvent(storyTable.stageIndex);
        }
    }

    private void CallRetryEvent(int stageIndex)
    {
        mOnRetry?.Invoke(stageIndex);
    }

    public void OnClickPrev()
    {
        if (storyTable == null)
        {
            return;
        }

        StageTable stage = null;
        if (storyTable.stage == 1)
        {
            bool isSetNowChapterMaxStage = true;
            StageTable maxClearStage = StageTableManager.Instance.GetStageTable(SaveManager.Instance.GetClearStage());
            if (maxClearStage != null)
            {
                if (maxClearStage.chapter == storyTable.chapter)
                {
                    // 같은 챕터일 경우, 최대 스테이지를 클리어한 스테이지로 세팅
                    isSetNowChapterMaxStage = false;
                    stage = maxClearStage;
                }
            }
            
            if (isSetNowChapterMaxStage)
            {
                stage = StageTableManager.Instance.GetMaxChapterStageTable(storyTable.chapter);
            }
        }
        else
        {
            int sort = StageTableManager.Instance.GetChapterSort(storyTable.chapter);
            stage = StageTableManager.Instance.GetStageTableByChapterInfo(sort, storyTable.stage - 1);
        }
        
        SetStage(stage);
    }

    public void OnClickNext()
    {
        if (storyTable == null)
        {
            return;
        }
        
        StageTable stage = null;
        bool isNowMaxStage = false;
        bool isCheckMaxStage = true;
        StageTable maxClearStage = StageTableManager.Instance.GetStageTable(SaveManager.Instance.GetClearStage());
        if (maxClearStage != null)
        {
            if (maxClearStage.chapter == storyTable.chapter)
            {
                // 같은 챕터일 경우, 최대 스테이지 체크한다.
                isCheckMaxStage = false;
                isNowMaxStage = (storyTable.stageIndex == maxClearStage.stageIndex);
            }
        }

        if (isCheckMaxStage)
        {
            isNowMaxStage = storyTable.stage >= StageTableManager.Instance.GetMaxChapterStage(storyTable.chapter);
        }
        
        int sort = StageTableManager.Instance.GetChapterSort(storyTable.chapter);
        if (isNowMaxStage)
        {
            stage = StageTableManager.Instance.GetStageTableByChapterInfo(sort, 1);
        }
        else
        {
            stage = StageTableManager.Instance.GetStageTableByChapterInfo(sort, storyTable.stage + 1);
        }
        
        SetStage(stage);
    }
}