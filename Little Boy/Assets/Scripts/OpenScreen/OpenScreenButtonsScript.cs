using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OpenScreenButtonsScript : MonoBehaviour
{
    
    public void OpenLevel(int i)
    {
        SceneManager.LoadSceneAsync("Level"+i);
    }
    public void OpenLevelsSelection()
    {
        SceneManager.LoadSceneAsync("LevelsScene");
    }
    
    public void RestartLevel()
    {
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name);
    }
    
}
