using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace UI.Common.Base
{
    public class BaseIndexPrefab : MonoBehaviour
    {
        [FormerlySerializedAs("RootPrefab")] [SerializeField] private GameObject rootPrefab;
        [FormerlySerializedAs("PrefabCheckTag")] [SerializeField] private string prefabCheckTag;
        
        private int index;

        private Component baseComponent;
        
        [Serializable]
        public class BaseIndexPrefabSimpleEvent : UnityEvent{}
        
        [Serializable]
        public class BaseIndexPrefabEvent : UnityEvent<int>{}
        
        [Serializable]
        public class BaseIndexPrefabEnableEvent : UnityEvent<BaseIndexPrefab, int, bool>{}

        public BaseIndexPrefabEnableEvent mOnSetEnableEvent;
        
    
        /*
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }*/

        public void SetIndex(int newIndex)
        {
            index = newIndex;
        }

        public int GetIndex()
        {
            return index;
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

        public GameObject GetRootPrefab()
        {
            return rootPrefab;
        }

        public void SetVisible(bool isEnable)
        {
            GameObject prerabRoot = GetRootPrefab();
            if (prerabRoot)
            {
                prerabRoot.SetActive(isEnable);
            }
            else
            {
                gameObject.SetActive(isEnable);
            }
            
            mOnSetEnableEvent.Invoke(this, GetIndex(), isEnable);
        }
        
        public string GetPrefabCheckTag()
        {
            return prefabCheckTag;
        }
    }
}