using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

// _SJ      단순 광고 영상 페널
public class SimpleAdvertisementMoviePanel : MonoBehaviour, IPreloader
{
    [FormerlySerializedAs("RemainCircleImage")] [SerializeField] private Image remainCircleImage; 
    [FormerlySerializedAs("RemainTimeField")] [SerializeField] private TMP_Text remainTimeField;
    [FormerlySerializedAs("AnimPlayer")] [SerializeField] private SpineAnimPlayer animPlayer;
    [FormerlySerializedAs("PlayMovieTime")] [SerializeField] private float playMovieTime = 30.0f;
    
    private float remainPossibleOffTime = 30.0f;
    
    /*
    // Start is called before the first frame update
    void Start()
    {
    }*/

    // Update is called once per frame
    void Update()
    {
        if (remainPossibleOffTime > 0.0f)
        {
            remainPossibleOffTime -= Time.deltaTime;
            if (remainPossibleOffTime <= 0.0f)
            {
                remainPossibleOffTime = 0.0f;
            }
            
            RefreshRemainTime();
        }
    }

    private void OnDestroy()
    {
        AdvertisementManager.Instance.ResetPlayAdEvent();
    }

    public void InitThis()
    {
        AdvertisementManager.Instance.SetPlayAdEvent(PlayAdMovie);
    }

    private void PlayAdMovie()
    {
        remainPossibleOffTime = playMovieTime;
        gameObject.SetActive(true);
        if (animPlayer)
        {
            animPlayer.PlayAnim();
        }
    }

    private void RefreshRemainTime()
    {
        if (remainCircleImage)
        {
            remainCircleImage.fillAmount = (playMovieTime <= 0.0f) ? 0.0f : (remainPossibleOffTime / playMovieTime);
        }
        
        if (remainTimeField)
        {
            remainTimeField.SetText(((int)remainPossibleOffTime).ToString());
        }
    }

    public void OnClickExit()
    {
        if (remainPossibleOffTime > 0.0f)
        {
            return;
        }
        
        gameObject.SetActive(false);
        AdvertisementManager.Instance.OnEndPlayAd();
    }

    public void OnExecutePreload()
    {
        InitThis();
    }
}