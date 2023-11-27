using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void SceneOne()
    {
        SceneManager.LoadScene("PointCloudScene");
    }
    
    public void SceneTwo()
    {
        SceneManager.LoadScene("TerrainScene");
    }
    
    public void SceneThree()
    {
        SceneManager.LoadScene("SplineSurfaceScene");
    }

    public void ReturnMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
