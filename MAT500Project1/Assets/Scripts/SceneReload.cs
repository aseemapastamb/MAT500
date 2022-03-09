using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneReload : MonoBehaviour
{
    public void ReloadScene()
    {
        //Application.LoadLevel(0);
        SceneManager.LoadScene(0);
    }
}
