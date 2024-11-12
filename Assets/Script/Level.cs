using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class Level : MonoBehaviour
{
    //Public은 외부
    //이미지 배열 (Inspector에 볼 수 있음)
    public Sprite[] chageImage;
    public Button button;

    // Private 은 내부
    // 이미지
    Image image;
    //이미지 배열번호 변수 
    int imageNumber = 0;
    gameManager gameManager;
    
    [Serializable]
    public class FindObjectEvent : UnityEvent{}

    [FormerlySerializedAs("OnFindObjectEvent")]
    [SerializeField]
    public FindObjectEvent mOnFindObject;

    // Start is called before the first frame update
    void Start()
    {
        //이 스크립트를 가진 오브젝트의 Image
        image = this.gameObject.GetComponent<Image>();
        //이 스크립트를 가진 오브젝트의 button
        button = this.gameObject.GetComponent<Button>();

        //gameManager = GameObject.Find("GameManager").GetComponent<gameManager>();

        //Button 로직 추가 클릭할 때 hideObject 함수를 호출한다.
        Button btn = button.GetComponent<Button>();
        btn.onClick.AddListener(hideObject);
    }

    // Update is called once per frame
    void Update()
    {
       
    }

    void hideObject()
    {
        if (imageNumber >= 1)
        {
            return;
        }
        
        mOnFindObject?.Invoke();
        
        imageNumber++; //imageNumber 1추가

        //만약 imageNumber가 넣은 이미지보다 수가 적으면 해당 배열 이미지 출력 넘으면 0부터 다시 시작
        if(imageNumber < chageImage.Length)
        {
            image.sprite = chageImage[imageNumber];
            //gameManager.finded();
        }
        else
        {
            button.interactable = false;
        }
    }
}
