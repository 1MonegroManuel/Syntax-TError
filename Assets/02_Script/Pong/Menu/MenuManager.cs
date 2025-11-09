using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    // Método para ir a la escena del juego
    public void PlayGame()
    {
        SceneManager.LoadScene("game");
    }

    // Método para cambiar a la escena Desk
    public void QuitGame()
    {
        SceneManager.LoadScene("Desk");
    }

    public void ScoreGame()
    {
        SceneManager.LoadScene("ScoreScene");
    }
}
