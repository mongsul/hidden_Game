using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

#if UNITY_EDITOR
using TMPro.EditorUtilities;
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace UI.Common.Base
{
    public class BaseIndexButton : UIBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Tooltip("버튼에서 사용할 번호")]
        [FormerlySerializedAs("Index")] [SerializeField] private int index;

        [Tooltip("출력 타입\n\n"
                 + "Default type always use default image\n"
                 + " - 항상 기본 이미지(defaultDisplayInfo)만 사용\n\n"
                 + "Use each display info\n"
                 + " - 각각의 출력 정보를 사용"
        )]
        [SerializeField] [FormerlySerializedAs("DisplayInfo")] private BaseButton.BaseButtonDisplayInfo displayInfo;
    
        [FormerlySerializedAs("IsUseDefaultAnim")]
        [Tooltip("기본 누르기 액션을 사용하는가 여부")]
        [SerializeField] private bool isUseDefaultAnim = true;

        [Tooltip("버튼 활성화 여부 (기본 활성화 외로 제어가 필요한 경우 사용\n이벤트 실행 여부만 적용")]
        [FormerlySerializedAs("IsActivateThis")] [SerializeField] private bool isActivateThis = true;

        [Tooltip("토글 버튼으로 사용할 것인가 여부")]
        [FormerlySerializedAs("IsUseToggleButton")] [SerializeField] private bool isUseToggleButton;

        [Tooltip("토글 버튼 사용시 기본 상태")]
        [FormerlySerializedAs("IsActivateToggle")] [SerializeField] private bool isActivateToggleDefaultValue;

        [FormerlySerializedAs("RootPrefab")] [SerializeField] private GameObject rootPrefab;

        [FormerlySerializedAs("PressSound")] [SerializeField] private string pressSound = "AS_UI_SFX_BTN_BASIC_TOUCH";
        [FormerlySerializedAs("FailPressSound")] [SerializeField] private string failPressSound = "AS_UI_SFX_BTN_FAIL_TOUCH";

        private bool _isOveredThis;
        private bool isActivateToggle;
        
        private Component baseComponent;

