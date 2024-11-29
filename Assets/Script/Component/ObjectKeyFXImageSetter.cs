using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

// _SJ       키값으로 활성화 해주는 함수
public class ObjectKeyFXImageSetter : MonoBehaviour
{
    [Serializable]
    private struct KeyImageGroup
    {
        [SerializeField] public string activateKey;
        [SerializeField] public Texture activateImage;
    }
    
    [SerializeField] private List<KeyImageGroup> activateImageList;
    [SerializeField] private string defaultActivateKey;
    [SerializeField] private Renderer particle;
    
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
        for (int i = 0; i < activateImageList.Count; i++)
        {
            KeyImageGroup group = activateImageList[i];
            if (group.activateKey.Equals(defaultActivateKey))
            {
                gameObject.SetActive(true);
                SetImage(group.activateImage);
                return;
            }
        }
        
        gameObject.SetActive(false);
    }

    private void SetImage(Texture image)
    {
        if (!particle)
        {
            return;
        }

        particle.material.mainTexture = image;
    }
}