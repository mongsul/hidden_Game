using System.Collections;
using System.Collections.Generic;
using UI.Common;
using UnityEngine;
using UnityEngine.Serialization;

public class SimpleGlobalMsgPopup : MonoBehaviour, IPreloader
{
    [FormerlySerializedAs("MsgField")] [SerializeField] private LocalizeTextField msgField;
    
    /*
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }*/

    public void OnExecutePreload()
    {
        MessageManager.Instance.SetMsgPopup(this);
    }

    public void SetMsg(string msg)
    {
        if (msgField)
        {
            msgField.SetText(msg);
        }
    }

    public void SetMsg(LocalizeTextField.LocalizeInfo msg)
    {
        if (msgField)
        {
            msgField.SetText(msg);
        }
    }
}