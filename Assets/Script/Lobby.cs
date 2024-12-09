using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Library;
using Script.Library;
using Spine;
using Spine.Unity;
using UI.Common;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Event = Spine.Event;

public enum LobbyInit
{
    None = 0, // 일반 입장 상태 (일반 타이틀 연출 사용)
    OpenNewChapter, // 새로운 챕터 열림
    ComebackByRetry, // 다시 하기에서 돌아왔을 때
}

public class Lobby : MonoBehaviour
{
    private enum LobbyState
    {
        None = 0, // 일반 타이틀 상태
        PlayTitleDirection,
        EndTitleDirection,
        PlayDirection,
        ReadyToStart,
        PlayStartTouchDirection,
        OnStart,
        ZoomOut,
    }

    [FormerlySerializedAs("LogoSpine")] [SerializeField] private SkeletonGraphic logoSpine;
    [FormerlySerializedAs("MachineSpine")] [SerializeField] private SkeletonGraphic machineSpine;
    [FormerlySerializedAs("ChapterTitleField")] [SerializeField] private LocalizeTextField chapterTitleField;

    [FormerlySerializedAs("NoticeTouchMachineObject")] [SerializeField] private GameObject noticeTouchMachineObject;
    [FormerlySerializedAs("TitleObject")] [SerializeField] private GameObject titleObject;
    
    [FormerlySerializedAs("BackButton")] [SerializeField] private GameObject backButton;
    [FormerlySerializedAs("TouchGameStartPlayer")] [SerializeField] private SpineAnimPlayer touchGameStartPlayer;

    [FormerlySerializedAs("StartButton")] [SerializeField] private GameObject startButton;
    [FormerlySerializedAs("ToolBoxObject")] [SerializeField] private GameObject toolBoxObject;
    [FormerlySerializedAs("VisibleButtonSwitcher")] [SerializeField] private ObjectSwitcher visibleButtonSwitcher;

    [FormerlySerializedAs("DropFXParticle")] [SerializeField] private ParticleSystem dropFXParticle;

    [FormerlySerializedAs("TitleImage")] [SerializeField] private Image titleImage;
    [FormerlySerializedAs("TitleButton")] [SerializeField] private GameObject titleButton;
    [FormerlySerializedAs("TitleDirectionAnim")] [SerializeField] private SpineAnimPlayer titleDirectionAnim;

    [FormerlySerializedAs("BookOpenPanel")] [SerializeField] private GameObject bookOpenPanel;
    [FormerlySerializedAs("BookOpenDirectionAnim")] [SerializeField] private SpineAnimPlayer bookOpenDirectionAnim;
    [FormerlySerializedAs("BookCloseDirectionAnim")] [SerializeField] private SpineAnimPlayer bookCloseDirectionAnim;
    [FormerlySerializedAs("BookCloseImage")] [SerializeField] private Image bookCloseImage;
    [FormerlySerializedAs("StartBookOpenDirectioinDelayTime")] [SerializeField] private float startBookOpenDirectioinDelayTime = 1.5f;
    
    [FormerlySerializedAs("BgColorSetter")] [SerializeField] private ObjectKeyColorSetter bgColorSetter;
    [SerializeField] private ObjectKeyFXImageSetter fxImageSetter;
    
    [SerializeField] private BookShelfPopup bookShelf;

    [SerializeField][SpineEvent] private string changeSkinEventName;
    [SerializeField] private List<GameObject> changeSkinDeactivateObjectList; // 스킨 바꿀때 off -> on 처리 해줄 오브젝트 목록
    [SerializeField] private GameObject themeSoundAudio;

    [SerializeField] private BuyItemPanel adRemoverBuyPopup;

    [SerializeField] private SoundPlayer chapterChangeSound;
    [SerializeField] private SoundPlayer themeChangeSound;

    private int stageValue = 0;
    private StageTable nextStage;

    private int lastClearStage;
    private StageTable lastStage;
    private LobbyState lobbyState = LobbyState.None;
    private int machinePreviewIndex; // 자판기 확대 전 번호
    private int machineStartIndex; // 자판기 확대 후 번호

    private List<ObjectKeyActivator> themeActivatorList;

    private LobbyInit initState = LobbyInit.None;
    private int initParam = 0; // 초기화 연출에 같이 사용할 인자값

