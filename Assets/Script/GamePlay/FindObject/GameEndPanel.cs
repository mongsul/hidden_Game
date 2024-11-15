using System.Collections;
using System.Collections.Generic;
using Script.Library;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Serialization;

public class GameEndPanel : MonoBehaviour
{
    [FormerlySerializedAs("MainSpine")] [SerializeField] private SkeletonGraphic mainSpine;

    [FormerlySerializedAs("TitleObject")] [SerializeField] private GameObject titleObject;
    
    // Start is called before the first frame update
    void Start()
    {
        SpineUtilLibrary.AttachToSpineBone(mainSpine, "TitleImage", titleObject);
    }

    /*
    // Update is called once per frame
    void Update()
    {
    }*/
}
