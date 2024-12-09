using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Library;
using Script.Component;
using Script.Library;
using Spine;
using Spine.Unity;
using TMPro;
using UI.Common;
using UI.Common.Base;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

// _SJ      물건 찾는 모드
public class FindObjectMode : MonoBehaviour
{
    private enum FindFXKind
    {
        TouchToCollect = 0,
        TouchToWrong,
        UseHint,
        OpenWrapBox,
    }
    
#if UNITY_EDITOR
    [FormerlySerializedAs("IsFindCountOne")] [SerializeField] private bool isFindCountOne = false;
#endif
    
    [FormerlySerializedAs("StageIndex")] [SerializeField] private int stageIndex;

    [FormerlySerializedAs("TotalStandardPanel")] [SerializeField] private RectTransform totalStandardRect;
    [FormerlySerializedAs("ToolBox")] [SerializeField] private GameObject toolBox;
    [FormerlySerializedAs("GameEndPanel")] [SerializeField] private GameObject gameEndPanel;
    [FormerlySerializedAs("GameEndPlayer")] [SerializeField] private SpineAnimPlayer gameEndPlayer;

    [FormerlySerializedAs("GameOverPanel")] [SerializeField] private GameObject gameOverPanel;
    
    [FormerlySerializedAs("BaseScroll")] [SerializeField] private ScrollRect baseScroll;
    [SerializeField] private RectTransform scrollViewportRect;

    private float defaultScrollViewportRectOffsetMinY = 0.0f;  // [2024/12/9 kmlee123, 24-12-9-1] (code add)

    [FormerlySerializedAs("PrefabBaseObject")] [SerializeField] private GameObject prefabBaseObject;
    [SerializeField] private RectTransform prefabBaseRect;

    [FormerlySerializedAs("ScaleRateValueField")] [SerializeField] private TMP_Text scaleRateValueField;
    [FormerlySerializedAs("ChangeScaleRateList")] [SerializeField] private List<float> changeScaleRateList = new List<float>();
    private int nowScaleRateListIndex = 0;

    [FormerlySerializedAs("RemainSpecialFindCountField")] [SerializeField] private TMP_Text remainSpecialFindCountField;
    [FormerlySerializedAs("RemainFindCountField")] [SerializeField] private TMP_Text remainFindCountField;
    [FormerlySerializedAs("RemainTouchCount")] [SerializeField] private TMP_Text remainTouchCountField;

    [FormerlySerializedAs("TouchCounter")] [SerializeField] private TouchCover touchCounter;
    [FormerlySerializedAs("FXList")] [SerializeField] private BasePrefabDynamicList fxList;
    [FormerlySerializedAs("MainCamera")] [SerializeField] private Camera mainCamera;

    [FormerlySerializedAs("DisplayTouchRect")] [SerializeField] private RectTransform displayTouchRect;
    [FormerlySerializedAs("DisplayWrongRect")] [SerializeField] private RectTransform displayWrongRect;
    [FormerlySerializedAs("DisplayDirectionRect")] [SerializeField] private RectTransform displayDirectionRect;
    [FormerlySerializedAs("DisplayEndDirectionDelayTime")] [SerializeField] private float displayEndDirectionDelayTime = 3.0f;
    
    [FormerlySerializedAs("FindSoundAudio")] [SerializeField] private GameObject findSoundAudio;
    [SerializeField] private GameObject chapterSoundAudio;

    [FormerlySerializedAs("BottomToolBoxRect")] [SerializeField] private RectTransform bottomToolBoxRect;
    [FormerlySerializedAs("BottonToolBoxDefaultSize")] [SerializeField] private float bottonToolBoxDefaultSize = 200.0f;
    [FormerlySerializedAs("BannerAdPanel")] [SerializeField] private GameObject bannerAdPanel;
    
    [FormerlySerializedAs("TopToolBoxRect")] [SerializeField] private RectTransform topToolBoxRect;

    [FormerlySerializedAs("HintCountField")] [SerializeField] private TMP_Text hintCountField;

    [FormerlySerializedAs("HintADSpine")] [SerializeField] private SkeletonGraphic hintADSpine;
    [FormerlySerializedAs("HintADAnim")] [SerializeField] private SpineAnimPlayer hintADAnim;
    [FormerlySerializedAs("HintAdMsgField")] [SerializeField] private LocalizeTextField hintAdMsgField;

    [FormerlySerializedAs("HintSwitcher")] [SerializeField] private ObjectSwitcher hintSwitcher;
    [FormerlySerializedAs("HintAnimPlayer")] [SerializeField] private SpineAnimPlayer hintAnimPlayer;

    [SerializeField] private BuyTouchCountPanel buyTouchCountPanel;
    [SerializeField] private BuyItemPanel buyPanel;
    
    [FormerlySerializedAs("DebugField")] [SerializeField] private TMP_Text debugField;

    [SerializeField] private GameObject gotoNextStagePanel;
    [SerializeField] private GameObject gotoBookShelfPanel;
    
    [SerializeField] private SoundPlayer displayAdHintSound;
    [SerializeField] private SoundPlayer clearGameSound;
    [SerializeField] private SoundPlayer gameOverSound;
    
    private GameObject baseFindObject; // 찾기 페널 (stage1-1등, 로드 후 인스턴스 생성 한 파일)
    private RectTransform baseFindRect; // 찾기 페널 RectTransform
    private SpriteAnimPlayer animPlayer;
    
    private StageTable nowProgressStage;

    private int remainSpecialFindCount = 0; // 숨어있는 녀석들
    private int remainFindCount = 0; // 찾아야 할 횟수
    private int remainTouchCount = 0; // 틀렸을 때 까이는 횟수
    private float damagedLifeInRange = 10.0f; // 이 값보다 작으면 목숨 까임

    private List<Level> findList = new List<Level>();

