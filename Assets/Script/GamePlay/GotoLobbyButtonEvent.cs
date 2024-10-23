using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GotoLobbyButtonEvent : MonoBehaviour
{
    [SerializeField] private Button button; 
    
    // Start is called before the first frame update
    void Start()
    {
        if (!button)
        {
            button = GetComponent<Button>();
        }

        if (button)
        {
            button.onClick.AddListener(GotoLobby);
        }
    }

    private void GotoLobby()
    {
        SceneManager.LoadScene("Lobby");
    }
}
