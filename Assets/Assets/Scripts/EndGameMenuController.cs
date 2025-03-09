using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndGameMenuController : MonoBehaviour
{
    public void PlayAgain()
    {
        // Load the chess game scene directly, maintaining the same game mode
        SceneManager.LoadScene("ChessGame");
    }

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}