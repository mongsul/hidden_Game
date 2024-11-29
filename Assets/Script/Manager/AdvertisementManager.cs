using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum AdWatchError
{
    Success = 0, // 성공
    FailRemainAdNotExist, // 남은 광고가 없음
    FailNowWatchedAd, // 광고를 아직 시청중임
}

// _SJ      광고 관리자
public class AdvertisementManager : SingletonTemplate<AdvertisementManager>
{
    public class AdvertisementEvent : UnityEvent{}
    
    public class AdvertisementWatchEvent : UnityEvent<AdWatchError>{}
    
    [SerializeField]
    private AdvertisementEvent mOnPlayAD = new AdvertisementEvent(); // 재생 이벤트 (출력하는 쪽 기준)

    [SerializeField]
    private AdvertisementWatchEvent mOnWatchEndAD = new AdvertisementWatchEvent(); // 재생 끝났음 이벤트 (보는 쪽 기준)

    private bool isNowWatchedAD = false; 
    
    /*
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }*/

    public void SetPlayAdEvent(UnityAction playStart)
    {
        ResetPlayAdEvent();
        mOnPlayAD.AddListener(playStart);
    }

    public void ResetPlayAdEvent()
    {
        mOnPlayAD.RemoveAllListeners();
    }

    public void PlayAd(UnityAction<AdWatchError> onWatchEndEvent)
    {
        if (!ItemManager.Instance.IsActivateAD())
        {
            // 광고 제거 활성화 되어있으면 바로 끝내기처리
            OnEndPlayAd();
            return;
        }

        if (isNowWatchedAD)
        {
            OnEndPlayAdWithError(AdWatchError.FailNowWatchedAd);
            return; // 광고 시청중 (혹은 광고없음)
        }
        
        isNowWatchedAD = true;
        mOnWatchEndAD.RemoveAllListeners();
        mOnWatchEndAD.AddListener(onWatchEndEvent);
        mOnPlayAD?.Invoke();
        return;
    }

    private void OnEndPlayAdWithError(AdWatchError error)
    {
        mOnWatchEndAD?.Invoke(error);
    }

    public void OnEndPlayAd()
    {
        isNowWatchedAD = false;
        mOnWatchEndAD?.Invoke(AdWatchError.Success);
    }
}