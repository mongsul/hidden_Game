using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

// _SJ      BaseSimplePrefab 세트 (리스트와 다르게 여기는 활성화-비활성화 된 리스트를 분리)
namespace UI.Common.Base
{
    public class BasePrefabDynamicList : MonoBehaviour
    {
        public struct PrefabSimpleList
        {
            public List<BaseSimplePrefab> DeactivateList;
            public List<BaseSimplePrefab> List;
        }
        
        [FormerlySerializedAs("BaseSimplePrefabList")] [SerializeField] private List<GameObject> baseSimplePrefabList;
        [FormerlySerializedAs("AddPrefabTransform")] [SerializeField] private Transform addPrefabTransform;

        [Tooltip("최대 제어 가능 개수. 0이하면 무한으로 사용.")]
        [FormerlySerializedAs("MaximumActivateCount")] [SerializeField] private int maximumActivateCount;

        // -1번은 클래스 체크 안된 번호
        private const int NONE_TYPICAL_INDEX = -1;

        private Dictionary<int, PrefabSimpleList> prefabMap; // 프리팝 관리 맵
    
        [Serializable]
        public class TabInitEvent : UnityEvent<BaseSimplePrefab> {}
    
        // Event delegates triggered on click.
        [FormerlySerializedAs("OnTabInitEvent")]
        [SerializeField]
        public TabInitEvent mOnInit;

        private void InitThis()
        {
            if (prefabMap != null)
            {
                return;
            }

            prefabMap = new Dictionary<int, PrefabSimpleList>();
            
            //AddToTab(gameObject); (초기 클래스 체크 일단 안함)
        }
        
        // Start is called before the first frame update
        void Start()
        {
            if (prefabMap == null)
            {
                InitThis();
            }
        }

        /*
        // Update is called once per frame
        void Update()
        {
        }*/
    
        private void InitToIndexPrefab(BaseSimplePrefab prefab, int index)
        {
            PrefabSimpleList list;
            if (prefabMap.ContainsKey(index))
            {
                list = prefabMap[index];
            }
            else
            {
                list = new PrefabSimpleList();
                list.List = new List<BaseSimplePrefab>();
                list.DeactivateList = new List<BaseSimplePrefab>();
            }
            
            list.List.Add(prefab);

            if (prefabMap.ContainsKey(index))
            {
                prefabMap[index] = list;
            }
            else
            {
                prefabMap.Add(index, list);
            }
            
            prefab.mOnSetEnableEvent.AddListener(OnSetActivatePrefab);
            prefab.SetIndex(index);
            mOnInit.Invoke(prefab);
        }

        private GameObject GetSimplePrefab(int index)
        {
            if (baseSimplePrefabList == null)
            {
                return null;
            }
            
            if ((index < 0) || (index >= baseSimplePrefabList.Count))
            {
                return null;
            }

            return baseSimplePrefabList[index];
        }

        public BaseSimplePrefab AddPrefab(int index)
        {
            Transform baseTransform = addPrefabTransform ? addPrefabTransform : transform;
            if (!baseTransform)
            {
                return null;
            }
            
            GameObject simplePrefab = GetSimplePrefab(index);
            if (!simplePrefab)
            {
                return null;
            }

            GameObject newGameObject = Instantiate(simplePrefab, Vector3.zero, Quaternion.identity, baseTransform);
            if (!newGameObject)
            {
                return null;
            }

            newGameObject.gameObject.SetActive(true);
            BaseSimplePrefab prefab = newGameObject.GetComponent<BaseSimplePrefab>();
            if (!prefab)
            {
                return null;
            }
            
            InitToIndexPrefab(prefab, index);
            return prefab;
        }

        public void DisableChildObject(int index = NONE_TYPICAL_INDEX)
        {
            if (prefabMap == null)
            {
                return;
            }

            if (index == NONE_TYPICAL_INDEX)
            {
                List<int> indexList = new List<int>(prefabMap.Keys);
                for (int i = 0; i < indexList.Count; i++)
                {
                    DisableChildObject(i);
                }

                return;
            }
        
            PrefabSimpleList list;
            if (prefabMap.ContainsKey(index))
            {
                list = prefabMap[index];
            }
            else
            {
                return;
            }
            
            int count = list.List.Count - 1;
            for (int i = count; i >= 0; i--)
            {
                BaseSimplePrefab prefab = list.List[i];
                if (prefab)
                {
                    prefab.SetVisible(false, false);
                }
                
                list.DeactivateList.Add(prefab);
            }
            
            list.List.Clear();
            prefabMap[index] = list;
        }

        public BaseSimplePrefab GetNewActivePrefab(int index = 0)
        {
            if (index < 0)
            {
                return null;
            }

            if (prefabMap == null)
            {
                InitThis();
                if (prefabMap == null)
                {
                    return null;
                }
            }
            
            if (baseSimplePrefabList == null)
            {
                return null;
            }
            
            if (index >= baseSimplePrefabList.Count)
            {
                return null;
            }
        
            PrefabSimpleList list;
            if (prefabMap.ContainsKey(index))
            {
                list = prefabMap[index];
            }
            else
            {
                list = new PrefabSimpleList();
                list.List = new List<BaseSimplePrefab>();
                list.DeactivateList = new List<BaseSimplePrefab>();
            }

            BaseSimplePrefab prefab = null;
            if (list.DeactivateList.Count > 0)
            {
                prefab = list.DeactivateList[0];
                list.DeactivateList.RemoveAt(0);
                list.List.Add(prefab);
                    
                if (prefabMap.ContainsKey(index))
                {
                    prefabMap[index] = list;
                }
                else
                {
                    prefabMap.Add(index, list);
                }
            }
            else
            {
                if (maximumActivateCount <= 0)
                {
                    prefab = AddPrefab(index);
                }
                else
                {
                    int nowCount = list.DeactivateList.Count + list.List.Count;
                    if (maximumActivateCount > nowCount)
                    {
                        prefab = AddPrefab(index);
                    }
                }
            }
            
            prefab.gameObject.SetActive(true);
            return prefab;
        }

        public void OnSetActivatePrefab(BaseSimplePrefab prefab, int index, bool isActivate)
        {
            if (!prefab)
            {
                return;
            }
            
            PrefabSimpleList list;
            if (prefabMap.ContainsKey(index))
            {
                list = prefabMap[index];
            }
            else
            {
                list = new PrefabSimpleList();
                list.List = new List<BaseSimplePrefab>();
                list.DeactivateList = new List<BaseSimplePrefab>();
            }

            prefab.gameObject.SetActive(isActivate);
            if (!isActivate)
            {
                list.List.Remove(prefab);
                if (!list.DeactivateList.Contains(prefab))
                {
                    list.DeactivateList.Add(prefab);
                }
            }
            else
            {
                list.DeactivateList.Remove(prefab);
                if (!list.List.Contains(prefab))
                {
                    list.List.Add(prefab);
                }
            }
            
            if (prefabMap.ContainsKey(index))
            {
                prefabMap[index] = list;
            }
            else
            {
                prefabMap.Add(index, list);
            }
        }
    }
}