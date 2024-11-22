using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Library;
using Script.Component;
using TMPro;
using UI.Common;
using UI.Common.Base;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

// _SJ      물건 찾는 모드
public class FindObjectMode : MonoBehaviour
{
    private enum FindFXKind
    {
        TouchToCollect = 0,
        TouchToWrong,
        UseHint,
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
    
    [FormerlySerializedAs("PrefabBaseObject")] [SerializeField]
    private GameObject prefabBaseObject;

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

    [FormerlySerializedAs("BottomToolBoxRect")] [SerializeField] private RectTransform bottomToolBoxRect;
    [FormerlySerializedAs("BottonToolBoxDefaultSize")] [SerializeField] private float bottonToolBoxDefaultSize = 200.0f;
    [FormerlySerializedAs("BannerAdPanel")] [SerializeField] private GameObject bannerAdPanel;
    
    [FormerlySerializedAs("TopToolBoxRect")] [SerializeField] private RectTransform topToolBoxRect;

    private GameObject baseFindObject; // 찾기 페널 (stage1-1등, 로드 후 인스턴스 생성 한 파일)
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

    // Start is called before the first frame update
    void Start()
    {
        isPlayFXSound = SaveManager.Instance.GetSoundOptionValue(false);
        if (findSoundAudio)
        {
            findSoundList = findSoundAudio.GetComponentsInChildren<AudioSource>(true).ToList();
        }

        damagedLifeInRange = ClientTableManager.Instance.GetBaseFloatValue("DamagedLifeInRange", 10.0f);
        
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
        
        if (bottomToolBoxRect)
        {
            // 배너 광고 및 하단 세이프존 처리
            bool isActivateAD = ItemManager.Instance.IsActivateAD(); // 광고 활성화 여부
            float bottomHeight = bottonToolBoxDefaultSize;
            Vector2 safeZonePos = ResolutionManager.Instance.GetSafeZonePos();
            float safeZoneBottomHeight = safeZonePos.y;
            if (isActivateAD)
            {
                float bannerHeight = ClientTableManager.Instance.GetBaseFloatValue("BannerADHeight", 200.0f);
                if (bannerHeight < safeZoneBottomHeight)
                {
                    bannerHeight = safeZoneBottomHeight;
                }

                bottomHeight += bannerHeight;
                
                if (bannerAdPanel)
                {
                    RectTransform bannerRect = CodeUtilLibrary.GetRectTransform(bannerAdPanel);
                    if (bannerRect)
                    {
                        Vector2 bannerSize = bannerRect.sizeDelta;
                        bannerSize.y = bannerHeight;
                        bannerRect.sizeDelta = bannerSize;
                    }
                    
                    bannerAdPanel.SetActive(true);
                }
            }
            else
            {
                bottomHeight += safeZoneBottomHeight;
                if (bannerAdPanel)
                {
                    bannerAdPanel.SetActive(false);
                }
            }

            Vector2 bottomSize = bottomToolBoxRect.sizeDelta;
            bottomSize.y = bottomHeight;
            bottomToolBoxRect.sizeDelta = bottomSize;
            //toolBoxHeight += bottomHeight;
            
            if (scrollViewportRect)
            {
                Vector2 viewportPos = scrollViewportRect.offsetMin;
                viewportPos.y += bottomSize.y;
                scrollViewportRect.offsetMin = viewportPos;
            }
        }
        
#if UNITY_EDITOR
        if (stageIndex < 1)
        {
            return;
        }

        // 테스트 기능 (스테이지 지정시 바로 시작)
        SetStage(stageIndex);
#endif
    }

    /*
    // Update is called once per frame
    void Update()
    {
    }*/

