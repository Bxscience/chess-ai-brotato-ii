using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public string GameSceneName = "ChessGame";
    public void StartPlayerVsPlayer()
    {
        PlayerPrefs.SetInt("AIEnabled", 0);
        PlayerPrefs.Save();
        SceneManager.LoadScene(GameSceneName);
    }

    public void StartPlayerVsAI()
    {
        PlayerPrefs.SetInt("AIEnabled", 1);
        PlayerPrefs.Save();
        SceneManager.LoadScene(GameSceneName);
    }
}