    //private float toolBoxHeight;
    private Vector3 imageSize;

    private List<AudioSource> findSoundList = new List<AudioSource>();
    private bool isPlayFXSound = false;
    private List<ObjectKeySoundPlayer> chapterSoundList = new List<ObjectKeySoundPlayer>();

    private float displayHintADDelayTime = 60.0f;
    private float onceDisplayHintADTime = 3.0f;
    private float maxHintADCount = 3;
    private float remainDelayHintADTime = 0.0f;
    private float remainDisplayHintADTime = 0.0f;

    private LobbyInit initToLobby;
    private int lobbyInitParam;

    private string debugStr = "";
    private float touchDist = 0.0f;
    private float touchedScaleRate = 1.0f;

    private float nowScaleRate = 1.0f;
    private float minScaleRate = 1.0f;

    private Vector2 centerPos;

    private bool isSetStageIndex = false;
    private bool isPlayRetry = false;

    private bool isAlreadyDisplayAddTouchCount = false;

    // Start is called before the first frame update
    void Start()
    {
        PreloadManager.ExecutePreload();
        isPlayFXSound = SaveManager.Instance.GetSoundOptionValue(false);
        if (findSoundAudio)
        {
            findSoundList = findSoundAudio.GetComponentsInChildren<AudioSource>(true).ToList();
        }

        if (chapterSoundAudio)
        {
            chapterSoundList = chapterSoundAudio.GetComponentsInChildren<ObjectKeySoundPlayer>(true).ToList();
        }

        damagedLifeInRange = ClientTableManager.Instance.GetBaseFloatValue("DamagedLifeInRange", 10.0f);
        displayHintADDelayTime = ClientTableManager.Instance.GetBaseFloatValue("DisplayHintADDelayTime", 60.0f);
        onceDisplayHintADTime = ClientTableManager.Instance.GetBaseFloatValue("OnceDisplayHintADTime", 3.0f);
        
        if (topToolBoxRect)
        {
            // 상단 세이프존 처리
            Vector2 safeZoneSize = ResolutionManager.Instance.GetSafeZoneSize();
            float safeZoneTopHeight = safeZoneSize.y;
            Vector2 topSize = topToolBoxRect.sizeDelta;
            topSize.y += safeZoneTopHeight;
            topToolBoxRect.sizeDelta = topSize;
            //toolBoxHeight += safeZoneTopHeight;
            
            if (scrollViewportRect)
            {
                Vector2 viewportPos = scrollViewportRect.offsetMax;
                viewportPos.y -= topSize.y;
                scrollViewportRect.offsetMax = viewportPos;
            }
        }
        
        // [2024/12/9 kmlee123, 24-12-9-1] (code modify)
        // if (bottomToolBoxRect)
        // {
        //     // 배너 광고 및 하단 세이프존 처리
        //     bool isActivateAD = ItemManager.Instance.IsActivateAD(); // 광고 활성화 여부
        //     float bottomHeight = bottonToolBoxDefaultSize;
        //     Vector2 safeZonePos = ResolutionManager.Instance.GetSafeZonePos();
        //     float safeZoneBottomHeight = safeZonePos.y;
        //     if (isActivateAD)
        //     {
        //         float bannerHeight = ClientTableManager.Instance.GetBaseFloatValue("BannerADHeight", 200.0f);
        //         if (bannerHeight < safeZoneBottomHeight)
        //         {
        //             bannerHeight = safeZoneBottomHeight;
        //         }
        //
        //         bottomHeight += bannerHeight;
        //
        //         if (bannerAdPanel)
        //         {
        //             RectTransform bannerRect = CodeUtilLibrary.GetRectTransform(bannerAdPanel);
        //             if (bannerRect)
        //             {
        //                 Vector2 bannerSize = bannerRect.sizeDelta;
        //                 bannerSize.y = bannerHeight;
        //                 bannerRect.sizeDelta = bannerSize;
        //             }
        //          
        //             bannerAdPanel.SetActive(true);
        //         }
        //     }
        //     else
        //     {
        //         bottomHeight += safeZoneBottomHeight;
        //         if (bannerAdPanel)
        //         {
        //             bannerAdPanel.SetActive(false);
        //         }
        //     }
        //
        //     Vector2 bottomSize = bottomToolBoxRect.sizeDelta;
        //     bottomSize.y = bottomHeight;
        //     bottomToolBoxRect.sizeDelta = bottomSize;
        //     //toolBoxHeight += bottomHeight;
        // 
        //     if (scrollViewportRect)
        //     {
        //         Vector2 viewportPos = scrollViewportRect.offsetMin;
        //         viewportPos.y += bottomSize.y;
        //         scrollViewportRect.offsetMin = viewportPos;
        //     }
        // }
        defaultScrollViewportRectOffsetMinY = scrollViewportRect.offsetMin.y;
        RefreshBottomUIsSize();
        AdvertisementManager.Instance.StartBannerAd(bannerAdPanel, RefreshBottomUIsSize);

        if (hintADSpine && hintAdMsgField)
        {
            if (hintAdMsgField)
            {
                SpineUtilLibrary.AttachToSpineBone(hintADSpine, "Ad_Text", hintAdMsgField.gameObject);
            }
            
            hintADSpine.gameObject.SetActive(false);
        }

        RefreshHintCount();
        
#if UNITY_EDITOR
        if (stageIndex < 1)
        {
            return;
        }

        // 테스트 기능 (스테이지 지정시 바로 시작)
        if (!isSetStageIndex)
        {
            SetStage(stageIndex, isPlayRetry);
        }
#endif
    }
    
