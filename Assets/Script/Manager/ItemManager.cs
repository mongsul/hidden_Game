using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum ItemType
{
    None = 0,
    THEME,
    HINT,
    PURCHASE,
    ADREMOVE,
    Max, // 최대 종류 개수 체크
}

public enum FunctionItemType
{
    None = 0,
    Hint, // 힌트 기능 아이템
    ADRemover, // 광고 제거
    Max, // 최대 종류 개수 체크
}

public enum ThemeUnlockType
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

public enum ItemState
{
    None, // 예외처리 용도
    InvalidItem, // 유효하지 않은 아이템
    NoHave, // 구매하지 않은 아이템
    Have, // 보유 아이템
    Equip, // 보유 + 장착한 아이템
}

public class ThemeTable : ClientTable
{
    public int themeIdx;
    public int themeSort;
    public ThemeUnlockType unlockType;
    public string resourceName;
}

public enum EquipmentSlot
{
    None, // 예외처리 용도
    Theme, // 테마
}

public class HaveItemInfo
{
    public ItemState State;
    public int Index;
    public int HaveCount;
    
    public ClientTable Table;

    public int SortValue; // 정렬 값

    public T GetTable<T>() where T : ClientTable
    {
        return Table as T;
    }

    public ItemTable GetItemTable()
    {
        return GetTable<ItemTable>();
    }
}

// _SJ      아이템 관리자
public class ItemManager : SingletonTemplate<ItemManager>
{
    private Dictionary<int, ItemTable> itemTableMap = new Dictionary<int, ItemTable>();
    private Dictionary<int, ThemeTable> themeTableMap = new Dictionary<int, ThemeTable>();
    
    private Dictionary<FunctionItemType, int> functionItemKeyMap = new Dictionary<FunctionItemType, int>();
    
    private Dictionary<int, int> haveItemMap = new Dictionary<int, int>();
    private Dictionary<EquipmentSlot, int> equipmentMap = new Dictionary<EquipmentSlot, int>();
    private Dictionary<ItemType, EquipmentSlot> itemTypeSlotKeyMap = new Dictionary<ItemType, EquipmentSlot>();
    
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
        // 아이템 타입 - 슬롯 번호 값 작성
        itemTypeSlotKeyMap.Clear();
        itemTypeSlotKeyMap.Add(ItemType.THEME, EquipmentSlot.Theme);
        
