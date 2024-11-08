using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

// _SJ      텍스트 매테리얼 관리자
namespace Core.Component
{
    public class TextMaterialManager : UIBehaviour
    {
        [Serializable]
        private class TextFontInfo
        {
            [SerializeField] public Material material;
            [SerializeField] public Color color;
        }

        [FormerlySerializedAs("Text")] [SerializeField] private TMP_Text text;
        [FormerlySerializedAs("MaterialList")] [SerializeField] private List<TextFontInfo> materialList;
        [FormerlySerializedAs("DefaultMaterialKey")] [SerializeField] private int defaultMaterialKey = 0;
    
        // Start is called before the first frame update
        protected override void Start()
        {
            base.Start();
            SetMaterialListIndex(defaultMaterialKey);
        }

        /*
    // Update is called once per frame
    void Update()
    {
    }*/
    
#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            SetMaterialListIndex(defaultMaterialKey);
        }

        protected override void Reset()
        {
            base.Reset();
        }
#endif

        private bool GetMaterial(int listIndex, out TextFontInfo fontInfo)
        {
            if (materialList == null)
            {
                fontInfo = new TextFontInfo();
                return false;
            }
        
            if ((listIndex < 0) || (listIndex >= materialList.Count))
            {
                fontInfo = new TextFontInfo();
                return false;
            }
        
            fontInfo = materialList[listIndex];
            return true;
        }

        public void SetMaterialListIndex(int listIndex)
        {
            if (!text)
            {
                text = gameObject.GetComponent<TMP_Text>();
            }
        
            if (text)
            {
                if (GetMaterial(listIndex, out TextFontInfo fontInfo))
                {
                    defaultMaterialKey = listIndex;
                    text.material = fontInfo.material;
                    text.fontSharedMaterial = fontInfo.material;
                    text.color = fontInfo.color;
                }
            }
        }
    }
}