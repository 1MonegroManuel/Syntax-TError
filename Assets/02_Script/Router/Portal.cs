using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    [Header("Configuración de Teletransporte")]
    [Tooltip("Objeto que será movido cuando se toque el plano")]
    public Transform objetoAMover;
    
    [Tooltip("Posición destino a donde se moverá el objeto")]
    public Vector3 posicionDestino;
    
    [Tooltip("Si está marcado, el objeto se mueve instantáneamente. Si no, se mueve suavemente")]
    public bool movimientoInstantaneo = false;
    
    [Tooltip("Velocidad de movimiento (solo si movimientoInstantaneo es false)")]
    public float velocidadMovimiento = 5f;
    
    [Tooltip("Si está marcado, se puede tocar desde cualquier dispositivo. Si no, solo funciona con mouse")]
    public bool usarRaycast = true;
    
    [Tooltip("Si está marcado, el portal funciona con trigger (el jugador entra y se teletransporta). Si no, funciona con clic")]
    public bool usarTrigger = true;
    
    [Tooltip("Tag del objeto que activará el portal (por defecto 'Player')")]
    public string tagObjetoActivador = "Player";
    
    [Header("Configuración de Cámara")]
    [Tooltip("Objeto al que la cámara enfocará después de pasar el portal (opcional)")]
    public Transform objetoEnfoqueCamara;
    
    private Camera camaraPrincipal;
    private bool moviendo = false;
    private MianCamara controladorCamara;
    
    void Start()
    {
        // Si no se asigna un objeto, intentar encontrarlo por tag "Player"
        if (objetoAMover == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                objetoAMover = player.transform;
            }
        }
        
        // Obtener la cámara principal
        camaraPrincipal = Camera.main;
        if (camaraPrincipal == null)
        {
            camaraPrincipal = FindObjectOfType<Camera>();
        }
        
        // Obtener el controlador de cámara
        if (camaraPrincipal != null)
        {
            controladorCamara = camaraPrincipal.GetComponent<MianCamara>();
            if (controladorCamara == null)
            {
                controladorCamara = camaraPrincipal.transform.root.GetComponent<MianCamara>();
            }
        }
        
        // Si usa trigger, asegurar que hay un collider configurado como trigger
        if (usarTrigger)
        {
            Collider col = GetComponent<Collider>();
            if (col == null)
            {
                Debug.LogWarning("Portal: No se encontró un Collider. Agregando BoxCollider como trigger.");
                BoxCollider boxCol = gameObject.AddComponent<BoxCollider>();
                boxCol.isTrigger = true;
            }
            else if (!col.isTrigger)
            {
                Debug.LogWarning("Portal: El Collider no está configurado como trigger. Configurándolo ahora.");
                col.isTrigger = true;
            }
        }
    }

    void Update()
    {
        // Sistema de detección de toque con raycast (funciona para móvil y PC)
        // Solo funciona si NO está usando trigger
        if (!usarTrigger && usarRaycast && Input.GetMouseButtonDown(0))
        {
            Ray ray = camaraPrincipal.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.gameObject == this.gameObject)
                {
                    MoverObjeto();
                }
            }
        }
    }
    
    // Método para cuando se toca con el mouse (solo PC)
    void OnMouseDown()
    {
        if (!usarRaycast && !usarTrigger)
        {
            MoverObjeto();
        }
    }
    
    // Método para cuando un objeto entra en el trigger del portal
    void OnTriggerEnter(Collider other)
    {
        if (usarTrigger && other.CompareTag(tagObjetoActivador))
        {
            // Si objetoAMover no está asignado, usar el objeto que entró
            if (objetoAMover == null)
            {
                objetoAMover = other.transform;
                Debug.Log($"Portal: Asignando automáticamente {other.gameObject.name} como objeto a mover.");
            }
            
            // Si el objeto que entró es el que debe moverse, teletransportarlo
            if (other.transform == objetoAMover || other.transform.IsChildOf(objetoAMover) || 
                other.transform.root == objetoAMover || objetoAMover.IsChildOf(other.transform))
            {
                Debug.Log($"Portal activado por {other.gameObject.name}. Teletransportando a {posicionDestino}");
                MoverObjeto(other.transform);
            }
        }
    }
    
    void MoverObjeto(Transform objetoEspecifico = null)
    {
        Transform objetoAMoverActual = objetoEspecifico != null ? objetoEspecifico : objetoAMover;
        
        if (objetoAMoverActual == null)
        {
            Debug.LogWarning("Portal: No hay objeto asignado para mover. Asigna un objeto en el inspector.");
            return;
        }
        
        if (moviendo && !movimientoInstantaneo)
        {
            Debug.Log("Portal: Ya se está moviendo, esperando...");
            return; // Ya se está moviendo
        }
        
        Debug.Log($"Portal: Moviendo {objetoAMoverActual.name} desde {objetoAMoverActual.position} a {posicionDestino}");
        
        // Manejar CharacterController (desactivar temporalmente para poder mover)
        CharacterController charController = objetoAMoverActual.GetComponent<CharacterController>();
        bool charControllerEnabled = false;
        if (charController != null)
        {
            charControllerEnabled = charController.enabled;
            charController.enabled = false;
        }
        
        // Manejar Rigidbody (desactivar temporalmente para evitar conflictos)
        Rigidbody rb = objetoAMoverActual.GetComponent<Rigidbody>();
        bool rbWasKinematic = false;
        Vector3 rbVelocity = Vector3.zero;
        Vector3 rbAngularVelocity = Vector3.zero;
        if (rb != null)
        {
            rbWasKinematic = rb.isKinematic;
            rbVelocity = rb.velocity;
            rbAngularVelocity = rb.angularVelocity;
            rb.isKinematic = true;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        
        if (movimientoInstantaneo)
        {
            objetoAMoverActual.position = posicionDestino;
            
            // Restaurar CharacterController
            if (charController != null && charControllerEnabled)
            {
                charController.enabled = true;
            }
            
            // Restaurar Rigidbody
            if (rb != null)
            {
                rb.isKinematic = rbWasKinematic;
            }
            
            // Cambiar la cámara a vista general después del teletransporte
            CambiarCamaraAVistaGeneral();
            
            Debug.Log($"Portal: Teletransporte completado. Nueva posición: {objetoAMoverActual.position}");
        }
        else
        {
            StartCoroutine(MoverSuavemente(objetoAMoverActual, charController, charControllerEnabled, rb, rbWasKinematic));
        }
    }
    
    IEnumerator MoverSuavemente(Transform objetoAMoverActual, CharacterController charController, bool charControllerEnabled, Rigidbody rb, bool rbWasKinematic)
    {
        moviendo = true;
        Vector3 posicionInicial = objetoAMoverActual.position;
        float distancia = Vector3.Distance(posicionInicial, posicionDestino);
        float tiempoTotal = distancia / velocidadMovimiento;
        float tiempoTranscurrido = 0f;
        
        while (tiempoTranscurrido < tiempoTotal)
        {
            tiempoTranscurrido += Time.deltaTime;
            float t = tiempoTranscurrido / tiempoTotal;
            objetoAMoverActual.position = Vector3.Lerp(posicionInicial, posicionDestino, t);
            yield return null;
        }
        
        objetoAMoverActual.position = posicionDestino;
        
        // Restaurar CharacterController
        if (charController != null && charControllerEnabled)
        {
            charController.enabled = true;
        }
        
        // Restaurar Rigidbody
        if (rb != null)
        {
            rb.isKinematic = rbWasKinematic;
        }
        
        Debug.Log($"Portal: Movimiento suave completado. Nueva posición: {objetoAMoverActual.position}");
        moviendo = false;
        
        // Cambiar la cámara a vista general después del movimiento suave
        CambiarCamaraAVistaGeneral();
    }
    
    /// <summary>
    /// Cambia la cámara para enfocar un objeto cuando se pasa el portal
    /// </summary>
    void CambiarCamaraAVistaGeneral()
    {
        if (controladorCamara != null)
        {
            // Si hay un objeto de enfoque asignado en el portal, usarlo
            if (objetoEnfoqueCamara != null)
            {
                controladorCamara.EstablecerObjetoEnfoque(objetoEnfoqueCamara);
            }
            
            controladorCamara.CambiarAVistaGeneral();
            Debug.Log($"Portal: Cámara enfocando objeto{(objetoEnfoqueCamara != null ? ": " + objetoEnfoqueCamara.name : "")}");
        }
        else
        {
            Debug.LogWarning("Portal: No se encontró el controlador de cámara (MianCamara). Asegúrate de que la cámara tenga el script MianCamara.");
        }
    }
}