#if UNITY_EDITOR
        //private const string kUILayerName = "UI";
    
        private const string K_STANDARD_SPRITE_PATH = "UI/Skin/UISprite.psd";
        private const string K_BACKGROUND_SPRITE_PATH = "UI/Skin/Background.psd";
        private const string K_INPUT_FIELD_BACKGROUND_PATH = "UI/Skin/InputFieldBackground.psd";
        private const string K_KNOB_PATH = "UI/Skin/Knob.psd";
        private const string K_CHECKMARK_PATH = "UI/Skin/Checkmark.psd";
        private const string K_DROPDOWN_ARROW_PATH = "UI/Skin/DropdownArrow.psd";
        private const string K_MASK_PATH = "UI/Skin/UIMask.psd";

        private static TMP_DefaultControls.Resources _sStandardResources;
    
        private const float K_WIDTH = 160f;
        private const float K_THICK_HEIGHT = 30f;
        //private const float K_THIN_HEIGHT = 20f;
        // private static Vector2 s_TextElementSize = new Vector2(100f, 100f);
        private static Vector2 _sThickElementSize = new Vector2(K_WIDTH, K_THICK_HEIGHT);
        //private static Vector2 s_ThinElementSize = new Vector2(kWidth, kThinHeight);
        //private static Vector2 s_ImageElementSize = new Vector2(100f, 100f);
        private static Color _sDefaultSelectableColor = new Color(1f, 1f, 1f, 1f);
        //private static Color s_PanelColor = new Color(1f, 1f, 1f, 0.392f);
        //private static Color s_TextColor = new Color(50f / 255f, 50f / 255f, 50f / 255f, 1f);

        private static TMP_DefaultControls.Resources GetStandardResources()
        {
            if (_sStandardResources.standard == null)
            {
                _sStandardResources.standard = AssetDatabase.GetBuiltinExtraResource<Sprite>(K_STANDARD_SPRITE_PATH);
                _sStandardResources.background = AssetDatabase.GetBuiltinExtraResource<Sprite>(K_BACKGROUND_SPRITE_PATH);
                _sStandardResources.inputField = AssetDatabase.GetBuiltinExtraResource<Sprite>(K_INPUT_FIELD_BACKGROUND_PATH);
                _sStandardResources.knob = AssetDatabase.GetBuiltinExtraResource<Sprite>(K_KNOB_PATH);
                _sStandardResources.checkmark = AssetDatabase.GetBuiltinExtraResource<Sprite>(K_CHECKMARK_PATH);
                _sStandardResources.dropdown = AssetDatabase.GetBuiltinExtraResource<Sprite>(K_DROPDOWN_ARROW_PATH);
                _sStandardResources.mask = AssetDatabase.GetBuiltinExtraResource<Sprite>(K_MASK_PATH);
            }
            return _sStandardResources;
        }
    
        private static void SetLayerRecursively(GameObject go, int layer)
        {
            go.layer = layer;
            Transform t = go.transform;
            for (int i = 0; i < t.childCount; i++)
                SetLayerRecursively(t.GetChild(i).gameObject, layer);
        }
    
        private static void SetParentAndAlign(GameObject child, GameObject parent)
        {
            if (parent == null)
                return;

            Undo.SetTransformParent(child.transform, parent.transform, "");

            RectTransform rectTransform = child.transform as RectTransform;
            if (rectTransform)
            {
                rectTransform.anchoredPosition = Vector2.zero;
                Vector3 localPosition = rectTransform.localPosition;
                localPosition.z = 0;
                rectTransform.localPosition = localPosition;
            }
            else
            {
                child.transform.localPosition = Vector3.zero;
            }
            child.transform.localRotation = Quaternion.identity;
            child.transform.localScale = Vector3.one;

            SetLayerRecursively(child, parent.layer);
        }

        private static void SetPositionVisibleinSceneView(RectTransform canvasRTransform, RectTransform itemTransform)
        {
            // Find the best scene view
            SceneView sceneView = SceneView.lastActiveSceneView;

            if (sceneView == null && SceneView.sceneViews.Count > 0)
                sceneView = SceneView.sceneViews[0] as SceneView;

            // Couldn't find a SceneView. Don't set position.
            if (sceneView == null || sceneView.camera == null)
                return;

            // Create world space Plane from canvas position.
            Camera camera = sceneView.camera;
            Vector3 position = Vector3.zero;
            Vector2 localPlanePosition;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRTransform, new Vector2(camera.pixelWidth / 2.0f, camera.pixelHeight / 2.0f), camera, out localPlanePosition))
            {
                // Adjust for canvas pivot
                localPlanePosition.x = localPlanePosition.x + canvasRTransform.sizeDelta.x * canvasRTransform.pivot.x;
                localPlanePosition.y = localPlanePosition.y + canvasRTransform.sizeDelta.y * canvasRTransform.pivot.y;

                localPlanePosition.x = Mathf.Clamp(localPlanePosition.x, 0, canvasRTransform.sizeDelta.x);
                localPlanePosition.y = Mathf.Clamp(localPlanePosition.y, 0, canvasRTransform.sizeDelta.y);

                // Adjust for anchoring
                position.x = localPlanePosition.x - canvasRTransform.sizeDelta.x * itemTransform.anchorMin.x;
                position.y = localPlanePosition.y - canvasRTransform.sizeDelta.y * itemTransform.anchorMin.y;

                Vector3 minLocalPosition;
                minLocalPosition.x = canvasRTransform.sizeDelta.x * (0 - canvasRTransform.pivot.x) + itemTransform.sizeDelta.x * itemTransform.pivot.x;
                minLocalPosition.y = canvasRTransform.sizeDelta.y * (0 - canvasRTransform.pivot.y) + itemTransform.sizeDelta.y * itemTransform.pivot.y;

                Vector3 maxLocalPosition;
                maxLocalPosition.x = canvasRTransform.sizeDelta.x * (1 - canvasRTransform.pivot.x) - itemTransform.sizeDelta.x * itemTransform.pivot.x;
                maxLocalPosition.y = canvasRTransform.sizeDelta.y * (1 - canvasRTransform.pivot.y) - itemTransform.sizeDelta.y * itemTransform.pivot.y;

                position.x = Mathf.Clamp(position.x, minLocalPosition.x, maxLocalPosition.x);
                position.y = Mathf.Clamp(position.y, minLocalPosition.y, maxLocalPosition.y);
            }

            itemTransform.anchoredPosition = position;
            itemTransform.localRotation = Quaternion.identity;
            itemTransform.localScale = Vector3.one;
        }
    
        private static void PlaceUIElementRoot(GameObject element, MenuCommand menuCommand)
        {
            GameObject parent = menuCommand.context as GameObject;
            bool explicitParentChoice = true;
            if (parent == null)
            {
                parent = TMPro_CreateObjectMenu.GetOrCreateCanvasGameObject();
                explicitParentChoice = false;

                // If in Prefab Mode, Canvas has to be part of Prefab contents,
                // otherwise use Prefab root instead.
                PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
                if (prefabStage != null && !prefabStage.IsPartOfPrefabContents(parent))
                    parent = prefabStage.prefabContentsRoot;
            }

            if (parent.GetComponentsInParent<Canvas>(true).Length == 0)
            {
                // Create canvas under context GameObject,
                // and make that be the parent which UI element is added under.
                GameObject canvas = TMPro_CreateObjectMenu.CreateNewUI();
                Undo.SetTransformParent(canvas.transform, parent.transform, "");
                parent = canvas;
            }

            GameObjectUtility.EnsureUniqueNameForSibling(element);

            SetParentAndAlign(element, parent);
            if (!explicitParentChoice) // not a context click, so center in sceneview
                SetPositionVisibleinSceneView(parent.GetComponent<RectTransform>(), element.GetComponent<RectTransform>());

            // This call ensure any change made to created Objects after they where registered will be part of the Undo.
            Undo.RegisterFullObjectHierarchyUndo(parent == null ? element : parent, "");

            // We have to fix up the undo name since the name of the object was only known after reparenting it.
            Undo.SetCurrentGroupName("Create " + element.name);

            Selection.activeGameObject = element;
        }
    
        private static GameObject CreateUIElementRoot(string name, Vector2 size)
        {
            GameObject child = new GameObject(name);
            RectTransform rectTransform = child.AddComponent<RectTransform>();
            rectTransform.sizeDelta = size;
            return child;
        }

        /*
    private static void SetDefaultColorTransitionValues(Selectable slider)
    {
        ColorBlock colors = slider.colors;
        colors.highlightedColor = new Color(0.882f, 0.882f, 0.882f);
        colors.pressedColor = new Color(0.698f, 0.698f, 0.698f);
        colors.disabledColor = new Color(0.521f, 0.521f, 0.521f);
    }*/
    
        public static GameObject CreateButton(TMP_DefaultControls.Resources resources)
        {
            GameObject buttonRoot = CreateUIElementRoot("BaseIndexButton", _sThickElementSize);

            //GameObject childText = new GameObject("Text (TMP)");
            //childText.AddComponent<RectTransform>();
            //SetParentAndAlign(childText, buttonRoot);

            //BaseButton bt = buttonRoot.AddComponent<BaseButton>();
            buttonRoot.AddComponent<BaseIndexButton>();
            //SetDefaultColorTransitionValues(bt);
        
            Image image = buttonRoot.AddComponent<Image>();
            image.sprite = resources.standard;
            image.type = Image.Type.Sliced;
            image.color = _sDefaultSelectableColor;

            /*
        TextMeshProUGUI text = childText.AddComponent<TextMeshProUGUI>();
        text.text = "Button";
        text.alignment = TextAlignmentOptions.Center;
        SetDefaultTextValues(text);

        RectTransform textRectTransform = childText.GetComponent<RectTransform>();
        textRectTransform.anchorMin = Vector2.zero;
        textRectTransform.anchorMax = Vector2.one;
        textRectTransform.sizeDelta = Vector2.zero;
        */

            return buttonRoot;
        }
    
        [MenuItem("GameObject/UI/Ataraxy/BaseIndexButton", false, 10001)]
        public static void AddButton(MenuCommand menuCommand)
        {
            GameObject go = CreateButton(GetStandardResources());

            // Override font size
            //TMP_Text textComponent = go.GetComponentInChildren<TMP_Text>();
            //textComponent.fontSize = 24;

            PlaceUIElementRoot(go, menuCommand);
        }
