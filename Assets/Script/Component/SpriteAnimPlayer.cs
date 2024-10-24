using System;
using System.Collections.Generic;
using Core.Library;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Script.Component
{
    public class SpriteAnimPlayer : MonoBehaviour, IPreloader
    {
        [Tooltip("재생할 이미지")]
        [FormerlySerializedAs("PlayImage")] [SerializeField] private Image playImage; 
        
        [Tooltip("폴더 경로입니다. Assets/Resource 내부 폴더로 로드합니다.\nPrefabs/UI/  와 같은 방식으로 작성 부탁드립니다.")]
        [FormerlySerializedAs("FolderPath")] [SerializeField] private string folderPath;

        [Tooltip("재생 속도")]
        [FormerlySerializedAs("PlaySpeed")] [SerializeField] private float playSpeed = 1.0f;

        [Tooltip("반복 재생 횟수. 0이면 무한 재생")]
        [FormerlySerializedAs("PlayLoopCount")] [SerializeField] private int playLoopCount = 1;
        
        [Tooltip("끝나고 나서 활성화 되는 버튼")]
        [FormerlySerializedAs("OnEndDisplayButton")] [SerializeField] private Button onEndDisplayButton;

        private const float OnceFrame = 1.0f / 30.0f;
        private List<Sprite> spriteList = new List<Sprite>();
        private bool isNowPlay = false;
        private int remainPlayCount = 0;
        private int nowDisplayFrame = 0;
        private float nowCheckFrameTime = 0.0f;

        private void Awake()
        {
            LoadSprite();
        }

        // Start is called before the first frame update
        void Start()
        {
            if (!playImage)
            {
                playImage = GetComponent<Image>();
            }

            if (!onEndDisplayButton)
            {
                onEndDisplayButton = GetComponent<Button>();
                onEndDisplayButton.interactable = false;
            }

            if (onEndDisplayButton)
            {
                ColorBlock block = onEndDisplayButton.colors;
                block.disabledColor = Color.white;
                onEndDisplayButton.colors = block;
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (!isNowPlay)
            {
                return;
            }

            /*
            if (nowDisplayFrame == spriteList.Count - 1)
            {
                nowDisplayFrame = 0;
            }*/

            float delta = Time.deltaTime * playSpeed;
            nowCheckFrameTime += delta;
            int addFrame = (int)Math.Floor(nowCheckFrameTime / OnceFrame);
            if (addFrame < 0.0f)
            {
                addFrame = 0;
            }
            if (addFrame > 0)
            {
                nowCheckFrameTime -= addFrame * nowCheckFrameTime;
                nowDisplayFrame += addFrame;
            
                RefreshImage();
            }
            
            if (nowDisplayFrame >= spriteList.Count)
            {
                nowDisplayFrame = spriteList.Count - 1;
                RefreshImage();
                if (remainPlayCount > 0)
                {
                    remainPlayCount--;
                    if (remainPlayCount <= 0)
                    {
                        isNowPlay = false;
                    }
                    else
                    {
                        nowDisplayFrame -= spriteList.Count;
                    }
                    
                    OnEndPlayAnim();
                }
            }
        }

        private void OnDestroy()
        {
            spriteList.Clear();
            spriteList = null;
        }

        private void OnEnable()
        {
            PlayAnim();
        }

        public void OnExecutePreload()
        {
            LoadSprite();
        }

        private void LoadSprite()
        {
            if (spriteList.Count > 0)
            {
                return;
            }
            
            int count = 1;
            spriteList.Clear();
            //string spritePath = "Resource/" + folderPath;
            while (true)
            {
                Sprite sprite = CodeUtilLibrary.LoadSprite(folderPath, count.ToString());
                if (!sprite)
                {
                    break;
                }
                
                spriteList.Add(sprite);
                count++;
            }
        }

        private void RefreshImage()
        {
            if (!playImage)
            {
                return;
            }

            playImage.sprite = GetNowSprite();
        }

        private Sprite GetNowSprite()
        {
            if ((nowDisplayFrame < 0) || (nowDisplayFrame >= spriteList.Count))
            {
                return null;
            }

            return spriteList[nowDisplayFrame];
        }

        public void PlayAnim()
        {
            if (onEndDisplayButton)
            {
                onEndDisplayButton.interactable = false;
            }
            
            gameObject.SetActive(true);
            remainPlayCount = 0;
            nowDisplayFrame = 0;
            nowCheckFrameTime = 0.0f;
            remainPlayCount = playLoopCount;
            isNowPlay = true;
            RefreshImage();
        }

        private void OnEndPlayAnim()
        {
            if (onEndDisplayButton)
            {
                onEndDisplayButton.interactable = true;
            }
        }
    }
}