using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

// _SJ       키값으로 활성화 해주는 함수
public class ObjectKeyColorSetter : MonoBehaviour
{
    [Serializable]
    private struct KeyColorGroup
    {
        [SerializeField] public string activateKey;
        [SerializeField] public Color activateColor;
    }
    
    [SerializeField] private List<KeyColorGroup> activateColorList;
    [SerializeField] private string defaultActivateKey;
    [SerializeField] private Graphic targetGraphic;
    
    // Start is called before the first frame update
    void Start()
    {
        SetActivateKey(defaultActivateKey);
    }

    /*
    // Update is called once per frame
    void Update()
    {
    }*/

    public void SetActivateKey(string key)
    {
        defaultActivateKey = key;
        for (int i = 0; i < activateColorList.Count; i++)
        {
            KeyColorGroup group = activateColorList[i];
            if (group.activateKey.Equals(defaultActivateKey))
            {
                SetColor(group.activateColor);
                break;
            }
        }
    }

    private void SetColor(Color color)
    {
        if (!targetGraphic)
        {
            targetGraphic = gameObject.GetComponent<Graphic>();
        }

        if (targetGraphic)
        {
            targetGraphic.color = color;
        }
    }
}