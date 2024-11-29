using System;
using System.Collections;
using System.Collections.Generic;
using UI.Common.Base;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class OptionSetterToggle : MonoBehaviour, IOptionValueConnector
{
    [FormerlySerializedAs("SaveKey")] [SerializeField]
    private string saveKey;

    [FormerlySerializedAs("OnButton")] [SerializeField] private OptionBaseButton onButton;
    [FormerlySerializedAs("OffButton")] [SerializeField] private OptionBaseButton offButton;

    [FormerlySerializedAs("IsUseInLobby")] [SerializeField] private bool isUseInLobby;

    private bool isActivate;
    
    [Serializable]
    public class ToggleValueEvent : UnityEvent<bool> { }

    // Event delegates change toggle.
    [FormerlySerializedAs("OnValueChangeEvent")]
    [SerializeField]
    public ToggleValueEvent mOnValueChange;

    /*
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }*/

    public void InitThis()
    {
        bool optionValue = Convert.ToBoolean(SaveManager.Instance.GetOptionValue(saveKey));
        SetValue(optionValue);
    }

    public string GetOptionSaveKey()
    {
        return saveKey;
    }
    
    public void SetValue(bool boolValue)
    {
        isActivate = boolValue;

        if (onButton)
        {
            onButton.SetActivate(isActivate);
        }

        if (offButton)
        {
            offButton.SetActivate(!isActivate);
        }
    }
    
    public bool GetValue()
    {
        return isActivate;
    }

    public void SetActivate()
    {
        SetValue(true);
        SetApplyOption(true);
    }

    public void SetDeactivate()
    {
        SetValue(false);
        SetApplyOption(false);
    }

    public void SetApplyOption()
    {
        SetApplyOption(isActivate);
    }

    public void SetApplyOption(bool value)
    {
        mOnValueChange?.Invoke(value);
    }

    public void ValueToSaveData()
    {
        SaveManager.Instance.SetOptionValue(GetOptionSaveKey(), GetValue().ToString());
    }

    public bool IsUseLobby()
    {
        return isUseInLobby;
    }
}