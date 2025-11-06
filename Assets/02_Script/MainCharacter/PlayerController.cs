using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movimiento del jugador")]
    public float moveSpeed = 4f;
    public float gravity = -9.81f;
    public float rotationSpeed = 10f;
    public float jumpHeight = 2f;
    public int maxJumps = 2;
    
    [Header("Super Salto")]
    [Tooltip("Multiplicador de salto cuando está sobre Grappler")]
    public float grapplerJumpMultiplier = 3.0f;

    [Header("Referencias")]
    public Transform model;
    public Animator animator;
    public ParticleSystem stepParticles;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private bool isJumping;
    private int jumpCount;

    private bool touchingFloor = false; // ✅ Detecta si está tocando el tag "Floor"
    private bool touchingGrappler = false; // ✅ Detecta si está tocando el tag "Grappler"
    private float originalJumpHeight; // ✅ Guarda el valor original del salto

    void Start()
    {
        controller = GetComponent<CharacterController>();
        
        // Guardar el valor original del salto
        originalJumpHeight = jumpHeight;

        if (animator == null && model != null)
            animator = model.GetComponent<Animator>();

        if (animator == null)
            Debug.LogWarning("⚠️ No se encontró el Animator. Arrastra el modelo hijo al campo 'Model'.");

        if (stepParticles != null)
            stepParticles.Stop();
            
        Debug.Log($"✅ PlayerController inicializado. Salto original: {originalJumpHeight}");
    }

    void Update()
    {
        // Verificar si está en el suelo
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
            isJumping = false;
            animator.SetBool("IsJumping", false);

            // ✅ Solo reinicia el contador si toca algo con tag "Floor" o "Grappler"
            if (touchingFloor || touchingGrappler)
                jumpCount = 0;
        }

        // ✅ Movimiento adaptado a vista cenital (cámara desde arriba)
        float moveX = Input.GetAxis("Horizontal"); // A (-1) / D (+1)
        float moveZ = Input.GetAxis("Vertical");   // W (+1) / S (-1)

        // W → -X
        // S → +X
        // A → -Z
        // D → +Z
        Vector3 move = new Vector3(-moveZ, 0, moveX);

        if (move.magnitude >= 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(move);
            model.rotation = Quaternion.Slerp(model.rotation, targetRotation, Time.deltaTime * rotationSpeed);
            controller.Move(move * moveSpeed * Time.deltaTime);
        }

        // Animación de correr
        bool isRunning = move.magnitude > 0.1f && !isJumping;
        animator.SetBool("IsRunning", isRunning);

        // Partículas de pasos
        if (stepParticles != null)
        {
            if (isRunning && isGrounded)
            {
                if (!stepParticles.isPlaying)
                    stepParticles.Play();
            }
            else
            {
                if (stepParticles.isPlaying)
                    stepParticles.Stop();
            }
        }

        // Saltar / doble salto
        if (Input.GetButtonDown("Jump") && jumpCount < maxJumps)
        {
            // Calcular la altura del salto según la superficie
            float currentJumpHeight = jumpHeight;
            
            if (touchingGrappler)
            {
                currentJumpHeight = originalJumpHeight * grapplerJumpMultiplier;
                Debug.Log($"🚀 SUPER SALTO! Altura: {currentJumpHeight} (Multiplicador: {grapplerJumpMultiplier}x)");
            }
            else
            {
                currentJumpHeight = originalJumpHeight;
                Debug.Log($"🦘 Salto normal. Altura: {currentJumpHeight}");
            }
            
            velocity.y = Mathf.Sqrt(currentJumpHeight * -2f * gravity);
            isJumping = true;
            jumpCount++;

            animator.SetBool("IsRunning", false);
            animator.SetBool("IsJumping", true);
        }

        // Aplicar gravedad
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    // Detectar colisiones del CharacterController
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.collider.CompareTag("Floor"))
        {
            touchingFloor = true;
            touchingGrappler = false;
            Debug.Log("🏠 Tocando Floor");
        }
        else if (hit.collider.CompareTag("Grappler"))
        {
            touchingGrappler = true;
            touchingFloor = false;
            Debug.Log("🚀 Tocando Grappler - ¡Listo para super salto!");
        }
        else
        {
            touchingFloor = false;
            touchingGrappler = false;
        }
    }

    // Evento llamado desde la animación
    public void Step()
    {
        if (stepParticles != null && controller.isGrounded)
        {
            stepParticles.Play();
        }
    }
    
    // Método público para cambiar el multiplicador de salto del Grappler
    public void SetGrapplerJumpMultiplier(float multiplier)
    {
        grapplerJumpMultiplier = multiplier;
        Debug.Log($"🔧 Multiplicador de salto del Grappler cambiado a: {multiplier}x");
    }
    
    // Método público para obtener el estado de las superficies
    public bool IsTouchingGrappler()
    {
        return touchingGrappler;
    }
    
    public bool IsTouchingFloor()
    {
        return touchingFloor;
    }
    private void OnTriggerEnter(Collider other)
    {
        // Verificar si el objeto con el que colisionamos tiene el tag "Killer"
        if (other.CompareTag("Killer"))
        {
            // Llamar al método que reinicia la escena
            RestartScene();
        }
    }
    private void RestartScene()
    {
        // Obtener el nombre de la escena actual y recargarla
        string sceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(sceneName);
    }
}