    private bool isGameRetry = false;

    private bool isChangeSkin = false;
    private List<ObjectKeySoundPlayer> themeSoundList;
    
    // Start is called before the first frame update
    void Start()
    {
        PreloadManager.ExecutePreload();
        themeActivatorList = ObjectKeyActivator.GetAllKeyActivatorList();

        if (themeSoundAudio)
        {
            themeSoundList = themeSoundAudio.GetComponentsInChildren<ObjectKeySoundPlayer>(true).ToList();
        }
        
        if (machineSpine)
        {
            SkeletonUtility bone = SpineUtilLibrary.SpawnHierarchySpineBone(machineSpine);
            if (bone)
            {
                Dictionary<string, Transform> transformMap = CodeUtilLibrary.MakeTransformNameMap(bone.gameObject);
                if (transformMap != null)
                {
                    SpineUtilLibrary.AttachToSpineBone(transformMap, "Start", startButton);
                
                    if (chapterTitleField)
                    {
                        SpineUtilLibrary.AttachToSpineBone(transformMap, "Title", chapterTitleField.gameObject);
                    }
                }
            }
            
            SpineUtilLibrary.BindSpineEventFunction(machineSpine, OnMachineEvent);
        }

        RefreshTheme();

        if (titleImage)
        {
            SpineUtilLibrary.AttachToSpineBone(logoSpine, "Logo", titleImage.gameObject);
        }

        Vector2 safeZone = ResolutionManager.Instance.GetSafeZonePos();
        if (toolBoxObject)
        {
            RectTransform toolBoxRect = CodeUtilLibrary.GetRectTransform(toolBoxObject);
            if (toolBoxRect)
            {
                //Vector2 nowPos = toolBoxRect.anchoredPosition;
                //nowPos.y += safeZone.y;
                //toolBoxRect.anchoredPosition = nowPos;
                Vector2 nowSize = toolBoxRect.sizeDelta;
                nowSize.y += safeZone.y;
                toolBoxRect.sizeDelta = nowSize;
            }
        }

        if (visibleButtonSwitcher)
        {
            RectTransform visibleRect = CodeUtilLibrary.GetRectTransform(visibleButtonSwitcher.gameObject);
            if (visibleRect)
            {
                Vector2 nowPos = visibleRect.anchoredPosition;
                nowPos.y += safeZone.y;
                visibleRect.anchoredPosition = nowPos;
            }
        }
        
        lastClearStage = SaveManager.Instance.GetClearStage();
        lastStage = StageTableManager.Instance.GetStageTable(lastClearStage);

        SetStartStageIndex();
        
        StartTitleLoopDirection();

        bool isExecuteStartDirection = (initState == LobbyInit.None);
        
        if (isExecuteStartDirection)
        {
            StartTitleDirection();
            
            if (bookCloseImage)
            {
                bookCloseImage.color = Color.white;
            }

            if (bookCloseDirectionAnim)
            {
                bookCloseDirectionAnim.PlayLastAnim();
            }
        }
        else
        {
            if (titleDirectionAnim)
            {
                titleDirectionAnim.PlayLastAnim();
            }
        }

        switch (initState)
        {
            case LobbyInit.OpenNewChapter:
            {
                StartOpenBookDirection();
            }
                break;
            case LobbyInit.ComebackByRetry:
            {
                if (bookShelf)
                {
                    bookShelf.gameObject.SetActive(true);
                    bookShelf.SetDisplayStageBook(initParam);
                }
            }
                break;

            default:  /// [2024/12/9 kmlee123, 24-12-9-1] (code add)
            {
                AdvertisementManager.Instance.LoadRewardedAd();  // [2024/12/9 kmlee123, 24-12-9-1] (code add)
            }
            break;
        }

        AdvertisementManager.Instance.ReleaseBannerAd();  // [2024/12/9 kmlee123, 24-12-9-1] (code add)
    }

    /*
    // Update is called once per frame
    void Update()
    {
        if (machineSpine)
        {
            machineSpine.AnimationState.ToString()
        }
    }*/

    private void StartOpenBookDirection()
    {
        if (bookCloseImage)
        {
            bookCloseImage.color = Color.clear;
        }
        
        if (titleButton)
        {
            titleButton.SetActive(true);
        }
        
        if (bookOpenPanel)
        {
            bookOpenPanel.SetActive(true);
        }

        Invoke("StartPlayOpenBookAnim", startBookOpenDirectioinDelayTime);
    }

