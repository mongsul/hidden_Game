using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Library;
using UnityEngine;
using UnityEngine.Serialization;

// _SJ      사운드 볼륨 체인져
public class SoundVolumeChanger : MonoBehaviour
{
    [FormerlySerializedAs("Sound")] [SerializeField]
    protected AudioSource sound;

    [FormerlySerializedAs("IsBGM")] [SerializeField] protected bool isBGM;

    /*
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }*/

    private void Awake()
    {
        bool isUseSound = SaveManager.Instance.GetSoundOptionValue(isBGM);
        SetUseVolume(isUseSound);
    }

    protected void SetUseVolume(bool isUseSound)
    {
        if (sound)
        {
            //CodeUtilLibrary.SetColorLog($"SetUseVolume[{name}] : isUseSound[{isUseSound}]", "aqua");
            sound.volume = isUseSound ? 1.0f : 0.0f;
        }
    }

    public static void ApplySoundOptionToScene(bool isBGM, bool isUseSound)
    {
        //bool bgmVolume = SaveManager.Instance.GetSoundOptionValue(true);
        //bool fxVolume = SaveManager.Instance.GetSoundOptionValue(false);
        List<SoundVolumeChanger> soundChangerList = CodeUtilLibrary.GetComponentsListInActiveScene<SoundVolumeChanger>();
        for (int i = 0; i < soundChangerList.Count; i++)
        {
            SoundVolumeChanger volumeChanger = soundChangerList[i];
            if (volumeChanger)
            {
                if (volumeChanger.isBGM == isBGM)
                {
                    volumeChanger.SetUseVolume(isUseSound);
                }
            }
        }
    }

    public static void RefreshSoundByScene()
    {
        bool bgmVolume = SaveManager.Instance.GetSoundOptionValue(true);
        bool fxVolume = SaveManager.Instance.GetSoundOptionValue(false);
        List<SoundVolumeChanger> soundChangerList = CodeUtilLibrary.GetComponentsListInActiveScene<SoundVolumeChanger>();
        for (int i = 0; i < soundChangerList.Count; i++)
        {
            SoundVolumeChanger volumeChanger = soundChangerList[i];
            if (volumeChanger)
            {
                volumeChanger.SetUseVolume(volumeChanger.isBGM ? bgmVolume : fxVolume);
            }
        }
    }

    public static void RefreshSoundByGameObject(GameObject gameObject)
    {
        if (!gameObject)
        {
            return;
        }
        
        bool bgmVolume = SaveManager.Instance.GetSoundOptionValue(true);
        bool fxVolume = SaveManager.Instance.GetSoundOptionValue(false);
        List<SoundVolumeChanger> soundChangerList = gameObject.GetComponentsInChildren<SoundVolumeChanger>(true).ToList();
        for (int i = 0; i < soundChangerList.Count; i++)
        {
            SoundVolumeChanger volumeChanger = soundChangerList[i];
            if (volumeChanger)
            {
                volumeChanger.SetUseVolume(volumeChanger.isBGM ? bgmVolume : fxVolume);
            }
        }
    }
}