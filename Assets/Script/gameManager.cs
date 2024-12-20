using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class gameManager : MonoBehaviour
{
    public GameObject gameOverText;
    public GameObject remainedImageText;
    [SerializeField] int remainedImage = 26;
    int maxImage;
    // 전체 레벨의 +1을 입력 예를들어 100개 레벨이 있으면 101 입력.
    int maxLevel = 6;

    // Start is called before the first frame update
    void Start()
    {
        PreloadManager.ExecutePreload(); // _SJ     이미지 프리로드
        maxImage = remainedImage;
        //remainedImageText.GetComponent<TMP_Text>().text = remainedImage + " ";
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void finded()
    {
        remainedImage--;
        if (remainedImage == 0)
        {
            Debug.Log("Game Over");
            //여기다 게임오버 텍스트 오브젝트 Setactive(true) 추가
            gameOverText.SetActive(true);
            //remainedImageText.GetComponent<TMP_Text>().text = remainedImage + " ";
            //nextLevel();
        }
        else
        {
            Debug.Log("Found Remained Image: " + remainedImage);
            //여기다 남은 그림 개수 빼는거 추가.
            //remainedImageText.GetComponent<TMP_Text>().text = remainedImage + " ";

        }
    }

    public void GotoLobby()
    {
        string sceneName = "Lobby"; // 여기에 이름 넣어주기
        SceneManager.LoadScene(sceneName);
    }

    public void nextLevel()
    {
        Scene scene = SceneManager.GetActiveScene();
        string sceneName = scene.name;
        int currentLevel = (int)char.GetNumericValue(sceneName[6]);
        currentLevel++;

        if (currentLevel < maxLevel)
        {
            SceneManager.LoadScene("Level " + currentLevel);
        }
        else
        {
            SceneManager.LoadScene("Lobby");
        }
    }
}
