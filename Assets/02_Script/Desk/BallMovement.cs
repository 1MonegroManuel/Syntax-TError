using UnityEngine;

public class BallMovement : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    [Tooltip("Velocidad de movimiento en el eje Z")]
    public float moveSpeed = 5f;
    
    [Tooltip("Distancia máxima que se moverá la pelota")]
    public float maxDistance = 10000f;
    
    [Tooltip("Dirección del movimiento (1 = hacia adelante, -1 = hacia atrás)")]
    public int direction = 1;
    
    [Header("Configuración de Timing")]
    [Tooltip("Tiempo de espera después del video antes de mover la pelota (en segundos)")]
    public float delayAfterVideo = 4.0f;
    
    [Header("Configuración de Animación")]
    [Tooltip("Si debe usar animación suave (Lerp) o movimiento directo")]
    public bool useSmoothMovement = true;
    
    [Tooltip("Velocidad de la animación suave")]
    public float smoothSpeed = 2f;
    
    [Header("Referencias")]
    [Tooltip("Referencia al VideoTrigger para detectar cuando termina el video")]
    public VideoTrigger videoTrigger;
    
    [Header("Configuración de Física")]
    [Tooltip("Si debe desactivar el Rigidbody hasta que termine el video")]
    public bool disableRigidbodyUntilVideo = true;
    
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private bool shouldMove = false;
    private bool hasMoved = false;
    private bool videoFinished = false;
    private float delayTimer = 0f;
    private bool isWaitingForVideo = true; // ✅ Control para esperar el video
    private Rigidbody ballRigidbody; // ✅ Referencia al Rigidbody
    
    void Start()
    {
        // Guardar la posición inicial
        startPosition = transform.position;
        
        // Calcular la posición objetivo
        targetPosition = startPosition + Vector3.forward * maxDistance * direction;
        
        // Obtener referencia al Rigidbody
        ballRigidbody = GetComponent<Rigidbody>();
        if (ballRigidbody == null)
        {
            Debug.LogWarning("⚠️ No se encontró Rigidbody en la pelota. Agrega un Rigidbody para mejor control físico.");
        }
        
        // Desactivar Rigidbody si está configurado
        if (disableRigidbodyUntilVideo && ballRigidbody != null)
        {
            ballRigidbody.isKinematic = true;
            Debug.Log("🔒 Rigidbody desactivado - Pelota completamente inmóvil hasta que termine el video");
        }
        
        // Buscar automáticamente el VideoTrigger si no está asignado
        if (videoTrigger == null)
        {
            videoTrigger = FindObjectOfType<VideoTrigger>();
            if (videoTrigger != null)
            {
                Debug.Log("✅ VideoTrigger encontrado automáticamente para BallMovement");
            }
            else
            {
                Debug.LogWarning("⚠️ No se encontró VideoTrigger. Asigna manualmente la referencia.");
            }
        }
        
        // Suscribirse al evento de fin de video
        VideoTrigger.OnVideoCompleted += OnVideoFinished;
        
        Debug.Log($"🏀 BallMovement inicializado. Posición inicial: {startPosition}, Objetivo: {targetPosition}");
        Debug.Log("🏀 Pelota en modo de espera - NO se moverá hasta que termine la cinemática");
    }
    
    void Update()
    {
        // ✅ Solo procesar si NO está esperando el video
        if (!isWaitingForVideo)
        {
            // Manejar el delay después del video
            if (videoFinished && !shouldMove)
            {
                delayTimer += Time.deltaTime;
                if (delayTimer >= delayAfterVideo)
                {
                    shouldMove = true;
                    Debug.Log($"🏀 Delay completado ({delayAfterVideo}s) - Iniciando movimiento de la pelota");
                }
            }
            
            // Mover la pelota cuando esté listo
            if (shouldMove && !hasMoved)
            {
                MoveBall();
            }
        }
    }
    
    void MoveBall()
    {
        if (useSmoothMovement)
        {
            // Movimiento suave usando Lerp
            transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);
            
            // Verificar si ha llegado cerca del objetivo
            if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
            {
                transform.position = targetPosition;
                hasMoved = true;
                Debug.Log("🏀 Pelota llegó al destino (movimiento suave)");
            }
        }
        else
        {
            // Movimiento directo
            Vector3 movement = Vector3.forward * moveSpeed * direction * Time.deltaTime;
            transform.position += movement;
            
            // Verificar si ha llegado al objetivo
            float distanceTraveled = Vector3.Distance(startPosition, transform.position);
            if (distanceTraveled >= maxDistance)
            {
                transform.position = targetPosition;
                hasMoved = true;
                Debug.Log("🏀 Pelota llegó al destino (movimiento directo)");
            }
        }
    }
    
    // Método que se ejecuta cuando termina el video
    void OnVideoFinished()
    {
        isWaitingForVideo = false; // ✅ Ya no está esperando el video
        
        // Reactivar Rigidbody si estaba desactivado
        if (disableRigidbodyUntilVideo && ballRigidbody != null)
        {
            ballRigidbody.isKinematic = false;
            Debug.Log("🔓 Rigidbody reactivado - Pelota lista para movimiento físico");
        }
        
        videoFinished = true;
        delayTimer = 0f;
        Debug.Log($"🏀 ¡CINEMÁTICA TERMINADA! Iniciando delay de {delayAfterVideo} segundos antes del movimiento");
    }
    
    // Método público para iniciar el movimiento manualmente
    public void StartMovement()
    {
        Debug.Log("🏀 Iniciando movimiento de la pelota manualmente");
        isWaitingForVideo = false; // ✅ Salir del modo de espera
        
        // Reactivar Rigidbody si estaba desactivado
        if (disableRigidbodyUntilVideo && ballRigidbody != null)
        {
            ballRigidbody.isKinematic = false;
            Debug.Log("🔓 Rigidbody reactivado manualmente");
        }
        
        shouldMove = true;
        hasMoved = false;
        videoFinished = true; // Marcar como si el video hubiera terminado
    }
    
    // Método público para detener el movimiento
    public void StopMovement()
    {
        Debug.Log("🏀 Deteniendo movimiento de la pelota");
        shouldMove = false;
    }
    
    // Método público para resetear la posición
    public void ResetPosition()
    {
        Debug.Log("🏀 Reseteando posición de la pelota");
        transform.position = startPosition;
        shouldMove = false;
        hasMoved = false;
    }
    
    // Método público para cambiar la dirección
    public void ChangeDirection(int newDirection)
    {
        direction = newDirection;
        targetPosition = startPosition + Vector3.forward * maxDistance * direction;
        Debug.Log($"🏀 Dirección cambiada a: {direction}");
    }
    
    // Método público para cambiar el delay después del video
    public void SetDelayAfterVideo(float newDelay)
    {
        delayAfterVideo = newDelay;
        Debug.Log($"🏀 Delay después del video cambiado a: {delayAfterVideo} segundos");
    }
    
    // Método público para verificar el estado de la pelota
    public bool IsWaitingForVideo()
    {
        return isWaitingForVideo;
    }
    
    public bool HasVideoFinished()
    {
        return videoFinished;
    }
    
    public bool IsMoving()
    {
        return shouldMove && !hasMoved;
    }
    
    // Métodos para controlar el Rigidbody
    public void EnableRigidbody()
    {
        if (ballRigidbody != null)
        {
            ballRigidbody.isKinematic = false;
            Debug.Log("🔓 Rigidbody activado manualmente");
        }
    }
    
    public void DisableRigidbody()
    {
        if (ballRigidbody != null)
        {
            ballRigidbody.isKinematic = true;
            Debug.Log("🔒 Rigidbody desactivado manualmente");
        }
    }
    
    public bool IsRigidbodyEnabled()
    {
        return ballRigidbody != null && !ballRigidbody.isKinematic;
    }
    
    void OnDestroy()
    {
        // Limpiar eventos
        VideoTrigger.OnVideoCompleted -= OnVideoFinished;
    }
    
    // Método para debug - mostrar información en el Inspector
    void OnDrawGizmosSelected()
    {
        if (Application.isPlaying)
        {
            // Dibujar línea desde la posición inicial hasta el objetivo
            Gizmos.color = Color.green;
            Gizmos.DrawLine(startPosition, targetPosition);
            
            // Dibujar esfera en la posición objetivo
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(targetPosition, 0.5f);
        }
        else
        {
            // En modo editor, mostrar la trayectoria prevista
            Vector3 startPos = transform.position;
            Vector3 endPos = startPos + Vector3.forward * maxDistance * direction;
            
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(startPos, endPos);
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(endPos, 0.5f);
        }
    }
}