    // Update is called once per frame
    void Update()
    {
        if (remainDelayHintADTime > 0.0f)
        {
            remainDelayHintADTime -= Time.deltaTime;
            if (remainDelayHintADTime <= 0.0f)
            {
                DisplayHintAD();
            }
        }

        if (remainDisplayHintADTime > 0.0f)
        {
            remainDisplayHintADTime -= Time.deltaTime;
            if (remainDisplayHintADTime <= 0.0f)
            {
                EraseHintAD();
            }
        }

        CheckMultiTouch();

        //if (Input.GetMouseButtonUp(1))
        /*if (Input.GetKeyUp(KeyCode.M))
        {
            PlayTouchFX(Input.mousePosition);
        }*/
        
    #if UNITY_EDITOR
        ScrollingCheck();
        if (Input.GetKeyUp(KeyCode.H))
        {
            ItemManager.Instance.AddFunctionItem(FunctionItemType.Hint, 5);
            RefreshHintCount();
        }
    #endif
    }

    public void RefreshBottomUIsSize()  // [2024/12/9 kmlee123, 24-12-9-1] (code add)
    {
        if (bottomToolBoxRect)
        {
            // 배너 광고 및 하단 세이프존 처리
            float bottomHeight = bottonToolBoxDefaultSize;
            Vector2 safeZonePos = ResolutionManager.Instance.GetSafeZonePos();
            float safeZoneBottomHeight = safeZonePos.y;

            float bannerHeight = AdvertisementManager.Instance.GetBannerHeight();
            if (bannerHeight < safeZoneBottomHeight)
            {
                bannerHeight = safeZoneBottomHeight;
            }

            bottomHeight += bannerHeight;

            Vector2 bottomSize = bottomToolBoxRect.sizeDelta;
            bottomSize.y = bottomHeight;
            bottomToolBoxRect.sizeDelta = bottomSize;
            //toolBoxHeight += bottomHeight;
            
            if (scrollViewportRect)
            {
                Vector2 viewportPos = scrollViewportRect.offsetMin;
                viewportPos.y = defaultScrollViewportRectOffsetMinY + bottomSize.y;
                scrollViewportRect.offsetMin = viewportPos;
            }
        }
    }

    #region Stage
    public void SetStage(int stage, bool isRetry = false)
    {
        isSetStageIndex = true;
        stageIndex = stage;
        isPlayRetry = isRetry;
        isAlreadyDisplayAddTouchCount = false;
        
        #if UNITY_EDITOR
        CodeUtilLibrary.SetColorLog($"SetStage : stage[{stage}], isRetry[{isRetry}]", "lime");
        #endif

        nowProgressStage = StageTableManager.Instance.GetStageTable(stageIndex);
        if (nowProgressStage == null)
        {
            return;
        }
        
        int chapter = StageTableManager.Instance.GetChapterSort(nowProgressStage.chapter);
        int chapterStage = nowProgressStage.stage;
        bool isHaveRemoveAD = ItemManager.Instance.IsPossibleUseFunctionItem(FunctionItemType.ADRemover);
        int hpRate = isHaveRemoveAD ? 2 : 1;
        InitRemainTouchCount(nowProgressStage.touchCount * hpRate);
        
        if (!prefabBaseRect)
        {
            return;
        }
        
        GameObject stagePrefab = ProjectUtilLibrary.LoadStagePrefab(chapter, chapterStage);
        if (!stagePrefab)
        {
            GotoLobby(); // 일단 로비로 강제송환한다.
            return;
        }
        
        GameObject newStagePrefab = Instantiate<GameObject>(stagePrefab, Vector3.zero, Quaternion.identity, prefabBaseRect);
        if (newStagePrefab)
        {
            InitStagePrefab(newStagePrefab);
        }
        
        SetDefaultScaleRate();
        OnGameStart();
    }
    #endregion

    #region StagePrefab
    public void SetScaleRate(float scaleRate)
    {
        SetScaleRate(scaleRate, new Vector2(0.5f, 0.5f));
    }
    
    public void SetScaleRate(float scaleRate, Vector2 centerRate)
    {
        scaleRate = Math.Clamp(scaleRate, minScaleRate, 3.0f);
        if (nowScaleRate == scaleRate)
        {
            return; // 안함
        }
        
        //CodeUtilLibrary.SetColorLog($"SetScaleRate : X{scaleRate}");
        
        if (scaleRateValueField)
        {
            //scaleRateValueField.SetText($"X{scaleRate}");
            scaleRateValueField.SetText(string.Format("X{0:F2}", scaleRate));
        }
        
        nowScaleRate = scaleRate;

        Vector3 rate = new Vector3(1.0f, 1.0f, 0.0f) * scaleRate;
        if (!prefabBaseRect)
        {
            return;
        }

        prefabBaseRect.sizeDelta = imageSize;
        prefabBaseRect.localScale = rate;

        // 가운데 맞춤
        if (baseScroll)
        {
            baseScroll.normalizedPosition = centerRate;
        }
    }

    private void AddScaleRate(Vector2 center, float delta)
    {
        if (!prefabBaseRect)
        {
            return;
        }
        
        // 스크린 위치 -> 로컬포인트로 변경
        Vector2 centerLoc;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(prefabBaseRect, center, mainCamera, out centerLoc);
        Vector2 scrollCenter = centerLoc / prefabBaseRect.sizeDelta; // 스크롤 중심점 구하기
        Vector2 newCenter = scrollCenter - (Vector2.one * (delta * 0.5f)); // 새로운 중심점 연산
        SetScaleRate(nowScaleRate + delta, newCenter);
        
        //AddDebugLog($"AddScaleRate : touchDelta[{delta}], center[{center}, centerLoc{centerLoc}], realCenter[{scrollCenter} -> {newCenter}]");
    }

