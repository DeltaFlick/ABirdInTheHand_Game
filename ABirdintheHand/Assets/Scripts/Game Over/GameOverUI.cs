using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void BackToLobby()
    {
        SceneManager.LoadScene("MainLevel");
    }

    public void TitleScreen()
    {
        SceneManager.LoadScene("MainMenu");
    }
}