using UnityEngine;

public class PaddleController : MonoBehaviour
{
    public float speed = 8f;         // Velocidad de movimiento
    public bool isLeftPaddle;        // true = izquierda (W/S), false = derecha (Up/Down)

    void Update()
    {
        float move = 0f;

        if (isLeftPaddle)
        {
            if (Input.GetKey(KeyCode.W)) move = 1f;
            if (Input.GetKey(KeyCode.S)) move = -1f;
        }
        else
        {
            if (Input.GetKey(KeyCode.UpArrow)) move = 1f;
            if (Input.GetKey(KeyCode.DownArrow)) move = -1f;
        }

        transform.Translate(Vector2.up * move * speed * Time.deltaTime);

        // Limitar movimiento dentro de la cámara
        float yClamp = Mathf.Clamp(transform.position.y, -4f, 4f);
        transform.position = new Vector2(transform.position.x, yClamp);
    }
}
