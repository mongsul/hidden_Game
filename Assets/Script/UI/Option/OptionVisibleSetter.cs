using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class OptionVisibleSetter : MonoBehaviour
{
    // 로비에서 출력 하는가 여부
    [FormerlySerializedAs("IsLobbyDisplay")] [SerializeField] private bool isLobbyDisplay = true;
    
    /*
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }*/

    public void SetIsLobby(bool isLobby)
    {
        gameObject.SetActive(isLobbyDisplay ? isLobby : !isLobby);
    }
}