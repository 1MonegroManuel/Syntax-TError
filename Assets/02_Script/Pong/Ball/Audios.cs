using UnityEngine;

public class Pelota : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip sonidoColision;
    
    public AudioSource audioSourceGoal;
    public AudioClip sonidoGoal;
    
    void Start()
    {
        // Si no se ha asignado un AudioSource, intentar obtenerlo del mismo GameObject
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
        
        // Si aún no hay AudioSource, crear uno
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        // Verificar si la colisión es con una paleta
        if (collision.gameObject.CompareTag("LeftPaddle") || 
            collision.gameObject.CompareTag("RightPaddle"))
        {
            // Reproducir el sonido si está asignado
            if (sonidoColision != null && audioSource != null)
            {
                audioSource.PlayOneShot(sonidoColision);
            }
        }else if (collision.gameObject.CompareTag("GoalLeft") || collision.gameObject.CompareTag("GoalRight"))
        {
            if (sonidoGoal != null && audioSource != null)
            {
                audioSource.PlayOneShot(sonidoGoal);
            }
        }
    }
}
