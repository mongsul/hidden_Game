using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[System.Serializable]
public class ClientTable
{
}

[System.Serializable]
class ClientBaseTable : ClientTable
{
    public string rowValue;
    public string value;
}

public enum LanguageCode
{
    ko = 0, // 한국어
    en, // 영어
    jp, // 일본어
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
    
    public void InitTable()
    {
        if (isInitTable)
        {
            return;
        }
        
        InitBaseTable();
        InitLangTable();

        isInitTable = true;
    }

    private void InitBaseTable()
    {
        List<ClientBaseTable> baseTableList = LoadTable<ClientBaseTable>("ClientBaseOption");
        if (baseTableList == null)
        {
            return;
        }

        if (baseTableList == null)
        {
            return;
        }

        for (int i = 0; i < baseTableList.Count; i++)
        {
            ClientBaseTable table = baseTableList[i];
            simpleClientBaseTable.Add(table.rowValue, table.value);
        }
    }

    private void InitLangTable()
    {
        /*
        List<MetaLanguage> langTableList = LoadTable<MetaLanguage>("Language");
        for (int i = 0; i < langTableList.Count; i++)
        {
            MetaLanguage lang = langTableList[i];
            string key = lang.recordCd;
            if (!string.IsNullOrEmpty(key))
            {
                AddLanguageValue(LanguageType.KR, key, lang.kr);
                AddLanguageValue(LanguageType.EN, key, lang.en);
                AddLanguageValue(LanguageType.JP, key, lang.jp);
                AddLanguageValue(LanguageType.CH, key, lang.zhCn);
                AddLanguageValue(LanguageType.TW, key, lang.zhTw);
            }
        }*/
        
        /*ClientBaseTable langTable = LoadTable<ClientBaseTable>("Language");
        if (langTable == null)
        {
            return;
        }*/
    }
    #endregion

    #region Base
    public string GetBaseValue(string key)
    {
        //InitTable();
        
        if (simpleClientBaseTable.ContainsKey(key))
        {
            return simpleClientBaseTable[key];
        }

        return "";
    }
    #endregion

    #region Language
    public string GetLanguageValue(LanguageCode lang, string key)
    {
        //InitTable();

        /*
        if (clientLanguageTable.ContainsKey(lang))
        {
            if (clientLanguageTable[lang].ContainsKey(key))
            {
                return clientLanguageTable[lang][key];
            }
        }*/

        return "";
    }

    public string GetLanguageValue(string key)
    {
        return "";
    }
    #endregion
}