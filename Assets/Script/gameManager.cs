using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class gameManager : MonoBehaviour
{
    public GameObject gameOverText;
    public GameObject remainedImageText; 
    int remainedImage = 5;
    int maxImage;

    // Start is called before the first frame update
    void Start()
    {
        maxImage = remainedImage;
        remainedImageText.GetComponent<TMP_Text>().text = remainedImage + " / " + maxImage;
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
            remainedImageText.GetComponent<TMP_Text>().text = remainedImage + " / " + maxImage;
        }
        else
        {
            Debug.Log("Found Remained Image: " + remainedImage);
            //여기다 남은 그림 개수 빼는거 추가.
            remainedImageText.GetComponent<TMP_Text>().text = remainedImage + " / " + maxImage;

        }
    }
}
