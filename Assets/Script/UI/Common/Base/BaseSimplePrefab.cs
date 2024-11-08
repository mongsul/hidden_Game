using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace UI.Common.Base
{
    public class BaseSimplePrefab : MonoBehaviour
    {
        [FormerlySerializedAs("RootPrefab")] [SerializeField] private GameObject rootPrefab;
        
        private int index; // 이 프리팝의 번호가 아닌 리스트의 프리팝 타입의 번호
        
        private Component baseComponent;
        
        [Serializable]
        public class BaseIndexPrefabSimpleEvent : UnityEvent{}
        
        [Serializable]
        public class BaseIndexPrefabEnableEvent : UnityEvent<BaseSimplePrefab, int, bool>{}

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

        public void SetVisible(bool isEnable, bool isExicuteEvnet = true)
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

            if (isExicuteEvnet)
            {
                mOnSetEnableEvent.Invoke(this, index, isEnable);
            }
        }

        public void SetIndex(int newIndex)
        {
            index = newIndex;
        }

        public int GetIndex()
        {
            return index;
        }
    }
}