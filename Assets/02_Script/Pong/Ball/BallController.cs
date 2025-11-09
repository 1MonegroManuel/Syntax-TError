using UnityEngine;

public class BallController : MonoBehaviour
{
    public float speed = 6f;
    private Rigidbody2D rb;
    private SpriteRenderer sr;


    
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip sonidoColision;
    
    public AudioSource audioSourceGoal;
    public AudioClip sonidoGoal;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>(); // 🔹 Para cambiar color
        LaunchBall();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        if (audioSourceGoal == null)
        {
            audioSourceGoal = gameObject.AddComponent<AudioSource>();
        }
    }

    void LaunchBall()
    {
        float x = Random.Range(0, 2) == 0 ? -1 : 1;
        float y = Random.Range(-0.5f, 0.5f);

        Vector2 direction = new Vector2(x, y).normalized;
        rb.velocity = direction * speed;
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        // Aumenta velocidad progresivamente en cada rebote
        rb.velocity = rb.velocity.normalized * (speed += 0.1f);

        // 🔹 Cambiar color según paleta
        if (col.gameObject.CompareTag("LeftPaddle"))
        {
            sr.color = Color.blue; // ejemplo: azul para la izquierda
        }
        else if (col.gameObject.CompareTag("RightPaddle"))
        {
            sr.color = Color.red; // ejemplo: rojo para la derecha
        }
        if (audioSource != null)
        {
            audioSource.PlayOneShot(sonidoColision);
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("GoalLeft"))
        {
            ScoreManager.Instance.AddPoint(false);
            
            if (audioSourceGoal != null)
            {
                audioSourceGoal.PlayOneShot(sonidoGoal);
            }
            ResetBall();
        }
        else if (col.CompareTag("GoalRight"))
        {
            ScoreManager.Instance.AddPoint(true);

            if (audioSourceGoal != null)
            {
                audioSourceGoal.PlayOneShot(sonidoGoal);
            }
            ResetBall();
        }
    }

    void ResetBall()
    {
        transform.position = Vector2.zero;
        rb.velocity = Vector2.zero;
        speed = 6f;

        sr.color = Color.white; // 🔹 La bola vuelve a blanco al reiniciar
        Invoke("LaunchBall", 1f);
    }
}
