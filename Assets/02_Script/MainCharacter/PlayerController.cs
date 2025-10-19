using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movimiento del jugador")]
    public float moveSpeed = 4f;
    public float gravity = -9.81f;
    public float rotationSpeed = 10f;
    public float jumpHeight = 2f;
    public int maxJumps = 2;

    [Header("Referencias")]
    public Transform model;
    public Animator animator;
    public ParticleSystem stepParticles;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private bool isJumping;
    private int jumpCount;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        if (animator == null && model != null)
            animator = model.GetComponent<Animator>();

        if (animator == null)
            Debug.LogWarning("⚠️ No se encontró el Animator. Arrastra el modelo hijo al campo 'Model'.");

        if (stepParticles != null)
            stepParticles.Stop();
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
            jumpCount = 0;
        }

        // Movimiento adaptado a vista cenital (desde arriba)
        float moveX = Input.GetAxis("Horizontal"); // A (-1) / D (+1)
        float moveZ = Input.GetAxis("Vertical");   // W (+1) / S (-1)

        // Reasignamos los ejes como pediste:
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

        // Animación correr
        bool isRunning = move.magnitude > 0.1f && !isJumping;
        animator.SetBool("IsRunning", isRunning);

        // Controlar partículas de pasos
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
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            isJumping = true;
            jumpCount++;

            animator.SetBool("IsRunning", false);
            animator.SetBool("IsJumping", true);
        }

        // Aplicar gravedad
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    // Llamado desde la animación de paso
    public void Step()
    {
        if (stepParticles != null && controller.isGrounded)
        {
            stepParticles.Play();
        }
    }
}