        Dictionary<int, ItemTable> defaultHaveItemMap = InitItemTable();
        InitThemeTable();
        InitItemBySaveData(defaultHaveItemMap);
    }

    #region NormalItem
    private Dictionary<int, ItemTable> InitItemTable()
    {
        Dictionary<int, ItemTable> defaultHaveItemMap = new Dictionary<int, ItemTable>();
        List<ItemTable> tableList = ClientTableManager.LoadTable<ItemTable>("Item");
        itemTableMap.Clear();
        functionItemKeyMap.Clear();
        if (tableList == null)
        {
            return defaultHaveItemMap;
        }

        for (int i = 0; i < tableList.Count; i++)
        {
            ItemTable table = tableList[i];
            if (!itemTableMap.ContainsKey(table.itemIdx))
            {
                itemTableMap.Add(table.itemIdx, table);
                if (table.defaultHaveCount > 0)
                {
                    defaultHaveItemMap.Add(table.itemIdx, table);
                }
                
                FunctionItemType functionType = ToFunctionItemType(table.itemType);
                if (functionType != FunctionItemType.None)
                {
                    if (!functionItemKeyMap.ContainsKey(functionType))
                    {
                        functionItemKeyMap.Add(functionType, table.itemIdx);
                    }
                }
            }
        }

        return defaultHaveItemMap;
    }

    public ItemTable GetItemTable(int index)
    {
        if (itemTableMap.ContainsKey(index))
        {
            return itemTableMap[index];
        }

        return null;
    }

    public List<ItemTable> GetItemTableList()
    {
        return itemTableMap.Values.ToList();
    }

    private void InitItemBySaveData(Dictionary<int, ItemTable> defaultHaveItemMap)
    {
        haveItemMap.Clear();
        int i;
        
        bool isValidSaveData = SaveManager.Instance.IsValidSaveData();
        if (!isValidSaveData)
        {
            // 세이브데이터가 없어서 아이템 정보 새로 작성
            List<int> indexList = defaultHaveItemMap.Keys.ToList();
            for (i = 0; i < indexList.Count; i++)
            {
                int key = indexList[i];
                ItemTable table = defaultHaveItemMap[key];
                SaveManager.Instance.SetValue(SaveKind.Item, key.ToString(), table.defaultHaveCount.ToString());

                // 기본 아이템 장착처리
                EquipmentSlot slot = ItemTypeToEquipmentSlot(table.itemType);
                if (slot != EquipmentSlot.None)
                {
                    EquipItem(slot, key);
                }
            }
            
            SaveHaveItem(); // 세이브 데이터 작성해둠
        }

        List<string> haveItemList = SaveManager.Instance.GetValueKeyList(SaveKind.Item);
        if (haveItemList != null)
        {
            for (i = 0; i < haveItemList.Count; i++)
            {
                string key = haveItemList[i];
                int haveCount = SaveManager.Instance.GetIntValue(SaveKind.Item, key);
                haveItemMap.Add(Convert.ToInt32(key), haveCount);
            }
        }

        List<string> equipList = SaveManager.Instance.GetValueKeyList(SaveKind.Equipment);
        if (equipList != null)
        {
            for (i = 0; i < equipList.Count; i++)
            {
                string key = equipList[i];
                EquipmentSlot slot = Enum.Parse<EquipmentSlot>(key);
                int equipItemIndex = SaveManager.Instance.GetIntValue(SaveKind.Equipment, key);
                if (equipmentMap.ContainsKey(slot))
                {
                    equipmentMap[slot] = equipItemIndex;
                }
                else
                {
                    equipmentMap.Add(slot, equipItemIndex);
                }
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
        if (count < 0)
        {
            count = 0;
        }
        
        if (haveItemMap.ContainsKey(itemIndex))
        {
            haveItemMap[itemIndex] = count;
        }
        else
        {
            haveItemMap.Add(itemIndex, count);
        }
        
        SaveManager.Instance.SetValue(SaveKind.Item, itemIndex.ToString(), count.ToString());
    }

    public void AddHaveItemCount(int itemIndex, int addValue = 1)
    {
        int newCount = 0;
        if (haveItemMap.ContainsKey(itemIndex))
        {
            newCount = haveItemMap[itemIndex] + addValue;
            if (newCount < 0)
            {
                newCount = 0;
            }
            
            haveItemMap[itemIndex] = newCount;
        }
        else
        {
            newCount = addValue;
            haveItemMap.Add(itemIndex, addValue);
        }
        
        SaveManager.Instance.SetValue(SaveKind.Item, itemIndex.ToString(), newCount.ToString());
    }

    public void SaveHaveItem()
    {
        SaveManager.Instance.SaveFile(SaveKind.Item);
    }
    #endregion

    #region FunctionItem
    private FunctionItemType ToFunctionItemType(ItemType itemType)
    {
        switch (itemType)
        {
            case ItemType.HINT:
                return FunctionItemType.Hint;
            case ItemType.ADREMOVE:
                return FunctionItemType.ADRemover;
        }

        return FunctionItemType.None;
    }
    
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
            SaveHaveItem();
            return true;
        }

        return false;
    }

    public void AddFunctionItem(FunctionItemType itemType, int addCount = 1)
    {
        int itemIndex = GetFunctionTypeToItemIndex(itemType);
        AddHaveItemCount(itemIndex, addCount);
    }
    #endregion

    #region HaveItem
    private void SetHaveItemInfo(int index, ref HaveItemInfo haveItem, ClientTable table = null)
    {
        haveItem.Index = index;
        haveItem.Table = table;

        ItemTable itemTable = GetItemTable(index);
        if (itemTable == null)
        {
            haveItem.HaveCount = 0;
            haveItem.State = ItemState.InvalidItem;
            return;
        }

        if (table == null)
        {
            haveItem.Table = itemTable;
        }
        
        haveItem.HaveCount = GetHaveItemCount(index);
        if (haveItem.HaveCount < 1)
        {
            haveItem.State = ItemState.NoHave;
            return;
        }

        bool isEquippedItem = IsEquippedItem(index, itemTable);
        haveItem.State = isEquippedItem ? ItemState.Equip : ItemState.Have;
    }
    
    public HaveItemInfo MakeHaveItemInfo(int index, ClientTable table = null)
    {
        HaveItemInfo haveItem = new HaveItemInfo();
        SetHaveItemInfo(index, ref haveItem, table);
        return haveItem;
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
            if (!themeTableMap.ContainsKey(table.themeIdx))
            {
                themeTableMap.Add(table.themeIdx, table);
            }
        }
    }

    public List<ThemeTable> GetThemeTableList()
    {
        return themeTableMap.Values.ToList();
    }

    public ThemeTable GetThemeTable(int index)
    {
        if (themeTableMap.ContainsKey(index))
        {
            return themeTableMap[index];
        }

        return null;
    }

    public string GetNowEquippedThemeName()
    {
        int index = GetEquippedItemIndex(EquipmentSlot.Theme);
        ThemeTable theme = GetThemeTable(index);
        if (theme != null)
        {
            return theme.resourceName;
        }

        return "Normal"; // 만~약을 대비한 예외처리
    }
    #endregion

    #region Equipment
    public void EquipItem(EquipmentSlot slot, int index)
    {
        if (equipmentMap.ContainsKey(slot))
        {
            equipmentMap[slot] = index;
        }
        else
        {
            equipmentMap.Add(slot, index);
        }
        
        SaveManager.Instance.SetValue(SaveKind.Equipment, slot.ToString(), index.ToString());
        SaveManager.Instance.SaveFile(SaveKind.Equipment); // 바로 저장
    }

    public void EquipItem(int index)
    {
        EquipItem(index, GetItemTable(index));
    }

    public void EquipItem(int index, ItemTable item)
    {
        if (item == null)
        {
            return;
        }

        EquipmentSlot slot = ItemTypeToEquipmentSlot(item.itemType);
        if (slot != EquipmentSlot.None)
        {
            EquipItem(slot, index);
        }
    }

    public EquipmentSlot ItemTypeToEquipmentSlot(ItemType itemType)
    {
        if (!itemTypeSlotKeyMap.ContainsKey(itemType))
        {
            return EquipmentSlot.None;
        }

        return itemTypeSlotKeyMap[itemType];
    }

    public int GetEquippedItemIndex(EquipmentSlot slot)
    {
        if (!equipmentMap.ContainsKey(slot))
        {
            return 0;
        }

        return equipmentMap[slot];
    }

    public HaveItemInfo GetEquippedItem(EquipmentSlot slot)
    {
        return MakeHaveItemInfo(GetEquippedItemIndex(slot));
    }

    public bool IsEquippedSlot(EquipmentSlot slot)
    {
        if (!equipmentMap.ContainsKey(slot))
        {
            return false;
        }

        int index = equipmentMap[slot];
        return itemTableMap.ContainsKey(index);
    }

    public bool IsEquippedItem(int index)
    {
        return IsEquippedItem(index, GetItemTable(index));
    }

    public bool IsEquippedItem(int index, ItemTable itemTable)
    {
        if (itemTable == null)
        {
            return false;
        }

        EquipmentSlot slot = ItemTypeToEquipmentSlot(itemTable.itemType);
        if (slot == EquipmentSlot.None)
        {
            return false;
        }
        
        int equippedItem = GetEquippedItemIndex(slot);
        return (index == equippedItem);
    }
    #endregion

    #region Ad
    // 광고가 활성화 되어있는가 여부
    public bool IsActivateAD()
    {
        return IsPossibleUseFunctionItem(FunctionItemType.ADRemover);
    }
    #endregion
}