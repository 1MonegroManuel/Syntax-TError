using UnityEngine;
using UnityEngine.Video;
using System;

public class VideoTrigger : MonoBehaviour
{
    [Header("Configuración del Video")]
    [Tooltip("Arrastra aquí el archivo MP4 desde Assets/09_Videos/PelotaEvent/Pelota_de_Béisbol_Cae_y_Rebota.mp4")]
    public VideoClip videoClip; // El video MP4 que se reproducirá
    public VideoPlayer videoPlayer; // Referencia al VideoPlayer
    
    [Header("Configuración de Renderizado")]
    [Tooltip("Cómo se mostrará el video")]
    public VideoRenderMode renderMode = VideoRenderMode.CameraNearPlane;
    public Camera targetCamera; // Cámara donde se mostrará el video
    
    [Header("Configuración del Trigger")]
    public string triggerTag = "BallGame"; // Tag del collider que activará el video
    public bool playOnce = true; // Si solo debe reproducirse una vez
    public bool pauseGameplay = true; // Si debe pausar el gameplay durante el video
    
    [Header("Referencias")]
    public GameObject videoCanvas; // Canvas donde se mostrará el video
    public GameObject player; // Referencia al jugador
    
    private bool hasPlayed = false; // Control para reproducir solo una vez
    private PlayerController playerController; // Referencia al controlador del jugador
    
    // Evento que se ejecuta cuando termina el video
    public static event Action OnVideoCompleted;
    
    void Start()
    {
        // Asignar automáticamente todas las referencias necesarias
        AssignReferencesAutomatically();
        
        // Configurar el VideoPlayer
        SetupVideoPlayer();
        
        // Ocultar el canvas del video inicialmente
        if (videoCanvas != null)
        {
            videoCanvas.SetActive(false);
        }
    }
    
