using System.Collections;
using System.Collections.Generic;
using UI.Common;
using UnityEngine;

public class BuyTouchCountPanel : MonoBehaviour
{
    [SerializeField] private LocalizeTextField addTouchCountField;
    
    // Start is called before the first frame update
    void Start()
    {
        if (addTouchCountField)
        {
            addTouchCountField.SetContents(ClientTableManager.Instance.GetBaseValue("AddTouchCountByAd"));
        }
    }

    /*
    // Update is called once per frame
    void Update()
    {
    }*/
}