using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class OptionPopup : MonoBehaviour
{
    private List<OptionSetterToggle> toggleList;
    
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
        if (toggleList == null)
        {
            toggleList = gameObject.GetComponentsInChildren<OptionSetterToggle>().ToList();
        }
        
        for (int i = 0; i < toggleList.Count; i++)
        {
            OptionSetterToggle toggle = toggleList[i];
            if (toggle)
            {
                toggle.InitThis();
            }
        }
    }

    private void SaveOption()
    {
        if (toggleList == null)
        {
            return;
        }
        
        for (int i = 0; i < toggleList.Count; i++)
        {
            OptionSetterToggle toggle = toggleList[i];
            if (toggle)
            {
                toggle.ValueToSaveData();
            }
        }
        
        SaveManager.Instance.Save();
    }

    private void CancelOption()
    {
        if (toggleList == null)
        {
            return;
        }
        
        for (int i = 0; i < toggleList.Count; i++)
        {
            OptionSetterToggle toggle = toggleList[i];
            if (toggle)
            {
                toggle.InitThis();
                toggle.SetApplyOption();
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
    #endregion
}