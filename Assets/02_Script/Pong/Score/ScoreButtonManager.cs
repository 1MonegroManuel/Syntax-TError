using UnityEngine;
using UnityEngine.UI;

public class ScoreButtonManager : MonoBehaviour
{
    [Header("Botones")]
    public Button refreshButton;
    public Button clearButton;
    
    [Header("Score Display")]
    public ScoreDisplay scoreDisplay;
    
    void Start()
    {
        // Configurar botones automáticamente si no están asignados
        SetupButtons();
    }
    
    void SetupButtons()
    {
        // Si no se ha asignado el ScoreDisplay, intentar encontrarlo
        if (scoreDisplay == null)
        {
            scoreDisplay = FindObjectOfType<ScoreDisplay>();
        }
        
        // Configurar botón de refresh
        if (refreshButton != null)
        {
            refreshButton.onClick.AddListener(OnRefreshClicked);
        }
        
        // Configurar botón de clear
        if (clearButton != null)
        {
            clearButton.onClick.AddListener(OnClearClicked);
        }
    }
    
    public void OnRefreshClicked()
    {
        if (scoreDisplay != null)
        {
            scoreDisplay.RefreshScores();
            Debug.Log("Botón Refresh: Puntuaciones actualizadas");
        }
        else
        {
            Debug.LogError("ScoreButtonManager: No se encontró ScoreDisplay");
        }
    }
    
    public void OnClearClicked()
    {
        if (scoreDisplay != null)
        {
            scoreDisplay.ClearAllScores();
            Debug.Log("Botón Clear: Puntuaciones eliminadas");
        }
        else
        {
            Debug.LogError("ScoreButtonManager: No se encontró ScoreDisplay");
        }
    }
    
    // Métodos públicos para usar desde otros scripts
    public void RefreshScores()
    {
        OnRefreshClicked();
    }
    
    public void ClearScores()
    {
        OnClearClicked();
    }
}
