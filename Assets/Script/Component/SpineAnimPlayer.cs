using System;
using System.Collections;
using System.Collections.Generic;
using Script.Library;
using Spine;
using Spine.Unity;
using UI.Common;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using Event = Spine.Event;

// _SJ      스파인 애님 플레이어
public class SpineAnimPlayer : MonoBehaviour
{
    [Serializable]
    public enum EditPlayParamType
    {
        None = 0,
        Plus,
        Minus,
        Multiply,
        Divide,
    }
    
    [Serializable]
    public class PlayParamEvent : UnityEvent<EditPlayParamType, int, int>{}
    
    [Serializable]
    public class PlaySimpleEvent : UnityEvent{}
    
    [Serializable]
    public class PlayObjectEvent : UnityEvent<GameObject>{}
    
    [Serializable]
    public class PlaySpineEvent : UnityEvent<Event>{}

    [Serializable]
    private struct SpineStreamGroup
    {
        [FormerlySerializedAs("AnimName")] [SerializeField]
        public string animName;
        
        [FormerlySerializedAs("OnEndPlayEvent")] [SerializeField]
        public PlayParamEvent mOnEndPlay;

        [FormerlySerializedAs("IsPlayLoop")] [SerializeField] public bool isPlayLoop;

        [FormerlySerializedAs("EndParamType")] [SerializeField] public EditPlayParamType endParamType;
        [FormerlySerializedAs("EndListIndex")] [SerializeField] public int endListIndex;
        [FormerlySerializedAs("EndValue")] [SerializeField] public int endValue;
    }

    [Serializable]
    private struct SpineEventGroup
    {
        [SerializeField] [SpineEvent] public string EventName;
        [SerializeField] public PlaySpineEvent CallEvent;
    }

    [SerializeField] private List<SpineStreamGroup> spineStreamAnimList;
    [SerializeField] private SkeletonGraphic baseSpine;
    [SerializeField] private List<SpineEventGroup> eventList; 

    [SerializeField] public PlaySimpleEvent mOnStartPlay;
    [SerializeField] public PlayObjectEvent mOnStartPlayWithObject;
    [SerializeField] public PlaySimpleEvent mOnEndPlayAll;
    [SerializeField] public PlayObjectEvent mOnEndPlayAllWithObject;
    
    private int nowPlayIndex;
    private LocalizeTextField.LocalizeInfo nameInfo = new LocalizeTextField.LocalizeInfo();
    
    // Start is called before the first frame update
    void Start()
    {
        int eventCount = (eventList == null) ? 0 : eventList.Count;
        if (eventCount > 0)
        {
            SpineUtilLibrary.BindSpineEventFunction(baseSpine, OnSpineEvent);
        }
    }

    /*
    // Update is called once per frame
    void Update()
    {
    }*/

    public SkeletonGraphic GetSpine()
    {
        return baseSpine;
    }

    private void OnSpineEvent(TrackEntry trackEntry, Event e)
    {
        for (int i = 0; i < eventList.Count; i++)
        {
            if (SpineUtilLibrary.IsEqualEvent(e, eventList[i].EventName))
            {
                if (eventList[i].CallEvent != null)
                {
                    eventList[i].CallEvent?.Invoke(e);
                }
            }
        }
    }

    private bool GetSpineInfo(int listIndex, out SpineStreamGroup spineInfo)
    {
        if ((listIndex < 0) || (spineStreamAnimList.Count <= listIndex))
        {
            spineInfo = new SpineStreamGroup();
            return false;
        }

        spineInfo = spineStreamAnimList[listIndex];
        return true;
    }

    private bool PlayAnimStream()
    {
        SpineStreamGroup spine;
        if (!GetSpineInfo(nowPlayIndex, out spine))
        {
            return false;
        }

        nameInfo.localizeKey = spine.animName;
        string animName = LocalizeTextField.GetFormatStringByLocalizeInfo(nameInfo);
        SpineUtilLibrary.StopSpineAnim(baseSpine);
        TrackEntry animEntry = SpineUtilLibrary.PlaySpineAnim(baseSpine, animName, spine.isPlayLoop);
        if (spine.isPlayLoop)
        {
            return false; // 바로 종료 처리
        }
        
        if (animEntry == null)
        {
            return false;
        }
        
        animEntry.Complete += (trackEntry => OnEndPlayAnim());
        return true;
    }

    private void OnEndPlayAnim()
    {
        SpineStreamGroup spine;
        if (GetSpineInfo(nowPlayIndex, out spine))
        {
            spine.mOnEndPlay?.Invoke(spine.endParamType, spine.endListIndex, spine.endValue);
        }
        
        PlayNextAnim();
    }

    private void StartPlayAnim()
    {
        nowPlayIndex = -1;
        mOnStartPlay?.Invoke();
        mOnStartPlayWithObject?.Invoke(gameObject);
        PlayNextAnim();
    }

    private void PlayNextAnim()
    {
        nowPlayIndex++;
        if (spineStreamAnimList.Count <= nowPlayIndex)
        {
            OnEndPlayAll();
            return;
        }

        bool isPlayAnimStream = PlayAnimStream();
        if (!isPlayAnimStream)
        {
            OnEndPlayAnim();
        }
    }

    private void OnEndPlayAll()
    {
        mOnEndPlayAll?.Invoke();
        mOnEndPlayAllWithObject?.Invoke(gameObject);
    }

    public void PlusParamValue(EditPlayParamType paramType, int listIndex, int value)
    {
        if (nameInfo.contentsList == null)
        {
            return;
        }

        if (nameInfo.contentsList.Count < listIndex + 1)
        {
            return;
        }

        int paramValue = Convert.ToInt32(nameInfo.contentsList[listIndex]);
        switch (paramType)
        {
            case EditPlayParamType.Plus:
                paramValue += value;
                break;
            case EditPlayParamType.Minus:
                paramValue -= value;
                break;
            case EditPlayParamType.Multiply:
                paramValue *= value;
                break;
            case EditPlayParamType.Divide:
                paramValue /= value;
                break;
        }

        nameInfo.contentsList[listIndex] = paramValue.ToString();
    }

    public void StopAnim()
    {
        SpineUtilLibrary.StopSpineAnim(baseSpine);
    }

    #region PlayAnim
    public void PlayAnim(List<string> paramList)
    {
        nameInfo.contentsList = paramList;
        StartPlayAnim();
    }

    public void PlayAnim()
    {
        nameInfo.contentsList = null;
        StartPlayAnim();
    }

    public void PlayAnim(string param)
    {
        nameInfo.contentsList = new List<string>();
        nameInfo.contentsList.Add(param);
        StartPlayAnim();
    }

    public void PlayAnim(int param)
    {
        nameInfo.contentsList = new List<string>();
        nameInfo.contentsList.Add(param.ToString());
        StartPlayAnim();
    }
    #endregion

    #region PlayAnimETC
    public void PlayLastAnim()
    {
        nowPlayIndex = spineStreamAnimList.Count - 2;
        mOnStartPlay?.Invoke();
        mOnStartPlayWithObject?.Invoke(gameObject);
        PlayNextAnim();
    }
    #endregion
}