using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    // Carga una nueva partida (va directamente a la escena principal)
    public void NewGame()
    {
        Debug.Log("Iniciando nuevo juego...");
        SceneManager.LoadScene("Desk"); // Cambia "Desk" si el nombre de tu escena varía
    }

    // Cargar una partida guardada (puedes personalizar la lógica)
    public void LoadGame()
    {
        Debug.Log("Cargando partida guardada...");
        // Aquí podrías agregar tu sistema de guardado/carga (PlayerPrefs, JSON, etc.)
        // Por ahora solo muestra un mensaje.
    }

    // Abre el menú de configuraciones
    public void Settings()
    {
        Debug.Log("Abriendo menú de configuración...");
        // Aquí puedes activar un panel UI con opciones de audio, gráficos, etc.
    }

    // Sale del juego (solo funciona en el build, no en el editor)
    public void Exit()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();
    }
}