    private void StartPlayOpenBookAnim()
    {
        if (bookOpenDirectionAnim)
        {
            bookOpenDirectionAnim.gameObject.SetActive(true);
            bookOpenDirectionAnim.PlayAnim();
        }
    }

    public void OnEndBookDirection()
    {
        if (bookCloseImage)
        {
            bookCloseImage.color = Color.white;
        }
        
        if (bookOpenPanel)
        {
            bookOpenPanel.SetActive(false);
        }

        if (titleButton)
        {
            titleButton.SetActive(false);
        }

        if (initState == LobbyInit.OpenNewChapter)
        {
            if (lastStage != null)
            {
                int chapterSort = StageTableManager.Instance.GetChapterSort(lastStage.chapter);
                if (chapterSort % 2 == 0)
                {
                    // 짝수 챕터 클리어 했을 경우, 다음 자판기로 교체 애니메이션 출력
                    SpineUtilLibrary.PlaySpineAnim(machineSpine, "StageChange", false, OnEndPlayStageChange);
                }
                else
                {
                    OnEndPlayStageChange();
                }
            }
        }
    }

    private void OnEndPlayStageChange()
    {
        if (chapterChangeSound)
        {
            chapterChangeSound.PlaySound();
        }
        
        SetNextStage(StageTableManager.Instance.GetNextStageTable(stageValue));
        if (nextStage != null)
        {
            stageValue = nextStage.stageIndex;
        }
        
        StartTitleLoopDirection();
    }

    private void OnMachineEvent(TrackEntry trackEntry, Event e)
    {
        if (SpineUtilLibrary.IsEqualEvent(e, changeSkinEventName))
        {
            SetEquippedTheme();
        }
    }

    public void StartLevelStage()
    {
        string sceneName = "FindObjectScene";//$"Level {level}";
        SceneManager.sceneLoaded += GameSceneLoaded;
        SceneManager.LoadScene(sceneName);
    }

    public void SetDirection(LobbyInit init = LobbyInit.None, int param = 0)
    {
        initState = init;
        initParam = param;
    }

    public void OnClickStartOnTitleDirection()
    {
        /*
        if (lobbyState == LobbyState.EndTitleDirection)
        {
            InitMachineAnimState();
        }*/
    }

    public void InitMachineAnimState()
    {
        lobbyState = LobbyState.None;
        if (backButton)
        {
            backButton.SetActive(false);
        }
        
        if (noticeTouchMachineObject)
        {
            noticeTouchMachineObject.SetActive(true);
        }

        if (toolBoxObject)
        {
            toolBoxObject.SetActive(true);
        }

        if (visibleButtonSwitcher)
        {
            visibleButtonSwitcher.gameObject.SetActive(true);
        }

        if (titleButton)
        {
            titleButton.SetActive(false);
        }

        if (titleObject)
        {
            titleObject.SetActive(true);
        }

        if (nextStage == null)
        {
            SetLockState();
        }
    }

    private void StartTitleDirection()
    {
        if (backButton)
        {
            backButton.SetActive(false);
        }

        if (noticeTouchMachineObject)
        {
            noticeTouchMachineObject.SetActive(true);
        }

        if (toolBoxObject)
        {
            toolBoxObject.SetActive(false);
        }

        if (visibleButtonSwitcher)
        {
            visibleButtonSwitcher.gameObject.SetActive(false);
        }

        if (titleObject)
        {
            titleObject.SetActive(true);
        }

        if (titleButton)
        {
            titleButton.SetActive(true);
        }
        
        if (titleDirectionAnim)
        {
            /*
            if (titleImage)
            {
                Sprite logo = CodeUtilLibrary.LoadLocalizeSprite("ui/Logo/", "Logo_{0}");
                titleImage.sprite = logo;
            }*/
            
            titleDirectionAnim.PlayAnim();
        }
        else
        {
            OnClickStartOnTitleDirection();
        }
    }

    private void StartTitleLoopDirection()
    {
        InitMachineAnimState();
        
        if (titleButton)
        {
            titleButton.SetActive(false);
        }
        
        if (machineSpine)
        {
            if (nextStage != null)
            {
                DisplayLoopPreviewMachine();
            }
            else
            {
                SetLockState();
            }
        }
    }

