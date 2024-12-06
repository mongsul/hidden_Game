using System.Collections;
using System.Collections.Generic;
using Core.Library;
using UnityEngine;

public class ObjectKeySoundPlayer : SoundVolumeChanger
{
    [SerializeField] private List<string> activateKeyValueList;
    
    // Start is called before the first frame update
    void Start()
    {
        isBGM = true;
    }

    /*
    // Update is called once per frame
    void Update()
    {
    }*/

    public void SetNowActivateKey(string key)
    {
        if (sound)
        {
            bool isNowActivate = activateKeyValueList.Contains(key);
            //CodeUtilLibrary.SetColorLog($"SetNowActivateKey[{name}] : key[{key}], isNowActivate[{isNowActivate}]", "lime");
            if (isNowActivate)
            {
                if (!sound.isPlaying)
                {
                    sound.Play();
                }
            }
            else
            {
                sound.Stop();
            }
        }
    }
}