using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Library;
using TMPro;
using UnityEngine;

public class LocalizeFontGroup : MonoBehaviour
{
    [Serializable]
    private class LocalizeFontBase
    {
        [SerializeField] public TMP_FontAsset UseFont;
        [SerializeField] public Material FontMaterial;
    }
    
    [Serializable]
    private class LocalizeFont : LocalizeFontBase
    {
        [SerializeField] public string LocalizeCode;
    }

    [SerializeField] private List<LocalizeFont> fontList;
    [SerializeField] private LocalizeFontBase defaultFont;
    
    // Start is called before the first frame update
    void Start()
    {
        if (Application.isPlaying)
        {
            ClientTableManager.Instance.AddChangeLanguageCodeEvent(RefreshLocalize);
            RefreshLocalize();
        }
    }

    /*
    // Update is called once per frame
    void Update()
    {
    }*/
    
    public void RefreshLocalize()
    {
        SetLocalizeCode(ClientTableManager.Instance.GetNowLanguageCode());
    }

    private LocalizeFontBase GetFont(string code)
    {
        for (int i = 0; i < fontList.Count; i++)
        {
            if (fontList[i].LocalizeCode.Equals(code))
            {
                return fontList[i];
            }
        }
        
        return defaultFont;
    }

    private void SetLocalizeCode(string code)
    {
        TMP_Text[] textFieldList = CodeUtilLibrary.GetComponentsInActiveScene<TMP_Text>();
        if (textFieldList == null)
        {
            return;
        }

        LocalizeFontBase font = GetFont(code);
        if (font == null)
        {
            return;
        }

        for (int i = 0; i < textFieldList.Length; i++)
        {
            TMP_Text text = textFieldList[i]; 
            text.font = font.UseFont;
            text.material = font.FontMaterial;
            text.fontSharedMaterial = font.FontMaterial;
        }
    }
}