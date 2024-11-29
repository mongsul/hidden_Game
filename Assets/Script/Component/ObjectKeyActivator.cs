using System.Collections;
using System.Collections.Generic;
using Core.Library;
using UnityEngine;
using UnityEngine.Serialization;

public enum ObjectKeyGroup
{
    None = 0,
    Theme, // 테마
    Chapter, // 챕터
}

// _SJ      오브젝트를 키값으로 활성화 해주는 함수
public class ObjectKeyActivator : MonoBehaviour
{
    [FormerlySerializedAs("KeyGroupType")] [SerializeField] private ObjectKeyGroup keyGroupType;
    [FormerlySerializedAs("ActivateKeyValueList")] [SerializeField] private List<string> activateKeyValueList;
    
    /*
    // Start is called before the first frame update
    void Start()
    {
    }*/

    /*
    // Update is called once per frame
    void Update()
    {
    }*/

    public ObjectKeyGroup GetKeyGroup()
    {
        return keyGroupType;
    }

    public void SetNowActivateKey(string key)
    {
        bool isNowActivate = activateKeyValueList.Contains(key);
        gameObject.SetActive(isNowActivate);
    }

    public static List<ObjectKeyActivator> GetAllKeyActivatorList()
    {
        return CodeUtilLibrary.GetComponentsListInActiveScene<ObjectKeyActivator>();
    }
}