    private void InitStagePrefab(GameObject stagePrefab)
    {
        int findCount = 0;
        int specialCount = 0;
        int i;

        GameObject lastFindObject = baseFindObject;
        baseFindObject = stagePrefab;
        
        //stagePrefab.transform.localPosition = Vector3.zero;
        baseFindRect = CodeUtilLibrary.GetRectTransform(stagePrefab);
        if (baseFindRect)
        {
            imageSize = baseFindRect.sizeDelta;
            //imageSize.y += toolBoxHeight;
            baseFindRect.sizeDelta = imageSize;
            baseFindRect.anchoredPosition = Vector2.zero;
            //rect.position = Vector3.zero;
            /*rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.localPosition = Vector3.zero;
            rect.sizeDelta = Vector2.zero;*/
        }

        findList = stagePrefab.GetComponentsInChildren<Level>(true).ToList();
        if (findList != null)
        {
            for (i = 0; i < findList.Count; i++)
            {
                Level findObject = findList[i];
                if (findObject)
                {
                    findObject.index = i;
                    findObject.mOnFindObject.AddListener(OnFindObject);
                    findObject.mOnFindWrabObject.AddListener(OnFindWrapObject);
                    findObject.mOnPress.AddListener(PlayTouchFX);
                    findCount++;
                }
            }
        }

        List<ObjectChanger> wrapperList = stagePrefab.GetComponentsInChildren<ObjectChanger>(true).ToList();
        if (wrapperList != null)
        {
            for (i = 0; i < wrapperList.Count; i++)
            {
                if (wrapperList[i].IsWrappedObject())
                {
                    specialCount++;
                }
            }
        }

        if (touchCounter)
        {
            BackLineComponent backline = baseFindObject.GetComponentInChildren<BackLineComponent>();
            int sibliningIndex = 2;
            if (backline)
            {
                sibliningIndex = backline.transform.GetSiblingIndex() + 1;
            }
                
            touchCounter.gameObject.transform.SetParent(baseFindObject.transform);
            touchCounter.gameObject.transform.SetSiblingIndex(sibliningIndex);
            RectTransform touchRect = CodeUtilLibrary.GetRectTransform(touchCounter.transform);
            if (touchRect)
            {
                touchRect.anchorMin = Vector2.zero;
                touchRect.anchorMax = Vector2.one;
                touchRect.localScale = Vector3.one;
                touchRect.anchoredPosition = Vector3.zero;
                touchRect.sizeDelta = Vector2.zero;
            }
        }

        bool isOne = false;
#if UNITY_EDITOR
        isOne = isFindCountOne;
#endif
        if (lastFindObject)
        {
            Destroy(lastFindObject); // 기존에 사용하던 녀석은 삭제처리
        }
        
        if (totalStandardRect)
        {
            minScaleRate = CodeUtilLibrary.GetInRectMinFitSize(totalStandardRect.rect.size, imageSize);
        }
        
        // 찾는 개수 한개
        if (isOne)
        {
            InitRemainFindObjectCount(5);
            InitRemainSpecialFindObjectCount(0);
            InitRemainTouchCount(200);
        }
        else
        {
            InitRemainFindObjectCount(findCount - specialCount);
            InitRemainSpecialFindObjectCount(specialCount);
        }
    }

    private void SetDefaultScaleRate()
    {
        nowScaleRate = 0.0f;
        nowScaleRateListIndex = 0;
        float nowRate = GetScaleRateByList(nowScaleRateListIndex);
        SetScaleRate(nowRate);
    }

    public void RotateScaleRate()
    {
        nowScaleRateListIndex++;
        if (nowScaleRateListIndex >= changeScaleRateList.Count)
        {
            nowScaleRateListIndex = 0;
        }
        
        float nowRate = GetScaleRateByList(nowScaleRateListIndex);
        SetScaleRate(nowRate);
    }

    private float GetScaleRateByList(int listIndex)
    {
        if ((listIndex < 1) || (listIndex >= changeScaleRateList.Count))
        {
            return 1.0f;
        }

        return changeScaleRateList[listIndex];
    }
    #endregion

    #region Rule
    private void InitRemainFindObjectCount(int count)
    {
        remainFindCount = count;
        RefreshRemainFindObjectCount();
    }

    private void RefreshRemainFindObjectCount()
    {
        if (remainFindCountField)
        {
            remainFindCountField.SetText($"{remainFindCount}");
        }
    }
    private void InitRemainSpecialFindObjectCount(int count)
    {
        remainSpecialFindCount = count;
        RefreshRemainSpecialFindObjectCount();
    }

    private void RefreshRemainSpecialFindObjectCount()
    {
        if (remainSpecialFindCountField)
        {
            remainSpecialFindCountField.SetText($"{remainSpecialFindCount}");
        }
    }

    private void OnFindWrapObject(Level findObject)
    {
        if (findObject)
        {
            BaseSimplePrefab hint = findObject.GetHint();
            if (hint)
            {
                OnEndPlayHintFX(hint.gameObject);
            }

            int keyIndex = findObject.index;
            bool isAddToList = true;

            for (int i = 0; i < findList.Count; i++)
            {
                if (findList[i].index == keyIndex)
                {
                    isAddToList = false;
                    break;
                }
            }

            if (isAddToList)
            {
                findList.Add(findObject);
            }

            findObject.SetHint(); // 힌트 오브젝트 초기화 처리
        }
    }

    private void OnFindObject(Level findObject)
    {
        if (findObject.IsEndFindObject())
        {
            return;
        }

        //InitDisplayHintADTime(); // 광고 출력 시간 초기화
        
        if (findObject)
        {
            BaseSimplePrefab hint = findObject.GetHint();
            if (hint)
            {
                OnEndPlayHintFX(hint.gameObject);
            }
        }

        AddRecord("TouchToCollect");
            
        int keyIndex = findObject.index;
        for (int i = 0; i < findList.Count; i++)
        {
            if (findList[i].index == keyIndex)
            {
                findList.RemoveAt(i);
                break;
            }
        }
        
        if (findObject.GetIsWrapped())
        {
            remainSpecialFindCount--;
            RefreshRemainSpecialFindObjectCount();
        }
        else
        {
            remainFindCount--;
            RefreshRemainFindObjectCount();
        }

        int remainCount = remainSpecialFindCount + remainFindCount;
        if (remainCount <= 0)
        {
            // 모드 끝내기
            OnFindAll();
        }
    }