    public void OnEndZoomOut()
    {
        DisplayLoopPreviewMachine();
        OnEndStarTitleDirection();
    }

    private void DisplayLoopPreviewMachine()
    {
        string animName = $"Machine1Button{machinePreviewIndex}On";
        //string animName = $"Machine1Button{0}On";
#if UNITY_EDITOR
        //CodeUtilLibrary.SetColorLog($"DisplayLoopPreviewMachine : animName[{animName}]", "aqua");
#endif
        
        //SpineUtilLibrary.PlaySpineAnim(machineSpine, $"Machine1Button0On", true);
        //SpineUtilLibrary.StopSpineAnim(machineSpine);
        SpineUtilLibrary.PlaySpineAnim(machineSpine, animName, true);
    }

    public void OnEndStarTitleDirection()
    {
        lobbyState = LobbyState.EndTitleDirection;
        InitMachineAnimState();
    }

    private void SetZoomOut()
    {
        lobbyState = LobbyState.ZoomOut;
        if (backButton)
        {
            backButton.SetActive(false);
        }
        
        string animName = $"Machine3Button{machinePreviewIndex}On";
#if UNITY_EDITOR
        //CodeUtilLibrary.SetColorLog($"PlayMachineDirection : animName[{animName}]", "aqua");
#endif
        SpineUtilLibrary.PlaySpineAnim(machineSpine, animName, false, OnEndZoomOut);
    }

    public void StartBySavedLevel()
    {
        lobbyState = LobbyState.OnStart;
        if (stageValue != 0)
        {
            isGameRetry = false;
            StartLevelStage();
        }
        else
        {
#if UNITY_EDITOR
            CodeUtilLibrary.SetColorLog("StartLevel stageIndex is invalid!!", "red");
#endif
        }
    }

    private void GameSceneLoaded(Scene next, LoadSceneMode mode)
    {
        // 신 전환 후 스크립트 취득
        GameObject gameManagerObject = GameObject.Find("GameManager");
        if (!gameManagerObject)
        {
            return;
        }
        
        FindObjectMode findObjectMode = gameManagerObject.GetComponent<FindObjectMode>();
        if (findObjectMode)
        {
            findObjectMode.SetStage(stageValue, isGameRetry);
        }
    }

    private void PlayMachineDirection()
    {
        lobbyState = LobbyState.PlayDirection;

        if (noticeTouchMachineObject)
        {
            noticeTouchMachineObject.SetActive(false);
        }

        if (titleObject)
        {
            titleObject.SetActive(false);
        }

        if (toolBoxObject)
        {
            toolBoxObject.SetActive(false);
        }

        if (visibleButtonSwitcher)
        {
            visibleButtonSwitcher.gameObject.SetActive(false);
        }

        string animName = $"Machine2Button{machinePreviewIndex}On";
        #if UNITY_EDITOR
        //CodeUtilLibrary.SetColorLog($"PlayMachineDirection : animName[{animName}]", "aqua");
        #endif
        SpineUtilLibrary.PlaySpineAnim(machineSpine, animName, false, OnEndPlayMachineDirection);
    }

    public void OnClickMachine()
    {
        if (nextStage == null)
        {
            return;
        }
        
        if (!IsVisibleUI())
        {
            return;
        }
        
        if (lobbyState == LobbyState.None)
        {
            PlayMachineDirection();
        }
        /*else if (lobbyState == LobbyState.ReadyToStart)
        {
            PlayMachineTouchDirection();
        }*/
    }

    public void OnEndPlayMachineDirection()
    {
        string animName = $"button{machineStartIndex}";
        SpineUtilLibrary.PlaySpineAnim(machineSpine, animName, true);
        
        if (backButton)
        {
            backButton.SetActive(true);
        }
        
        lobbyState = LobbyState.ReadyToStart;
    }

    public void OnClickStart()
    {
        if (lobbyState == LobbyState.None)
        {
            PlayMachineDirection();
        }
        else if (lobbyState == LobbyState.ReadyToStart)
        {
            PlayMachineTouchDirection();
        }
    }

