using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class PongGame : MonoBehaviour
{
    [Header("Configuración de Colisión")]
    [Tooltip("Tag del jugador (por defecto 'Player')")]
    public string tagJugador = "Player";

    [Header("UI")]
    [Tooltip("Referencia al texto que muestra 'Press E'")]
    public TMP_Text pressEText;

    [Header("Configuración de Escena")]
    [Tooltip("Nombre de la escena a la que se cambiará cuando se presione E")]
    public string nombreEscenaDestino = "PongMainMenu";

    private bool playerInRange = false;

    void Start()
    {
        // Ocultar el texto al inicio
        if (pressEText != null)
        {
            pressEText.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        // Si el jugador está en rango y presiona E, cambiar de escena
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            CambiarEscena();
        }
    }

    /// <summary>
    /// Detecta cuando un objeto entra en el trigger del collider
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        // Verificar si el objeto que entró tiene el tag del jugador
        if (other.CompareTag(tagJugador))
        {
            playerInRange = true;
            // Mostrar el texto "Press E"
            if (pressEText != null)
            {
                pressEText.gameObject.SetActive(true);
            }
            Debug.Log("✅ Jugador detectado. Presiona E para entrar al juego Pong.");
        }
    }

    /// <summary>
    /// Detecta cuando un objeto sale del trigger del collider
    /// </summary>
    private void OnTriggerExit(Collider other)
    {
        // Verificar si el objeto que salió tiene el tag del jugador
        if (other.CompareTag(tagJugador))
        {
            playerInRange = false;
            // Ocultar el texto "Press E"
            if (pressEText != null)
            {
                pressEText.gameObject.SetActive(false);
            }
            Debug.Log("❌ Jugador salió del área.");
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