    private void OnFindAll()
    {
        remainDelayHintADTime = 0.0f;
        remainDisplayHintADTime = 0.0f;
        
        Vector2 scaleRate = Vector2.one;

        if (totalStandardRect)
        {
            scaleRate = CodeUtilLibrary.GetInRectFitSize(totalStandardRect.rect.size, imageSize);
        }
        
        //SetScaleRate(1.0f);

        if (displayDirectionRect && baseFindObject)
        {
            baseFindObject.transform.SetParent(displayDirectionRect);
            baseFindObject.transform.localPosition = Vector3.zero;
            baseFindObject.transform.localScale = scaleRate;
        }
        
        if (toolBox)
        {
            toolBox.SetActive(false);
        }

        AddRecord("GameClear");
        SaveManager.Instance.SetClearStage(stageIndex);
        
        if (animPlayer)
        {
            animPlayer.mOnEndPlay.RemoveAllListeners();
            animPlayer.mOnEndPlay.AddListener(OnEndPlayDirection);
            animPlayer.gameObject.SetActive(true);
        }
    }
    
    private void InitRemainTouchCount(int count)
    {
        remainTouchCount = count;
        RefreshRemainTouchCount();
    }

    private void RefreshRemainTouchCount()
    {
        if (remainTouchCountField)
        {
            remainTouchCountField.SetText($"{remainTouchCount}");
        }
    }

    private void OnEndPlayDirection()
    {
        Invoke("DisplayEndAdCheck", displayEndDirectionDelayTime);
    }

    private void DisplayEndAdCheck()
    {
        bool isDisplayAd = isPlayRetry ? false : nowProgressStage.afterAd;
        if (isDisplayAd)
        {
            AdvertisementManager.Instance.PlayAd(OnWatchedGameEndAd);
        }
        else
        {
            OnDisplayGameEndPanel();
        }
    }

    private void OnWatchedGameEndAd(AdWatchError error)
    {
        OnDisplayGameEndPanel();
    }

    private void OnDisplayGameEndPanel()
    {
        if (clearGameSound)
        {
            clearGameSound.PlaySound();
        }
        
        if (gameEndPanel)
        {
            gameEndPanel.SetActive(true);
        }

        if (gameEndPlayer)
        {
            gameEndPlayer.PlayAnim();
        }

        bool isDisplayNextStage = true;
        if (isPlayRetry)
        {
            if (stageIndex >= SaveManager.Instance.GetClearStage())
            {
                // 최대 저장된 클리어 스테이지 체크
                isDisplayNextStage = false;
            }
            else
            {
                StageTable nextStage = StageTableManager.Instance.GetNextStageTable(stageIndex);
                if (nextStage == null)
                {
                    // 마지막 스테이지였음
                    isDisplayNextStage = false;
                }
                else if (nowProgressStage.chapter != nextStage.chapter)
                {
                    // 챕터가 달라짐
                    isDisplayNextStage = false;
                }
                else
                {
                    int sort = StageTableManager.Instance.GetChapterSort(nextStage.chapter);
                    GameObject nextPrefab = ProjectUtilLibrary.LoadStagePrefab(sort, nextStage.stage);
                    isDisplayNextStage = (nextPrefab != null); // 다음 스테이지 프리팝 유효할 경우에 출력함
                }
            }
        }

        if (gotoNextStagePanel)
        {
            gotoNextStagePanel.SetActive(isDisplayNextStage);
        }

        if (gotoBookShelfPanel)
        {
            gotoBookShelfPanel.SetActive(isPlayRetry);
        }
    }

    private bool IsGameClear()
    {
        return (remainFindCount <= 0);
    }

    private void OnGameStart()
    {
        if (gameEndPanel)
        {
            gameEndPanel.SetActive(false);
        }

        if (gameOverPanel)
        {
            gameOverPanel.SetActive(false);
        }
        
        if (toolBox)
        {
            toolBox.SetActive(true);
        }

        if (baseFindObject)
        {
            // 연출 애님 플레이어 세팅
            animPlayer = baseFindObject.GetComponentInChildren<SpriteAnimPlayer>(true);
            if (animPlayer)
            {
                if (animPlayer.gameObject.activeSelf)
                {
                    animPlayer.gameObject.SetActive(false);
                }
                else
                {
                    animPlayer.RefreshResource();
                }
            }
        }

        if (hintADSpine)
        {
            hintADSpine.gameObject.SetActive(false);
        }
        
        maxHintADCount = ClientTableManager.Instance.GetBaseIntValue("MaxHintADCount", 3);
        remainDelayHintADTime = displayHintADDelayTime;
        remainDisplayHintADTime = 0.0f;

        int chapterSoundCount = (chapterSoundList == null) ? 0 : chapterSoundList.Count;
        if (chapterSoundCount < 1)
        {
            if (chapterSoundAudio)
            {
                chapterSoundList = chapterSoundAudio.GetComponentsInChildren<ObjectKeySoundPlayer>(true).ToList();
            }
        }
        
        if (chapterSoundList != null)
        {
            string chapter = StageTableManager.Instance.GetChapterSort(nowProgressStage.chapter).ToString();
            for (int i = 0; i < chapterSoundList.Count; i++)
            {
                chapterSoundList[i].SetNowActivateKey(chapter);
            }
        }
    }

