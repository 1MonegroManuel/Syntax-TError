using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BallRouteMovement : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    [Tooltip("Velocidad constante de la pelota")]
    public float moveSpeed = 5f;

    [Tooltip("Multiplicador de rebote (1 = rebote perfecto, <1 = pierde energía)")]
    [Range(0f, 1f)] public float bounceFactor = 1f;

    [Tooltip("Si debe usar gravedad (normalmente desactivado para movimiento plano)")]
    public bool useGravity = false;

    [Header("Configuración de Ruta")]
    [Tooltip("Puntos por los que pasará la pelota")]
    public Transform[] routePoints;

    [Tooltip("Si la ruta debe repetirse en bucle")]
    public bool loopRoute = true;

    [Tooltip("Distancia mínima para considerar que llegó al punto")]
    public float pointReachThreshold = 0.2f;

    [Header("Configuración de Video")]
    [Tooltip("Referencia al VideoTrigger para detectar cuando termina el video")]
    public VideoTrigger videoTrigger;

    [Tooltip("Tiempo de espera después del video antes de comenzar el movimiento")]
    public float delayAfterVideo = 3f;

    [Header("Debug / Estado")]
    [SerializeField] private int currentTargetIndex = 0;
    [SerializeField] private bool videoFinished = false;
    [SerializeField] private bool waitingDelay = false;
    private float delayTimer = 0f;

    private Rigidbody rb;
    private Vector3 currentDirection;
    private bool followingRoute = false;
    private bool movementActive = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = useGravity;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.constraints = RigidbodyConstraints.None;
        rb.isKinematic = true; // 🚫 pelota quieta hasta que termine el video

        // Buscar automáticamente el VideoTrigger si no está asignado
        if (videoTrigger == null)
            videoTrigger = FindObjectOfType<VideoTrigger>();

        // Suscribirse al evento del VideoTrigger
        VideoTrigger.OnVideoCompleted += OnVideoFinished;

        // Preparar ruta si existe
        if (routePoints != null && routePoints.Length > 0)
        {
            followingRoute = true;
            currentTargetIndex = 0;
            SetNextTarget();
        }
        else
        {
            followingRoute = false;
            currentDirection = transform.forward;
        }

        Debug.Log("🏀 Pelota lista pero esperando a que termine el video...");
    }

    void Update()
    {
        // Espera hasta que el video haya terminado y el delay haya pasado
        if (!movementActive)
        {
            if (videoFinished && waitingDelay)
            {
                delayTimer += Time.deltaTime;
                if (delayTimer >= delayAfterVideo)
                {
                    StartMovement();
                }
            }
            return;
        }

        // Mantener velocidad constante
        if (rb.velocity.magnitude != moveSpeed)
        {
            rb.velocity = rb.velocity.normalized * moveSpeed;
        }

        if (followingRoute)
        {
            FollowRoute();
        }
    }

    private void FollowRoute()
    {
        if (routePoints == null || routePoints.Length == 0) return;

        Transform target = routePoints[currentTargetIndex];
        Vector3 toTarget = (target.position - transform.position).normalized;

        // Suaviza la transición hacia el siguiente punto
        currentDirection = Vector3.Lerp(currentDirection, toTarget, Time.deltaTime * 2f);
        rb.velocity = currentDirection * moveSpeed;

        // Si está muy cerca del punto, pasar al siguiente
        float distance = Vector3.Distance(transform.position, target.position);
        if (distance <= pointReachThreshold)
        {
            AdvanceToNextPoint();
        }
    }

    private void AdvanceToNextPoint()
    {
        currentTargetIndex++;

        if (currentTargetIndex >= routePoints.Length)
        {
            if (loopRoute)
            {
                currentTargetIndex = 0;
            }
            else
            {
                followingRoute = false;
                rb.velocity = Vector3.zero;
                Debug.Log("🏁 Ruta completada.");
                return;
            }
        }

        SetNextTarget();
    }

    private void SetNextTarget()
    {
        if (routePoints.Length == 0) return;

        Vector3 direction = (routePoints[currentTargetIndex].position - transform.position).normalized;
        currentDirection = direction;

        Debug.Log($"🎯 Nuevo objetivo: {routePoints[currentTargetIndex].name}");
    }

    void OnCollisionEnter(Collision collision)
    {
        // Si colisiona con uno de los puntos de la ruta → avanzar al siguiente
        for (int i = 0; i < routePoints.Length; i++)
        {
            if (collision.gameObject.transform == routePoints[i])
            {
                Debug.Log($"🎯 Colisión con punto de ruta: {collision.gameObject.name}");
                currentTargetIndex = i;
                AdvanceToNextPoint();
                return;
            }
        }

        // Si choca con otra cosa → rebote normal
        Vector3 normal = collision.contacts[0].normal;
        currentDirection = Vector3.Reflect(currentDirection, normal).normalized;
        rb.velocity = currentDirection * moveSpeed * bounceFactor;

        Debug.Log($"💥 Rebote con {collision.gameObject.name}");
    }

    // 🔔 Se llama cuando el video termina
    private void OnVideoFinished()
    {
        videoFinished = true;
        waitingDelay = true;
        delayTimer = 0f;
        Debug.Log($"🎬 Video terminado. Esperando {delayAfterVideo}s antes de iniciar movimiento...");
    }

    private void StartMovement()
    {
        rb.isKinematic = false;
        movementActive = true;
        waitingDelay = false;

        rb.velocity = currentDirection * moveSpeed;
        Debug.Log("🏀 Movimiento iniciado después del video");
    }

    public void ResetRoute()
    {
        currentTargetIndex = 0;
        followingRoute = routePoints.Length > 0;
        movementActive = false;
        rb.isKinematic = true;
        videoFinished = false;
        waitingDelay = false;
        delayTimer = 0f;
        Debug.Log("♻️ Ruta reseteada y esperando nuevo video");
    }

    void OnDestroy()
    {
        VideoTrigger.OnVideoCompleted -= OnVideoFinished;
    }

    void OnDrawGizmosSelected()
    {
        // Dibujar la ruta
        if (routePoints != null && routePoints.Length > 1)
        {
            Gizmos.color = Color.yellow;
            for (int i = 0; i < routePoints.Length - 1; i++)
            {
                Gizmos.DrawLine(routePoints[i].position, routePoints[i + 1].position);
            }

            if (loopRoute)
                Gizmos.DrawLine(routePoints[routePoints.Length - 1].position, routePoints[0].position);
        }

        // Dirección actual
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, currentDirection * 2f);
    }
}
