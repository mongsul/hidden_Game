using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Script.Component
{
    public class ObjectEraser : MonoBehaviour
    {
        [Tooltip("이 버튼을 누르면 지워집니다")]
        [FormerlySerializedAs("MainButton")] [SerializeField] private Button mainButton;
        
        [Tooltip("MainButton을 눌렀을 때 지워지는 오브젝트")]
        [FormerlySerializedAs("EraseObject")] [SerializeField] private GameObject eraseObject;
        
        // Start is called before the first frame update
        void Start()
        {
            if (!mainButton)
            {
                mainButton = GetComponent<Button>();
            }

            if (mainButton)
            {
                mainButton.onClick.AddListener(OnClick);
            }
        }

        /*
    // Update is called once per frame
    void Update()
    {
    }*/

        private void OnClick()
        {
            if (eraseObject)
            {
                eraseObject.SetActive(false);
            }
        }
    }
}