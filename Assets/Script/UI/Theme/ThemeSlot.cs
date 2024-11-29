using System;
using System.Collections;
using System.Collections.Generic;
using UI.Common;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ThemeSlot : MonoBehaviour
{
    [FormerlySerializedAs("StateSwitcher")] [SerializeField] private ObjectSwitcher stateSwitcher;
    [FormerlySerializedAs("ThemeIcon")] [SerializeField] private Image themeIcon;
    [FormerlySerializedAs("LockPanel")] [SerializeField] private GameObject lockPanel;
    [FormerlySerializedAs("GuideToUnlock")] [SerializeField] private LocalizeTextField guideToUnlock;

    [Serializable]
    public class ThemeEvent : UnityEvent{}
    
    [FormerlySerializedAs("OnRefreshEvent")]
    [SerializeField]
    public ThemeEvent mOnRefresh;
    
    [FormerlySerializedAs("OnEquippedEvent")]
    [SerializeField]
    public ThemeEvent mOnEquipped;
    
    private ThemeTable table;
    private HaveItemInfo haveItem;
    
    /*
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }*/

    public void SetItemInfo(HaveItemInfo item)
    {
        table = item.GetTable<ThemeTable>();
        haveItem = item;
        int switcherIndex = 1;
        bool isLock = false;

        switch (haveItem.State)
        {
            case ItemState.Have:
            {
            }
                break;
            case ItemState.Equip:
            {
                switcherIndex = 0;
            }
                break;
            case ItemState.NoHave:
            {
                isLock = true;
                
                if (guideToUnlock)
                {
                    LocalizeTextField.LocalizeInfo localizeInfo = new LocalizeTextField.LocalizeInfo();
                    switch (table.unlockType)
                    {
                        case ThemeUnlockType.CHAPTERCLEAR:
                        {
                            localizeInfo.localizeKey = "ChapterUnlockText";
                            int rewardChapterIndex = StageTableManager.Instance.GetChapterByRewardItem(table.themeIdx);
                            ChapterTable chapter = StageTableManager.Instance.GetChapterTable(rewardChapterIndex);
                            if (chapter != null)
                            {
                                localizeInfo.SetContent(chapter.chapterSort.ToString());
                            }
                        }
                            break;
                        case ThemeUnlockType.AD:
                        {
                            localizeInfo.localizeKey = "WatchAd";
                        }
                            break;
                        case ThemeUnlockType.PURCHASE:
                        {
                            localizeInfo.localizeKey = "Purchase";
                        }
                            break;
                    }
                    
                    guideToUnlock.SetText(localizeInfo);
                }
            }
                break;
            default:
            {
                gameObject.SetActive(false);
            }
                return;
        }
        
        gameObject.SetActive(true);

        if (themeIcon)
        {
            string path = "ui/Theme_icon/";
            Sprite image = ResourceManager.Instance.LoadImage(new ResourcePathData(path), GetItemImageName(table.resourceName));
            themeIcon.sprite = image;
        }

        if (stateSwitcher)
        {
            stateSwitcher.SetActiveByChildIndex(switcherIndex);
        }

        if (lockPanel)
        {
            lockPanel.gameObject.SetActive(isLock);
        }
    }

    private string GetItemImageName(string themeName)
    {
        return $"Theme_icon_{themeName}";
    }

    public void SetSelectThis()
    {
        if (haveItem.State == ItemState.Have)
        {
            ItemManager.Instance.EquipItem(haveItem.Index);
            CallRefreshList();
            CallRefreshEquippedTheme();
        }
    }

    public void TryGetTheme()
    {
        if (haveItem.State != ItemState.NoHave)
        {
            return;
        }

        switch (table.unlockType)
        {
            case ThemeUnlockType.AD:
            {
                // 광고로 획득
                AdvertisementManager.Instance.PlayAd(OnEndWatchThemeAD);
            }
                break;
            case ThemeUnlockType.PURCHASE:
            {
                // 구매로 획득
                ShopManager.Instance.RequestBuyProductBySupplyItemIndex(table.themeIdx, OnPurchaseTheme);
            }
                break;
        }
    }

    private void OnPurchaseTheme(PurchaseError error, int index)
    {
        if (error == PurchaseError.Success)
        {
            // 성공처리
            CallRefreshList();
            return;
        }
        
        // 실패 처리
        MessageManager.Instance.SetMsg(error.ToString());
    }

    public void CallRefreshEquippedTheme()
    {
        mOnEquipped?.Invoke();
    }

    public void CallRefreshList()
    {
        mOnRefresh?.Invoke();
    }

    private void OnEndWatchThemeAD(AdWatchError error)
    {
        if (error == AdWatchError.Success)
        {
            ItemManager.Instance.AddHaveItemCount(table.themeIdx);
            ItemManager.Instance.SaveHaveItem();
            CallRefreshList();
        }
        else
        {
            MessageManager.Instance.SetMsg(error.ToString());
        }
    }
}