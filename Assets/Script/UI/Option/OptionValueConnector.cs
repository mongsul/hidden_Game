using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IOptionValueConnector
{
    public void InitThis();
    public void ValueToSaveData();
    public void SetApplyOption();
    public bool IsUseLobby();
}