using System.Collections;
using System.Collections.Generic;
using Core.Library;
using UnityEngine;

public class WebConnector : MonoBehaviour
{
    [SerializeField] private string urlAddress;

    /*
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }*/

    public void OpenURL()
    {
        CodeUtilLibrary.OpenURL(urlAddress);
    }
}