    public void OnTouchCover(RectTransform parentRect, PointerEventData eventData, float dist)
    {
        #if UNITY_EDITOR
        //CodeUtilLibrary.SetColorLog($"OnTouchCover : dist[{dist}]", "aqua");
        #endif
        if (dist < damagedLifeInRange)
        {
            if (remainTouchCount > 0)
            {
                remainTouchCount--;

                if (remainTouchCountField)
                {
                    remainTouchCountField.SetText(remainTouchCount.ToString());
                }
                
                if (remainTouchCount <= 0)
                {
                    OnGameOver();
                }

                PlayWrongFX(parentRect, eventData);
                //ProjectUtilLibrary.VibrateByBaseValue();
                VibrationLibrary.Instance.ExecuteSimpleVibration();
            }
        }
    }

    private void OnGameOver()
    {
        if (isAlreadyDisplayAddTouchCount)
        {
            OnRealGameOver();
        }
        else
        {
            isAlreadyDisplayAddTouchCount = true;
            DisplayAddTouchAdPanel();
        }
    }

    private void OnRealGameOver()
    {
        AddRecord("GameOver");
        
        if (gameOverSound)
        {
            gameOverSound.PlaySound();
        }
        
        SaveManager.Instance.SaveFile(SaveKind.Record); // 기록 저장
        
        if (gameOverPanel)
        {
            gameOverPanel.SetActive(true);
        }
    }
    #endregion

    #region Util
    public void GotoLobby()
    {
        GotoLobbyDirectionCheck(isPlayRetry ? LobbyInit.ComebackByRetry : LobbyInit.None, stageIndex);
    }
    
    public void GotoLobbyDirectionCheck(LobbyInit init = LobbyInit.None, int param = 0)
    {
        initToLobby = init;
        lobbyInitParam = param;
        SceneManager.sceneLoaded += GameSceneLoaded;
        
        SceneManager.LoadScene("Lobby");
    }

    private void GameSceneLoaded(Scene next, LoadSceneMode mode)
    {
        // 신 전환 후 스크립트 취득
        GameObject lobbyMainObject = GameObject.Find("LobbyMain");
        if (!lobbyMainObject)
        {
            return;
        }
        
        Lobby lobby = lobbyMainObject.GetComponent<Lobby>();
        if (lobby)
        {
            lobby.SetDirection(initToLobby, lobbyInitParam);
        }
    }

    public void GotoLobbyOrNextStage()
    {
        if (IsGameClear())
        {
            // 게임 클리어 다음 스테이지 체크
            GotoNextStage();
        }
        else
        {
            // 게임 오버
            GotoLobby();
        }
    }

    public void GotoNextStage()
    {
        StageTable nextStage = StageTableManager.Instance.GetNextStageTable(stageIndex);
        if (nextStage == null)
        {
            // 마지막 스테이지였음
            GotoLobbyDirectionCheck(isPlayRetry ? LobbyInit.ComebackByRetry : LobbyInit.None, stageIndex);
            return;
        }

        if (nowProgressStage.chapter != nextStage.chapter)
        {
            // 챕터가 달라짐
            GotoLobbyDirectionCheck(isPlayRetry ? LobbyInit.ComebackByRetry : LobbyInit.OpenNewChapter, stageIndex);
            return;
        }
        
        int sort = StageTableManager.Instance.GetChapterSort(nextStage.chapter);
        GameObject nextPrefab = ProjectUtilLibrary.LoadStagePrefab(sort, nextStage.stage);
        if (nextPrefab == null)
        {
            GotoLobby(); // 다음 스테이지 프리팝 유효하지 않으면, 로비로 돌아감
            return;
        }
        
        SetStage(nextStage.stageIndex);
    }
    #endregion

    #region Hint
    public void UseHint()
    {
        if (findList == null)
        {
            return;
        }

        if (findList.Count < 1)
        {
            return;
        }

        if (!ItemManager.Instance.UseFunctionItem(FunctionItemType.Hint))
        {
            return;
        }

        RefreshHintCount();
        int levelIndex = Random.Range(0, findList.Count - 1);
        Level findObject = findList[levelIndex];
        if (!findObject)
        {
            return;
        }

        PlayHintFX(findObject);
        findList.RemoveAt(levelIndex);
        
        if (baseScroll)
        {
            RectTransform rect = CodeUtilLibrary.GetRectTransform(findObject.transform);
            RectTransform parentRect = CodeUtilLibrary.GetRectTransform(findObject.transform.parent.transform);
            if (rect && parentRect)
            {
                Vector2 pos = parentRect.sizeDelta * rect.anchorMin;
                Vector2 backupPos = pos; 
                pos.x += rect.localPosition.x;
                pos.y += rect.localPosition.y;
                baseScroll.horizontalNormalizedPosition = pos.x / parentRect.sizeDelta.x;
                baseScroll.verticalNormalizedPosition = pos.y / parentRect.sizeDelta.y;
                //CodeUtilLibrary.SetColorLog($"UseHint : local[{rect.localPosition}] pos[{pos}], backup[{backupPos}], focus[{baseScroll.horizontalNormalizedPosition}, {baseScroll.verticalNormalizedPosition}]", "aqua");
            }
        }
    }

    public void DisplayPurchaseHint()
    {
        if (buyPanel)
        {
            int productIndex = ShopManager.Instance.ProductTypeToIndex(ItemType.HINT);
            buyPanel.SetPurchaseItem(productIndex, OnSuccessPurchaseHint);
        }
    }

    private void OnSuccessPurchaseHint(ProductTable product)
    {
        if (product != null)
        {
            RefreshHintCount();
        }
    }
    #endregion

    #region Record
    private void AddRecord(FindFXKind fxKind)
    {
        SaveManager.Instance.AddStageRecord(stageIndex, fxKind.ToString());
    }
    
    private void AddRecord(string recordTitle)
    {
        SaveManager.Instance.AddStageRecord(stageIndex, recordTitle);
    }
    #endregion
    