#endif
    
        [Serializable]
        public class ButtonClickedEvent : UnityEvent<int> {}

        // Event delegates triggered on click.
        [FormerlySerializedAs("OnClickEvent")]
        [SerializeField]
        public ButtonClickedEvent mOnClick = new ButtonClickedEvent();
    
        [Serializable]
        public class ButtonPresseEvent : UnityEvent<int> {}

        // Event delegates triggered on press.
        [FormerlySerializedAs("OnPressEvent")]
        [SerializeField]
        public ButtonPresseEvent mOnPress = new ButtonPresseEvent();
    
        [Serializable]
        public class ButtonReleaseEvent : UnityEvent<int> {}

        // Event delegates triggered on release.
        [FormerlySerializedAs("OnReleaseEvent")]
        [SerializeField]
        public ButtonReleaseEvent mOnRelease = new ButtonReleaseEvent();
    
        [Serializable]
        public class ButtonDisableClickedEvent : UnityEvent<int> {}

        // Event delegates triggered on disable click.
        [FormerlySerializedAs("OnDisableClickedEvent")]
        [SerializeField]
        public ButtonDisableClickedEvent mOnDisableClick = new ButtonDisableClickedEvent();

        [Serializable]
        public class ButtonChangeToggleEvent : UnityEvent<int, bool> {}
        
        // Event delegates change toggle.
        [FormerlySerializedAs("OnChangeToggleEvent")]
        [SerializeField]
        public ButtonChangeToggleEvent mOnChangeToggle = new ButtonChangeToggleEvent();

        [Serializable]
        public class ButtonTabEvent : UnityEvent<bool> {}
        
        // Event delegates select by tab.
        [FormerlySerializedAs("OnChangeToggleEvent")]
        [SerializeField]
        public ButtonTabEvent mOnSelectTab = new ButtonTabEvent();

        protected BaseIndexButton()
        {
        }
    
        // Start is called before the first frame update
        protected override void Start()
        {
            base.Start();
        
            InitThis();
        }

        // Update is called once per frame
        //void Update()
        //{
        //}
    
