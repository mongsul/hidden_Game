using System;
using System.Collections;
using System.Collections.Generic;
using Core.Library;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ChapterBookPanel : MonoBehaviour
{
    [SerializeField] private ObjectSwitcher openSwitcher;
    [SerializeField] private Image chapterCoverImage;

    [Serializable]
    public class ChapterBookEvent : UnityEvent<ChapterTable>{}
    
    [FormerlySerializedAs("OnSelectEvent")]
    [SerializeField]
    public ChapterBookEvent mOnSelect = new ChapterBookEvent();

    private ChapterTable chapter;
    
    /*
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }*/

    public void SetClose()
    {
        if (openSwitcher)
        {
            openSwitcher.SetActiveByChildIndex(0);
        }
    }

    public void SetChapter(int chapterSort)
    {
        chapter = StageTableManager.Instance.GetChapterTableBySort(chapterSort);
        if (chapter == null)
        {
            return;
        }
        
        if (openSwitcher)
        {
            openSwitcher.SetActiveByChildIndex(1);
        }

        if (chapterCoverImage)
        {
            Sprite bookIcon = CodeUtilLibrary.LoadSprite("ui/BookShelf/icon/", chapter.episodeIcon);
            chapterCoverImage.sprite = bookIcon;
        }
    }

    public void OnClickChapter()
    {
        mOnSelect?.Invoke(chapter);
    }
}