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

// _SJ      스파인 애님 플레이어
public class SpineAnimPlayer : MonoBehaviour
{
    [Serializable]
    public enum EditPlayParamType
    {
        Plus = 0,
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

    [FormerlySerializedAs("SpineStreamAnimList")] [SerializeField] private List<SpineStreamGroup> spineStreamAnimList;
    [FormerlySerializedAs("BaseSpine")] [SerializeField] private SkeletonGraphic baseSpine;

    [FormerlySerializedAs("OnStartPlayEvent")] [SerializeField]
    public PlaySimpleEvent mOnStartPlay;
    
    [FormerlySerializedAs("OnStartPlayWithObjectEvent")] [SerializeField]
    public PlayObjectEvent mOnStartPlayWithObject;

    [FormerlySerializedAs("OnEndPlayAllEvent")] [SerializeField]
    public PlaySimpleEvent mOnEndPlayAll;
    
    [FormerlySerializedAs("OnEndPlayAllWithObjectEvent")] [SerializeField]
    public PlayObjectEvent mOnEndPlayAllWithObject;
    
    private int nowPlayIndex;
    private LocalizeTextField.LocalizeInfo nameInfo = new LocalizeTextField.LocalizeInfo();
    
    /*
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }*/

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
}