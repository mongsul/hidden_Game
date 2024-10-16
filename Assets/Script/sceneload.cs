using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class sceneload : MonoBehaviour
{
    public void onstartbuttonClick()
    {
        SceneManager.LoadScene("Level 1");
    }
}
