using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UI.Common.Base;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class ThemePopup : MonoBehaviour
{
    [FormerlySerializedAs("ThemeItemList")] [SerializeField] private BasePrefabList themeItemList;
    [FormerlySerializedAs("PopupPanelRect")] [SerializeField] private RectTransform popupPanelRect;

    [Serializable]
    public class ThemeEvent : UnityEvent{}
    
    [FormerlySerializedAs("OnEquipEvent")]
    [SerializeField]
    public ThemeEvent mOnEquip;
    
    // Start is called before the first frame update
    void Start()
    {
        if (popupPanelRect)
        {
            float safeZoneValue = ResolutionManager.Instance.GetSafeZonePos().y;
            Vector2 size = popupPanelRect.sizeDelta;
            size.y += safeZoneValue;
            popupPanelRect.sizeDelta = size;
        }
    }

    /*
    // Update is called once per frame
    void Update()
    {
    }*/

    private void OnEnable()
    {
        InitThis();
    }

    public void InitThis()
    {
        RefreshThis();
    }

    public void OnInitSlot(int index, BaseIndexPrefab prefab)
    {
        ThemeSlot slot = prefab.GetBasePrefab<ThemeSlot>();
        if (slot)
        {
            slot.mOnRefresh.AddListener(OnRefresh);
            slot.mOnEquipped.AddListener(OnEquipped);
        }
    }

    private void RefreshThis()
    {
        if (!themeItemList)
        {
            return;
        }

        int i;
        themeItemList.DisableChildObject();
        List<ThemeTable> themeTableList = ItemManager.Instance.GetThemeTableList();
        List<HaveItemInfo> haveItemList = new List<HaveItemInfo>();
        for (i = 0; i < themeTableList.Count; i++)
        {
            ThemeTable table = themeTableList[i];
            HaveItemInfo haveItem = ItemManager.Instance.MakeHaveItemInfo(table.themeIdx, table);
            if (haveItem != null)
            {
                haveItem.SortValue = table.themeSort;
                haveItemList.Add(haveItem);
            }
        }

        // 정렬 처리
        haveItemList = haveItemList.OrderByDescending(info => info.State).ThenByDescending(info => info.SortValue).ToList();
        
        // 정보 세팅
        for (i = 0; i < haveItemList.Count; i++)
        {
            HaveItemInfo haveItem = haveItemList[i];
            ThemeSlot slot = themeItemList.GetBasePrefab<ThemeSlot>(i);
            if (slot)
            {
                slot.SetItemInfo(haveItem);
            }
        }
    }

    private void OnRefresh()
    {
        RefreshThis();
    }

    private void OnEquipped()
    {
        mOnEquip?.Invoke();
    }
}