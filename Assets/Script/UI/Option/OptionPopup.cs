using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class OptionPopup : MonoBehaviour
{
    private List<IOptionValueConnector> valueConnectorList;
    
    /*
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }*/

    public void OnEnable()
    {
        LoadOption();
    }

    public void OnDisable()
    {
        SaveOption();
    }

    private void LoadOption()
    {
        if (valueConnectorList == null)
        {
            valueConnectorList = gameObject.GetComponentsInChildren<IOptionValueConnector>().ToList();
        }
        
        for (int i = 0; i < valueConnectorList.Count; i++)
        {
            IOptionValueConnector valueConnector = valueConnectorList[i];
            if (valueConnector != null)
            {
                valueConnector.InitThis();
            }
        }
    }

    private void SaveOption()
    {
        if (valueConnectorList == null)
        {
            return;
        }
        
        for (int i = 0; i < valueConnectorList.Count; i++)
        {
            IOptionValueConnector valueConnector = valueConnectorList[i];
            if (valueConnector != null)
            {
                valueConnector.ValueToSaveData();
            }
        }
        
        SaveManager.Instance.Save();
    }

    private void CancelOption()
    {
        if (valueConnectorList == null)
        {
            return;
        }
        
        for (int i = 0; i < valueConnectorList.Count; i++)
        {
            IOptionValueConnector valueConnector = valueConnectorList[i];
            if (valueConnector != null)
            {
                valueConnector.InitThis();
                valueConnector.SetApplyOption();
            }
        }
    }

    #region ApplyOption
    public void RefreshBGM(bool value)
    {
        SoundVolumeChanger.ApplySoundOptionToScene(true, value);
    }
    
    public void RefreshFX(bool value)
    {
        SoundVolumeChanger.ApplySoundOptionToScene(false, value);
    }

    public void RefreshVibration(bool value)
    {
        // 딱히 적용사항 없을듯?
    }

    public void RefreshLang(string value)
    {
        ClientTableManager.Instance.SetLanguageCode(value);
    }

    public void SetLanguageList(OptionValueSelector selector)
    {
        selector.SetValueList(ClientTableManager.Instance.GetLanguageCodeList());
        
        string value = SaveManager.Instance.GetLanguageCode();
        selector.SetSelect(value);
    }
    #endregion
}