using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Script.Component
{
    public class ObjectChanger : MonoBehaviour
    {
        [Tooltip("이벤트 링크시켜줄 버튼")]
        [FormerlySerializedAs("LinkButton")] [SerializeField] private Button linkButton;
        
        [Tooltip("처음에 출력해둘 오브젝트")]
        [FormerlySerializedAs("OriginalObject")] [SerializeField] private GameObject originalObject;

        [Tooltip("링크되어있는 버튼을 눌렀을 때 교체 출력해줄 오브젝트")]
        [FormerlySerializedAs("ReplaceObject")] [SerializeField] private GameObject replaceObject;

        // Start is called before the first frame update
        void Start()
        {
            if (originalObject)
            {
                originalObject.SetActive(true);
            }

            if (replaceObject)
            {
                replaceObject.SetActive(false);
            }
            
            if (!linkButton)
            {
                linkButton = GetComponent<Button>();
            }

            if (linkButton)
            {
                linkButton.onClick.AddListener(OnClick);
            }
        }

        /*
        // Update is called once per frame
        void Update()
        {
        }*/

        private void OnClick()
        {
            if (originalObject)
            {
                originalObject.SetActive(false);
            }

            if (replaceObject)
            {
                replaceObject.SetActive(true);
            }
        }
    }
}