    void AssignReferencesAutomatically()
    {
        Debug.Log("🔧 Asignando referencias automáticamente...");
        
        // Buscar automáticamente el VideoPlayer si no está asignado
        if (videoPlayer == null)
        {
            videoPlayer = GetComponent<VideoPlayer>();
            if (videoPlayer != null)
            {
                Debug.Log("✅ VideoPlayer encontrado automáticamente en el mismo GameObject.");
            }
            else
            {
                Debug.LogError("❌ No se encontró VideoPlayer. Agrega un VideoPlayer al GameObject.");
                return;
            }
        }
        
        // Buscar automáticamente el jugador si no está asignado
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                Debug.Log("✅ Jugador encontrado automáticamente por tag 'Player'.");
            }
            else
            {
                Debug.LogWarning("⚠️ No se encontró GameObject con tag 'Player'.");
            }
        }
        
        // Obtener el PlayerController
        if (player != null)
        {
            playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                Debug.Log("✅ PlayerController encontrado automáticamente.");
            }
            else
            {
                Debug.LogWarning("⚠️ No se encontró PlayerController en el jugador.");
            }
        }
        
        // Buscar automáticamente la cámara principal si no está asignada
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
            if (targetCamera != null)
            {
                Debug.Log("✅ Cámara principal encontrada automáticamente.");
            }
            else
            {
                Debug.LogWarning("⚠️ No se encontró cámara principal.");
            }
        }
        
        Debug.Log("🔧 Asignación automática completada.");
    }
    
    
    void SetupVideoPlayer()
    {
        if (videoPlayer == null) return;
        
        // Asignar el video clip
        if (videoClip != null)
        {
            videoPlayer.clip = videoClip;
        }
        else
        {
            Debug.LogWarning("⚠️ No se asignó un VideoClip. Arrastra el archivo MP4 'Pelota_de_Béisbol_Cae_y_Rebota.mp4' desde Assets/09_Videos/PelotaEvent/ al campo Video Clip.");
        }
        
        // Configurar el VideoPlayer para que NO se reproduzca automáticamente
        videoPlayer.playOnAwake = false;
        videoPlayer.isLooping = false;
        videoPlayer.renderMode = renderMode;
        
        // Asegurar que el video esté detenido al inicio
        videoPlayer.Stop();
        
        Debug.Log("✅ VideoPlayer configurado: NO se reproducirá automáticamente");
        
        // Configurar la cámara objetivo si es necesario
        if (renderMode == VideoRenderMode.CameraNearPlane || renderMode == VideoRenderMode.CameraFarPlane)
        {
            if (targetCamera == null)
            {
                targetCamera = Camera.main;
                if (targetCamera == null)
                {
                    Debug.LogWarning("⚠️ No se encontró cámara principal. Asigna una cámara al campo Target Camera.");
                }
            }
            videoPlayer.targetCamera = targetCamera;
        }
        
        // Suscribirse al evento de fin de video
        videoPlayer.loopPointReached += OnVideoFinished;
    }
    
    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"🔍 COLLIDER DETECTADO: {other.name} con tag '{other.tag}' entró al collider");
        Debug.Log($"🔍 Buscando tag: '{triggerTag}'");
        
        // Verificar si el objeto que entró tiene el tag correcto
        if (other.CompareTag(triggerTag))
        {
            Debug.Log($"🎯 ¡ÉXITO! Jugador entró al área {triggerTag}");
            
            // Si debe reproducirse solo una vez y ya se reprodujo, no hacer nada
            if (playOnce && hasPlayed)
            {
                Debug.Log("📹 Video ya se reprodujo anteriormente.");
                return;
            }
            
            // Reproducir el video
            PlayVideo();
        }
        else
        {
            Debug.Log($"❌ Tag incorrecto. Esperado: '{triggerTag}', Recibido: '{other.tag}'");
        }
    }
    
    void PlayVideo()
    {
        Debug.Log("🎬 TRIGGER ACTIVADO: Reproduciendo video por detección de collider...");
        
        if (videoPlayer == null)
        {
            Debug.LogError("❌ VideoPlayer es null. Agrega un componente VideoPlayer al GameObject.");
            return;
        }
        
        if (videoClip == null)
        {
            Debug.LogError("❌ VideoClip es null. Arrastra el archivo MP4 al campo Video Clip.");
            return;
        }
        
        // Verificar que el video no esté ya reproduciéndose
        if (videoPlayer.isPlaying)
        {
            Debug.LogWarning("⚠️ El video ya se está reproduciendo. Ignorando solicitud.");
            return;
        }
        
        Debug.Log($"🎬 Reproduciendo video: {videoClip.name}");
        Debug.Log($"📹 Render Mode: {videoPlayer.renderMode}");
        Debug.Log($"🎥 Target Camera: {(videoPlayer.targetCamera != null ? videoPlayer.targetCamera.name : "None")}");
        
        // Pausar el gameplay si está configurado
        if (pauseGameplay && playerController != null)
        {
            playerController.enabled = false;
            Debug.Log("⏸️ Gameplay pausado durante la reproducción del video.");
        }
        
        // Mostrar el canvas del video
        if (videoCanvas != null)
        {
            videoCanvas.SetActive(true);
        }
        
        // Reproducir el video
        videoPlayer.Play();
        
        // Marcar como reproducido
        hasPlayed = true;
    }
    
    void OnVideoFinished(VideoPlayer vp)
    {
        Debug.Log("🏁 Video terminado.");
        
        // Reanudar el gameplay
        if (pauseGameplay && playerController != null)
        {
            playerController.enabled = true;
            Debug.Log("▶️ Gameplay reanudado.");
        }
        
        // Ocultar el canvas del video
        if (videoCanvas != null)
        {
            videoCanvas.SetActive(false);
        }
        
        // Detener el video
        videoPlayer.Stop();
        
        // Disparar el evento para que otros scripts sepan que el video terminó
        OnVideoCompleted?.Invoke();
        Debug.Log("📢 Evento OnVideoCompleted disparado");
    }
    
    // Método público para reproducir el video manualmente
    public void PlayVideoManually()
    {
        Debug.Log("🔧 Reproduciendo video manualmente...");
        PlayVideo();
    }
    
    // Método para probar la configuración del video
    [ContextMenu("Probar Configuración del Video")]
    public void TestVideoConfiguration()
    {
        Debug.Log("🔍 === DIAGNÓSTICO DE CONFIGURACIÓN DEL VIDEO ===");
        Debug.Log($"VideoPlayer: {(videoPlayer != null ? "✅ Asignado" : "❌ No asignado")}");
        Debug.Log($"VideoClip: {(videoClip != null ? $"✅ Asignado ({videoClip.name})" : "❌ No asignado")}");
        Debug.Log($"Render Mode: {(videoPlayer != null ? videoPlayer.renderMode.ToString() : "N/A")}");
        Debug.Log($"Target Camera: {(videoPlayer != null && videoPlayer.targetCamera != null ? videoPlayer.targetCamera.name : "❌ No asignada")}");
        Debug.Log($"Player Controller: {(playerController != null ? "✅ Encontrado" : "❌ No encontrado")}");
        Debug.Log($"Trigger Tag: {triggerTag}");
        Debug.Log("🔍 === FIN DEL DIAGNÓSTICO ===");
    }
    
    // Método público para detener el video
    public void StopVideo()
    {
        if (videoPlayer != null)
        {
            videoPlayer.Stop();
            OnVideoFinished(videoPlayer);
        }
    }
    
    void OnDestroy()
    {
        // Limpiar eventos
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoFinished;
        }
    }
}
