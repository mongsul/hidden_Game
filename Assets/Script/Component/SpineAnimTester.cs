using System.Collections;
using System.Collections.Generic;
using Script.Library;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpineAnimTester : MonoBehaviour
{
    [SerializeField] private SkeletonGraphic spine;
    [SerializeField] [SpineAnimation] private string animName;
    [SerializeField] private Button button; 
    [SerializeField] private TMP_Text animNameField; 

    // Start is called before the first frame update
    void Start()
    {
        if (button)
        {
            button.onClick.AddListener(OnClickButton);
        }
    }

    /*
    // Update is called once per frame
    void Update()
    {
    }*/

    private void OnClickButton()
    {
        SpineUtilLibrary.PlaySpineAnim(spine, animName, false);
        
        if (animNameField)
        {
            animNameField.SetText(animName);
        }
    }
}
