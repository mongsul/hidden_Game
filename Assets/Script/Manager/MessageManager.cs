using System.Collections;
using System.Collections.Generic;
using Core;
using UI.Common;
using UnityEngine;

public class MessageManager : SimpleManagerBase<MessageManager>
{
    private SimpleGlobalMsgPopup msgPopup;
    /*
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }*/

    public void SetMsgPopup(SimpleGlobalMsgPopup msg)
    {
        msgPopup = msg;
    }

    public void SetMsg(string msg)
    {
        if (msgPopup)
        {
            msgPopup.gameObject.SetActive(true);
            msgPopup.SetMsg(msg);
        }
    }

    public void SetMsg(LocalizeTextField.LocalizeInfo msg)
    {
        if (msgPopup)
        {
            msgPopup.gameObject.SetActive(true);
            msgPopup.SetMsg(msg);
        }
    }
}