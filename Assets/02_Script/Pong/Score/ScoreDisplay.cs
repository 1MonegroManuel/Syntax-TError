using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ScoreDisplay : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text scoreText;
    
    [Header("Botones (Opcional)")]
    public Button refreshButton;
    public Button clearButton;
    
    [Header("Configuración")]
    public int maxScoresToShow = 10; // Máximo de puntuaciones a mostrar
    public string noScoresMessage = "Sin Registros";
    
    void Start()
    {
        // Si no se ha asignado el texto, intentar obtenerlo del mismo GameObject
        if (scoreText == null)
        {
            scoreText = GetComponent<TMP_Text>();
        }
        
        // Configurar botones automáticamente
        SetupButtons();
        
        LoadAndDisplayScores();
    }
    
    void SetupButtons()
    {
        // Configurar botón de refresh
        if (refreshButton != null)
        {
            refreshButton.onClick.AddListener(RefreshScores);
        }
        
        // Configurar botón de clear
        if (clearButton != null)
        {
            clearButton.onClick.AddListener(ClearAllScores);
        }
    }
    
    public void LoadAndDisplayScores()
    {
        if (scoreText == null)
        {
            Debug.LogError("ScoreDisplay: No se ha asignado el componente TMP_Text");
            return;
        }
        
        // Cargar las puntuaciones desde el archivo
        string[] scores = ScoreSaver.LoadScores();
        
        if (scores.Length == 0)
        {
            scoreText.text = noScoresMessage;
            return;
        }
        
        // Mostrar las últimas puntuaciones (máximo maxScoresToShow)
        int startIndex = Mathf.Max(0, scores.Length - maxScoresToShow);
        string displayText = "";
        
        for (int i = startIndex; i < scores.Length; i++)
        {
            displayText += scores[i].Trim();
            if (i < scores.Length - 1)
            {
                displayText += "\n";
            }
        }
        
        scoreText.text = displayText;
        Debug.Log($"ScoreDisplay: Mostrando {scores.Length} puntuaciones");
    }
    
    // Método público para refrescar las puntuaciones (útil para botones)
    public void RefreshScores()
    {
        LoadAndDisplayScores();
    }
    
    // Método para limpiar las puntuaciones (útil para botones)
    public void ClearAllScores()
    {
        ScoreSaver.ClearScores();
        LoadAndDisplayScores();
    }
}
