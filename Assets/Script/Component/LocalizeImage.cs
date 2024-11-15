using System.Collections;
using System.Collections.Generic;
using Core.Library;
using TMPro;
using UI.Common;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class LocalizeImage : MonoBehaviour
{
    [FormerlySerializedAs("LocalizeImage")] [SerializeField] private Image localizeImage;

    [FormerlySerializedAs("FilePath")] [SerializeField] private string filePath;
    [FormerlySerializedAs("FileName")] [SerializeField] private string fileName;
    [FormerlySerializedAs("IsUpper")] [SerializeField] private bool isUpper;

    private LocalizeTextField.LocalizeInfo localizeInfo;
    
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
    
    private void InitThis()
    {
        RefreshLocalize();
    }

    public void SetText(string text)
    {
        localizeInfo.localizeKey = text;
        SetText(localizeInfo);
    }

    public void SetText(LocalizeTextField.LocalizeInfo localize)
    {
        if (!localizeImage)
        {
            localizeImage = gameObject.GetComponent<Image>();
        }

        if (localizeImage)
        {
            string localizeName = LocalizeTextField.GetFormatStringByLocalizeInfo(localizeInfo);
            localizeImage.sprite = CodeUtilLibrary.LoadSprite(filePath, localizeName);
        }
    }

    public void RefreshLocalize()
    {
        if (localizeInfo == null)
        {
            localizeInfo = new LocalizeTextField.LocalizeInfo();
            localizeInfo.contentsList = new List<string>();
        }

        localizeInfo.localizeKey = fileName;
        
        string langCode = SaveManager.Instance.GetLanguageCode();
        if (isUpper)
        {
            langCode = langCode.ToUpper();
        }

        if (localizeInfo.contentsList.Count > 0)
        {
            localizeInfo.contentsList[0] = langCode;
        }
        else
        {
            localizeInfo.contentsList.Add(langCode);
        }
        
        SetText(localizeInfo);
    }
}