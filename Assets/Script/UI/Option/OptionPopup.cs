using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class OptionPopup : MonoBehaviour
{
    [FormerlySerializedAs("IsDisplayLobby")] [SerializeField] private bool isDisplayLobby = true;
    
    private List<IOptionValueConnector> valueConnectorList;
    private List<OptionVisibleSetter> visibleSetterList;
    
    /*
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }*/

    public void OnEnable()
    {
        LoadOption();
    }

    public void OnDisable()
    {
        SaveOption();
    }

    private void LoadOption()
    {
        int i;
        if (valueConnectorList == null)
        {
            valueConnectorList = gameObject.GetComponentsInChildren<IOptionValueConnector>().ToList();
        }

        int start = valueConnectorList.Count - 1;
        for (i = start; i >= 0; i--)
        {
            IOptionValueConnector valueConnector = valueConnectorList[i];
            if (valueConnector != null)
            {
                if (valueConnector.IsUseLobby() == isDisplayLobby)
                {
                    valueConnector.InitThis();
                }
                else
                {
                    valueConnectorList.RemoveAt(i);
                }
            }
            else
            {
                valueConnectorList.RemoveAt(i);
            }
        }

        if (visibleSetterList == null)
        {
            visibleSetterList = gameObject.GetComponentsInChildren<OptionVisibleSetter>(true).ToList();
        }
        
        for (i = 0; i < visibleSetterList.Count; i++)
        {
            OptionVisibleSetter visibleSetter = visibleSetterList[i];
            if (visibleSetter != null)
            {
                visibleSetter.SetIsLobby(isDisplayLobby);
            }
        }
    }

    private void SaveOption()
    {
        if (valueConnectorList == null)
        {
            return;
        }
        
        for (int i = 0; i < valueConnectorList.Count; i++)
        {
            IOptionValueConnector valueConnector = valueConnectorList[i];
            if (valueConnector != null)
            {
                valueConnector.ValueToSaveData();
            }
        }
        
        SaveManager.Instance.SaveFile(SaveKind.Option);
    }

    private void CancelOption()
    {
        if (valueConnectorList == null)
        {
            return;
        }
        
        for (int i = 0; i < valueConnectorList.Count; i++)
        {
            IOptionValueConnector valueConnector = valueConnectorList[i];
            if (valueConnector != null)
            {
                valueConnector.InitThis();
                valueConnector.SetApplyOption();
            }
        }
    }

    #region ApplyOption
    public void RefreshBGM(bool value)
    {
        SoundVolumeChanger.ApplySoundOptionToScene(true, value);
    }
    
    public void RefreshFX(bool value)
    {
        SoundVolumeChanger.ApplySoundOptionToScene(false, value);
    }

    public void RefreshVibration(bool value)
    {
        VibrationLibrary.SetIsUseVibration(value);
    }

    public void RefreshLang(string value)
    {
        ClientTableManager.Instance.SetLanguageCode(value);
    }

    public void SetLanguageList(OptionValueSelector selector)
    {
        selector.SetValueList(ClientTableManager.Instance.GetLanguageCodeList());
        
        string value = SaveManager.Instance.GetLanguageCode();
        selector.SetSelect(value);
    }

    public void OnClickRestorePurchase()
    {
    }
    #endregion

    #region Function
    public void GotoLobby()
    {
        SceneManager.sceneLoaded += GameSceneLoaded;
        
        SceneManager.LoadScene("Lobby");
    }

    private void GameSceneLoaded(Scene next, LoadSceneMode mode)
    {
        // 신 전환 후 스크립트 취득
        GameObject lobbyMainObject = GameObject.Find("LobbyMain");
        if (!lobbyMainObject)
        {
            return;
        }
        
        Lobby lobby = lobbyMainObject.GetComponent<Lobby>();
        if (lobby)
        {
            lobby.SetDirection();
        }
    }
    #endregion
}