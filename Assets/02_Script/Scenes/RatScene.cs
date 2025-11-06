using UnityEngine;
using UnityEngine.SceneManagement;

public class RatScene : MonoBehaviour
{
    [Header("Configuración de Escena")]
    [Tooltip("Nombre de la escena a la que se cambiará cuando el jugador colisione")]
    public string nombreEscenaDestino = "RatBox";

    [Header("Configuración de Colisión")]
    [Tooltip("Tag del jugador (por defecto 'Player')")]
    public string tagJugador = "Player";

    /// <summary>
    /// Detecta cuando un objeto entra en el trigger del collider
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        // Verificar si el objeto que entró tiene el tag del jugador
        if (other.CompareTag(tagJugador))
        {
            Debug.Log($"✅ Jugador detectado. Cambiando a la escena: {nombreEscenaDestino}");
            CambiarEscena();
        }
    }

    /// <summary>
    /// Cambia la escena a la escena destino especificada
    /// </summary>
    private void CambiarEscena()
    {
        if (string.IsNullOrEmpty(nombreEscenaDestino))
        {
            Debug.LogError("❌ Error: El nombre de la escena destino no está configurado.");
            return;
        }

        try
        {
            SceneManager.LoadScene(nombreEscenaDestino);
            Debug.Log($"✅ Escena cambiada exitosamente a: {nombreEscenaDestino}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Error al cargar la escena '{nombreEscenaDestino}': {e.Message}");
            Debug.LogError("⚠️ Asegúrate de que la escena esté agregada en Build Settings.");
        }
    }
}

