using System.Collections;
using System.Collections.Generic;
using Core.Library;
using Script.Library;
using Spine;
using Spine.Unity;
using UI.Common;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class Lobby : MonoBehaviour
{
    private enum LobbyState
    {
        None = 0,
        PlayDirection,
        ReadyToStart,
        PlayStartTouchDirection,
        OnStart,
        ZoomOut,
    }
    
    [FormerlySerializedAs("MachineSpine")] [SerializeField] private SkeletonGraphic machineSpine;
    [FormerlySerializedAs("MachinePlayer")] [SerializeField] private SpineAnimPlayer machinePlayer;
    [FormerlySerializedAs("ChapterTitleField")] [SerializeField] private LocalizeTextField chapterTitleField;

    [FormerlySerializedAs("NoticeTouchMachineObject")] [SerializeField] private GameObject noticeTouchMachineObject;
    [FormerlySerializedAs("TitleObject")] [SerializeField] private GameObject titleObject;
    
    [FormerlySerializedAs("BackButton")] [SerializeField] private GameObject backButton;
    [FormerlySerializedAs("ZoomOutPlayer")] [SerializeField] private SpineAnimPlayer zoomOutPlayer;
    [FormerlySerializedAs("TouchGameStartPlayer")] [SerializeField] private SpineAnimPlayer touchGameStartPlayer;

    [FormerlySerializedAs("StartButton")] [SerializeField] private GameObject startButton;
    [FormerlySerializedAs("ToolBoxObject")] [SerializeField] private GameObject toolBoxObject;
    [FormerlySerializedAs("VisibleButtonSwitcher")] [SerializeField] private ObjectSwitcher visibleButtonSwitcher;

    [FormerlySerializedAs("DropFXParticle")] [SerializeField] private ParticleSystem dropFXParticle;
    
    private int stageValue = 0;
    private StageTable nextStage;

    private int lastClearStage;
    private StageTable lastStage;
    private LobbyState lobbyState = LobbyState.None;
    
    // Start is called before the first frame update
    void Start()
    {
        lastClearStage = SaveManager.Instance.GetClearStage();
        lastStage = StageTableManager.Instance.GetStageTable(lastClearStage);

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
        }

        InitMachineAnimState();

        if (machineSpine)
        {
            int stage = GetStageForMachineButton(lastStage);
            SpineUtilLibrary.PlaySpineAnim(machineSpine, $"Machine1Button{stage}On", true);
        }
        
        SetStartStageIndex();
        
        if (chapterTitleField)
        {
            int chapter = (lastStage != null) ? lastStage.chapter : 1;
            chapterTitleField.SetText($"ChaterTitle_{chapter}");
        }
    }

    /*
    // Update is called once per frame
    void Update()
    {
    }*/

    public void StartLevelStage(int level)
    {
        string sceneName = "FindObjectScene";//$"Level {level}";
        SceneManager.sceneLoaded += GameSceneLoaded;
        SceneManager.LoadScene(sceneName);
    }

    public void InitMachineAnimState()
    {
        lobbyState = LobbyState.None;
        if (backButton)
        {
            backButton.SetActive(false);
        }

        if (titleObject)
        {
            titleObject.SetActive(true);
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
    }

    private void SetZoomOut()
    {
        lobbyState = LobbyState.ZoomOut;
        if (backButton)
        {
            backButton.SetActive(false);
        }

        if (zoomOutPlayer)
        {
            int stage = (lastStage != null) ? lastStage.stage : 0;
            zoomOutPlayer.PlayAnim(stage);
        }
    }

    public void StartBySavedLevel()
    {
        lobbyState = LobbyState.OnStart;
        if (stageValue != 0)
        {
            StartLevelStage(stageValue);
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
            findObjectMode.SetStage(stageValue);
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

        string animName = $"Machine2Button{GetStageForMachineButton(lastStage)}On";
        TrackEntry animEntry = SpineUtilLibrary.PlaySpineAnim(machineSpine, animName, false);
        if (animEntry != null)
        {
            animEntry.Complete += (trackEntry => OnEndPlayMachineDirection());
        }
        else
        {
            OnEndPlayMachineDirection();
        }
        
        /*
        if (machinePlayer)
        {
            int stage = (lastStage != null) ? lastStage.stage : 0;
            machinePlayer.PlayAnim(stage);
        }
        else
        {
            OnEndPlayMachineDirection();
        }*/
    }

    public void OnClickMachine()
    {
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
        int nextStageIndex = GetStageForMachineButton(nextStage);
        string animName = $"button{nextStageIndex}";
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
        touchGameStartPlayer.PlayAnim(stageValue);
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
            savedStage = StageTableManager.Instance.GetNextStageTableIndex(savedStage);
        }

        if (!StageTableManager.Instance.IsValidStage(savedStage))
        {
            savedStage = clearStage;
        }

        stageValue = savedStage;
        nextStage = StageTableManager.Instance.GetStageTable(stageValue);
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

        if (table.chapter % 2 == 0)
        {
            return table.stage + GetMachineOneLineCount();
        }

        return table.stage;
    }
}