    private void PlayMachineTouchDirection()
    {
        if (!touchGameStartPlayer)
        {
            StartBySavedLevel();
            return;
        }

        if (dropFXParticle)
        {
            dropFXParticle.gameObject.SetActive(false);
        }

        if (backButton)
        {
            backButton.SetActive(false);
        }

        if (noticeTouchMachineObject)
        {
            noticeTouchMachineObject.SetActive(false);
        }

        if (titleObject)
        {
            titleObject.SetActive(false);
        }

        if (chapterTitleField)
        {
            chapterTitleField.gameObject.SetActive(false);
        }
        
        lobbyState = LobbyState.PlayStartTouchDirection;
        touchGameStartPlayer.PlayAnim(machineStartIndex);
    }

    private void SetLockState()
    {
        SpineUtilLibrary.PlaySpineAnim(machineSpine, "Lock", true);

        if (chapterTitleField)
        {
            chapterTitleField.SetText("ComingSoon");
        }

        if (noticeTouchMachineObject)
        {
            noticeTouchMachineObject.SetActive(false);
        }
    }

    private void SetStartStageIndex()
    {
        int clearStage = SaveManager.Instance.GetClearStage();
        int savedStage = clearStage;
        if (!StageTableManager.Instance.IsValidStage(savedStage))
        {
            savedStage = StageTableManager.Instance.GetMinStageIndex();
        }
        else
        {
            if (initState != LobbyInit.OpenNewChapter)
            {
                savedStage = StageTableManager.Instance.GetNextStageTableIndex(savedStage);
            }
        }

        /*
        if (!StageTableManager.Instance.IsValidStage(savedStage))
        {
            //savedStage = clearStage;
        }*/

        stageValue = savedStage;
        SetNextStage(StageTableManager.Instance.GetStageTable(stageValue));
    }

    private void SetNextStage(StageTable next)
    {
        nextStage = next;

        machinePreviewIndex = 0;
        machineStartIndex = 0;
        bool isValidLastStage = (lastStage != null);
        if (isValidLastStage)
        {
            machinePreviewIndex = lastStage.stage;
        }

        if (nextStage != null)
        {
            int sort = StageTableManager.Instance.GetChapterSort(nextStage.chapter);
            GameObject nextPrefab = ProjectUtilLibrary.LoadStagePrefab(sort, nextStage.stage);
            if (nextPrefab == null)
            {
                nextStage = null;
            }
        }
        
        if (isValidLastStage && (nextStage != null))
        {
            int lastChapter = lastStage.chapter;
            int nextChapter = nextStage.chapter;
            int lastChapterSort = StageTableManager.Instance.GetChapterSort(lastChapter);
            if (lastChapter == nextChapter)
            {
                // 같은 챕터일 경우
                int lineCount = (lastChapterSort % 2 == 0) ? GetMachineOneLineCount() : 0;
                machinePreviewIndex = lastStage.stage + lineCount;
                machineStartIndex = nextStage.stage + lineCount;
            }
            else
            {
                // 다른 챕터일 경우
                if (lastChapterSort % 2 == 0)
                {
                    // 마지막에 클리어한 챕터가 짝수
                    machinePreviewIndex = 0;
                    machineStartIndex = 1;
                }
                else
                {
                    machinePreviewIndex = GetMachineOneLineCount();
                    machineStartIndex = machinePreviewIndex + 1;
                }
            }
        }
        
        if (chapterTitleField)
        {
            int chapter = 0;
            if (nextStage == null)
            {
                chapter = (lastStage != null) ? lastStage.chapter : 1;
            }
            else
            {
                chapter = nextStage.chapter;
            }
            
            int chapterSort = StageTableManager.Instance.GetChapterSort(chapter);
            chapterTitleField.SetText($"ChaterTitle_{chapterSort}");
        }
        
        #if UNITY_EDITOR
        //CodeUtilLibrary.SetColorLog($"SetNextStage : machinePreviewIndex[{machinePreviewIndex}], machineStartIndex[{machineStartIndex}]", "aqua");
        #endif
    }

    public void SetInitState()
    {
        if ((lobbyState > LobbyState.None) && (lobbyState < LobbyState.OnStart))
        {
            SetZoomOut();
        }
    }

