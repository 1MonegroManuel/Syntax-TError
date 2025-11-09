using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    [SerializeField] private float delayTime = 3f; // Tiempo de espera en segundos

    // Start is called before the first frame update
    void Start()
    {
        // Inicia la corrutina para regresar al menú después del delay
        StartCoroutine(ReturnToMenuAfterDelay());
    }

    // Corrutina que espera 3 segundos y luego regresa al menú
    private IEnumerator ReturnToMenuAfterDelay()
    {
        // Espera el tiempo especificado
        yield return new WaitForSeconds(delayTime);
        
        // Regresa a la escena del menú principal
        ReturnToMenu();
    }

    // Método para regresar al menú principal
    public void ReturnToMenu()
    {
        SceneManager.LoadScene("PongMainMenu"); // Asume que la escena del menú se llama "PongMainMenu"
    }
}