    #region FX
    private void OnEndPlayFX(GameObject fxObject, int index)
    {
        if (!fxList)
        {
            return;
        }
        
        if (fxObject)
        {
            BaseSimplePrefab prefab = fxObject.GetComponent<BaseSimplePrefab>();
            if (prefab)
            {
                fxList.OnSetActivatePrefab(prefab, index, false);
            }
        }
    }
    
    public void PlayTouchFX(Vector2 worldPos)
    {
        if (fxList)
        {
            BaseSimplePrefab prefab = fxList.GetNewActivePrefab((int)FindFXKind.TouchToCollect);
            PlayFX(baseFindRect, prefab, worldPos, baseFindRect, OnEndPlayTouchFX);
        }
    }
    
    public void PlayTouchFX(RectTransform parentRect, PointerEventData eventData, bool isOpenWrapBox)
    {
        if (fxList)
        {
            PlayFindSound();
            BaseSimplePrefab prefab = fxList.GetNewActivePrefab((int)(isOpenWrapBox ? FindFXKind.OpenWrapBox : FindFXKind.TouchToCollect));
            //PlayFX(parentRect, prefab, eventData, displayTouchRect, OnEndPlayTouchFX);
            PlayFX(parentRect, prefab, eventData, baseFindRect, isOpenWrapBox ? OnEndPlayOpenWrapBoxFX : OnEndPlayTouchFX);
        }
    }

    public void OnEndPlayTouchFX(GameObject fxObject)
    {
        OnEndPlayFX(fxObject, (int)FindFXKind.TouchToCollect);
    }
    
    private void PlayWrongFX(RectTransform parentRect, PointerEventData eventData)
    {
        AddRecord(FindFXKind.TouchToWrong);
        
        if (fxList)
        {
            BaseSimplePrefab prefab = fxList.GetNewActivePrefab((int)FindFXKind.TouchToWrong);
            //PlayFX(parentRect, prefab, eventData, displayWrongRect, OnEndPlayWrongFX);
            PlayFX(parentRect, prefab, eventData, baseFindRect, OnEndPlayWrongFX);
        }
    }

    public void OnEndPlayWrongFX(GameObject fxObject)
    {
        OnEndPlayFX(fxObject, (int)FindFXKind.TouchToWrong);
    }
    
    private void PlayHintFX(Level findObject)
    {
        if (findObject)
        {
            if (fxList)
            {
                BaseSimplePrefab prefab = fxList.GetNewActivePrefab((int)FindFXKind.UseHint);
                if (prefab)
                {
                    findObject.SetHint(prefab);
                }
            }
        }

        AddRecord(FindFXKind.UseHint);
    }

    public void OnEndPlayHintFX(GameObject fxObject)
    {
        OnEndPlayFX(fxObject, (int)FindFXKind.UseHint);
    }

    private void PlayFX(RectTransform parentRect, BaseSimplePrefab prefab, PointerEventData eventData, RectTransform displayRect, UnityAction<GameObject> endPlayEvent)
    {
        if (!prefab)
        {
            return;
        }

        RectTransform rect = CodeUtilLibrary.GetRectTransform(prefab.transform);
        GameObject displayPanel = fxList ? fxList.gameObject : null;
        if (rect && displayRect)
        {
            Vector3 pos;
            RectTransformUtility.ScreenPointToWorldPointInRectangle(displayRect, eventData.position,
                mainCamera, out pos);
            rect.SetParent(displayRect);
            rect.position = pos;
        }

        SpineAnimPlayer anim = prefab.GetBasePrefab<SpineAnimPlayer>();
        if (animPlayer)
        {
            anim.mOnEndPlayAllWithObject.RemoveAllListeners();
            anim.mOnEndPlayAllWithObject.AddListener(endPlayEvent);
            anim.PlayAnim();
        }
    }

    private void PlayFX(RectTransform parentRect, BaseSimplePrefab prefab, Vector2 worldPos, RectTransform displayRect, UnityAction<GameObject> endPlayEvent)
    {
        if (!prefab)
        {
            return;
        }

        RectTransform rect = CodeUtilLibrary.GetRectTransform(prefab.transform);
        GameObject displayPanel = fxList ? fxList.gameObject : null;
        if (rect && displayRect)
        {
            Vector3 pos;
            RectTransformUtility.ScreenPointToWorldPointInRectangle(displayRect, worldPos,
                mainCamera, out pos);
            rect.SetParent(displayRect);
            rect.position = pos;
        }

        SpineAnimPlayer anim = prefab.GetBasePrefab<SpineAnimPlayer>();
        if (animPlayer)
        {
            anim.mOnEndPlayAllWithObject.RemoveAllListeners();
            anim.mOnEndPlayAllWithObject.AddListener(endPlayEvent);
            anim.PlayAnim();
        }
    }
    
    public void PlayOpenWrapBoxFX(RectTransform parentRect, PointerEventData eventData)
    {
        if (fxList)
        {
            BaseSimplePrefab prefab = fxList.GetNewActivePrefab((int)FindFXKind.OpenWrapBox);
            //PlayFX(parentRect, prefab, eventData, displayTouchRect, OnEndPlayTouchFX);
            PlayFX(parentRect, prefab, eventData, baseFindRect, OnEndPlayTouchFX);
        }
    }

    public void OnEndPlayOpenWrapBoxFX(GameObject fxObject)
    {
        OnEndPlayFX(fxObject, (int)FindFXKind.OpenWrapBox);
    }
    #endregion

    #region Sound
    private void PlayFindSound()
    {
        if (!SaveManager.Instance.GetSoundOptionValue(false))
        {
            return;
        }
        
        if (findSoundList == null)
        {
            return;
        }

        if (findSoundList.Count < 1)
        {
            return;
        }

        int index = Random.Range(0, findSoundList.Count);
        AudioSource nowPlaySource = findSoundList[index];
        if (nowPlaySource)
        {
            //#if UNITY_EDITOR
            //CodeUtilLibrary.SetColorLog($"PlayFindSound[{index}] : {nowPlaySource.clip}", "aqua");
            //#endif
            nowPlaySource.Play();
        }
    }
    #endregion

