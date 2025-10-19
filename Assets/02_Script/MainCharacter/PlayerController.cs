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
    public ParticleSystem stepParticles; // 🔹 NUEVO

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

        // Movimiento lateral
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        Vector3 move = new Vector3(moveX, 0, moveZ);

        if (move.magnitude >= 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(move);
            model.rotation = Quaternion.Slerp(model.rotation, targetRotation, Time.deltaTime * rotationSpeed);
            controller.Move(move * moveSpeed * Time.deltaTime);
        }

        // Animación correr
        bool isRunning = move.magnitude > 0.1f && !isJumping;
        animator.SetBool("IsRunning", isRunning);

        // 🔹 Controlar partículas de pasos
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

        // Gravedad
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
    // 🔹 Esta función se llamará desde la animación
    public void Step()
    {
        if (stepParticles != null && controller.isGrounded)
        {
            stepParticles.Play();
        }
    }

}
