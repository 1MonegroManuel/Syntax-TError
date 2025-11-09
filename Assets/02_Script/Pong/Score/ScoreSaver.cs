using UnityEngine;
using System.IO;
using System;

public class ScoreSaver : MonoBehaviour
{
    private static string filePath;
    
    // Método para inicializar la ruta del archivo de forma segura
    private static void InitializeFilePath()
    {
        if (string.IsNullOrEmpty(filePath))
        {
            filePath = Path.Combine(Application.persistentDataPath, "scores.txt");
            Debug.Log($"Ruta del archivo inicializada: {filePath}");
        }
    }
    
    public static void SaveScore(int leftScore, int rightScore, string winner)
    {
        try
        {
            // Asegurar que la ruta esté inicializada
            InitializeFilePath();
            
            // Crear el contenido del archivo solo con la puntuación
            string scoreEntry = $"{leftScore}-{rightScore}\n";
            
            // Agregar la nueva puntuación al archivo (append)
            // File.AppendAllText crea el archivo automáticamente si no existe
            File.AppendAllText(filePath, scoreEntry);
            
            Debug.Log($"Puntuación guardada: {scoreEntry.Trim()}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error al guardar la puntuación: {e.Message}");
        }
    }
    
    public static string[] LoadScores()
    {
        try
        {
            // Asegurar que la ruta esté inicializada
            InitializeFilePath();
            
            if (File.Exists(filePath))
            {
                return File.ReadAllLines(filePath);
            }
            else
            {
                Debug.Log("No hay archivo de puntuaciones guardadas");
                return new string[0];
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error al cargar las puntuaciones: {e.Message}");
            return new string[0];
        }
    }
    
    public static void ClearScores()
    {
        try
        {
            // Asegurar que la ruta esté inicializada
            InitializeFilePath();
            
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                Debug.Log("Puntuaciones eliminadas");
            }
            else
            {
                Debug.Log("No hay archivo de puntuaciones para eliminar");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error al eliminar las puntuaciones: {e.Message}");
        }
    }
    
    // Método para obtener la ruta del archivo (útil para debugging)
    public static string GetFilePath()
    {
        return filePath;
    }
}
