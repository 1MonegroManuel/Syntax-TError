using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class finalBossController : MonoBehaviour
{
    [Header("Vida y Salud")]
    public float maxHealth = 1000f;
    [SerializeField] private float currentHealth;
    public float healthRegenRate = 0f; // Regeneración de vida por segundo (0 = desactivado)
    public float healthRegenDelay = 5f; // Tiempo después del daño antes de regenerar
    private float lastDamageTime = 0f;
    public bool isDead = false;
    public bool isInvulnerable = false;
    public float invulnerabilityDuration = 0.5f; // Tiempo de invulnerabilidad después de recibir daño
    
    [Header("UI de Vida")]
    public Slider healthBarSlider; // Barra de vida (opcional)
    public GameObject healthBarCanvas; // Canvas con la barra de vida (opcional)
    public bool showHealthBar = true;
    
    [Header("Efectos Visuales")]
    public GameObject damageEffect; // Efecto al recibir daño (opcional)
    public GameObject deathEffect; // Efecto al morir (opcional)
    public float damageFlashDuration = 0.1f;
    private Renderer[] renderers; // Para hacer efecto de parpadeo al recibir daño
    private Color[] originalColors;
    
    [Header("Animaciones")]
    public Animator animator; // Permite asignar manualmente. Si es null, se busca en hijos (Armature/huesos)
    private string currentAnimation;
    private float animationCooldown = 0f;
    public string deathAnimationName = "Death"; // Nombre de la animación de muerte
    public string damageAnimationName = "Damage"; // Nombre de la animación de daño (opcional)
    [Range(0.1f, 2f)] public float animationSpeed = 1f; // Velocidad global de animación
    
    [Header("Ataques")]
    public GameObject projectilePrefab; // Prefab del objeto a lanzar
    public float attackCooldown = 3f; // Tiempo entre ataques
    private float lastAttackTime = 0f;
    
    [Header("Puntos de Spawn de Tentáculos")]
    public Transform tentacleRightUpper; // Tentáculo derecho superior (Attack1)
    public Transform tentacleLeftUpper; // Tentáculo izquierdo superior (Attack2)
    public Transform tentacleUpper1; // Primer tentáculo superior (Attack3)
    public Transform tentacleUpper2; // Segundo tentáculo superior (Attack3)
    public Transform tentacleUpper3; // Tercer tentáculo superior (Attack3)
    public Transform tentacleUpper4; // Cuarto tentáculo superior (Attack3)
    
    [Header("Posiciones")]
    public Vector3 bossPosition = new Vector3(-0.038077f, 20.25f, 42.39f);
    public Vector3 arenaPosition = new Vector3(-0.029737f, 21.8f, 43.224f);
    
    [Header("Movimiento")]
    public Transform player; // Referencia al jugador
    public float followSpeed = 2f; // Velocidad de seguimiento
    public float rotationSpeed = 5f; // Velocidad de rotación hacia el jugador
    private Vector3 initialPosition;
    
    [Header("Configuración de Proyectiles")]
    public float projectileSpeed = 10f;
    public float projectileLifetime = 5f;
    
    [Header("Duración de Animaciones (segundos)")]
    public float idleDuration = 2f;
    public float attack1Duration = 3f;
    public float attack2Duration = 3f;
    public float attack3Duration = 4f;
    
    [Header("Eventos")]
    public System.Action<float> OnHealthChanged; // Evento cuando cambia la vida
    public System.Action OnBossDeath; // Evento cuando el jefe muere
    
    private bool isAttacking = false;
    private bool isDying = false;
    
    void Start()
    {
        // Resolver Animator: usar el asignado o buscar en hijos (ej. Armature/huesos)
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>(true);
        }
        if (animator == null)
        {
            Debug.LogError("Animator no encontrado. Asigna el Animator del 'Armature' o hueso raíz en el Inspector.");
        }
        else
        {
            animator.speed = animationSpeed;
        }
        
        // Inicializar sistema de vida
        currentHealth = maxHealth;
        isDead = false;
        isDying = false;
        
        // Obtener renderers para efectos visuales
        renderers = GetComponentsInChildren<Renderer>();
        originalColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i].material != null)
            {
                // Obtener el color original del material usando el método auxiliar
                originalColors[i] = GetMaterialColor(renderers[i].material);
            }
        }
        
        // Establecer posición inicial
        transform.position = bossPosition;
        initialPosition = transform.position;
        
        // Buscar jugador si no está asignado
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }
        
        // Configurar UI de vida
        UpdateHealthBar();
        if (healthBarCanvas != null)
        {
            healthBarCanvas.SetActive(showHealthBar);
        }
        
        // Iniciar con animación idle
        PlayAnimation("Idle");
    }
    
    void Update()
    {
        // No hacer nada si está muerto
        if (isDead || isDying)
        {
            return;
        }
        
        // Regenerar vida si está configurado
        if (healthRegenRate > 0 && Time.time >= lastDamageTime + healthRegenDelay)
        {
            RegenerateHealth();
        }
        
        // Seguir al jugador si existe
        if (player != null)
        {
            FollowPlayer();
        }
        
        // Controlar animaciones y ataques
        if (!isAttacking && Time.time >= lastAttackTime + attackCooldown)
        {
            // Seleccionar ataque aleatorio
            int randomAttack = Random.Range(1, 4); // 1, 2 o 3
            StartCoroutine(ExecuteAttack(randomAttack));
        }
        
        // Actualizar cooldown de animación
        if (animationCooldown > 0)
        {
            animationCooldown -= Time.deltaTime;
        }
        
        // Actualizar invulnerabilidad
        if (isInvulnerable && Time.time >= lastDamageTime + invulnerabilityDuration)
        {
            isInvulnerable = false;
        }
    }
    
    void FollowPlayer()
    {
        // Solo rotar hacia el jugador, sin moverse
        Vector3 direction = (player.position - transform.position).normalized;
        
        // Ignorar la componente Y para que solo rote horizontalmente
        direction.y = 0;
        
        // Rotar hacia el jugador solo si hay dirección válida
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
        }
        
        // El jefe se mantiene en su posición inicial, no se mueve
    }
    
    IEnumerator ExecuteAttack(int attackNumber)
    {
        isAttacking = true;
        lastAttackTime = Time.time;
        
        switch (attackNumber)
        {
            case 1:
                yield return StartCoroutine(Attack1());
                break;
            case 2:
                yield return StartCoroutine(Attack2());
                break;
            case 3:
                yield return StartCoroutine(Attack3());
                break;
        }
        
        // Volver a idle después del ataque
        PlayAnimation("Idle");
        yield return new WaitForSeconds(idleDuration);
        
        isAttacking = false;
    }
    
    IEnumerator Attack1()
    {
        // Atacar desde tentáculo derecho superior
        PlayAnimation("Attack1");
        yield return new WaitForSeconds(attack1Duration);
        
        if (tentacleRightUpper != null && projectilePrefab != null)
        {
            LaunchProjectile(tentacleRightUpper.position, arenaPosition);
        }
    }
    
    IEnumerator Attack2()
    {
        // Atacar desde tentáculo izquierdo superior
        PlayAnimation("Attack2");
        yield return new WaitForSeconds(attack2Duration);
        
        if (tentacleLeftUpper != null && projectilePrefab != null)
        {
            LaunchProjectile(tentacleLeftUpper.position, arenaPosition);
        }
    }
    
    IEnumerator Attack3()
    {
        // Atacar desde los 4 tentáculos superiores
        PlayAnimation("Attack3");
        yield return new WaitForSeconds(attack3Duration);
        
        if (projectilePrefab != null)
        {
            // Lanzar desde los 4 tentáculos
            Transform[] tentacles = { tentacleUpper1, tentacleUpper2, tentacleUpper3, tentacleUpper4 };
            
            foreach (Transform tentacle in tentacles)
            {
                if (tentacle != null)
                {
                    LaunchProjectile(tentacle.position, arenaPosition);
                }
            }
        }
    }
    
    void LaunchProjectile(Vector3 startPosition, Vector3 targetPosition)
    {
        // Instanciar proyectil
        GameObject projectile = Instantiate(projectilePrefab, startPosition, Quaternion.identity);
        
        // Calcular dirección hacia la arena
        Vector3 direction = (targetPosition - startPosition).normalized;
        
        // Configurar movimiento del proyectil
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = direction * projectileSpeed;
        }
        else
        {
            // Si no tiene Rigidbody, usar coroutine para moverlo
            StartCoroutine(MoveProjectile(projectile.transform, startPosition, targetPosition));
        }
        
        // Destruir proyectil después de un tiempo
        Destroy(projectile, projectileLifetime);
    }
    
    IEnumerator MoveProjectile(Transform projectile, Vector3 startPos, Vector3 targetPos)
    {
        float elapsedTime = 0f;
        float journeyTime = Vector3.Distance(startPos, targetPos) / projectileSpeed;
        
        while (elapsedTime < journeyTime)
        {
            elapsedTime += Time.deltaTime;
            float fractionOfJourney = elapsedTime / journeyTime;
            projectile.position = Vector3.Lerp(startPos, targetPos, fractionOfJourney);
            yield return null;
        }
    }
    
    void PlayAnimation(string animationName)
    {
        if (animator != null && currentAnimation != animationName)
        {
            animator.Play(animationName);
            currentAnimation = animationName;
        }
    }
    
    // ========== SISTEMA DE VIDA ==========
    
    /// <summary>
    /// Recibe daño y actualiza la vida del jefe
    /// </summary>
    /// <param name="damage">Cantidad de daño a recibir</param>
    public void TakeDamage(float damage)
    {
        if (isDead || isDying || isInvulnerable)
        {
            return;
        }
        
        // Aplicar daño
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        
        // Actualizar tiempo de último daño
        lastDamageTime = Time.time;
        isInvulnerable = true;
        
        // Actualizar UI
        UpdateHealthBar();
        
        // Invocar evento
        OnHealthChanged?.Invoke(currentHealth);
        
        // Efectos visuales
        StartCoroutine(DamageFlash());
        
        if (damageEffect != null)
        {
            Instantiate(damageEffect, transform.position, Quaternion.identity);
        }
        
        // Reproducir animación de daño si existe
        if (!string.IsNullOrEmpty(damageAnimationName) && animator != null)
        {
            animator.SetTrigger("TakeDamage");
        }
        
        // Verificar si está muerto
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    /// <summary>
    /// Cura al jefe
    /// </summary>
    /// <param name="healAmount">Cantidad de vida a curar</param>
    public void Heal(float healAmount)
    {
        if (isDead || isDying)
        {
            return;
        }
        
        currentHealth += healAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        
        UpdateHealthBar();
        OnHealthChanged?.Invoke(currentHealth);
    }
    
    /// <summary>
    /// Regenera vida automáticamente
    /// </summary>
    void RegenerateHealth()
    {
        if (currentHealth < maxHealth && !isDead && !isDying)
        {
            Heal(healthRegenRate * Time.deltaTime);
        }
    }
    
    /// <summary>
    /// Mata al jefe
    /// </summary>
    public void Die()
    {
        if (isDead || isDying)
        {
            return;
        }
        
        isDying = true;
        isDead = true;
        
        // Dejar de atacar
        StopAllCoroutines();
        
        // Reproducir animación de muerte
        if (!string.IsNullOrEmpty(deathAnimationName) && animator != null)
        {
            PlayAnimation(deathAnimationName);
        }
        
        // Efecto de muerte
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }
        
        // Invocar evento de muerte
        OnBossDeath?.Invoke();
        
        // Desactivar componentes
        StartCoroutine(DeathSequence());
    }
    
    /// <summary>
    /// Secuencia de muerte del jefe
    /// </summary>
    IEnumerator DeathSequence()
    {
        // Esperar a que termine la animación de muerte
        yield return new WaitForSeconds(2f);
        
        // Desactivar el jefe
        if (healthBarCanvas != null)
        {
            healthBarCanvas.SetActive(false);
        }
        
        // Desactivar colliders y otros componentes
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }
        
        // Ocultar el modelo después de un tiempo
        yield return new WaitForSeconds(3f);
        
        gameObject.SetActive(false);
    }
    
    /// <summary>
    /// Obtiene el nombre de la propiedad de color del material (si existe)
    /// </summary>
    private string GetColorPropertyName(Material material)
    {
        if (material == null) return null;
        
        // Lista de propiedades de color comunes en diferentes shaders
        string[] colorProperties = { "_Color", "_BaseColor", "_MainColor", "_TintColor" };
        
        foreach (string prop in colorProperties)
        {
            if (material.HasProperty(prop))
            {
                return prop;
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// Obtiene el color del material usando la propiedad disponible
    /// </summary>
    private Color GetMaterialColor(Material material)
    {
        if (material == null) return Color.white;
        
        string colorProp = GetColorPropertyName(material);
        if (colorProp != null)
        {
            return material.GetColor(colorProp);
        }
        
        return Color.white; // Color predeterminado si no tiene propiedad de color
    }
    
    /// <summary>
    /// Establece el color del material usando la propiedad disponible
    /// </summary>
    private void SetMaterialColor(Material material, Color color)
    {
        if (material == null) return;
        
        string colorProp = GetColorPropertyName(material);
        if (colorProp != null)
        {
            material.SetColor(colorProp, color);
        }
    }
    
    /// <summary>
    /// Efecto visual de parpadeo al recibir daño
    /// </summary>
    IEnumerator DamageFlash()
    {
        // Cambiar color a rojo
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i].material != null)
            {
                // Cambiar el color usando el método auxiliar
                SetMaterialColor(renderers[i].material, Color.red);
            }
        }
        
        yield return new WaitForSeconds(damageFlashDuration);
        
        // Restaurar color original
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i].material != null)
            {
                // Restaurar el color original usando el método auxiliar
                SetMaterialColor(renderers[i].material, originalColors[i]);
            }
        }
    }
    
    /// <summary>
    /// Actualiza la barra de vida en la UI
    /// </summary>
    void UpdateHealthBar()
    {
        if (healthBarSlider != null)
        {
            healthBarSlider.value = currentHealth / maxHealth;
        }
    }
    
    /// <summary>
    /// Obtiene la vida actual
    /// </summary>
    public float GetCurrentHealth()
    {
        return currentHealth;
    }
    
    /// <summary>
    /// Obtiene el porcentaje de vida (0-1)
    /// </summary>
    public float GetHealthPercentage()
    {
        return currentHealth / maxHealth;
    }
    
    /// <summary>
    /// Restablece la vida del jefe al máximo
    /// </summary>
    public void ResetHealth()
    {
        currentHealth = maxHealth;
        isDead = false;
        isDying = false;
        UpdateHealthBar();
    }
    
    // Método para configurar los puntos de spawn de tentáculos manualmente si no están asignados
    void SetupTentacleSpawnPoints()
    {
        // Si no están asignados, crear puntos de spawn en posiciones relativas
        if (tentacleRightUpper == null)
        {
            GameObject tentaclePoint = new GameObject("TentacleRightUpper");
            tentaclePoint.transform.SetParent(transform);
            tentaclePoint.transform.localPosition = new Vector3(1f, 1f, 0f); // Ajustar según modelo
            tentacleRightUpper = tentaclePoint.transform;
        }
        
        if (tentacleLeftUpper == null)
        {
            GameObject tentaclePoint = new GameObject("TentacleLeftUpper");
            tentaclePoint.transform.SetParent(transform);
            tentaclePoint.transform.localPosition = new Vector3(-1f, 1f, 0f); // Ajustar según modelo
            tentacleLeftUpper = tentaclePoint.transform;
        }
        
        // Configurar los 4 tentáculos superiores
        if (tentacleUpper1 == null)
        {
            GameObject tentaclePoint = new GameObject("TentacleUpper1");
            tentaclePoint.transform.SetParent(transform);
            tentaclePoint.transform.localPosition = new Vector3(0.5f, 1f, 0.5f);
            tentacleUpper1 = tentaclePoint.transform;
        }
        
        if (tentacleUpper2 == null)
        {
            GameObject tentaclePoint = new GameObject("TentacleUpper2");
            tentaclePoint.transform.SetParent(transform);
            tentaclePoint.transform.localPosition = new Vector3(-0.5f, 1f, 0.5f);
            tentacleUpper2 = tentaclePoint.transform;
        }
        
        if (tentacleUpper3 == null)
        {
            GameObject tentaclePoint = new GameObject("TentacleUpper3");
            tentaclePoint.transform.SetParent(transform);
            tentaclePoint.transform.localPosition = new Vector3(0.5f, 1f, -0.5f);
            tentacleUpper3 = tentaclePoint.transform;
        }
        
        if (tentacleUpper4 == null)
        {
            GameObject tentaclePoint = new GameObject("TentacleUpper4");
            tentaclePoint.transform.SetParent(transform);
            tentaclePoint.transform.localPosition = new Vector3(-0.5f, 1f, -0.5f);
            tentacleUpper4 = tentaclePoint.transform;
        }
    }
    
    // Método para invocar desde el editor o Start
    void OnValidate()
    {
        if (Application.isPlaying && transform.position != bossPosition)
        {
            transform.position = bossPosition;
        }
        
        // Asegurar que la vida actual no exceda la máxima
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
    }
    
    // Método para detectar colisiones con proyectiles o ataques del jugador
    void OnTriggerEnter(Collider other)
    {
        // Detectar si es un proyectil o ataque del jugador
        // Ajusta según tu sistema de daño
        if (other.CompareTag("PlayerWeapon") || other.CompareTag("PlayerProjectile"))
        {
            // Obtener daño del objeto que impactó
            float damage = 10f; // Daño por defecto
            
            // Intentar obtener componente de daño si existe
            var damageDealer = other.GetComponent<IDamageDealer>();
            if (damageDealer != null)
            {
                damage = damageDealer.GetDamage();
            }
            
            TakeDamage(damage);
        }
    }
    
    // Método para debug (puedes llamarlo desde el inspector o desde otro script)
    [ContextMenu("Take 100 Damage")]
    void TestDamage()
    {
        TakeDamage(100f);
    }
    
    [ContextMenu("Reset Health")]
    void TestResetHealth()
    {
        ResetHealth();
    }
}

// Interfaz opcional para objetos que causan daño
public interface IDamageDealer
{
    float GetDamage();
}
