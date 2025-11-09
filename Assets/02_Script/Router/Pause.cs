using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Pause : MonoBehaviour
{
    [Header("Referencias UI")]
    [Tooltip("Canvas del menú de pausa (opcional, si no se asigna usará el panel)")]
    public Canvas canvasPausa;
    
    [Tooltip("Panel del menú de pausa (debe estar desactivado al inicio)")]
    public GameObject panelPausa;
    
    [Tooltip("Botón para reanudar el juego")]
    public Button botonReanudar;
    
    [Tooltip("Botón para ir al menú principal")]
    public Button botonMenuPrincipal;
    
    [Tooltip("Botón para reiniciar la escena")]
    public Button botonReiniciar;
    
    [Header("Configuración")]
    [Tooltip("Nombre de la escena del menú principal")]
    public string nombreEscenaMenuPrincipal = "MainMenu";
    
    private bool juegoPausado = false;
    
    void Start()
    {
        // Buscar automáticamente el canvas si no está asignado
        if (canvasPausa == null)
        {
            // Buscar canvas con nombre que contenga "Pausa" o "Pause"
            Canvas[] todosLosCanvas = FindObjectsOfType<Canvas>();
            foreach (Canvas canvas in todosLosCanvas)
            {
                if (canvas.name.ToLower().Contains("pausa") || 
                    canvas.name.ToLower().Contains("pause") ||
                    canvas.name.ToLower().Contains("menu"))
                {
                    canvasPausa = canvas;
                    Debug.Log($"✅ Canvas de pausa encontrado automáticamente: {canvas.name}");
                    break;
                }
            }
        }
        
        // Si aún no hay canvas, buscar el panel y obtener su canvas padre
        if (canvasPausa == null && panelPausa != null)
        {
            Canvas canvasPadre = panelPausa.GetComponentInParent<Canvas>();
            if (canvasPadre != null)
            {
                canvasPausa = canvasPadre;
                Debug.Log($"✅ Canvas encontrado desde el panel: {canvasPadre.name}");
            }
        }
        
        // Asegurar que el canvas de pausa esté desactivado al inicio
        if (canvasPausa != null)
        {
            canvasPausa.gameObject.SetActive(false);
        }
        
        // Asegurar que el panel de pausa esté desactivado al inicio
        if (panelPausa != null)
        {
            panelPausa.SetActive(false);
        }
        
        // Configurar los botones
        if (botonReanudar != null)
        {
            botonReanudar.onClick.AddListener(ReanudarJuego);
        }
        
        if (botonMenuPrincipal != null)
        {
            botonMenuPrincipal.onClick.AddListener(IrAlMenuPrincipal);
        }
        
        if (botonReiniciar != null)
        {
            botonReiniciar.onClick.AddListener(ReiniciarEscena);
        }
        
        // Asegurar que el juego no esté pausado al inicio
        Time.timeScale = 1f;
        juegoPausado = false;
    }
    
    void Update()
    {
        // Detectar cuando se presiona Escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (juegoPausado)
            {
                ReanudarJuego();
            }
            else
            {
                PausarJuego();
            }
        }
    }
    
    /// <summary>
    /// Pausa el juego y muestra el menú de pausa
    /// </summary>
    public void PausarJuego()
    {
        juegoPausado = true;
        Time.timeScale = 0f; // Pausar el tiempo del juego
        
        // Mostrar el canvas de pausa (si está asignado)
        if (canvasPausa != null)
        {
            canvasPausa.gameObject.SetActive(true);
        }
        
        // Mostrar el panel de pausa (si está asignado y no hay canvas)
        if (panelPausa != null)
        {
            panelPausa.SetActive(true);
        }
        
        // Desbloquear el cursor para poder interactuar con los botones
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        Debug.Log("⏸️ Juego pausado - Canvas mostrado");
    }
    
    /// <summary>
    /// Reanuda el juego y oculta el menú de pausa
    /// </summary>
    public void ReanudarJuego()
    {
        juegoPausado = false;
        Time.timeScale = 1f; // Reanudar el tiempo del juego
        
        // Ocultar el canvas de pausa (si está asignado)
        if (canvasPausa != null)
        {
            canvasPausa.gameObject.SetActive(false);
        }
        
        // Ocultar el panel de pausa (si está asignado)
        if (panelPausa != null)
        {
            panelPausa.SetActive(false);
        }
        
        // Bloquear el cursor nuevamente (opcional, según tu juego)
        // Cursor.lockState = CursorLockMode.Locked;
        // Cursor.visible = false;
        
        Debug.Log("▶️ Juego reanudado - Canvas oculto");
    }
    
    /// <summary>
    /// Carga la escena del menú principal
    /// </summary>
    public void IrAlMenuPrincipal()
    {
        // Reanudar el tiempo antes de cambiar de escena
        Time.timeScale = 1f;
        
        // Cargar la escena del menú principal
        SceneManager.LoadScene(nombreEscenaMenuPrincipal);
        
        Debug.Log($"🏠 Yendo al menú principal: {nombreEscenaMenuPrincipal}");
    }
    
    /// <summary>
    /// Reinicia la escena actual
    /// </summary>
    public void ReiniciarEscena()
    {
        // Reanudar el tiempo antes de reiniciar
        Time.timeScale = 1f;
        
        // Obtener el nombre de la escena actual y recargarla
        string nombreEscenaActual = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(nombreEscenaActual);
        
        Debug.Log($"🔄 Reiniciando escena: {nombreEscenaActual}");
    }
    
    /// <summary>
    /// Obtiene si el juego está pausado
    /// </summary>
    public bool EstaPausado()
    {
        return juegoPausado;
    }
    
    void OnDestroy()
    {
        // Asegurar que el tiempo se restaure al destruir el objeto
        Time.timeScale = 1f;
    }
}
