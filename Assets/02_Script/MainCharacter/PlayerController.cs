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

    [Header("Ataques del jugador")]
    public float attackCooldown = 0.7f; // Tiempo entre golpes
    public int comboThreshold = 4;      // Número de golpes antes del combo
    public float attackRange = 1.5f;    // Rango de golpe
    public float attackDamage = 20f;    // Daño del golpe

    [Header("Referencias")]
    public Transform model;
    public Animator animator;
    public ParticleSystem stepParticles;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private bool isJumping;
    private int jumpCount;
    private bool touchingFloor = false;

    // Variables de ataque
    private bool canAttack = true;
    private bool lastAttackRight = false;
    private int attackCount = 0;
    private float lastAttackTime = 0f;
    private bool isInCombo = false;

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
        Movimiento();
        Saltar();
        Ataque();
    }

    // ------------------- MOVIMIENTO -------------------
    void Movimiento()
    {
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
            isJumping = false;
            animator.SetBool("IsJumping", false);

            if (touchingFloor)
                jumpCount = 0;
        }

        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 move = new Vector3(-moveZ, 0, moveX);

        if (move.magnitude >= 0.1f && canAttack && !isInCombo)
        {
            Quaternion targetRotation = Quaternion.LookRotation(move);
            model.rotation = Quaternion.Slerp(model.rotation, targetRotation, Time.deltaTime * rotationSpeed);
            controller.Move(move * moveSpeed * Time.deltaTime);
        }

        bool isRunning = move.magnitude > 0.1f && !isJumping;
        animator.SetBool("IsRunning", isRunning);

        // Control de partículas al caminar
        if (stepParticles != null)
        {
            if (isRunning && isGrounded)
            {
                if (!stepParticles.isPlaying)
                    stepParticles.Play();
            }
            else if (stepParticles.isPlaying)
            {
                stepParticles.Stop();
            }
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    // ------------------- SALTO -------------------
    void Saltar()
    {
        if (Input.GetButtonDown("Jump") && jumpCount < maxJumps)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            isJumping = true;
            jumpCount++;

            animator.SetBool("IsRunning", false);
            animator.SetBool("IsJumping", true);
        }
    }

    // ------------------- ATAQUES -------------------
    void Ataque()
    {
        if (isInCombo || !canAttack) return;

        if (Input.GetKeyDown(KeyCode.P))
        {
            // 🔄 Girar hacia el enemigo más cercano
            Transform enemigo = BuscarEnemigoCercano(8f);
            if (enemigo != null)
            {
                Vector3 dir = (enemigo.position - transform.position);
                dir.y = 0;
                if (dir != Vector3.zero)
                {
                    Quaternion targetRot = Quaternion.LookRotation(dir);
                    model.rotation = Quaternion.Slerp(model.rotation, targetRot, 1f);
                }
            }

            // Detiene movimiento
            animator.SetBool("IsRunning", false);
            isJumping = false;
            velocity = Vector3.zero;

            canAttack = false;
            lastAttackTime = Time.time;
            attackCount++;

            // Combo final
            if (attackCount >= comboThreshold)
            {
                animator.SetTrigger("Combo");
                attackCount = 0;
                isInCombo = true;
                Invoke(nameof(EndCombo), 1.3f);
            }
            else
            {
                if (lastAttackRight)
                {
                    animator.SetTrigger("AttackLeft");
                    lastAttackRight = false;
                }
                else
                {
                    animator.SetTrigger("AttackRight");
                    lastAttackRight = true;
                }

                Invoke(nameof(EnableAttack), attackCooldown);
            }

            Invoke(nameof(ResetCombo), 1.4f);
        }
    }

    // ------------------- ATAQUE EFECTIVO -------------------
    // 🔹 Este método lo llama el evento de animación
    public void AttackHit()
    {
        Collider[] enemigos = Physics.OverlapSphere(transform.position + transform.forward * 1f, attackRange);

        foreach (var col in enemigos)
        {
            if (col.CompareTag("Enemy"))
            {
                var enemigo = col.GetComponent<EnemyController>();
                if (enemigo != null)
                    enemigo.RecibirDaño(attackDamage);
            }
        }

        Debug.Log("💥 Golpe ejecutado, enemigos dentro del rango: " + enemigos.Length);
    }

    void EnableAttack() => canAttack = true;
    void EndCombo() { isInCombo = false; canAttack = true; }
    void ResetCombo() { if (Time.time - lastAttackTime > 1.4f) attackCount = 0; }

    // ------------------- COLISIONES -------------------
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        touchingFloor = hit.collider.CompareTag("Floor");
    }

    // ------------------- EFECTOS -------------------
    public void Step()
    {
        if (stepParticles != null && controller.isGrounded)
        {
            stepParticles.Play();
        }
    }

    // ------------------- DETECCIÓN DE ENEMIGOS -------------------
    Transform BuscarEnemigoCercano(float radio = 10f)
    {
        GameObject[] enemigos = GameObject.FindGameObjectsWithTag("Enemy");
        Transform masCercano = null;
        float distanciaMin = Mathf.Infinity;

        foreach (var e in enemigos)
        {
            float d = Vector3.Distance(transform.position, e.transform.position);
            if (d < distanciaMin && d <= radio)
            {
                distanciaMin = d;
                masCercano = e.transform;
            }
        }
        return masCercano;
    }

    // ------------------- DEBUG -------------------
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + transform.forward * 1f, attackRange);
    }
}