    #region Stage
    public void SetStage(int stage)
    {
        stageIndex = stage;

        nowProgressStage = StageTableManager.Instance.GetStageTable(stageIndex);
        if (nowProgressStage == null)
        {
            return;
        }
        
        int chapter = StageTableManager.Instance.GetChapterSort(nowProgressStage.chapter);
        int chapterStage = nowProgressStage.stage;
        
        InitRemainTouchCount(nowProgressStage.touchCount);
        
        if (!prefabBaseObject)
        {
            return;
        }
        
        string prefabName = $"stage{chapter}-{chapterStage}";
        string path = "Prefabs/Stage/";
        GameObject stagePrefab = ResourceManager.Instance.LoadPrefab<GameObject>(new ResourcePathData(path), prefabName);
        if (!stagePrefab)
        {
            GotoLobby(); // 일단 로비로 강제송환한다.
            return;
        }
        
        GameObject newStagePrefab = Instantiate<GameObject>(stagePrefab, Vector3.zero, Quaternion.identity, prefabBaseObject.transform);
        if (!newStagePrefab)
        {
            return;
        }

        InitStagePrefab(newStagePrefab);
        
        SetDefaultScaleRate();
        OnGameStart();
    }
    #endregion

    #region StagePrefab
    public void SetScaleRate(float scaleRate)
    {
        if (scaleRateValueField)
        {
            scaleRateValueField.SetText($"X{scaleRate}");
            //scaleRateValueField.SetText(string.Format("X{0:F2}", scaleRate));
        }
        
        if (!baseFindObject)
        {
            return;
        }

        Vector3 rate = new Vector3(1.0f, 1.0f, 0.0f) * scaleRate;
        prefabBaseObject.transform.localScale = rate;
        baseFindObject.transform.localScale = rate;
        RectTransform baseRect = CodeUtilLibrary.GetRectTransform(prefabBaseObject);
        if (!baseRect)
        {
            return;
        }
        
        baseRect.sizeDelta = imageSize * scaleRate;

        // 가운데 맞춤
        if (baseScroll)
        {
            baseScroll.horizontalNormalizedPosition = 0.5f;
            baseScroll.verticalNormalizedPosition = 0.5f;
        }
    }

    private void InitStagePrefab(GameObject stagePrefab)
    {
        int findCount = 0;
        int specialCount = 0;
        int i;

        GameObject lastFindObject = baseFindObject;
        baseFindObject = stagePrefab;
        
        //stagePrefab.transform.localPosition = Vector3.zero;
        RectTransform rect = CodeUtilLibrary.GetRectTransform(stagePrefab);
        if (rect)
        {
            imageSize = rect.sizeDelta;
            //imageSize.y += toolBoxHeight;
            rect.sizeDelta = imageSize;
            rect.anchoredPosition = Vector2.zero;
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
                touchRect.position = Vector3.zero;
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

            findObject.SetHint(); // 힌트 오브젝트 초기화 처리
        }
    }

    private void OnFindObject(Level findObject)
    {
        PlayFindSound();
        if (findObject.IsEndFindObject())
        {
            return;
        }
        
        if (findObject)
        {
            BaseSimplePrefab hint = findObject.GetHint();
            if (hint)
            {
                OnEndPlayHintFX(hint.gameObject);
            }
        }

        int keyIndex = findObject.index;
        for (int i = 0; i < findList.Count; i++)
        {
            if (findList[i].index == keyIndex)
            {
                findList.RemoveAt(i);
                break;
            }
        }

        AddRecord("TouchToCollect");
        
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
        
        /*
        GameObject activateObject = baseSwitcher ? baseSwitcher.GetActiveObject() : null;
        RectTransform prefabRect = activateObject ? CodeUtilLibrary.GetRectTransform(activateObject) : null;
        if (!prefabRect)
        {
            return;
        }

        Vector2 screenSize = Screen.safeArea.size;//SafeZoneChecker.GetScreenSizeWithSafeZone();
        //Vector2 screenSize = new Vector2(Screen.width, Screen.height);
        
        float screenRate = screenSize.x / screenSize.y;
        Vector2 prefabSize = prefabRect.sizeDelta;
        prefabSize.y += 30.0f;
        float prefabRate = prefabSize.x / prefabSize.y;
        float computeRate = 0.0f;
        bool isStandardHeight = true;
        
        if (screenRate >= prefabRate)
        {
            computeRate = screenSize.y / prefabSize.y;
        }
        else
        {
            isStandardHeight = false;
            computeRate = screenSize.x / prefabSize.x;
        }
        
        //prefabSize *= computeRate;
        SetScaleRate(computeRate);
        
        if (animPlayer)
        {
            animPlayer.transform.SetParent(activateObject.transform);
            RectTransform animRect = CodeUtilLibrary.GetRectTransform(animPlayer.transform);
            if (animRect)
            {
                animRect.anchorMin = Vector2.zero;
                animRect.anchorMax = Vector2.one;
                animRect.position = Vector3.zero;
                animRect.sizeDelta = Vector2.zero;
            }
            animPlayer.gameObject.SetActive(true);
        }
        
        RectTransform baseRect = CodeUtilLibrary.GetRectTransform(prefabBaseObject);
        if (baseRect)
        {
            Vector2 pivot = baseRect.pivot;
            if (isStandardHeight)
            {
                pivot.y = 0.5f;
            }
            else
            {
                pivot.x = 0.5f;
            }
            baseRect.pivot = pivot;
        }*/
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
        Invoke("OnDisplayGameEndPanel", displayEndDirectionDelayTime);
    }

