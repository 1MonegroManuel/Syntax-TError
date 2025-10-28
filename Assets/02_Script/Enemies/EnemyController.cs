using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class EnemyController : MonoBehaviour
{
    [Header("Comportamiento del enemigo")]
    public Transform target;               // Referencia al jugador
    public float moveSpeed = 2f;           // Velocidad de movimiento
    public float attackDistance = 1.5f;    // Distancia para atacar
    public float attackForce = 6f;         // Fuerza del empuje o "tacleada"
    public int bpm = 35;                   // Golpes por minuto (ataques)
    public float vida = 30f;               // Vida del enemigo
    public float knockForce = 5f;          // Fuerza del retroceso cuando recibe daño

    [Header("Componentes")]
    public Animator animator;

    private CharacterController controller;
    private bool canAttack = true;
    private bool muerto = false;
    private Vector3 knockback = Vector3.zero;
    private float attackCooldown;
    private float gravity = -9.81f;
    private Vector3 velocity;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        attackCooldown = 60f / bpm; // Convierte BPM a segundos entre ataques

        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                target = player.transform;
        }
    }

    void Update()
    {
        if (muerto || target == null) return;

        Vector3 dir = (target.position - transform.position);
        dir.y = 0f;
        float distance = dir.magnitude;
        dir.Normalize();

        // Aplica gravedad
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // Aplica knockback si está activo
        if (knockback.magnitude > 0.1f)
        {
            controller.Move(knockback * Time.deltaTime);
            knockback = Vector3.Lerp(knockback, Vector3.zero, 5f * Time.deltaTime);
            return;
        }

        // Movimiento o ataque según la distancia
        if (distance > attackDistance)
        {
            // Persigue al jugador
            controller.Move(dir * moveSpeed * Time.deltaTime);
            if (animator != null)
                animator.SetBool("IsRunning", true);
        }
        else
        {
            if (animator != null)
                animator.SetBool("IsRunning", false);

            if (canAttack)
                StartCoroutine(Attack(dir));
        }

        // Siempre mirar hacia el jugador (rotación suave)
        if (dir != Vector3.zero)
        {
            Quaternion rot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, 8f * Time.deltaTime);
        }

    }

    IEnumerator Attack(Vector3 dir)
    {
        canAttack = false;

        if (animator != null)
            animator.SetTrigger("Attack");

        // Pequeña tacleada hacia adelante
        knockback = dir * attackForce;

        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    // ---------------- DAÑO Y MUERTE ----------------
    public void RecibirDaño(float cantidad)
    {
        if (muerto) return;

        vida -= cantidad;
        Debug.Log($"{name} recibió {cantidad} de daño. Vida restante: {vida}");

        if (animator != null)
            animator.SetTrigger("Hit");

        if (vida <= 0)
            Matar();
    }

    void Matar()
    {
        muerto = true;
        canAttack = false;

        if (animator != null)
        {
            animator.SetBool("IsRunning", false);
            animator.SetTrigger("Die");
        }

        // Desactiva colisiones y movimiento
        controller.enabled = false;

        Destroy(gameObject, 2f); // Desaparece tras 2 segundos
    }

    // Cuando recibe un golpe con fuerza
    public void TakeHit(Vector3 hitDir, float force)
    {
        knockback = hitDir.normalized * force;
    }
}
