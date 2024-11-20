using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

// _SJ      사운드 플레이어
public class SoundPlayer : SoundVolumeChanger
{
    //[FormerlySerializedAs("IsPlayLoop")] [SerializeField] private bool isPlayLoop;
    
    /*
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }*/

    public void PlaySound()
    {
        if (sound)
        {
            sound.Play();
        }
    }
}