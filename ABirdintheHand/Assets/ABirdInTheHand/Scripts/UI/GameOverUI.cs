using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    public void BackToLobby()
    {
        SceneManager.LoadScene("MainLevel");
    }

    public void TitleScreen()
    {
        SceneManager.LoadScene("MainMenu");
    }
}