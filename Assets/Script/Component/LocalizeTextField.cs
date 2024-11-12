using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace UI.Common
{
    public class LocalizeTextField : MonoBehaviour
    {
        [Serializable]
        public struct LocalizeInfo
        {
            [FormerlySerializedAs("LocalizeKey")] [SerializeField] public string localizeKey;
            [FormerlySerializedAs("ContentsList")] [SerializeField] public List<string> contentsList;
        }
    
        [FormerlySerializedAs("TextField")] [SerializeField] private TMP_Text textField;

        [FormerlySerializedAs("Localize")] [SerializeField] private LocalizeInfo localize;

        private TMP_Text fieldValue;
    
        // Start is called before the first frame update
        void Start()
        {
            ClientTableManager.Instance.AddChangeLanguageCodeEvent(RefreshLocalize);
            InitThis();
        }

        /*
        // Update is called once per frame
        void Update()
        {
        }*/

        private void OnDestroy()
        {
            ClientTableManager.Instance.RemoveChangeLanguageCodeEvent(RefreshLocalize);
        }

#if UNITY_EDITOR
        protected void OnValidate()
        {
            //base.OnValidate();
        
            //InitThis();
        
            // 텍스트만 재처리
            if (textField/* && (!string.IsNullOrEmpty(localize.localizeKey))*/)
            {
                textField.SetText(localize.localizeKey);
            }
        }
    
        protected void Reset()
        {
            //base.Reset();
        }
#endif
    
        private void InitThis()
        {
            RefreshLocalize();
        }

        public void SetText(string text)
        {
            localize.localizeKey = text;
            SetText(localize);
        }

        public void SetContents(string content)
        {
            localize.contentsList = new List<string>();
            localize.contentsList.Add(content);
            SetText(localize);
        }

        public void SetContents(List<string> contents)
        {
            localize.contentsList = contents;
            SetText(localize);
        }

        public void SetContents(List<float> contents)
        {
            if (contents == null)
            {
                return;
            }
            
            localize.contentsList = new List<string>();
            for (int i = 0; i < contents.Count; i++)
            {
                localize.contentsList.Add(contents[i].ToString());
            }
            
            SetText(localize);
        }

        public void SetText(LocalizeInfo localizeInfo)
        {
            if (!fieldValue)
            {
                if (textField)
                {
                    fieldValue = textField;
                }
                else if (gameObject)
                {
                    fieldValue = gameObject.GetComponent<TMP_Text>();
                }
            }
            
            localize = localizeInfo;
        
            if (fieldValue)
            {
                fieldValue.SetText(GetLocalilzeString(localize));
            }
        }

        public void RefreshLocalize()
        {
            SetText(localize);
        }

        #region Static
        public static string GetLocalilzeString(LocalizeInfo localize)
        {
            string text = "";
            string localeText = "";
            bool isExecuteLocalize = true;
            
            if (localize.localizeKey != "")
            {
                localeText = ClientTableManager.Instance.GetLanguageValue(localize.localizeKey);
            }
            if ((localeText == "") || (localeText == "none"))
            {
                isExecuteLocalize = false;
                text = localize.localizeKey;
            }

            if (isExecuteLocalize)
            {
                int contentsCount = 0;
                if (localize.contentsList != null)
                {
                    contentsCount = localize.contentsList.Count;
                }
                if (contentsCount > 0)
                {
                    string localizeStr = ClientTableManager.Instance.GetLanguageValue(localize.localizeKey);
                    List<string> cutStr = new List<string>(localizeStr.Split('}'));
#if UNITY_EDITOR
                    //Debug.Log($"SetText - localizeStr[{localizeStr}], contentsCount[{contentsCount}], cutStrCount[{cutStr.Count}]");
#endif
                    if (contentsCount < cutStr.Count - 1)
                    {
                        int endCount = cutStr.Count - 1;
                        for (int i = contentsCount; i < endCount; i++)
                        {
                            localize.contentsList.Add(""); // 빈칸으로 메워둔다.
                        }
                    }
                    //Debug.Log($"SetText - contentsCount[{localize.contentsList.Count}]");
                    
                    object[] args = localize.contentsList.Cast<object>().ToArray();
                    text = string.Format(localizeStr, args);
                }
                else
                {
                    text = ClientTableManager.Instance.GetLanguageValue(localize.localizeKey);
                }
            }

            return text;
        }
        
        public static string GetFormatStringByLocalizeInfo(LocalizeInfo localize)
        {
            string text = "";
            string localeText = "";
            bool isExecuteLocalize = true;
            
            if (localize.localizeKey != "")
            {
                localeText = localize.localizeKey;
            }
            if ((localeText == "") || (localeText == "none"))
            {
                isExecuteLocalize = false;
                text = localize.localizeKey;
            }

            if (isExecuteLocalize)
            {
                int contentsCount = 0;
                if (localize.contentsList != null)
                {
                    contentsCount = localize.contentsList.Count;
                }
                if (contentsCount > 0)
                {
                    string localizeStr = localize.localizeKey;
                    List<string> cutStr = new List<string>(localizeStr.Split('}'));
#if UNITY_EDITOR
                    //Debug.Log($"SetText - localizeStr[{localizeStr}], contentsCount[{contentsCount}], cutStrCount[{cutStr.Count}]");
#endif
                    if (contentsCount < cutStr.Count - 1)
                    {
                        int endCount = cutStr.Count - 1;
                        for (int i = contentsCount; i < endCount; i++)
                        {
                            localize.contentsList.Add(""); // 빈칸으로 메워둔다.
                        }
                    }
                    //Debug.Log($"SetText - contentsCount[{localize.contentsList.Count}]");
                    
                    object[] args = localize.contentsList.Cast<object>().ToArray();
                    text = string.Format(localizeStr, args);
                }
                else
                {
                    text = localize.localizeKey;
                }
            }

            return text;
        }
        #endregion
    }
}