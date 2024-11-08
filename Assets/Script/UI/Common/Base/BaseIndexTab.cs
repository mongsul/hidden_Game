using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace UI.Common.Base
{
    public class BaseIndexTab : MonoBehaviour
    {
        [FormerlySerializedAs("IndexButtonPrefab")] [SerializeField] private GameObject indexButtonPrefab;
    
        [FormerlySerializedAs("IsUseToggle")] [SerializeField] private bool isUseToggle;

        [FormerlySerializedAs("DefaultSelectIndex")] [SerializeField] private int defaultSelectIndex;
    
        [FormerlySerializedAs("AddPrefabTransform")] [SerializeField] private Transform addPrefabTransform;
    
        private List<BaseIndexButton> _indexButtonList;
        private int selectIndex;
    
        [Serializable]
        public class TabEvent : UnityEvent<int, BaseIndexButton> {}
    
        // Event delegates triggered on click.
        [FormerlySerializedAs("OnTabInitEvent")]
        [SerializeField]
        public TabEvent mOnInit = new TabEvent();
    
        [Serializable]
        public class TabSelectEvent : UnityEvent<int, BaseIndexButton, bool> {}
    
        // Event delegates triggered on click.
        [FormerlySerializedAs("OnTabSelectEvent")]
        [SerializeField]
        public TabSelectEvent mOnSelect = new TabSelectEvent();
    
        // Start is called before the first frame update
        void Start()
        {
            selectIndex = -1;
            _indexButtonList = new List<BaseIndexButton>();
            AddToTab(gameObject);
        
            SelectButton(defaultSelectIndex);
        }

        /*
    // Update is called once per frame
    void Update()
    {
    }*/
    
        private bool AddToTab(GameObject buttonObject)
        {
            int count = _indexButtonList.Count;
            BaseIndexButton[] buttons = buttonObject.GetComponentsInChildren<BaseIndexButton>();
            foreach (BaseIndexButton button in buttons)
            {
                InitToBaseButton(button, count);
                _indexButtonList.Add(button);
                count++;
            }

            return (buttons.Length > 0);
        }

        private void InitToBaseButton(BaseIndexButton button, int count)
        {
            button.SetIndex(count);
            button.SetIsUseToggle(isUseToggle);
            button.SetToggleActivate(false); // 해제 상태로 초기화
            button.mOnClick.AddListener(ExecuteOnClickEvent);
            mOnInit.Invoke(count, button);
        }

        public void ExecuteOnClickEvent(int index)
        {
            //Debug.Log("ExecuteOnClickEvent : " + (isActivateThis ? "true" : "false"));
            BaseIndexButton lastButton = GetIndexButton(selectIndex);
            BaseIndexButton newSelectButton = GetIndexButton(index);

            // 버튼 해제 처리
            if (!isUseToggle && lastButton && (selectIndex != index))
            {
                mOnSelect.Invoke(selectIndex, lastButton, false);
                lastButton.ExecuteOnSelectTabEvent(false);
            }

            bool isExecuteSelect = true;
            if (isUseToggle && (selectIndex == index))
            {
                isExecuteSelect = false;
            }

            if (!isExecuteSelect || !newSelectButton)
            {
                return;
            }

            selectIndex = index;
            mOnSelect.Invoke(selectIndex, newSelectButton, true);
            newSelectButton.ExecuteOnSelectTabEvent(true);
        }

        public BaseIndexButton GetIndexButton(int index)
        {
            if (index < 0)
            {
                return null;
            }
        
            if (index >= _indexButtonList.Count)
            {
                while (true)
                {
                    if (!AddPrefab())
                    {
                        return null;
                    }

                    if (index < _indexButtonList.Count)
                    {
                        break;
                    }
                }
            }

            return _indexButtonList[index];
        }

        public void SelectButton(int index)
        {
            ExecuteOnClickEvent(index);
        }

        public void ResetButton()
        {
            SelectButton(-1);
        }

        public bool AddPrefab()
        {
            if (!indexButtonPrefab)
            {
                return false;
            }

            Transform baseTransform = addPrefabTransform ? addPrefabTransform : transform;
            if (!baseTransform)
            {
                return false;
            }
        
            GameObject newTabButton = Instantiate(indexButtonPrefab, baseTransform, true);
            if (!newTabButton)
            {
                return false;
            }

            newTabButton.gameObject.SetActive(true);
            return AddToTab(newTabButton.gameObject);
        }

        public void DisableChildObject()
        {
            if (_indexButtonList == null)
            {
                return;
            }
        
            int count = _indexButtonList.Count;
            for (int i = 0; i < count; i++)
            {
                BaseIndexButton indexButton = GetIndexButton(i);
                if (indexButton)
                {
                    GameObject buttonObject = indexButton.GetRootPrefab();
                    if (buttonObject)
                    {
                        buttonObject.SetActive(false);
                    }
                    else
                    {
                        indexButton.gameObject.SetActive(false);
                    }
                }
            }
        }

        public int GetListCount()
        {
            return _indexButtonList.Count;
        }
    }
}