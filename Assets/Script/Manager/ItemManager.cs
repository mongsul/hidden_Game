using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType
{
    None = 0,
    THEME,
    HINT,
    PURCHASE,
    Max, // 최대 종류 개수 체크
}

public enum FunctionItemType
{
    None = 0,
    Hint, // 힌트 기능 아이템
    Max, // 최대 종류 개수 체크
}

public enum ThemaUnlockType
{
    DEFAULT = 0,
    CHAPTERCLEAR,
    AD,
    PURCHASE,
}

public class ItemTable : ClientTable
{
    public int itemIdx;
    public ItemType itemType;
    public int defaultHaveCount;
    public int maxHaveCount;
}

public class ThemeTable : ClientTable
{
    public int itemIdx;
    public ThemaUnlockType unlockType;
    public string resourceName;
}

// _SJ      아이템 관리자
public class ItemManager : SingletonTemplate<ItemManager>
{
    private Dictionary<int, ItemTable> itemTableMap = new Dictionary<int, ItemTable>();
    private Dictionary<int, ThemeTable> themeTableMap = new Dictionary<int, ThemeTable>();
    
    private Dictionary<int, int> haveItemMap = new Dictionary<int, int>();
    private Dictionary<FunctionItemType, int> functionItemKeyMap = new Dictionary<FunctionItemType, int>();
    
    /*
    // Start is called before the first frame update
    void Start()
    {
        InitItem();
    }

    // Update is called once per frame
    void Update()
    {
    }*/

    public void InitItem()
    {
        InitItemTable();
        InitThemeTable();
    }

    #region NormalItem
    private void InitItemTable()
    {
        List<ItemTable> tableList = ClientTableManager.LoadTable<ItemTable>("Item");
        itemTableMap.Clear();
        if (tableList == null)
        {
            return;
        }

        for (int i = 0; i < tableList.Count; i++)
        {
            ItemTable table = tableList[i];
            if (!itemTableMap.ContainsKey(table.itemIdx))
            {
                itemTableMap.Add(table.itemIdx, table);
            }
        }
    }
    
    public int GetHaveItemCount(int itemIndex)
    {
        if (haveItemMap.ContainsKey(itemIndex))
        {
            return haveItemMap[itemIndex];
        }

        return 0;
    }

    public ItemType GetItemType(int itemIndex)
    {
        return ItemType.None;
    }

    public void SetHaveItemCount(int itemIndex, int count)
    {
        if (haveItemMap.ContainsKey(itemIndex))
        {
            haveItemMap[itemIndex] = count;
        }
        else
        {
            haveItemMap.Add(itemIndex, count);
        }
    }

    public void AddHaveItemCount(int itemIndex, int addValue)
    {
        if (haveItemMap.ContainsKey(itemIndex))
        {
            int newCount = haveItemMap[itemIndex] + addValue;
            if (newCount < 0)
            {
                newCount = 0;
            }
            
            haveItemMap[itemIndex] = newCount;
        }
        else
        {
            haveItemMap.Add(itemIndex, addValue);
        }
    }
    #endregion

    #region FunctionItem
    private int GetFunctionTypeToItemIndex(FunctionItemType itemType)
    {
        if (functionItemKeyMap.ContainsKey(itemType))
        {
            return functionItemKeyMap[itemType];
        }

        return 0;
    }
    
    public int GetHaveFunctionItemCount(FunctionItemType itemType)
    {
        int itemIndex = GetFunctionTypeToItemIndex(itemType);
        return GetHaveItemCount(itemIndex);
    }

    public bool IsPossibleUseFunctionItem(FunctionItemType itemType)
    {
        return GetHaveFunctionItemCount(itemType) > 0;
    }

    public bool UseFunctionItem(FunctionItemType itemType)
    {
        int itemIndex = GetFunctionTypeToItemIndex(itemType);
        if (GetHaveItemCount(itemIndex) > 0)
        {
            AddHaveItemCount(itemIndex, -1);
            return true;
        }

        return false;
    }
    #endregion

    #region Theme
    private void InitThemeTable()
    {
        List<ThemeTable> tableList = ClientTableManager.LoadTable<ThemeTable>("Theme");
        themeTableMap.Clear();
        if (tableList == null)
        {
            return;
        }

        for (int i = 0; i < tableList.Count; i++)
        {
            ThemeTable table = tableList[i];
            if (!themeTableMap.ContainsKey(table.itemIdx))
            {
                themeTableMap.Add(table.itemIdx, table);
            }
        }
    }
    #endregion

    #region Ad
    // 광고가 활성화 되어있는가 여부
    public bool IsActivateAD()
    {
        return true;
    }
    #endregion
}