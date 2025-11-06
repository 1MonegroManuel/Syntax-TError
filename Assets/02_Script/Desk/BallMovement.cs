using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Necesario para manejar la UI (fade)
using System.Collections; // Necesario para usar IEnumerator y Coroutines

public class BallMovement : MonoBehaviour
{
    // --- Configuración Original ---
    [Header("Configuración de Movimiento")]
    [Tooltip("Velocidad máxima de movimiento (ahora será la velocidad alcanzada después de la aceleración)")]
    public float maxSpeed = 5f;

    [Tooltip("Aceleración de la pelota")]
    public float acceleration = 2f;

    [Header("Configuración de Timing")]
    public float delayAfterVideo = 4.0f;

    [Header("Configuración de Animación")]
    public bool useSmoothMovement = true;
    public float smoothSpeed = 2f;

    [Header("Referencias")]
    public VideoTrigger videoTrigger;

    [Header("Configuración de Física")]
    public bool disableRigidbodyUntilVideo = true;

    // --- NUEVA CONFIGURACIÓN DE RUTA ---
    [Header("Configuración de Ruta Fija (Waypoints)")]
    [Tooltip("Lista de Transforms que definen la ruta. ¡Asígnalos en el Inspector!")]
    public Transform[] waypoints;

    [Tooltip("Distancia de cercanía para considerar que ha llegado al waypoint")]
    public float arrivalThreshold = 0.2f;

    // --- Variables Privadas ---
    private Vector3 startPosition;
    private int currentWaypointIndex = 0; // Índice del waypoint actual
    private bool shouldMove = false;
    private bool hasReachedEnd = false;
    private bool videoFinished = false;
    private float delayTimer = 0f;
    private bool isWaitingForVideo = true;
    private Rigidbody ballRigidbody;

    // Para manejar la aceleración
    private float currentSpeed = 0f;

    // Referencia al Image para el fade
    public Image fadeImage;

    // ********** MÉTODOS PRINCIPALES **********

    void Start()
    {
        // Inicialización de posiciones y referencias
        if (waypoints != null && waypoints.Length > 0)
        {
            startPosition = waypoints[0].position;
            transform.position = startPosition;
        }
        else
        {
            startPosition = transform.position;
            Debug.LogWarning("⚠️ No hay Waypoints asignados. La pelota se quedará inmóvil.");
        }

        ballRigidbody = GetComponent<Rigidbody>();
        if (disableRigidbodyUntilVideo && ballRigidbody != null)
        {
            ballRigidbody.isKinematic = true;
        }

        if (videoTrigger == null)
        {
            videoTrigger = FindObjectOfType<VideoTrigger>();
        }
        VideoTrigger.OnVideoCompleted += OnVideoFinished;
    }

    void Update()
    {
        if (hasReachedEnd || isWaitingForVideo) return;

        // Manejar el delay después del video
        if (videoFinished && !shouldMove)
        {
            delayTimer += Time.deltaTime;
            if (delayTimer >= delayAfterVideo)
            {
                shouldMove = true;
                Debug.Log($"🏀 Delay completado - Iniciando movimiento de la pelota");
            }
        }

        // Llamar al nuevo método que gestiona el recorrido de la ruta
        if (shouldMove)
        {
            MoveAlongWaypoints();
        }
    }

    // ********** NUEVO MÉTODO PARA RECORRER LOS WAYPOINTS **********

    public void MoveAlongWaypoints()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        if (currentWaypointIndex >= waypoints.Length)
        {
            if (!hasReachedEnd)
            {
                hasReachedEnd = true;
                shouldMove = false;
                Debug.Log("🏁 Pelota llegó al destino final de la ruta.");
            }
            return;
        }

        Vector3 targetPosition = waypoints[currentWaypointIndex].position;

        // Aceleración: aumentamos la velocidad hasta la velocidad máxima
        AccelerateTowards(targetPosition);
    }

    // ********** NUEVO MÉTODO DE MOVIMIENTO CON ACELERACIÓN **********

    void AccelerateTowards(Vector3 targetPosition)
    {
        float distanceToTarget = Vector3.Distance(transform.position, targetPosition);

        // Aceleramos hacia el target (sin exceder la velocidad máxima)
        if (currentSpeed < maxSpeed)
        {
            currentSpeed += acceleration * Time.deltaTime; // Aceleramos por el valor de "aceleración"
        }

        // Limitamos la velocidad máxima
        currentSpeed = Mathf.Min(currentSpeed, maxSpeed);

        // Mover la pelota usando la velocidad calculada
        float step = currentSpeed * Time.deltaTime;

        transform.position = Vector3.MoveTowards(transform.position, targetPosition, step);

        // Si estamos lo suficientemente cerca del waypoint, pasamos al siguiente
        if (distanceToTarget <= arrivalThreshold)
        {
            currentWaypointIndex++;
            Debug.Log($"✅ Waypoint {currentWaypointIndex} alcanzado. Pasando al siguiente...");
            currentSpeed = 0f; // Reiniciamos la velocidad al alcanzar el waypoint
        }
    }

    // ********** DETECCIÓN DEL TRIGGER EN LOS WAYPOINTS **********

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Waypoint"))
        {
            if (other.transform == waypoints[currentWaypointIndex])
            {
                currentWaypointIndex++;
                Debug.Log($"✅ Waypoint {currentWaypointIndex} alcanzado. Pasando al siguiente...");
            }
        }
    }

    // ********** COLISIÓN CON EL JUGADOR (Muerte y Reinicio de la Escena) **********

   

    // ********** MANEJO DE LA MUERTE DEL JUGADOR Y REINICIO DE LA ESCENA **********

    IEnumerator HandlePlayerDeath()
    {
        // Activamos el efecto de fade out (se vuelve opaco)
        yield return FadeScreen(1f);

        // Reiniciamos la escena
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // ********** EFECTO DE FADE (Desaparece o Aparece la Pantalla) **********

    IEnumerator FadeScreen(float targetAlpha)
    {
        float currentAlpha = fadeImage.color.a;
        float elapsedTime = 0f;
        float duration = 1f; // Duración del fade (en segundos)

        Color startColor = fadeImage.color;
        Color targetColor = new Color(startColor.r, startColor.g, startColor.b, targetAlpha);

        while (elapsedTime < duration)
        {
            fadeImage.color = Color.Lerp(startColor, targetColor, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        fadeImage.color = targetColor;
    }

    void OnVideoFinished()
    {
        isWaitingForVideo = false;
        if (disableRigidbodyUntilVideo && ballRigidbody != null)
        {
            ballRigidbody.isKinematic = false;
        }
        videoFinished = true;
        delayTimer = 0f;
    }

    // Reset de la pelota
    public void ResetPosition()
    {
        currentWaypointIndex = 0;
        hasReachedEnd = false;
        shouldMove = false;
        videoFinished = false;
        isWaitingForVideo = true;

        if (waypoints != null && waypoints.Length > 0)
        {
            transform.position = startPosition;
        }
    }

    // Desuscribirse del evento al destruir el objeto
    void OnDestroy()
    {
        VideoTrigger.OnVideoCompleted -= OnVideoFinished;
    }
}
