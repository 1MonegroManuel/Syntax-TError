using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controlador para objetos interactuables en la arena que dañan al jefe
/// </summary>
public class ArenaInteractableController : MonoBehaviour
{
    [Header("Referencia al Jefe")]
    public finalBossController bossController; // Referencia al controlador del jefe
    
    [Header("Configuración de Objetos")]
    public GameObject[] interactablePrefabs; // Prefabs de objetos que pueden aparecer
    public int maxObjectsInArena = 5; // Máximo de objetos simultáneos en la arena
    public float spawnInterval = 5f; // Tiempo entre spawns
    public float objectLifetime = 30f; // Tiempo que dura el objeto antes de desaparecer
    
    [Header("Área de Spawn (Arena)")]
    public Vector3 arenaCenter = new Vector3(-0.029737f, 21.8f, 43.224f); // Centro de la arena
    public float arenaRadius = 10f; // Radio exterior de la arena (radio máximo)
    public float innerRadius = 3f; // Radio interior (área vacía del centro donde está el jefe)
    public float spawnHeight = 21.8f; // Altura a la que spawnan los objetos
    
    [Header("Daño al Jefe")]
    public float damageAmount = 50f; // Daño que causa al jefe al interactuar
    
    [Header("Interacción con el Jugador")]
    public float interactionRange = 2f; // Distancia para interactuar
    public KeyCode interactionKey = KeyCode.E; // Tecla para interactuar
    public bool useTriggerCollider = true; // Si usa trigger en lugar de tecla
    
    [Header("Efectos Visuales")]
    public GameObject pickupEffect; // Efecto al recoger el objeto
    public GameObject spawnEffect; // Efecto al aparecer el objeto
    
    private List<GameObject> activeObjects = new List<GameObject>(); // Lista de objetos activos
    private Transform player; // Referencia al jugador
    private float lastSpawnTime = 0f;
    
    void Start()
    {
        // Buscar el jefe si no está asignado
        if (bossController == null)
        {
            bossController = FindObjectOfType<finalBossController>();
            if (bossController == null)
            {
                Debug.LogWarning("No se encontró el controlador del jefe final!");
            }
        }
        
        // Buscar al jugador
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        
        // Inicializar spawn de objetos
        lastSpawnTime = Time.time;
        
        // Validar que innerRadius sea menor que arenaRadius
        if (innerRadius >= arenaRadius)
        {
            Debug.LogWarning("innerRadius debe ser menor que arenaRadius. Ajustando valores...");
            innerRadius = Mathf.Max(0.1f, arenaRadius * 0.3f); // 30% del radio exterior como mínimo
        }
    }
    
    void Update()
    {
        // Spawnar objetos si es necesario
        if (activeObjects.Count < maxObjectsInArena && 
            Time.time >= lastSpawnTime + spawnInterval &&
            interactablePrefabs != null && interactablePrefabs.Length > 0)
        {
            SpawnInteractableObject();
            lastSpawnTime = Time.time;
        }
        
        // Verificar interacciones si no usa trigger
        if (!useTriggerCollider && player != null)
        {
            CheckPlayerInteraction();
        }
        
        // Limpiar objetos desactivados de la lista
        activeObjects.RemoveAll(obj => obj == null);
    }
    
    /// <summary>
    /// Spawnea un objeto interactuable en la arena en forma de dona (anillo)
    /// </summary>
    void SpawnInteractableObject()
    {
        // Seleccionar prefab aleatorio
        GameObject prefabToSpawn = interactablePrefabs[Random.Range(0, interactablePrefabs.Length)];
        
        // Calcular posición aleatoria en forma de dona (anillo)
        // Generar ángulo aleatorio
        float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        
        // Generar radio aleatorio entre innerRadius y arenaRadius (área de la dona)
        float randomRadius = Random.Range(innerRadius, arenaRadius);
        
        // Calcular posición en el anillo
        Vector3 spawnPosition = new Vector3(
            arenaCenter.x + Mathf.Cos(randomAngle) * randomRadius,
            spawnHeight,
            arenaCenter.z + Mathf.Sin(randomAngle) * randomRadius
        );
        
        // Instanciar objeto
        GameObject newObject = Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);
        
        // Agregar componente de interacción si no lo tiene
        ArenaInteractable interactable = newObject.GetComponent<ArenaInteractable>();
        if (interactable == null)
        {
            interactable = newObject.AddComponent<ArenaInteractable>();
        }
        
        // Configurar el objeto interactuable
        interactable.Setup(this, damageAmount);
        
        // Efecto de spawn
        if (spawnEffect != null)
        {
            Instantiate(spawnEffect, spawnPosition, Quaternion.identity);
        }
        
        // Agregar a la lista
        activeObjects.Add(newObject);
        
