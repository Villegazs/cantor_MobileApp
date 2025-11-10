using UnityEngine.UI;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using Match3;

public class Match3Manager : MonoBehaviour
{
    [Header("Level Configuration")]
    public LevelData currentLevel;

    [Header("UI References")]
    public TextMeshProUGUI movesText;
    public Image objectiveImage;
    public GameObject gameUI;

    [Header("Game State")]
    private int remainingMoves;
    private bool isGameActive = false;
    private bool objectiveCompleted = false;

    [Header("Events")]
    public UnityEvent OnGameWin;
    public UnityEvent OnGameLose;
    
    // Singleton para fácil acceso
    public static Match3Manager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        gameUI.SetActive(false);
    }

    // Inicializa el minijuego con el nivel actual
    public void StartMatch3Game(LevelData level)
    {
        if (level == null || level.minigameType != MinigameType.Match3)
        {
            Debug.LogError("Invalid level data for Match3!");
            return;
        }

        currentLevel = level;
        remainingMoves = level.maxMoves;
        objectiveCompleted = false;
        isGameActive = true;

        gameUI.SetActive(true);
        UpdateUI();
        Debug.Log($"Match3 Game Started: {level.levelName}");
        Debug.Log($"Objective: Get {level.objectiveType} to bottom in {level.maxMoves} moves");
    }

    // Llamar este método cada vez que el jugador haga un movimiento válido
    public void OnPlayerMove()
    {
        if (!isGameActive) return;
        
        remainingMoves--;
        UpdateUI();
        CheckGameState();
        
    }

    // Llamar este método cuando el objetivo llegue al suelo
    public void OnObjectiveReachedBottom(ObjectiveType objectiveType)
    {
        if (!isGameActive) return;

        // Para el Match3, el objetivo siempre es llevar la gema especial al suelo
        if (objectiveType == ObjectiveType.Special)
        {
            objectiveCompleted = true;
            CheckGameState();
        }
    }

    private void CheckGameState()
    {
        if (objectiveCompleted)
        {
            // Victoria!
            EndGame(true);
        }
        else if (remainingMoves <= 0)
        {
            // Derrota - se acabaron los movimientos
            EndGame(false);
        }
    }

    private void EndGame(bool won)
    {
        isGameActive = false;
        gameUI.SetActive(false);

        if (won)
        {
            Debug.Log("¡Victoria! Objetivo completado.");
            OnGameWin?.Invoke();
        }
        else
        {
            Debug.Log("Derrota. No se completó el objetivo a tiempo.");
            OnGameLose?.Invoke();
        }
    }

    private void UpdateUI()
    {
        movesText.text = $"{remainingMoves}";
        objectiveImage.sprite = currentLevel.gemType.sprite;
    }

    private string GetObjectiveName(ObjectiveType type)
    {
        switch (type)
        {
            case ObjectiveType.Wire:
                return "Wire";
            case ObjectiveType.Corn:
                return "Corn";
            case ObjectiveType.Gold:
                return "Gold";
            default:
                return "Unknown";
        }
    }

    // Método público para verificar si el juego está activo
    public bool IsGameActive()
    {
        return isGameActive;
    }

    // Método para reintentar el nivel
    public void RetryLevel()
    {
        if (currentLevel != null)
        {
            StartMatch3Game(currentLevel);
        }
    }
}
