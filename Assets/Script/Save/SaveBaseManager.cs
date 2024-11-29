using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class StringMapGroup
{
    private Dictionary<string, string> stringMap;

    public void SetMap(Dictionary<string, string> inStringMap)
    {
        stringMap = inStringMap;
    }

    public Dictionary<string, string> GetMap()
    {
        return stringMap;
    }

    public string GetValue(string key, string defaultValue = "")
    {
        if (stringMap == null)
        {
            return defaultValue;
        }

        if (stringMap.ContainsKey(key))
        {
            return stringMap[key];
        }

        return defaultValue;
    }

    public void SetValue(string key, string value)
    {
        if (stringMap == null)
        {
            stringMap = new Dictionary<string, string>();
        }

        if (stringMap.ContainsKey(key))
        {
            stringMap[key] = value;
        }
        else
        {
            stringMap.Add(key, value);
        }
    }
}

[System.Serializable]
public class StringMapSaveClass
{
    public string[] stringMapKeyList;
    public string[] stringMapValueList;

    public Dictionary<string, string> ToMap()
    {
        int keyCount = (stringMapKeyList != null) ? stringMapKeyList.Length : 0;
        int valueCount = (stringMapValueList != null) ? stringMapValueList.Length : 0;
        int endCount = Math.Min(keyCount, valueCount);
        Dictionary<string, string> dataMap = new Dictionary<string, string>();
        for (int i = 0; i < endCount; i++)
        {
            string key = stringMapKeyList[i];
            if (!dataMap.ContainsKey(key))
            {
                dataMap.Add(key, stringMapValueList[i]);
            }
        }

        return dataMap;
    }

    public void ByMap(Dictionary<string, string> dataMap)
    {
        stringMapKeyList = dataMap.Keys.ToArray();
        stringMapValueList = dataMap.Values.ToArray();
    }
}

// 세이브 종류 (프로젝트에 따라서 이걸 바꿔야 할 수도 있음)
public enum SaveKind
{
    None = 0, // 예외처리 용도, 건드리지 말것
    Option,
    Record,
    Item,
    Equipment,
    Max, // 최대 개수 체크 용도, 건드리지 말것
}

// _SJ      저장 기본 관리자 (얘는 최대한 복붙만해도 사용 가능하도록 구성)
public class SaveBaseManager : SingletonTemplate<SaveManager>
{
    /*
    // Start is called before the first frame update
    void Start()
    {   
    }

    // Update is called once per frame
    void Update()
    {
    }*/

    // 키 값은 저장 파일 명
    private Dictionary<string, StringMapGroup> saveGroup = new Dictionary<string, StringMapGroup>();

    #region Save
    private static string GetSaveFolder()
    {
#if UNITY_EDITOR
        return Application.dataPath + "/Save";
#else
        return Application.persistentDataPath + "/Save";
#endif
    }
    
    private static string GetSavePath(string fileName)
    {
        return GetSaveFolder() + $"/{fileName}.json";
    }

    private bool GetSaveMapGroup(string fileName, ref StringMapGroup saveMap)
    {
        if (saveGroup == null)
        {
            return false;
        }

        if (saveGroup.ContainsKey(fileName))
        {
            saveMap = saveGroup[fileName];
            return true;
        }

        return false;
    }

    private void SaveFile(string fileName)
    {
        StringMapGroup saveMap = new StringMapGroup();
        if (!GetSaveMapGroup(fileName, ref saveMap))
        {
            return;
        }

        StringMapSaveClass saveClass = new StringMapSaveClass();
        saveClass.ByMap(saveMap.GetMap());
        
        string json = JsonUtility.ToJson(saveClass, true);
        string folder = GetSaveFolder();

        if (!System.IO.Directory.Exists(folder))
        {
            System.IO.Directory.CreateDirectory(folder);
        }
        
        StreamWriter wr = new StreamWriter(GetSavePath(fileName), false);
        wr.WriteLine(json);
        wr.Close();
    }

    public void SaveFile(SaveKind kind)
    {
        SaveFile(kind.ToString());
    }

