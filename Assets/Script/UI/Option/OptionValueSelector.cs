using System;
using System.Collections;
using System.Collections.Generic;
using UI.Common;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class OptionValueSelector : MonoBehaviour, IOptionValueConnector
{
    [FormerlySerializedAs("Title")] [SerializeField] private LocalizeTextField title;

    [FormerlySerializedAs("DisplayValueList")] [SerializeField] private List<string> displayValueList;
    [FormerlySerializedAs("ValueList")] [SerializeField] private List<string> valueList;
    private int nowSelectIndex = 0;
    
    [Serializable]
    public class ValueSelectorEvent : UnityEvent<OptionValueSelector>{}
    
    [Serializable]
    public class ValueSelectorValueEvent : UnityEvent<string>{}

    [FormerlySerializedAs("OnInit")]
    [SerializeField]
    public ValueSelectorEvent mOnInit = new ValueSelectorEvent();

    [FormerlySerializedAs("OnChangeValue")]
    [SerializeField]
    public ValueSelectorValueEvent mOnChangeValue = new ValueSelectorValueEvent();

    /*
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }*/

    private string GetSelectValue()
    {
        if ((nowSelectIndex < 0) || (nowSelectIndex >= valueList.Count))
        {
            return "";
        }

        return valueList[nowSelectIndex];
    }

    private string GetSelectDisplayValue()
    {
        if ((nowSelectIndex < 0) || (nowSelectIndex >= displayValueList.Count))
        {
            return "";
        }

        return displayValueList[nowSelectIndex];
    }

    private void Refresh()
    {
        string selectValue = GetSelectDisplayValue();
        if (string.IsNullOrEmpty(selectValue))
        {
            selectValue = GetSelectValue();
        }

        if (title)
        {
            title.SetText(selectValue);
        }
    }

    public void SetDisplayValueList(List<string> displayList)
    {
        displayValueList = displayList;
    }

    public void SetValueList(List<string> inValueList, bool isAutoSetDisplayValue = true)
    {
        valueList = inValueList;
        if (valueList == null)
        {
            return;
        }

        displayValueList = new List<string>();
        for (int i = 0; i < valueList.Count; i++)
        {
            displayValueList.Add($"Option_{valueList[i]}");
        }
        
        Refresh();
    }

    public void SetSelectIndex(int index)
    {
        nowSelectIndex = index;
        Refresh();
    }

    public void SetSelect(string value)
    {
        int findIndex = valueList.FindIndex(x => x.Equals(value));
        if ((findIndex < 0) || (findIndex >= valueList.Count))
        {
            return;
        }
        
        SetSelectIndex(findIndex);
    }

    public void InitThis()
    {
        mOnInit?.Invoke(this);
    }
    
    public void ValueToSaveData()
    {
        string value = GetSelectValue();
        SaveManager.Instance.SetLanguageCode(value);
    }

    public void SetApplyOption()
    {
        string value = SaveManager.Instance.GetLanguageCode();
        SetSelect(value);
        mOnChangeValue?.Invoke(value);
    }

    public void OnClick_Prev()
    {
        nowSelectIndex--;
        if (nowSelectIndex < 0)
        {
            nowSelectIndex = valueList.Count - 1;
        }
        
        Refresh();

        string value = GetSelectValue();
        mOnChangeValue?.Invoke(value);
    }

    public void OnClick_Next()
    {
        nowSelectIndex++;
        if (nowSelectIndex >= valueList.Count)
        {
            nowSelectIndex = 0;
        }
        
        Refresh();

        string value = GetSelectValue();
        mOnChangeValue?.Invoke(value);
    }
}