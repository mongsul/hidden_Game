using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class ClientTable
{
}

[System.Serializable]
class ClientBaseTable : ClientTable
{
    public string rowValue;
    public string value;

    public bool IsValidValue()
    {
        return ((!string.IsNullOrEmpty(rowValue) && !string.IsNullOrEmpty(value)));
    }
}

public enum LanguageCode
{
    ko = 0, // 한국어
    en, // 영어
    ja, // 일본어
}

// _SJ      클라이언트 테이블 관리자. csv 테이블을 사용한다.
public class ClientTableManager : SingletonTemplate<ClientTableManager>
{
    //private const string DEFAULT_CLIENT_TABLE_PATH = "Assets/Resources/ClientTable/";
    
    private bool isInitTable = false; 
    
    // 단순 클라 기본 테이블 (문자열 키 - 문자열 값)
    private Dictionary<string, string> simpleClientBaseTable = new Dictionary<string, string>();
    
    // 언어 테이블 (언어 코드가 바뀌면 해당 테이블만 로드해서 값을 설정해준다.)
    private Dictionary<string, string> clientLanguageTable = new Dictionary<string, string>();
    private List<string> languageCodeList = new List<string>(); // 사용 가능한 언어 코드 목록
    private string nowLangCode; // 현재 사용중인 언어 코드
    
    public class LanguageEvent : UnityEvent{}

    [SerializeField]
    private LanguageEvent mOnChangeLanguage;
    
    #if UNITY_IOS
    private Dictionary<string, int> soundIDTable = new Dictionary<string, string>();
    #endif
    
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

    private void OnDestroy()
    {
        simpleClientBaseTable.Clear();
        clientLanguageTable.Clear();
        languageCodeList.Clear();
    }

    #region Init
    public static List<T> LoadTable<T>(string fileName) where T : ClientTable// : UnityEngine.ScriptableObject
    {
        //string fullPath = DEFAULT_CLIENT_TABLE_PATH + fileName + ".asset";
        //return AssetDatabase.LoadAssetAtPath<T>(fullPath);
        //string fullPath = DEFAULT_CLIENT_TABLE_PATH + fileName + ".csv";
        TextAsset data = ResourceManager.Instance.LoadText<TextAsset>(ResourcePath.ClientTable, fileName);//AssetDatabase.LoadAssetAtPath<TextAsset>(fullPath);
        if (data)
        {
            List<T> list = new List<T>(CSVSerializer.Deserialize<T>(data.text));
            return list;
        }

        return null;
    }

    public static List<T> LoadLanguageTable<T>(LanguageCode code) where T : ClientTable // : UnityEngine.ScriptableObject
    {
        return LoadLanguageTable<T>(code.ToString());
    }

    public static List<T> LoadLanguageTable<T>(string code) where T : ClientTable// : UnityEngine.ScriptableObject
    {
        TextAsset data = ResourceManager.Instance.LoadText<TextAsset>(ResourcePath.ClientLanguageTable, $"language_{code}");//AssetDatabase.LoadAssetAtPath<TextAsset>(fullPath);
        if (data)
        {
            List<T> list = new List<T>(CSVSerializer.Deserialize<T>(data.text));
            return list;
        }

        return null;
    }
    
    public void InitTable()
    {
        if (isInitTable)
        {
            return;
        }
        
        InitBaseTable();
        InitLangTable();
        InitIOSTable();

        isInitTable = true;
    }

    private void InitBaseTable()
    {
        List<ClientBaseTable> baseTableList = LoadTable<ClientBaseTable>("ClientBaseOption");
        simpleClientBaseTable.Clear();
        if (baseTableList == null)
        {
            return;
        }

        for (int i = 0; i < baseTableList.Count; i++)
        {
            ClientBaseTable table = baseTableList[i];
            if (!simpleClientBaseTable.ContainsKey(table.rowValue))
            {
                simpleClientBaseTable.Add(table.rowValue, table.value);
            }
        }
    }

    private void InitLangTable()
    {
        TextAsset[] langTableList = Resources.LoadAll<TextAsset>("ClientTable/Language");
        languageCodeList.Clear();
        mOnChangeLanguage = new LanguageEvent();
        if (langTableList != null)
        {
            for (int i = 0; i < langTableList.Length; i++)
            {
                TextAsset table = langTableList[i];
                if (table)
                {
                    string tableName = table.name;
                    string langCode = tableName.Substring(tableName.Length - 2, 2); // 끝의 2문자를 사용함
                    languageCodeList.Add(langCode);
                }
            }
        }

        string saveLangCode = SaveManager.Instance.GetLanguageCode();
        bool isDefaultCode = string.IsNullOrEmpty(saveLangCode);
        string initLangCode = isDefaultCode ? GetDefaultLanguageCode() : saveLangCode;
        SetLanguageCode(initLangCode, isDefaultCode);
    }
    #endregion

