using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

// _SJ      BaseIndexPrefab과 세트
namespace UI.Common.Base
{
    public class BasePrefabList : MonoBehaviour
    {
        [FormerlySerializedAs("BaseIndexPrefab")] [SerializeField] private GameObject baseIndexPrefab;
        [FormerlySerializedAs("AddPrefabTransform")] [SerializeField] private Transform addPrefabTransform;

        [FormerlySerializedAs("CheckOnInitByGameObject")] [SerializeField] private bool checkOnInitByGameObject = false;
        [FormerlySerializedAs("PrefabCheckTag")] [SerializeField] private string prefabCheckTag;
        
        private List<BaseIndexPrefab> prefabList;
    
        [Serializable]
        public class TabInitEvent : UnityEvent<int, BaseIndexPrefab> {}
    
        // Event delegates triggered on click.
        [FormerlySerializedAs("OnTabInitEvent")]
        [SerializeField]
        public TabInitEvent mOnInit = new TabInitEvent();

        private void InitThis()
        {
            if (prefabList != null)
            {
                if (prefabList.Count > 0)
                {
                    return;
                }
            }
            
            prefabList = new List<BaseIndexPrefab>();
            AddToTab(gameObject);
        }
        
        // Start is called before the first frame update
        protected virtual void Start()
        {
            if (prefabList == null)
            {
                InitThis();
            }
        }

        /*
        // Update is called once per frame
        void Update()
        {
        }*/
    
        private bool AddToTab(GameObject buttonObject)
        {
            int count = prefabList.Count;
            BaseIndexPrefab[] prefabs = buttonObject.GetComponentsInChildren<BaseIndexPrefab>();
            foreach (BaseIndexPrefab prefab in prefabs)
            {
                bool isPossibleAdd = true;
                if (checkOnInitByGameObject)
                {
                    isPossibleAdd = prefabCheckTag.Equals(prefab.GetPrefabCheckTag());
                }

                if (isPossibleAdd)
                {
                    InitToIndexPrefab(prefab, count);
                    prefabList.Add(prefab);
                    count++;
                }
            }

            return (prefabs.Length > 0);
        }

        private void InitToIndexPrefab(BaseIndexPrefab prefab, int count)
        {
            prefab.SetIndex(count);
            mOnInit.Invoke(count, prefab);
        }

        public bool AddPrefab()
        {
            if (!baseIndexPrefab)
            {
                return false;
            }

            Transform baseTransform = addPrefabTransform ? addPrefabTransform : transform;
            if (!baseTransform)
            {
                return false;
            }
        
            GameObject newTabButton = Instantiate(baseIndexPrefab, baseTransform, false);
            if (!newTabButton)
            {
                return false;
            }

            newTabButton.gameObject.SetActive(true);
            return AddToTab(newTabButton.gameObject);
        }

        public void DisableChildObject()
        {
            if (prefabList == null)
            {
                InitThis();
            }
            else if (prefabList.Count <= 0)
            {
                InitThis();
            }

            if (prefabList == null)
            {
                return;
            }
        
            int count = prefabList.Count;
            for (int i = 0; i < count; i++)
            {
                BaseIndexPrefab prefab = GetIndexPrefab(i);
                if (prefab)
                {
                    prefab.SetVisible(false);
                }
            }
        }

        public BaseIndexPrefab GetIndexPrefab(int index)
        {
            if (index < 0)
            {
                return null;
            }

            if (prefabList == null)
            {
                InitThis();
                if (prefabList == null)
                {
                    return null;
                }
            }
        
            if (index >= prefabList.Count)
            {
                while (true)
                {
                    if (!AddPrefab())
                    {
                        return null;
                    }

                    if (index < prefabList.Count)
                    {
                        break;
                    }
                }
            }

            return prefabList[index];
        }

        public T GetBasePrefab<T>(int index) where T : Component
        {
            BaseIndexPrefab prefab = GetIndexPrefab(index);
            if (!prefab)
            {
                return null;
            }

            return prefab.GetBasePrefab<T>();
        }

        public bool GetFastPrefab(out BaseIndexPrefab fastPrefab, out int index)
        {
            int count = prefabList.Count;
            for (int i = 0; i < count; i++)
            {
                BaseIndexPrefab prefab = GetIndexPrefab(i);
                if (prefab)
                {
                    if (!prefab.gameObject.activeSelf)
                    {
                        fastPrefab = prefab;
                        index = i;
                        return true;
                    }
                }
            }

            AddPrefab();
            fastPrefab = GetIndexPrefab(count);
            index = count;
            return (fastPrefab != null);
        }
        
        public bool GetFastPrefab<T>(out T basePrefab, out int index) where T : Component
        {
            GetFastPrefab(out BaseIndexPrefab prefab, out index);
            if (!prefab)
            {
                basePrefab = null;
                return false;
            }

            basePrefab = prefab.GetBasePrefab<T>();
            return (basePrefab != null);
        }

        public int GetCount()
        {
            if (prefabList == null)
            {
                return 0;
            }

            return prefabList.Count;
        }
    }
}