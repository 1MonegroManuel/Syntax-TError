using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MianCamara : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Transform del jugador que la cámara seguirá")]
    public Transform targetPlayer;
    
    [Header("Configuración de Seguimiento")]
    [Tooltip("Distancia de la cámara al jugador")]
    public float followDistance = 10f;
    
    [Tooltip("Altura de la cámara sobre el jugador")]
    public float followHeight = 5f;
    
    [Tooltip("Velocidad de suavizado del seguimiento")]
    public float followSmoothness = 5f;
    
    [Header("Configuración de Enfoque a Objeto")]
    [Tooltip("Objeto al que la cámara enfocará cuando se active el portal")]
    public Transform objetoEnfoque;
    
    [Tooltip("Distancia adicional que se alejará la cámara del player")]
    public float distanciaAlejamiento = 15f;
    
    [Tooltip("Altura adicional de la cámara cuando enfoca el objeto")]
    public float alturaAdicional = 8f;
    
    [Tooltip("Velocidad de transición al enfocar objeto")]
    public float transitionSpeed = 2f;
    
    [Tooltip("Peso del enfoque (0 = mira al player, 1 = mira completamente al objeto)")]
    [Range(0f, 1f)]
    public float pesoEnfoqueObjeto = 0.7f;
    
    private Camera camara;
    private bool enfocandoObjeto = false;
    private Vector3 posicionInicial;
    private Quaternion rotacionInicial;
    private bool guardadoEstadoInicial = false;
    private Transform padreOriginal = null; // Guardar el padre original (el player)
    private Vector3 posicionLocalOriginal; // Posición local original cuando está dentro del player
    
    void Start()
    {
        camara = GetComponent<Camera>();
        if (camara == null)
        {
            camara = Camera.main;
        }
        
        // Buscar el jugador automáticamente si no está asignado
        if (targetPlayer == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                targetPlayer = player.transform;
            }
        }
        
        // Detectar si la cámara está dentro del player
        if (transform.parent != null)
        {
            // Verificar si el padre es el player
            if (targetPlayer != null && transform.parent == targetPlayer)
            {
                padreOriginal = transform.parent;
                posicionLocalOriginal = transform.localPosition;
                Debug.Log("✅ Cámara detectada dentro del player. Se desvinculará en vista general.");
            }
            else if (targetPlayer != null && transform.IsChildOf(targetPlayer))
            {
                // La cámara es nieta o descendiente del player
                padreOriginal = transform.parent;
                posicionLocalOriginal = transform.localPosition;
                Debug.Log("✅ Cámara detectada como descendiente del player. Se desvinculará en vista general.");
            }
        }
        
        // Guardar el estado inicial de la cámara
        if (!guardadoEstadoInicial)
        {
            posicionInicial = transform.position;
            rotacionInicial = transform.rotation;
            guardadoEstadoInicial = true;
        }
    }
    
    void LateUpdate()
    {
        // Si está enfocando un objeto, actualizar la posición y rotación
        if (enfocandoObjeto && objetoEnfoque != null && targetPlayer != null)
        {
            EnfocarObjeto();
        }
        // Si la cámara está dentro del player y no está enfocando, no hacer nada
        // (el seguimiento se hace automáticamente por ser hijo del player)
        // Solo aplicar seguimiento si NO está dentro del player
        else if (!enfocandoObjeto && targetPlayer != null && transform.parent != targetPlayer && !transform.IsChildOf(targetPlayer))
        {
            SeguirJugador();
        }
    }
    
    void SeguirJugador()
    {
        // Calcular la posición deseada detrás y arriba del jugador
        Vector3 direccion = -targetPlayer.forward;
        Vector3 posicionDeseada = targetPlayer.position + direccion * followDistance;
        posicionDeseada.y = targetPlayer.position.y + followHeight;
        
        // Suavizar el movimiento de la cámara
        transform.position = Vector3.Lerp(transform.position, posicionDeseada, followSmoothness * Time.deltaTime);
        
        // Hacer que la cámara mire al jugador
        Vector3 direccionMirar = targetPlayer.position - transform.position;
        if (direccionMirar != Vector3.zero)
        {
            Quaternion rotacionDeseada = Quaternion.LookRotation(direccionMirar);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotacionDeseada, followSmoothness * Time.deltaTime);
        }
    }
    
    void EnfocarObjeto()
    {
        if (objetoEnfoque == null || targetPlayer == null) return;
        
        // Calcular punto medio entre player y objeto
        Vector3 puntoMedio = (targetPlayer.position + objetoEnfoque.position) * 0.5f;
        
        // Calcular dirección desde el player hacia el objeto
        Vector3 direccionPlayerObjeto = (objetoEnfoque.position - targetPlayer.position).normalized;
        
        // Calcular posición de la cámara: alejada del player en dirección perpendicular
        // y elevada para tener buena vista
        Vector3 direccionPerpendicular = Vector3.Cross(direccionPlayerObjeto, Vector3.up).normalized;
        if (direccionPerpendicular == Vector3.zero)
        {
            direccionPerpendicular = Vector3.Cross(direccionPlayerObjeto, Vector3.right).normalized;
        }
        
        // Posición de la cámara: alejada del punto medio, elevada
        Vector3 posicionDeseada = puntoMedio - direccionPerpendicular * distanciaAlejamiento;
        posicionDeseada.y = Mathf.Max(targetPlayer.position.y, objetoEnfoque.position.y) + alturaAdicional;
        
        // Suavizar movimiento
        transform.position = Vector3.Lerp(transform.position, posicionDeseada, followSmoothness * Time.deltaTime);
        
        // Calcular dirección de mirada: mezcla entre mirar al objeto y al player
        Vector3 direccionObjeto = (objetoEnfoque.position - transform.position).normalized;
        Vector3 direccionPlayer = (targetPlayer.position - transform.position).normalized;
        Vector3 direccionFinal = Vector3.Slerp(direccionPlayer, direccionObjeto, pesoEnfoqueObjeto);
        
        // Rotar la cámara
        if (direccionFinal != Vector3.zero)
        {
            Quaternion rotacionDeseada = Quaternion.LookRotation(direccionFinal);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotacionDeseada, followSmoothness * Time.deltaTime);
        }
    }
    
    /// <summary>
    /// Cambia la cámara para enfocar un objeto específico (se aleja y enfoca el objeto)
    /// </summary>
    public void CambiarAVistaGeneral()
    {
        if (!enfocandoObjeto && objetoEnfoque != null)
        {
            enfocandoObjeto = true;
            StartCoroutine(TransicionarAEnfoqueObjeto());
        }
        else if (objetoEnfoque == null)
        {
            Debug.LogWarning("⚠️ No hay objeto de enfoque asignado. Asigna un objeto en el campo 'Objeto Enfoque'.");
        }
    }
    
    /// <summary>
    /// Vuelve la cámara a seguir al jugador normalmente
    /// </summary>
    public void VolverASeguirJugador()
    {
        if (enfocandoObjeto)
        {
            enfocandoObjeto = false;
            StartCoroutine(TransicionarASeguimiento());
        }
    }
    
    IEnumerator TransicionarAEnfoqueObjeto()
    {
        if (objetoEnfoque == null || targetPlayer == null)
        {
            Debug.LogWarning("⚠️ No se puede enfocar: falta objeto de enfoque o player.");
            yield break;
        }
        
        // Si la cámara está dentro del player, desvincularla primero
        if (padreOriginal != null)
        {
            // Guardar la posición global antes de desvincular
            Vector3 posicionGlobalAntes = transform.position;
            Quaternion rotacionGlobalAntes = transform.rotation;
            
            // Desvincular la cámara del player para que no lo siga
            transform.SetParent(null);
            
            // Mantener la posición global después de desvincular
            transform.position = posicionGlobalAntes;
            transform.rotation = rotacionGlobalAntes;
            
            Debug.Log("🔓 Cámara desvinculada del player");
        }
        
        Vector3 posicionInicialTransicion = transform.position;
        Quaternion rotacionInicialTransicion = transform.rotation;
        
        // Calcular posición objetivo para enfocar el objeto
        Vector3 puntoMedio = (targetPlayer.position + objetoEnfoque.position) * 0.5f;
        Vector3 direccionPlayerObjeto = (objetoEnfoque.position - targetPlayer.position).normalized;
        Vector3 direccionPerpendicular = Vector3.Cross(direccionPlayerObjeto, Vector3.up).normalized;
        if (direccionPerpendicular == Vector3.zero)
        {
            direccionPerpendicular = Vector3.Cross(direccionPlayerObjeto, Vector3.right).normalized;
        }
        
        Vector3 posicionObjetivo = puntoMedio - direccionPerpendicular * distanciaAlejamiento;
        posicionObjetivo.y = Mathf.Max(targetPlayer.position.y, objetoEnfoque.position.y) + alturaAdicional;
        
        // Calcular rotación objetivo (mirar hacia el objeto con peso)
        Vector3 direccionObjeto = (objetoEnfoque.position - posicionObjetivo).normalized;
        Vector3 direccionPlayer = (targetPlayer.position - posicionObjetivo).normalized;
        Vector3 direccionFinal = Vector3.Slerp(direccionPlayer, direccionObjeto, pesoEnfoqueObjeto);
        Quaternion rotacionObjetivo = direccionFinal != Vector3.zero 
            ? Quaternion.LookRotation(direccionFinal) 
            : rotacionInicialTransicion;
        
        float tiempo = 0f;
        
        while (tiempo < 1f)
        {
            tiempo += Time.deltaTime * transitionSpeed;
            float t = Mathf.SmoothStep(0f, 1f, tiempo);
            
            // Actualizar posición objetivo en caso de que el objeto se mueva
            puntoMedio = (targetPlayer.position + objetoEnfoque.position) * 0.5f;
            direccionPlayerObjeto = (objetoEnfoque.position - targetPlayer.position).normalized;
            direccionPerpendicular = Vector3.Cross(direccionPlayerObjeto, Vector3.up).normalized;
            if (direccionPerpendicular == Vector3.zero)
            {
                direccionPerpendicular = Vector3.Cross(direccionPlayerObjeto, Vector3.right).normalized;
            }
            
            Vector3 posicionObjetivoActual = puntoMedio - direccionPerpendicular * distanciaAlejamiento;
            posicionObjetivoActual.y = Mathf.Max(targetPlayer.position.y, objetoEnfoque.position.y) + alturaAdicional;
            
            direccionObjeto = (objetoEnfoque.position - posicionObjetivoActual).normalized;
            direccionPlayer = (targetPlayer.position - posicionObjetivoActual).normalized;
            direccionFinal = Vector3.Slerp(direccionPlayer, direccionObjeto, pesoEnfoqueObjeto);
            rotacionObjetivo = direccionFinal != Vector3.zero 
                ? Quaternion.LookRotation(direccionFinal) 
                : rotacionInicialTransicion;
            
            transform.position = Vector3.Lerp(posicionInicialTransicion, posicionObjetivoActual, t);
            transform.rotation = Quaternion.Slerp(rotacionInicialTransicion, rotacionObjetivo, t);
            
            yield return null;
        }
        
        Debug.Log($"✅ Cámara enfocando objeto: {objetoEnfoque.name}");
    }
    
    IEnumerator TransicionarASeguimiento()
    {
        Vector3 posicionInicialTransicion = transform.position;
        Quaternion rotacionInicialTransicion = transform.rotation;
        
        // Calcular la posición objetivo
        Vector3 posicionObjetivo = posicionInicialTransicion;
        Quaternion rotacionObjetivo = rotacionInicialTransicion;
        
        if (targetPlayer != null)
        {
            // Si la cámara estaba dentro del player, volver a vincularla
            if (padreOriginal != null)
            {
                // Calcular la posición local objetivo
                Vector3 posicionLocalObjetivo = posicionLocalOriginal;
                
                float tiempo = 0f;
                
                while (tiempo < 1f)
                {
                    tiempo += Time.deltaTime * transitionSpeed;
                    float t = Mathf.SmoothStep(0f, 1f, tiempo);
                    
                    // Interpolar hacia la posición local objetivo
                    if (transform.parent == null)
                    {
                        // Si aún no está vinculada, calcular posición global
                        Vector3 posicionGlobalObjetivo = targetPlayer.TransformPoint(posicionLocalObjetivo);
                        transform.position = Vector3.Lerp(posicionInicialTransicion, posicionGlobalObjetivo, t);
                        
                        // Cuando estemos cerca, vincular
                        if (t > 0.5f && transform.parent == null)
                        {
                            transform.SetParent(padreOriginal);
                            transform.localPosition = Vector3.Lerp(
                                transform.parent.InverseTransformPoint(posicionInicialTransicion),
                                posicionLocalObjetivo,
                                (t - 0.5f) * 2f
                            );
                        }
                    }
                    else
                    {
                        // Ya está vinculada, usar posición local
                        transform.localPosition = Vector3.Lerp(
                            transform.localPosition,
                            posicionLocalObjetivo,
                            t
                        );
                    }
                    
                    yield return null;
                }
                
                // Asegurar que está vinculada y en la posición correcta
                if (transform.parent == null && padreOriginal != null)
                {
                    transform.SetParent(padreOriginal);
                }
                transform.localPosition = posicionLocalObjetivo;
            }
            else
            {
                // La cámara no estaba dentro del player, usar seguimiento normal
                Vector3 direccion = -targetPlayer.forward;
                posicionObjetivo = targetPlayer.position + direccion * followDistance;
                posicionObjetivo.y = targetPlayer.position.y + followHeight;
                
                Vector3 direccionMirar = targetPlayer.position - posicionObjetivo;
                rotacionObjetivo = direccionMirar != Vector3.zero 
                    ? Quaternion.LookRotation(direccionMirar) 
                    : rotacionInicialTransicion;
                
                float tiempo = 0f;
                
                while (tiempo < 1f)
                {
                    tiempo += Time.deltaTime * transitionSpeed;
                    float t = Mathf.SmoothStep(0f, 1f, tiempo);
                    
                    transform.position = Vector3.Lerp(posicionInicialTransicion, posicionObjetivo, t);
                    transform.rotation = Quaternion.Slerp(rotacionInicialTransicion, rotacionObjetivo, t);
                    
                    yield return null;
                }
            }
        }
        
        Debug.Log("✅ Cámara volvió a seguir al jugador");
    }
    
    /// <summary>
    /// Obtiene si la cámara está enfocando un objeto
    /// </summary>
    public bool EstaEnVistaGeneral()
    {
        return enfocandoObjeto;
    }
    
    /// <summary>
    /// Establece el objeto al que la cámara enfocará
    /// </summary>
    public void EstablecerObjetoEnfoque(Transform nuevoObjeto)
    {
        objetoEnfoque = nuevoObjeto;
        Debug.Log($"🎯 Objeto de enfoque establecido: {(nuevoObjeto != null ? nuevoObjeto.name : "null")}");
    }
}