    private void OnDisplayGameEndPanel()
    {
        if (gameEndPanel)
        {
            gameEndPanel.SetActive(true);
        }

        if (gameEndPanel)
        {
            gameEndPlayer.PlayAnim();
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
        AddRecord("GameOver");
        SaveManager.Instance.SaveFile(SaveKind.Record); // 기록 저장
        if (gameOverPanel)
        {
            gameOverPanel.SetActive(true);
        }
    }
    #endregion

    #region Util
    public void GotoLobby(bool isPlayEndChapterDirection = false)
    {
        if (isPlayEndChapterDirection)
        {
            SceneManager.sceneLoaded += GameSceneLoaded;
        }
        
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
            lobby.ToStartByInGame();
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
            GotoLobby(true); // 마지막 스테이지였음
            return;
        }

        if (nowProgressStage.chapter != nextStage.chapter)
        {
            GotoLobby(true); // 챕터가 달라짐
            return;
        }
        
        SetStage(nextStage.stageIndex);
    }

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

        int levelIndex = Random.Range(0, findList.Count - 1);
        Level findObject = findList[levelIndex];
        if (!findObject)
        {
            return;
        }

        PlayHintFX(findObject);

        // 포장지 없는 애들만 목록에서 없앰 (힌트 재출력 대비)
        if (!findObject.IsWraped())
        {
            findList.RemoveAt(levelIndex);
        }
        
        /*
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
                //baseScroll.horizontalNormalizedPosition = pos.x / parentRect.sizeDelta.x;
                //baseScroll.verticalNormalizedPosition = pos.y / parentRect.sizeDelta.y;
                CodeUtilLibrary.SetColorLog($"UseHint : local[{rect.localPosition}] pos[{pos}], backup[{backupPos}], focus[{baseScroll.horizontalNormalizedPosition}, {baseScroll.verticalNormalizedPosition}]", "aqua");
            }
        }*/
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
    
    public void PlayTouchFX(RectTransform parentRect, PointerEventData eventData)
    {
        if (fxList)
        {
            BaseSimplePrefab prefab = fxList.GetNewActivePrefab((int)FindFXKind.TouchToCollect);
            PlayFX(parentRect, prefab, eventData, displayTouchRect, OnEndPlayTouchFX);
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
            PlayFX(parentRect, prefab, eventData, displayWrongRect, OnEndPlayWrongFX);
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
                eventData.pressEventCamera, out pos);
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
    #endregion

    #region Sound
    private void PlayFindSound()
    {
        if (!isPlayFXSound)
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

        int index = Random.Range(0, findSoundList.Count - 1);
        AudioSource nowPlaySource = findSoundList[index];
        if (nowPlaySource)
        {
            nowPlaySource.Play();
        }
    }
    #endregion
}