using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 7f;
    public Transform respawnPoint; // Asigna en el inspector

    private Rigidbody rb;
    private int jumpCount = 0;       // Controla el número de saltos
    private int maxJumps = 2;        // Permite doble salto (1 en suelo, 1 en aire)

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        Move();
        Jump();
    }

    void Move()
    {
        float moveX = Input.GetAxis("Horizontal"); // A/D o Flechas izquierda/derecha
        float moveZ = Input.GetAxis("Vertical");   // W/S o Flechas arriba/abajo

        // Invertido como tú lo tenías
        Vector3 move = new Vector3(moveX * -1, 0, moveZ * -1) * moveSpeed;
        Vector3 newVelocity = new Vector3(move.x, rb.velocity.y, move.z);

        rb.velocity = newVelocity;
    }

    void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && jumpCount < maxJumps)
        {
            rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z); // reinicia el salto vertical
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            jumpCount++;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // Si toca el suelo (primer salto se reinicia)
        if (collision.gameObject.CompareTag("Ground"))
        {
            jumpCount = 0;
        }

        // Si toca un objeto con tag "Respawn"
        if (collision.gameObject.CompareTag("Respawn"))
        {
            if (respawnPoint != null)
            {
                transform.position = respawnPoint.position;
                rb.velocity = Vector3.zero;
                Debug.Log("Jugador transportado al punto de respawn");
            }
        }
    }
}
