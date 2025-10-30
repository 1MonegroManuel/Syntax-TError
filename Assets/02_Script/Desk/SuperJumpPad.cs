using UnityEngine;

public class SuperJumpPad : MonoBehaviour
{
    [Header("Configuración del Super Salto")]
    [Tooltip("Multiplicador de la fuerza de salto (ej: 3.0 = 3 veces más alto)")]
    public float jumpMultiplier = 3.0f;
    
    [Tooltip("Tag del jugador que puede usar el super salto")]
    public string playerTag = "Player";
    
    [Tooltip("Si debe mostrar efectos visuales")]
    public bool showEffects = true;
    
    [Header("Efectos Visuales")]
    [Tooltip("Partículas que se activan al saltar")]
    public ParticleSystem jumpParticles;
    
    [Tooltip("Sonido que se reproduce al saltar")]
    public AudioSource jumpSound;
    
    [Header("Configuración Avanzada")]
    [Tooltip("Si debe resetear el salto después de usarlo")]
    public bool resetAfterUse = true;
    
    [Tooltip("Tiempo de cooldown antes de poder usarlo de nuevo")]
    public float cooldownTime = 1.0f;
    
    private PlayerController playerController;
    private float originalJumpHeight;
    private bool isOnCooldown = false;
    private float cooldownTimer = 0f;
    private bool playerIsOnPad = false;
    
    void Start()
    {
        // Buscar automáticamente el jugador
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        if (player != null)
        {
            playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                // Guardar el valor original de salto
                originalJumpHeight = playerController.jumpHeight;
                Debug.Log($"✅ SuperJumpPad configurado. Salto original: {originalJumpHeight}, Multiplicador: {jumpMultiplier}x");
            }
            else
            {
                Debug.LogError("❌ No se encontró PlayerController en el jugador.");
            }
        }
        else
        {
            Debug.LogError($"❌ No se encontró GameObject con tag '{playerTag}'.");
        }
        
        // Configurar efectos visuales
        SetupEffects();
    }
    
    void Update()
    {
        // Manejar cooldown
        if (isOnCooldown)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0f)
            {
                isOnCooldown = false;
                Debug.Log("🔄 SuperJumpPad listo para usar nuevamente");
            }
        }
    }
    
    void SetupEffects()
    {
        // Configurar partículas si están asignadas
        if (jumpParticles != null)
        {
            jumpParticles.Stop();
        }
        
        // Configurar sonido si está asignado
        if (jumpSound != null)
        {
            jumpSound.playOnAwake = false;
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            playerIsOnPad = true;
            Debug.Log("🚀 Jugador entró al SuperJumpPad - Listo para super salto");
        }
    }
    
    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag(playerTag) && playerIsOnPad && !isOnCooldown)
        {
            // Verificar si el jugador está presionando salto
            if (Input.GetButtonDown("Jump"))
            {
                Debug.Log("🚀 Jugador presionó salto sobre SuperJumpPad");
                ActivateSuperJump();
            }
        }
    }
    
    void ActivateSuperJump()
    {
        if (playerController == null) return;
        
        Debug.Log($"🚀 Activando super salto! Multiplicador: {jumpMultiplier}x");
        
        // Aplicar el multiplicador de salto temporalmente
        playerController.jumpHeight = originalJumpHeight * jumpMultiplier;
        
        // Activar efectos visuales
        if (showEffects)
        {
            PlayJumpEffects();
        }
        
        // Iniciar cooldown si está habilitado
        if (resetAfterUse)
        {
            StartCooldown();
        }
        
        // Resetear el salto después de un pequeño delay para que funcione el salto actual
        StartCoroutine(ResetJumpAfterDelay(0.1f));
        
        Debug.Log($"🚀 Salto modificado a: {playerController.jumpHeight}");
    }
    
    System.Collections.IEnumerator ResetJumpAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ResetJumpHeight();
    }
    
    void PlayJumpEffects()
    {
        // Reproducir partículas
        if (jumpParticles != null)
        {
            jumpParticles.Play();
            Debug.Log("✨ Partículas de salto activadas");
        }
        
        // Reproducir sonido
        if (jumpSound != null)
        {
            jumpSound.Play();
            Debug.Log("🔊 Sonido de salto reproducido");
        }
    }
    
    void StartCooldown()
    {
        isOnCooldown = true;
        cooldownTimer = cooldownTime;
        Debug.Log($"⏰ Cooldown iniciado: {cooldownTime} segundos");
    }
    
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            playerIsOnPad = false;
            Debug.Log("👋 Jugador salió del SuperJumpPad");
            
            // Resetear el salto cuando el jugador sale
            if (resetAfterUse && playerController != null)
            {
                ResetJumpHeight();
            }
        }
    }
    
    void ResetJumpHeight()
    {
        if (playerController != null)
        {
            playerController.jumpHeight = originalJumpHeight;
            Debug.Log($"🔄 Salto reseteado a valor original: {originalJumpHeight}");
        }
    }
    
    // Método público para activar el super salto manualmente
    public void ActivateSuperJumpManually()
    {
        Debug.Log("🔧 Activando super salto manualmente");
        ActivateSuperJump();
    }
    
    // Método público para cambiar el multiplicador
    public void SetJumpMultiplier(float newMultiplier)
    {
        jumpMultiplier = newMultiplier;
        Debug.Log($"🔧 Multiplicador de salto cambiado a: {jumpMultiplier}x");
    }
    
    // Método público para resetear el salto
    public void ResetJump()
    {
        Debug.Log("🔧 Reseteando salto manualmente");
        ResetJumpHeight();
    }
    
    // Método para debug - mostrar información en el Inspector
    void OnDrawGizmosSelected()
    {
        // Dibujar el área del trigger
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, transform.localScale);
        
        // Dibujar texto con el multiplicador
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 2, $"Super Jump\n{jumpMultiplier}x");
        #endif
    }
    
    void OnDestroy()
    {
        // Asegurar que el salto se resetee al destruir el objeto
        if (playerController != null)
        {
            ResetJumpHeight();
        }
    }
}
