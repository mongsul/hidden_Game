using System.Collections;
using System.Collections.Generic;
using Core.Library;
using Script.Component;
using TMPro;
using UI.Common;
using UI.Common.Base;
using UnityEngine;
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
    
    [FormerlySerializedAs("ToolBox")] [SerializeField] private GameObject toolBox;
    [FormerlySerializedAs("GameEndPanel")] [SerializeField] private GameObject gameEndPanel;
    [FormerlySerializedAs("GameEndPlayer")] [SerializeField] private SpineAnimPlayer gameEndPlayer;

    [FormerlySerializedAs("GameOverPanel")] [SerializeField] private GameObject gameOverPanel;
    
    [FormerlySerializedAs("BaseScroll")] [SerializeField] private ScrollRect baseScroll;
    
    [FormerlySerializedAs("PrefabBaseObject")] [SerializeField]
    private GameObject prefabBaseObject;

    [FormerlySerializedAs("BaseSwitcher")] [SerializeField] private ObjectSwitcher baseSwitcher;

    [FormerlySerializedAs("ScaleRateValueField")] [SerializeField] private TMP_Text scaleRateValueField;
    [FormerlySerializedAs("ChangeScaleRateList")] [SerializeField] private List<float> changeScaleRateList = new List<float>();
    private int nowScaleRateListIndex = 0;
    
    private Dictionary<int, int> switcherStageMap = new Dictionary<int, int>(); // 스테이지 번호 - 스위쳐 순서 맵
    
    [FormerlySerializedAs("RemainFindCountField")] [SerializeField] private TMP_Text remainFindCountField;
    [FormerlySerializedAs("RemainTouchCount")] [SerializeField] private TMP_Text remainTouchCountField;

    [FormerlySerializedAs("TouchCounter")] [SerializeField] private TouchCover touchCounter;
    [FormerlySerializedAs("FXList")] [SerializeField] private BasePrefabDynamicList fxList;
    [FormerlySerializedAs("MainCamera")] [SerializeField] private Camera mainCamera;
    
    //[FormerlySerializedAs("AnimPlayer")] [SerializeField]
    private SpriteAnimPlayer animPlayer;
    
    private StageTable nowProgressStage;
    
    private int remainFindCount = 0;
    private int remainTouchCount = 0;
    private float damagedLifeInRange = 10.0f; // 이 값보다 작으면 목숨 까임

    private List<Level> findList = new List<Level>();

    // Start is called before the first frame update
    void Start()
    {
#if UNITY_EDITOR
        if (stageIndex < 1)
        {
            return;
        }

        // 테스트 기능 (스테이지 지정시 바로 시작)
        if (!switcherStageMap.ContainsKey(stageIndex))
        {
            SetStage(stageIndex);
        }
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

        bool isOne = false;
#if UNITY_EDITOR
        isOne = isFindCountOne;
#endif
        
        // 찾는 개수 한개
        if (isOne)
        {
            InitRemainFindObjectCount(1);
        }
        else
        {
            InitRemainFindObjectCount(nowProgressStage.findObjectCount);
        }

        damagedLifeInRange = ClientTableManager.Instance.GetBaseFloatValue("DamagedLifeInRange", 10.0f);
        
        InitRemainTouchCount(nowProgressStage.touchCount);
        
        if (baseSwitcher)
        {
            if (switcherStageMap.ContainsKey(stage))
            {
                // 이미 추가 되었을 경우, 번호만 바꿔준다.
                SetActiveIndexToSwitcher(switcherStageMap[stage]);
                OnGameStart();
                return;
            }
        }
        
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

        if (baseSwitcher)
        {
            if (!switcherStageMap.ContainsKey(stage))
            {
                int index = baseSwitcher.transform.childCount - 1;
                switcherStageMap.Add(stage, index);
                SetActiveIndexToSwitcher(index);
                OnGameStart();
                return;
            }
        }

        SetDefaultScaleRate();
        OnGameStart();
    }
    #endregion

    #region StagePrefab
    private void SetActiveIndexToSwitcher(int index)
    {
        if (!baseSwitcher)
        {
            return;
        }
        
        baseSwitcher.SetActiveByChildIndex(index);
        SetDefaultScaleRate();
    }

    public void SetScaleRate(float scaleRate)
    {
        if (scaleRateValueField)
        {
            scaleRateValueField.SetText($"X{scaleRate}");
            //scaleRateValueField.SetText(string.Format("X{0:F2}", scaleRate));
        }
        
        if (!baseSwitcher)
        {
            return;
        }

        GameObject activateObject = baseSwitcher ? baseSwitcher.GetActiveObject() : null;
        if (!activateObject)
        {
            return;
        }

        Vector3 rate = new Vector3(1.0f, 1.0f, 0.0f) * scaleRate;
        prefabBaseObject.transform.localScale = rate;
        activateObject.transform.localScale = rate;
        RectTransform baseRect = CodeUtilLibrary.GetRectTransform(prefabBaseObject);
        RectTransform prefabRect = CodeUtilLibrary.GetRectTransform(activateObject);
        if (!baseRect || !prefabRect)
        {
            return;
        }

        baseRect.sizeDelta = prefabRect.sizeDelta * scaleRate;

        // 가운데 맞춤
        if (baseScroll)
        {
            baseScroll.horizontalNormalizedPosition = 0.5f;
            baseScroll.verticalNormalizedPosition = 0.5f;
        }
    }

    private void InitStagePrefab(GameObject stagePrefab)
    {
        findList = new List<Level>(stagePrefab.GetComponentsInChildren<Level>());
        if (findList == null)
        {
            return;
        }

        for (int i = 0; i < findList.Count; i++)
        {
            Level findObject = findList[i];
            if (findObject)
            {
                findObject.mOnFindObject.AddListener(OnFindObject);
            }
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

    private void OnFindObject(Level findObject)
    {
        if (findObject)
        {
            BaseSimplePrefab hint = findObject.GetHint();
            if (hint)
            {
                OnEndPlayHintFX(hint.gameObject);
            }
        }
        
        remainFindCount--;
        RefreshRemainFindObjectCount();
        if (remainFindCount <= 0)
        {
            // 모드 끝내기
            OnFindAll();
        }
    }

    private void OnFindAll()
    {
        SetScaleRate(1.0f);
        if (toolBox)
        {
            toolBox.SetActive(false);
        }
        
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
        if (toolBox)
        {
            toolBox.SetActive(true);
        }

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

        GameObject activePrefab = baseSwitcher ? baseSwitcher.GetActiveObject() : null;
        if (activePrefab)
        {
            // 연출 애님 플레이어 세팅
            animPlayer = activePrefab.GetComponentInChildren<SpriteAnimPlayer>(true);
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

            if (touchCounter)
            {
                BackLineComponent backline = activePrefab.GetComponentInChildren<BackLineComponent>();
                int sibliningIndex = 2;
                if (backline)
                {
                    sibliningIndex = backline.transform.GetSiblingIndex() + 1;
                }
                
                touchCounter.gameObject.transform.SetParent(activePrefab.transform);
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
        }
    }

    public void OnTouchCover(Vector2 pos, float dist)
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

                PlayWrongFX(pos);
                //ProjectUtilLibrary.VibrateByBaseValue();
                VibrationLibrary.Instance.ExecuteSimpleVibration();
            }
        }
    }

    private void OnGameOver()
    {
        if (gameOverPanel)
        {
            gameOverPanel.SetActive(true);
        }
    }
    #endregion

    #region Util
    public void GotoLobby()
    {
        SceneManager.LoadScene("Lobby");
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
        SetStage(StageTableManager.Instance.GetNextStageTableIndex(stageIndex));
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
            
        findList.Remove(findObject);
        
        /*
        float scaleRate = 2.0f;
        SetScaleRate(scaleRate);

        if (baseScroll)
        {
            RectTransform rect = CodeUtilLibrary.GetRectTransform(findObject.transform);
            RectTransform parentRect = CodeUtilLibrary.GetRectTransform(findObject.transform.parent.transform);
            if (rect && parentRect)
            {
                Vector2 pos = parentRect.sizeDelta * rect.anchorMin * scaleRate;
                Vector2 backupPos = pos; 
                pos.x += rect.localPosition.x;
                pos.y += rect.localPosition.y;
                baseScroll.horizontalNormalizedPosition = pos.x / parentRect.sizeDelta.x;
                baseScroll.verticalNormalizedPosition = pos.y / parentRect.sizeDelta.y;
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
    
    private void PlayTouchFX()
    {
        AddRecord(FindFXKind.TouchToCollect);
    }

    public void OnEndPlayTouchFX(GameObject fxObject)
    {
        OnEndPlayFX(fxObject, (int)FindFXKind.TouchToCollect);
    }
    
    private void PlayWrongFX(Vector2 pos)
    {
        AddRecord(FindFXKind.TouchToWrong);

        /*
        if (fxList)
        {
            BaseSimplePrefab prefab = fxList.GetNewActivePrefab((int)FindFXKind.TouchToWrong);
            if (prefab)
            {
                RectTransform rect = CodeUtilLibrary.GetRectTransform(prefab.transform);
                if (rect)
                {
                    rect.anchorMin = Vector2.zero;
                    rect.anchorMax = Vector2.zero;
                    //rect.localPosition = pos;
                    //rect.localPosition = rect.worldToLocalMatrix.MultiplyPoint3x4(pos);
                    Vector2 newpos;
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, pos, mainCamera, out newpos);
                    rect.localPosition = newpos;
                }

                SpineAnimPlayer anim = prefab.GetBasePrefab<SpineAnimPlayer>();
                if (anim)
                {
                    anim.mOnEndPlayAllWithObject.RemoveAllListeners();
                    anim.mOnEndPlayAllWithObject.AddListener(OnEndPlayWrongFX);
                    anim.PlayAnim();
                }
            }
        }*/
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
    #endregion
}