    #region Base
    public string GetBaseValue(string key, string defaultValue = "")
    {
        if (simpleClientBaseTable.ContainsKey(key))
        {
            return simpleClientBaseTable[key];
        }

        return "";
    }

    public int GetBaseIntValue(string key, int defaultValue = 0)
    {
        string value = GetBaseValue(key, defaultValue.ToString());
        return string.IsNullOrEmpty(value) ? defaultValue : Convert.ToInt32(value);
    }

    public float GetBaseFloatValue(string key, float defaultValue = 0.0f)
    {
        string value = GetBaseValue(key, defaultValue.ToString());
        return string.IsNullOrEmpty(value) ? defaultValue : (float)Convert.ToDouble(value);
    }

    public bool GetBaseBoolValue(string key, bool defaultValue = false)
    {
        string value = GetBaseValue(key, defaultValue.ToString());
        return string.IsNullOrEmpty(value) ? defaultValue : Convert.ToBoolean(value);
    }
    #endregion

    #region Language
    private string GetDefaultLanguageCode()
    {
        LanguageCode code = LanguageCode.en;
        switch (Application.systemLanguage)
        {
            case SystemLanguage.Korean:
                code = LanguageCode.ko;
                break;
            case SystemLanguage.Japanese:
                code = LanguageCode.ja;
                break;
        }
        
        return code.ToString();
    }
    
    private void SetLanguageCode(LanguageCode code, bool isDefaultCode = false)
    {
        SetLanguageCode(code.ToString(), isDefaultCode);
    }

    public void SetLanguageCode(string code, bool isDefaultCode = false)
    {
        // 언어 코드 값 없을 때 예외처리
        if (!languageCodeList.Contains(code))
        {
            if (isDefaultCode)
            {
                code = "en";
            }
            else
            {
                SetLanguageCode(GetDefaultLanguageCode(), true);
                return;
            }
        }
        
        SaveManager.Instance.SetLanguageCode(code);
        
        List<ClientBaseTable> baseTableList = LoadLanguageTable<ClientBaseTable>(code);
        clientLanguageTable.Clear();
        if (baseTableList != null)
        {
            for (int i = 0; i < baseTableList.Count; i++)
            {
                ClientBaseTable table = baseTableList[i];
                if (table.IsValidValue())
                {
                    if (!clientLanguageTable.ContainsKey(table.rowValue))
                    {
                        clientLanguageTable.Add(table.rowValue, table.value);
                    }
                }
            }
        }

        CallOnChangeLanguageCode();
    }
    
    public string GetLanguageValue(string key)
    {
        if (clientLanguageTable.ContainsKey(key))
        {
            return clientLanguageTable[key];
        }
        
        return "";
    }

    public List<string> GetLanguageCodeList()
    {
        return languageCodeList;
    }

    public void AddChangeLanguageCodeEvent(UnityAction localizeAction)
    {
        mOnChangeLanguage?.AddListener(localizeAction);
    }

    private void CallOnChangeLanguageCode()
    {
        mOnChangeLanguage?.Invoke();
    }

    public void RemoveChangeLanguageCodeEvent(UnityAction localizeAction)
    {
        mOnChangeLanguage?.RemoveListener(localizeAction);
    }
    #endregion

    #region IOSSound
    private void InitIOSTable()
    {
#if UNITY_IOS
        InitIOSSoundIDTable();
#endif
    }
    
    #if UNITY_IOS
    private void InitIOSSoundIDTable()
    {
        List<ClientBaseTable> iosSoundTableList = LoadTable<ClientBaseTable>("SoundForIOS");
        if (iosSoundTableList == null)
        {
            return;
        }

        for (int i = 0; i < iosSoundTableList.Count; i++)
        {
            ClientBaseTable table = iosSoundTableList[i];
            if (!soundIDTable.ContainsKey(table.rowValue))
            {
                soundIDTable.Add(table.rowValue, Convert.ToInt32(table.value));
            }
        }
    }
    #endif

    public int GetIOSSoundID(string soundName)
    {
#if UNITY_IOS
        if (soundIDTable.ContainsKey(soundName))
        {
            return soundIDTable[soundName];
        }
#endif
        return 0;
    }
    
    #endregion
}