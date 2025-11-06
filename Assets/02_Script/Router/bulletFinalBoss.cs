using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bulletFinalBoss : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    public float speed = 20f; // Velocidad de la bala (más rápida que el proyectil del jefe)
    private Vector3 direction; // Dirección de movimiento
    
    [Header("Configuración de Daño")]
    public float damage = 10f; // Daño que causa al jugador
    
    [Header("Tiempo de Vida")]
    public float lifetime = 3f; // Tiempo antes de destruirse (3 segundos)
    
    private float spawnTime; // Tiempo en que se creó la bala
    
    void Start()
    {
        spawnTime = Time.time;
        
        // Calcular dirección hacia el jugador o hacia adelante
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            direction = (player.transform.position - transform.position).normalized;
        }
        else
        {
            // Si no hay jugador, moverse hacia adelante
            direction = transform.forward;
        }
        
        // Rotar la bala para que apunte en la dirección de movimiento
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
        
        // Destruir la bala después del tiempo de vida
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // Mover la bala en línea recta
        transform.position += direction * speed * Time.deltaTime;
        
        // Verificar si ha pasado el tiempo de vida (backup por si Destroy no funciona)
        if (Time.time - spawnTime >= lifetime)
        {
            Destroy(gameObject);
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        // Detectar colisión con el jugador
        if (other.CompareTag("Player"))
        {
            // Intentar quitar vida al jugador
            // Buscar componente de salud del jugador
            var playerHealth = other.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }
            else
            {
                // Intentar con otro nombre común de componente
                var health = other.GetComponent<Health>();
                if (health != null)
                {
                    health.TakeDamage(damage);
                }
                else
                {
                    // Buscar en el objeto padre
                    var parentHealth = other.GetComponentInParent<Health>();
                    if (parentHealth != null)
                    {
                        parentHealth.TakeDamage(damage);
                    }
                    else
                    {
                        var parentHealthAlt = other.GetComponentInParent<Health>();
                        if (parentHealthAlt != null)
                        {
                            parentHealthAlt.TakeDamage(damage);
                        }
                    }
                }
            }
            
            // Destruir la bala al impactar
            Destroy(gameObject);
        }
    }
    
    // Método público para establecer la dirección desde el jefe
    public void SetDirection(Vector3 dir)
    {
        direction = dir.normalized;
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }
}
