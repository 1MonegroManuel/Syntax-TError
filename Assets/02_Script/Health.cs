using UnityEngine;

public class Health : MonoBehaviour
{
    public float maxHealth = 100f;
    private float currentHealth;
    public bool destroyOnDeath = true;

    public Animator animator;

    void Start()
    {
        currentHealth = maxHealth;
        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            if (animator != null)
                animator.SetTrigger("Hit"); // si tienes animación de golpe
        }
    }

    private void Die()
    {
        if (animator != null)
            animator.SetTrigger("Die");

        if (destroyOnDeath)
            Destroy(gameObject, 2f); // espera un poco por la animación
    }
}
