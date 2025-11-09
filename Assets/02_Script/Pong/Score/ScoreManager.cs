using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    [Header("UI")]
    public TMP_Text leftScoreText;
    public TMP_Text rightScoreText;

    [Header("Reglas")]
    public int maxScore = 3;

    private int leftScore = 0;
    private int rightScore = 0;
    private bool gameEnded = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) { Destroy(gameObject); return; }
        UpdateUI();
    }

    public void AddPoint(bool leftPlayer)
    {
        if (gameEnded) return;

        if (leftPlayer) leftScore++;
        else rightScore++;

        UpdateUI();
        CheckWin();
    }

    void UpdateUI()
    {
        if (leftScoreText) leftScoreText.text = leftScore.ToString();
        if (rightScoreText) rightScoreText.text = rightScore.ToString();
    }

    void CheckWin()
    {
        if (leftScore >= maxScore)
        {
            gameEnded = true;
            // Guardar la puntuación antes de cambiar de escena
            ScoreSaver.SaveScore(leftScore, rightScore, "Jugador Izquierda");
            SceneManager.LoadScene("GameOver2");
        }
        else if (rightScore >= maxScore)
        {
            gameEnded = true;
            // Guardar la puntuación antes de cambiar de escena
            ScoreSaver.SaveScore(leftScore, rightScore, "Jugador Derecha");
            SceneManager.LoadScene("GameOver");
        }
    }

    // Opcional: por si quieres reiniciar desde otra parte
    public void ResetScores()
    {
        leftScore = rightScore = 0;
        gameEnded = false;
        UpdateUI();
    }
}