        // Destruir después de un tiempo
        Destroy(newObject, objectLifetime);
    }
    
    /// <summary>
    /// Verifica si el jugador está cerca de algún objeto y puede interactuar
    /// </summary>
    void CheckPlayerInteraction()
    {
        if (player == null) return;
        
        foreach (GameObject obj in activeObjects)
        {
            if (obj == null) continue;
            
            float distance = Vector3.Distance(player.position, obj.transform.position);
            
            if (distance <= interactionRange)
            {
                // Mostrar indicador de interacción (opcional)
                // Aquí puedes mostrar UI como "Presiona E para interactuar"
                
                if (Input.GetKeyDown(interactionKey))
                {
                    InteractWithObject(obj);
                }
            }
        }
    }
    
    /// <summary>
    /// Procesa la interacción con un objeto
    /// </summary>
    public void InteractWithObject(GameObject interactableObject)
    {
        if (interactableObject == null) return;
        
        // Hacer daño al jefe
        if (bossController != null)
        {
            bossController.TakeDamage(damageAmount);
            Debug.Log($"Objeto interactuado! El jefe recibió {damageAmount} de daño.");
        }
        
        // Efecto visual
        if (pickupEffect != null)
        {
            Instantiate(pickupEffect, interactableObject.transform.position, Quaternion.identity);
        }
        
        // Remover de la lista
        activeObjects.Remove(interactableObject);
        
        // Destruir objeto
        Destroy(interactableObject);
    }
    
    /// <summary>
    /// Dibuja el área de spawn en forma de dona en el editor
    /// </summary>
    void OnDrawGizmosSelected()
    {
        Vector3 center = new Vector3(arenaCenter.x, spawnHeight, arenaCenter.z);
        int segments = 32;
        float angleStep = 360f / segments;
        
        // Dibujar círculo exterior (radio máximo)
        Gizmos.color = Color.yellow;
        Vector3 prevPointOuter = center + Vector3.forward * arenaRadius;
        
        for (int i = 1; i <= segments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 newPoint = center + new Vector3(
                Mathf.Sin(angle) * arenaRadius,
                0,
                Mathf.Cos(angle) * arenaRadius
            );
            
            Gizmos.DrawLine(prevPointOuter, newPoint);
            prevPointOuter = newPoint;
        }
        
        // Dibujar círculo interior (área vacía del centro)
        Gizmos.color = Color.red;
        Vector3 prevPointInner = center + Vector3.forward * innerRadius;
        
        for (int i = 1; i <= segments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 newPoint = center + new Vector3(
                Mathf.Sin(angle) * innerRadius,
                0,
                Mathf.Cos(angle) * innerRadius
            );
            
            Gizmos.DrawLine(prevPointInner, newPoint);
            prevPointInner = newPoint;
        }
        
        // Dibujar centro
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(center, 0.5f);
    }
    
    /// <summary>
    /// Valida los valores en el editor
    /// </summary>
    void OnValidate()
    {
        // Asegurar que innerRadius sea menor que arenaRadius
        if (innerRadius >= arenaRadius)
        {
            innerRadius = Mathf.Max(0.1f, arenaRadius - 0.1f);
        }
        
        // Asegurar valores mínimos
        if (innerRadius < 0)
        {
            innerRadius = 0;
        }
        if (arenaRadius < 0)
        {
            arenaRadius = 1f;
        }
    }
    
    /// <summary>
    /// Limpia todos los objetos activos
    /// </summary>
    public void ClearAllObjects()
    {
        foreach (GameObject obj in activeObjects)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }
        activeObjects.Clear();
    }
}

/// <summary>
/// Componente que se agrega a los objetos interactuables de la arena
/// </summary>
public class ArenaInteractable : MonoBehaviour
{
    private ArenaInteractableController controller;
    private float damageAmount;
    private bool isInteracted = false;
    
    public void Setup(ArenaInteractableController controllerRef, float damage)
    {
        controller = controllerRef;
        damageAmount = damage;
        
        // Agregar trigger collider si no existe
        if (controller.useTriggerCollider)
        {
            Collider col = GetComponent<Collider>();
            if (col == null)
            {
                SphereCollider sphereCol = gameObject.AddComponent<SphereCollider>();
                sphereCol.radius = controller.interactionRange;
                sphereCol.isTrigger = true;
            }
            else if (!col.isTrigger)
            {
                col.isTrigger = true;
            }
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (controller.useTriggerCollider && 
            other.CompareTag("Player") && 
            !isInteracted)
        {
            isInteracted = true;
            controller.InteractWithObject(gameObject);
        }
    }
    
    // Rotación o animación del objeto (opcional)
    void Update()
    {
        // Rotar el objeto para hacerlo más visible
        transform.Rotate(0, 90f * Time.deltaTime, 0);
    }
}

