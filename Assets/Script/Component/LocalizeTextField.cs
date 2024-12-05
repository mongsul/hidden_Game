using System;
using System.Collections.Generic;
using System.Linq;
using Core.Library;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace UI.Common
{
    public class LocalizeTextField : MonoBehaviour
    {
        [Serializable]
        public class LocalizeInfo
        {
            [FormerlySerializedAs("LocalizeKey")] [SerializeField] public string localizeKey;
            [FormerlySerializedAs("ContentsList")] [SerializeField] public List<string> contentsList;

            public void SetContent(string content)
            {
                if (contentsList == null)
                {
                    contentsList = new List<string>();
                }

                if (contentsList.Count > 0)
                {
                    contentsList[0] = content;
                }
                else
                {
                    contentsList.Add(content);
                }
            }

            public void ClearContent()
            {
                if (contentsList != null)
                {
                    contentsList.Clear();
                }
            }
        }
    
        [FormerlySerializedAs("TextField")] [SerializeField] private TMP_Text textField;
        [FormerlySerializedAs("Localize")] [SerializeField] private LocalizeInfo localize;
        [SerializeField] private bool isApplyLocalize = true;
    
        // Start is called before the first frame update
        void Start()
        {
            if (Application.isPlaying)
            {
                ClientTableManager.Instance.AddChangeLanguageCodeEvent(RefreshLocalize);
            }
            
            InitThis();
        }

        /*
        // Update is called once per frame
        void Update()
        {
        }*/

        private void OnDestroy()
        {
            if (Application.isPlaying)
            {
                ClientTableManager.Instance.RemoveChangeLanguageCodeEvent(RefreshLocalize);
            }
        }

        private void OnEnable()
        {
            RefreshLocalize();
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
            localize.SetContent(content);
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
            if (!textField)
            {
                if (gameObject)
                {
                    textField = gameObject.GetComponent<TMP_Text>();
                }
            }
            
            localize = localizeInfo;
        
            if (textField)
            {
                string localizeString = GetFormatStringByLocalizeInfo(localize, isApplyLocalize ? Application.isPlaying : false);
                
                //CodeUtilLibrary.SetColorLog($"SetText : {localizeString}", "lime");
                if (!string.IsNullOrEmpty(localizeString))
                {
                    textField.SetText(localizeString);
                }
            }
        }

        public void RefreshLocalize()
        {
            SetText(localize);
        }

        #region Static
        public static string GetFormatStringByLocalizeInfo(LocalizeInfo localize, bool isLocalize = false)
        {
            if (localize == null)
            {
                return "";
            }

            if (string.IsNullOrEmpty(localize.localizeKey))
            {
                return "";
            }
                
            string localizeStr = localize.localizeKey;
            if (isLocalize)
            {
                if (!ClientTableManager.Instance.IsValidLanguageValue(localizeStr))
                {
                    return localizeStr;
                }
                
                localizeStr = ClientTableManager.Instance.GetLanguageValue(localizeStr);

                if (string.IsNullOrEmpty(localizeStr))
                {
                    return localize.localizeKey;
                }
            }

            int contentsCount = 0;
            if (localize.contentsList != null)
            {
                contentsCount = localize.contentsList.Count;
            }
                
            if (contentsCount > 0)
            {
                string[] split = localizeStr.Split('}');
                int cutCount = (split == null) ? 0 : split.Length;
//#if UNITY_EDITOR
                //Debug.Log($"SetText - localizeStr[{localizeStr}], contentsCount[{contentsCount}], cutStrCount[{cutStr.Count}]");
//#endif
                if (contentsCount < cutCount - 1)
                {
                    int endCount = cutCount - 1;
                    for (int i = contentsCount; i < endCount; i++)
                    {
                        localize.contentsList.Add(""); // 빈칸으로 메워둔다.
                    }
                }
                //Debug.Log($"SetText - contentsCount[{localize.contentsList.Count}]");
                    
                object[] args = localize.contentsList.Cast<object>().ToArray();
                return string.Format(localizeStr, args);
            }

            return localizeStr;
        }
        #endregion
    }
}