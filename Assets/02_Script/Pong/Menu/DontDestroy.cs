using UnityEngine;
using UnityEngine.SceneManagement;

public class DontDestroy : MonoBehaviour
{
    private static DontDestroy instance;
    private static bool hasInstance = false;

    void Awake()
    {
        // Verificar si ya existe una instancia
        if (instance == null && !hasInstance)
        {
            // Si no existe, esta es la primera instancia
            instance = this;
            hasInstance = true;
            DontDestroyOnLoad(gameObject);
            // Suscribirse al evento de cambio de escena
            SceneManager.sceneLoaded += OnSceneLoaded;
            Debug.Log("DontDestroy: Primera instancia creada");
        }
        else if (instance != this)
        {
            // Si ya existe otra instancia, destruir esta inmediatamente
            Debug.Log("DontDestroy: Instancia duplicada detectada, destruyendo...");
            DestroyImmediate(gameObject);
            return;
        }
    }

    void Start()
    {
        // Verificación adicional en Start
        if (instance != null && instance != this)
        {
            Debug.Log("DontDestroy: Verificación en Start - destruyendo duplicado");
            DestroyImmediate(gameObject);
            return;
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Si se carga la escena Desk, destruir el objeto de música
        if (scene.name == "Desk")
        {
            Debug.Log("DontDestroy: Escena Desk detectada, destruyendo objeto de música");
            DestroyInstance();
        }
    }

    void DestroyInstance()
    {
        if (instance != null && instance == this)
        {
            // Desuscribirse del evento antes de destruir
            SceneManager.sceneLoaded -= OnSceneLoaded;
            // Destruir el objeto
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        // Desuscribirse del evento si aún está suscrito
        SceneManager.sceneLoaded -= OnSceneLoaded;
        
        // Resetear la instancia cuando se destruye
        if (instance == this)
        {
            instance = null;
            hasInstance = false;
            Debug.Log("DontDestroy: Instancia destruida, reseteando singleton");
        }
    }
}