    #region Item
    private void SetHintCount(int count)
    {
        if (!hintSwitcher)
        {
            if (hintCountField)
            {
                hintCountField.SetText($"X{count}");
            }
            return;
        }
        
        int nowActiveSwitcherIndex = hintSwitcher.GetActiveIndex();
        bool isHaveHint = (count > 0);
        int newActiveSwitcherIndex = isHaveHint ? 0 : 1;

        if (isHaveHint)
        {
            if (hintCountField)
            {
                hintCountField.SetText($"X{count}");
            }
        }
        
        if (nowActiveSwitcherIndex != newActiveSwitcherIndex)
        {
            hintSwitcher.SetActiveByChildIndex(newActiveSwitcherIndex);

            if (isHaveHint)
            {
                if (hintAnimPlayer)
                {
                    hintAnimPlayer.StopAnim();
                }
            }
            else
            {
                if (hintAnimPlayer)
                {
                    hintAnimPlayer.PlayAnim();
                }
            }
        }
    }

    private void RefreshHintCount()
    {
        SetHintCount(ItemManager.Instance.GetHaveFunctionItemCount(FunctionItemType.Hint));
    }
    #endregion

    #region AD
    private void DisplayHintAD()
    {
        if (maxHintADCount < 1)
        {
            return;
        }

        if (displayAdHintSound)
        {
            displayAdHintSound.PlaySound();
        }

        // 힌트 나오는 횟수 적용
        maxHintADCount--;
        
        if (hintADSpine)
        {
            hintADSpine.gameObject.SetActive(true);
        }
        
        if (hintADAnim)
        {
            hintADAnim.PlayAnim();
        }
        
        remainDisplayHintADTime = onceDisplayHintADTime;
    }

    private void EraseHintAD()
    {
        SpineUtilLibrary.StopSpineAnim(hintADSpine);
        TrackEntry animEntry = SpineUtilLibrary.PlaySpineAnim(hintADSpine, "Ad_Up", false, OnEraseHintAD);
        remainDisplayHintADTime = 0.0f;
        remainDelayHintADTime = displayHintADDelayTime;
    }

    private void OnEraseHintAD()
    {
        if (hintADSpine)
        {
            hintADSpine.gameObject.SetActive(false);
        }
    }

    public void OnClickHintAD()
    {
        AdvertisementManager.Instance.PlayAd(OnEndWatchHintAD);
    }

    private void OnEndWatchHintAD(AdWatchError error)
    {
        if (error == AdWatchError.Success)
        {
            ItemManager.Instance.AddFunctionItem(FunctionItemType.Hint);
            RefreshHintCount();
            ItemManager.Instance.SaveHaveItem();
        
            //UseHint();
        }
        
        EraseHintAD();
    }

    private void InitDisplayHintADTime()
    {
        if (remainDelayHintADTime > 0.0f)
        {
            remainDelayHintADTime = displayHintADDelayTime;
        }
    }
    #endregion

    #region Debug
    private void AddDebugLog(string log)
    {
        if (!string.IsNullOrEmpty(debugStr))
        {
            //debugStr += "\n";
        }

        //debugStr += log;
        debugStr = log;

        if (debugField)
        {
            debugField.SetText(debugStr);
        }
    }
    #endregion
    
    #region Touch
    private void CheckMultiTouch()
    {
        int count = Input.touchCount;
        if (count == 2)
        {
            if (baseScroll)
            {
                baseScroll.enabled = false; // 2 터치일 때 스크롤링 이동을 막는다.
            }
            
            Touch touch1 = Input.GetTouch(0);
            Touch touch2 = Input.GetTouch(1);
            float nowDist = Vector2.Distance(touch1.position, touch2.position);
            if (touchDist > 0.0f)
            {
                float delta = nowDist - touchDist;
                delta *= 0.001f;
                AddScaleRate(centerPos, delta);
            }
            else if (baseFindRect)
            {
                centerPos = (touch1.position + touch2.position) * 0.5f;
            }
            
            touchDist = nowDist;
            touchedScaleRate = nowScaleRate;
        }
        else
        {
            touchDist = 0.0f;
            if (count < 2)
            {
                if (baseScroll)
                {
                    baseScroll.enabled = true; // 2포인트 이하면 스크롤 이동을 다시 가능하게함
                }
            }
        }
    }

    private void DisplayAddTouchAdPanel()
    {
        if (buyTouchCountPanel)
        {
            buyTouchCountPanel.gameObject.SetActive(true);
        }
    }

    public void OnClickWatchAddTouchAd()
    {
        AdvertisementManager.Instance.PlayAd(OnEndWatchAddTouchAd);
    }

    public void OnEndWatchAddTouchAd(AdWatchError error)
    {
        if (error == AdWatchError.Success)
        {
            int addCount = ClientTableManager.Instance.GetBaseIntValue("AddTouchCountByAd");
            remainTouchCount += addCount;
            RefreshRemainTouchCount();
            
            if (buyTouchCountPanel)
            {
                buyTouchCountPanel.gameObject.SetActive(false);
            }
        }
        else
        {
            OnEraseWatchAddTouchAd();
        }
    }

    public void OnEraseWatchAddTouchAd()
    {
        if (buyTouchCountPanel)
        {
            buyTouchCountPanel.gameObject.SetActive(false);
        }
        
        OnRealGameOver();
    }
    #endregion

    #region Scroll
    private void ScrollingCheck()
    {
        if (Input.GetKeyUp(KeyCode.UpArrow))
        {
            AddScaleRate(Input.mousePosition, 0.01f);
        }
        if (Input.GetKeyUp(KeyCode.DownArrow))
        {
            AddScaleRate(Input.mousePosition, -0.01f);
        }
    }
    #endregion
}