#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
        
            InitThis();
        }

        protected override void Reset()
        {
            base.Reset();

            displayInfo.Init();
        }
#endif

        public void SetActive(bool value)
        {
            gameObject.SetActive(value);
        
            switch (displayInfo.displayType)
            {
                case BaseButton.BaseButtonDisplayType.DefaultTypeAlwaysUseDefaultImage:
                {
                    BaseButton.BaseDisplayInfo defaultDisableDisplayInfo = displayInfo.defaultDisplayInfo;
                    defaultDisableDisplayInfo.color = Color.gray;
                    SetDisplayInfoToImage(defaultDisableDisplayInfo);
                }
                    break;
                case BaseButton.BaseButtonDisplayType.UseEachDisplayInfo:
                    SetDisplayInfoToImage(displayInfo.disableDisplayInfo);
                    break;
            }
        }

        public void SetIsCustomActivate(bool value)
        {
            isActivateThis = value;
        }

        public bool GetIsCustomActivate()
        {
            return isActivateThis;
        }

        private void InitThis()
        {
            isActivateToggle = isActivateToggleDefaultValue;
            SetDisplayInfoToImage(displayInfo.defaultDisplayInfo);
        }

        /*
    private void Press()
    {
        if (!IsActive())
            return;

        UISystemProfilerApi.AddMarker("BaseButton.onClick", this);
        mOnClick.Invoke();
    }*/
    
        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            if (!IsActive())
            {
                //return;
            }
        
            //ExecuteOnClickEvent();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            // 눌렸을 때
            //Debug.Log("ExecuteOnClickEvent : " + (isActivateThis ? "true" : "false"));
            PlaySound(isActivateThis ? pressSound : failPressSound);
            
            switch (displayInfo.displayType)
            {
                case BaseButton.BaseButtonDisplayType.DefaultTypeAlwaysUseDefaultImage:
                    SetDisplayInfoToImage(displayInfo.defaultDisplayInfo);
                    break;
                case BaseButton.BaseButtonDisplayType.UseEachDisplayInfo:
                    SetDisplayInfoToImage(displayInfo.pressDisplayInfo);
                    break;
            }
        
            //Debug.Log(());
            if (isUseDefaultAnim)
            {
                CancelInvoke(nameof(ExecuteOnPressEvent));
                //transform.DOScale(0.9f, 0.24f).SetEase(Ease.OutCubic);
            }
            //_canvasGroup.DOFade(0.8f, 0.24f).SetEase(Ease.OutCubic);
        
            ExecuteOnPressEvent();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            // 떼었을 때
            SetDisplayInfoToImage(displayInfo.defaultDisplayInfo);
            ExecuteOnReleaseEvent();

            if (isUseDefaultAnim)
            {
                //float delayTime = 0.24f;
               // bool isClick = (eventData.pointerClick && _isOveredThis);
                //transform.DOScale(1.0f, delayTime).SetEase(Ease.OutCubic).OnComplete(() => ClickCheck(isClick));
                //_canvasGroup.DOFade(1f, 0.24f).SetEase(Ease.OutCubic);
            }
            else
            {
                if (eventData.pointerClick && _isOveredThis)
                {
                    ExecuteOnClickEvent();
                }
            }
        }

        private void ClickCheck(bool isClick)
        {
            if (isClick)
            {
                ExecuteOnClickEvent();
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _isOveredThis = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isOveredThis = false;
        }

        public void SetImageSprite(Sprite imageSprite)
        {
            Image mainImage = GetComponent<Image>();
            if (mainImage)
            {
                mainImage.sprite = imageSprite;
            }
        }

        public void SetIsUseToggle(bool newIsUseToggle)
        {
            isUseToggleButton = newIsUseToggle;
        }

        public void SetToggleActivate(bool isToggle)
        {
            if (!isUseToggleButton)
            {
                return;
            }
            
            isActivateToggle = isToggle;
            mOnChangeToggle.Invoke(index, isActivateToggle);
        }
        
        public bool GetToggleActivate()
        {
            return isActivateToggle;
        }

        public virtual void ExecuteOnClickEvent()
        {
            //Debug.Log("ExecuteOnClickEvent : " + (isActivateThis ? "true" : "false"));
            if (isActivateThis)
            {
                mOnClick.Invoke(index);
                SetToggleActivate(!isActivateToggle);
            }
            else
            {
                mOnDisableClick.Invoke(index);
            }
        }

        public virtual void ExecuteOnPressEvent()
        {
            mOnPress.Invoke(index);
        }

        public virtual void ExecuteOnReleaseEvent()
        {
            mOnRelease.Invoke(index);
        }

        public virtual void ExecuteOnSelectTabEvent(bool isSelect)
        {
            SetToggleActivate(isSelect);
            mOnSelectTab.Invoke(isSelect);
        }

        public void SetIsUseDefaultPressAnim(bool newIsUseDefaultPreeAnim)
        {
            isUseDefaultAnim = newIsUseDefaultPreeAnim;
        }

        public void SetDisplayInfoToImage(BaseButton.BaseDisplayInfo baseDisplay)
        {
            Image mainImage = GetComponent<Image>();
            if (mainImage)
            {
                mainImage.sprite = baseDisplay.image;
                mainImage.color = baseDisplay.color;
            }
        }

        public void SetDisplayInfo(BaseButton.BaseButtonDisplayInfo newDisplayInfo)
        {
            displayInfo = newDisplayInfo;
            SetDisplayInfoToImage(displayInfo.defaultDisplayInfo);
        }

        public void SetIndex(int newIndex)
        {
            index = newIndex;
        }

        public int GetIndex()
        {
            return index;
        }

        public GameObject GetRootPrefab()
        {
            return rootPrefab;
        }

        public T GetBasePrefab<T>() where T : Component
        {
            if (baseComponent)
            {
                return baseComponent as T;
            }

            T baseComp;
            if (rootPrefab)
            {
                baseComp = rootPrefab.GetComponentInChildren<T>();
                if (baseComp)
                {
                    baseComponent = baseComp;
                    return baseComp;
                }
            }
            
            baseComp = gameObject.GetComponentInChildren<T>();
            if (baseComp != null)
            {
                baseComponent = baseComp;
                return baseComp;
            }
            
            baseComp = gameObject.GetComponentInParent<T>();
            baseComponent = baseComp;
            return baseComp;
        }

        private void PlaySound(string soundName)
        {
            if (!string.IsNullOrEmpty(soundName))
            {
                //SoundManager.Instance.PlayUIFxSoundName(soundName);
            }
        }
    }
}