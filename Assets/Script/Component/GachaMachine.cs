using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// _SJ      가챠 머신 (랜덤 뽑기)
public class GachaMachine<T>// : MonoBehaviour
{
    private T defaultValue; // 기본 값 (뽑기 실패했을 때 예외처리용 값)
    private int maxValue = 0; // 가중치 최대 값
    private Dictionary<T, int> weightMap = new Dictionary<T, int>(); // 키 - 가중치 맵

    /*
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }*/

    public void SetDefaultValue(T value)
    {
        defaultValue = value;
    }

    public void AddWeight(T key, int weight)
    {
        if (weight < 1)
        {
            return;
        }

        if (weightMap.ContainsKey(key))
        {
            maxValue -= weightMap[key];
            weightMap[key] = weight;
        }
        else
        {
            weightMap.Add(key, weight);
        }
        
        maxValue += weight;
    }

    public void RemoveWeight(T key)
    {
        if (weightMap.ContainsKey(key))
        {
            maxValue -= weightMap[key];
            weightMap.Remove(key);
        }
    }

    public T PickupValue()
    {
        if (!IsPossiblePickup())
        {
            return defaultValue;
        }
        
        if (maxValue < 1)
        {
            return defaultValue;
        }
        
        int randomValue = Random.Range(0, maxValue);
        int checkValue = 0;
        T lastT = defaultValue;
        #if UNITY_EDITOR
        //Debug.Log($"PickupValue - randomValue(0, {maxValue}) : [{randomValue}]");
        #endif
        foreach (T key in weightMap.Keys)
        {
            int addWeight = weightMap[key];
            checkValue += addWeight;
#if UNITY_EDITOR
                //Debug.Log($"PickupValue - checkValue : [{checkValue}], addWeight : [{addWeight}]");
#endif
            if (randomValue <= checkValue)
            {
                lastT = key;
                break;
            }
        }

#if UNITY_EDITOR
        //Debug.Log($"PickupValue - lastT : [{lastT}]");
#endif
        return lastT;
    }

    public void Clear(T defValue)
    {
        defaultValue = defValue;
        weightMap = new Dictionary<T, int>();
        maxValue = 0;
    }

    public int GetItemCount()
    {
        return weightMap.Count;
    }

    public bool IsPossiblePickup()
    {
        return (weightMap.Count > 0);
    }
}