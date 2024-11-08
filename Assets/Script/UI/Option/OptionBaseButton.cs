using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UI.Common;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class OptionBaseButton : UIBehaviour
{
    [FormerlySerializedAs("TitleLocalize")] [SerializeField] private LocalizeTextField.LocalizeInfo titleLocalize;

    [FormerlySerializedAs("OnOffSwitcher")] [SerializeField] private ObjectSwitcher onOffSwitcher;

    [FormerlySerializedAs("IsActivate")] [SerializeField] private bool isActivate;
    
    private List<LocalizeTextField> textFieldList;

    // Start is called before the first frame update
    protected override void Awake()
    {
        base.Awake();
        
        InitThis();
    }

    /*
    // Update is called once per frame
    void Update()
    {
    }*/
    
#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();

        InitThis();
    }

    /*
    protected override void Reset()
    {
        base.Reset();
    }*/
#endif
    
    private void InitThis()
    {
        if (textFieldList != null)
        {
            return;
        }

        textFieldList = gameObject.GetComponentsInChildren<LocalizeTextField>(true).ToList();
        for (int i = 0; i < textFieldList.Count; i++)
        {
            textFieldList[i].SetText(titleLocalize);
        }

        SetActivate(isActivate);
    }

    public void SetActivate(bool value)
    {
        isActivate = value;
        if (onOffSwitcher)
        {
            onOffSwitcher.SetActiveByChildIndex(isActivate ? 1 : 0);
        }
    }
}