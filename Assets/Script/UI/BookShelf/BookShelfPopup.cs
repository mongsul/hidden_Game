using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class BookShelfPopup : MonoBehaviour
{
    [SerializeField] private RectTransform shelfListRect;
    [SerializeField] private BookStoryBoardPopup storyBoard;
    
    private List<ChapterBookPanel> bookList;
    
    /*
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }*/

    private void OnEnable()
    {
        InitBookList();
    }

    private void InitBookList()
    {
        int i;
        if (storyBoard)
        {
            storyBoard.gameObject.SetActive(false);
        }
        
        if (bookList == null)
        {
            bookList = new List<ChapterBookPanel>();

            if (shelfListRect)
            {
                bookList = shelfListRect.GetComponentsInChildren<ChapterBookPanel>().ToList();
            }
        }
        
        if (bookList == null)
        {
            return;
        }

        int clearStage = SaveManager.Instance.GetClearStage();
        StageTable clearTable = StageTableManager.Instance.GetStageTable(clearStage);
        int openMaxChapter = 0; // 최대로 오픈된 챕터 sort
        if (clearTable != null)
        {
            openMaxChapter = StageTableManager.Instance.GetChapterSort(clearTable.chapter);
        }

        for (i = 0; i < bookList.Count; i++)
        {
            ChapterBookPanel book = bookList[i];
            if (book)
            {
                book.SetClose();

                int nowBookChapter = i + 1;
                if (nowBookChapter <= openMaxChapter)
                {
                    book.SetChapter(nowBookChapter);
                    book.mOnSelect.RemoveAllListeners();
                    book.mOnSelect.AddListener(OnSelectChapterBook);
                }
            }
        }
    }

    public void SetDisplayStageBook(int stageIndex)
    {
        SetDisplayStageBook(StageTableManager.Instance.GetStageTable(stageIndex));
    }

    public void SetDisplayStageBook(StageTable stage)
    {
        if (stage == null)
        {
            return;
        }
        
        if (storyBoard)
        {
            storyBoard.gameObject.SetActive(true);
            storyBoard.SetStage(stage);
        }
    }

    private void OnSelectChapterBook(ChapterTable chapter)
    {
        if (chapter == null)
        {
            return;
        }
        
        StageTable stage = StageTableManager.Instance.GetStageTableByChapterInfo(chapter.chapterSort, 1);
        SetDisplayStageBook(stage);
    }
}