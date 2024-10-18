using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Lobby : MonoBehaviour
{
    /*
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }*/

    public void StartStage()
    {
        string sceneName = ""; // 여기에 이름 넣어주기
        SceneManager.LoadScene(sceneName);
    }

    public void StartLevelStage(int level)
    {
        string sceneName = $"Level {level}"; // 여기에 이름 넣어주기
        SceneManager.LoadScene(sceneName);
    }

    public void StartOneLevelStage()
    {
        StartLevelStage(1);
    }
}