    public void SetVisibleUI(bool isVisible)
    {
        if (chapterTitleField)
        {
            //chapterTitleField.gameObject.SetActive(isVisible);
        }

        if (noticeTouchMachineObject)
        {
            noticeTouchMachineObject.SetActive(isVisible);
        }

        if (titleObject)
        {
            titleObject.SetActive(isVisible);
        }

        if (startButton)
        {
            startButton.SetActive(isVisible);
        }

        if (toolBoxObject)
        {
            toolBoxObject.SetActive(isVisible);
        }
    }

    public void OnClickInvisibleUI()
    {
        SetVisibleUI(false);

        if (visibleButtonSwitcher)
        {
            visibleButtonSwitcher.SetActiveByChildIndex(1);
        }
    }

    public void OnClickVisibleUI()
    {
        SetVisibleUI(true);

        if (visibleButtonSwitcher)
        {
            visibleButtonSwitcher.SetActiveByChildIndex(0);
        }
    }

    private bool IsVisibleUI()
    {
        if (visibleButtonSwitcher)
        {
            return (visibleButtonSwitcher.GetActiveIndex() == 0);
        }

        return true;
    }

    private int GetMachineOneLineCount()
    {
        return 5;
    }

    private int GetStageForMachineButton(StageTable table)
    {
        if (table == null)
        {
            return 0;
        }

        int chapterSort = StageTableManager.Instance.GetChapterSort(table.chapter);
        if (chapterSort % 2 == 0)
        {
            if (table.stage == 0)
            {
                return table.stage;
            }
            
            return table.stage + GetMachineOneLineCount();
        }

        return table.stage;
    }

    private void RefreshTheme()
    {
        ChangeTheme(false);
    }

    private void ChangeTheme(bool isChangeAnim)
    {
        isChangeSkin = false;
        //CodeUtilLibrary.SetColorLog($"ChangeTheme : isChangeAnim[{isChangeAnim}]", "aqua");
        if (isChangeAnim)
        {
            SetActivateThemeObject(false);

            if (themeChangeSound)
            {
                themeChangeSound.PlaySound();
            }
            
            TrackEntry animEntry = SpineUtilLibrary.PlaySpineAnim(machineSpine, "SkinChanging", false, OnEndPlayChangeSkin);
        }
        else
        {
            SetEquippedTheme();
        }
    }

    private void OnEndPlayChangeSkin()
    {
        SetActivateThemeObject(true);
        SetEquippedTheme();
            
        if (nextStage != null)
        {
            DisplayLoopPreviewMachine();
        }
        else
        {
            SetLockState();
        }
    }

    private void SetActivateThemeObject(bool isActivate)
    {
        for (int i = 0; i < changeSkinDeactivateObjectList.Count; i++)
        {
            GameObject obj = changeSkinDeactivateObjectList[i];
            if (obj)
            {
                obj.SetActive(isActivate);
            }
        }
    }

    private void SetEquippedTheme()
    {
        if (isChangeSkin)
        {
            return;
        }

        isChangeSkin = true;
        int i;
        string themeName = ItemManager.Instance.GetNowEquippedThemeName();
        if (machineSpine)
        {
            SpineUtilLibrary.SetSpineSkin(machineSpine, themeName);
        }

        if (bgColorSetter)
        {
            bgColorSetter.SetActivateKey(themeName);
        }

        if (fxImageSetter)
        {
            fxImageSetter.SetActivateKey(themeName);
        }

        if (themeActivatorList != null)
        {
            for (i = 0; i < themeActivatorList.Count; i++)
            {
                themeActivatorList[i].SetNowActivateKey(themeName);
            }
        }

        if (themeSoundList != null)
        {
            for (i = 0; i < themeSoundList.Count; i++)
            {
                themeSoundList[i].SetNowActivateKey(themeName);
            }
        }
    }

    public void OnEquippedTheme()
    {
        ChangeTheme(true);
    }

    public void StartRetryStage(int stageIndex)
    {
        isGameRetry = true;
        stageValue = stageIndex;
        StartLevelStage();
    }

    public void OnClickBuyAdRemover()
    {
        if (adRemoverBuyPopup)
        {
            adRemoverBuyPopup.SetPurchaseItem(ShopManager.Instance.ProductTypeToIndex(ItemType.ADREMOVE), OnPurchaseAdRemover);
        }
    }

    private void OnPurchaseAdRemover(ProductTable product)
    {
        // 광고 제거 구매 완료시 호출
    }
}