    private void Load(string fileName)
    {
        string savePath = GetSavePath(fileName);
        if (!File.Exists(savePath))
        {
            return;
        }

        //string json = File.ReadAllText(savePath);
        StreamReader rd = new StreamReader(savePath);
        string json = rd.ReadToEnd();
        StringMapSaveClass saveClass = JsonUtility.FromJson<StringMapSaveClass>(json);
        StringMapGroup mapGroup = new StringMapGroup();
        mapGroup.SetMap(saveClass.ToMap());
        saveGroup.Add(fileName, mapGroup);
    }

    private void Load(SaveKind kind)
    {
        Load(kind.ToString());
    }

    public virtual void Load()
    {
        int startIndex = (int)SaveKind.None + 1;
        int endIndex = (int)SaveKind.Max;
        saveGroup = new Dictionary<string, StringMapGroup>();
        for (int i = startIndex; i < endIndex; i++)
        {
            Load((SaveKind)i);
        }

        /* // enum값 없이 한꺼번에 가져올 수 있는 방법 필요..
        string saveFolder = GetSaveFolder();
        saveGroup = new Dictionary<string, StringMapGroup>();
        if (!File.Exists(saveFolder))
        {
            return;
        }

        //string[] savedFileList =  Resources.LoadAll<string>(saveFolder);
        */
    }
    #endregion

    #region Valid
    public bool IsValidSaveData()
    {
        if (saveGroup == null)
        {
            return false;
        }

        return saveGroup.Count > 0;
    }
    
    private bool IsValidSaveData(string fileName)
    {
        if (saveGroup == null)
        {
            return false;
        }

        if (saveGroup.ContainsKey(fileName))
        {
            return true;
        }

        return false;
    }

    public bool IsValidSaveData(SaveKind kind)
    {
        return IsValidSaveData(kind);
    }
    #endregion

    #region Value
    private string GetValue(string fileName, string key, string defaultValue = "")
    {
        StringMapGroup saveMap = new StringMapGroup();
        if (GetSaveMapGroup(fileName, ref saveMap))
        {
            return saveMap.GetValue(key, defaultValue);
        }

        return defaultValue;
    }

    private void SetValue(string fileName, string key, string value)
    {
        StringMapGroup saveMap = new StringMapGroup();
        if (!GetSaveMapGroup(fileName, ref saveMap))
        {
            if (saveGroup == null)
            {
                saveGroup = new Dictionary<string, StringMapGroup>();
            }
            
            saveMap.SetValue(key, value);
            saveGroup.Add(fileName, saveMap);
        }
        else
        {
            saveMap.SetValue(key, value);
            saveGroup[fileName] = saveMap;
        }
    }

    public string GetValue(SaveKind kind, string key, string defaultValue = "")
    {
        return GetValue(kind.ToString(), key, defaultValue);
    }

    public void SetValue(SaveKind kind, string key, string value)
    {
        SetValue(kind.ToString(), key, value);
    }

    public Dictionary<string, string> GetValueMap(string fileName)
    {
        StringMapGroup saveMap = new StringMapGroup();
        if (!GetSaveMapGroup(fileName, ref saveMap))
        {
            return null;
        }

        return saveMap.GetMap();
    }

    private Dictionary<string, string> GetValueMap(SaveKind kind)
    {
        return GetValueMap(kind.ToString());
    }

    public List<string> GetValueKeyList(SaveKind kind)
    {
        Dictionary<string, string> valueMap = GetValueMap(kind);
        if (valueMap == null)
        {
            return null;
        }

        return valueMap.Keys.ToList();
    }
    #endregion

    #region ValueApply
    public int GetIntValue(SaveKind kind, string key, int defaultValue = 0)
    {
        return Convert.ToInt32(GetValue(kind, key, defaultValue.ToString()));
    }

    public bool GetBoolValue(SaveKind kind, string key, bool defaultValue = false)
    {
        return Convert.ToBoolean(GetValue(kind, key, defaultValue.ToString()));
    }

    public float GetFloatValue(SaveKind kind, string key, float defaultValue = 0)
    {
        return (float)Convert.ToDouble(GetValue(kind, key, defaultValue.ToString()));
    }
    #endregion
}