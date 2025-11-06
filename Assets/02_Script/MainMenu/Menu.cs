using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    // Función para el botón Play
    public void Play()
    {
        // Aquí puedes cambiar "GameScene" por el nombre de tu escena de juego
        SceneManager.LoadScene("desk");
    }

    // Función para el botón Settings
    public void Settings()
    {
        SceneManager.LoadScene("Configuracion");
    }

    // Función para el botón Salir
    public void Quit()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
