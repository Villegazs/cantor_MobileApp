using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Orquesta el flujo completo del juego: diálogos -> minijuego -> diálogo de resultado
/// </summary>
public class GameFlowManager : MonoBehaviour
{
    [Header("Level Configuration")]
    public LevelData[] allLevels; // Arrastra aquí los 3 niveles en el Inspector
    private int currentLevelIndex = 0;
    private LevelData currentLevel;

    [Header("Managers References")]
    public DialogueManager dialogueManager;
    public Match3Manager match3Manager;
    public TriviaManager triviaManager;

    [Header("UI References")]
    public GameObject levelSelectorUI;
    public Image backgroundImage;
    public Image goalImageUI;
    public Image completeGoalImageUI;
    public TextMeshProUGUI levelTitleText;

    public GameObject[] levelButtons; // Arrastrar los botones de nivel aquí
    private const string PROGRESS_KEY = "GameProgress";

    private Sprite mainMenuBackground;
    
    public DialogueData dialogoFinal;

    // Singleton
    public static GameFlowManager Instance { get; private set; }

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
        mainMenuBackground = backgroundImage.sprite;
        // Verificar referencias
        if (dialogueManager == null) dialogueManager = DialogueManager.Instance;
        if (match3Manager == null) match3Manager = Match3Manager.Instance;
        if (triviaManager == null) triviaManager = TriviaManager.Instance;

        // Configurar eventos de los managers de minijuegos
        if (match3Manager != null)
        {
            match3Manager.OnGameWin.AddListener(OnMinigameWin);
            match3Manager.OnGameLose.AddListener(OnMinigameLose);
        }

        if (triviaManager != null)
        {
            triviaManager.OnGameWin.AddListener(OnMinigameWin);
            triviaManager.OnGameLose.AddListener(OnMinigameLose);
        }

        // Cargar y aplicar el progreso guardado
        LoadAndApplyProgress();
    }

    private void LoadAndApplyProgress()
    {
        int unlockedLevels = PlayerPrefs.GetInt(PROGRESS_KEY, 1); // Por defecto, nivel 1 desbloqueado

        // Aplicar estados a los botones
        for (int i = 0; i < levelButtons.Length; i++)
        {
            if (levelButtons[i] != null)
            {
                bool isUnlocked = i < unlockedLevels;
                levelButtons[i].GetComponent<UnityEngine.UI.Button>().interactable = isUnlocked;
                if(levelButtons[i].GetComponent<CanvasGroup>()) levelButtons[i].GetComponent<CanvasGroup>().alpha = isUnlocked ? 1f : 0.5f;


                // Si tienes un componente visual para mostrar el estado de bloqueo
                if (levelButtons[i].transform.Find("LockIcon") is Transform lockIcon)
                {
                    lockIcon.gameObject.SetActive(!isUnlocked);
                }
            }
        }
    }

    /// <summary>
    /// Inicia un nivel específico
    /// </summary>
    public void StartLevel(int levelIndex)
    {
        if (levelIndex < 0 || levelIndex >= allLevels.Length)
        {
            Debug.LogError($"Invalid level index: {levelIndex}");
            return;
        }

        currentLevelIndex = levelIndex;
        currentLevel = allLevels[levelIndex];

        Debug.Log($"Starting Level {currentLevel.levelNumber}: {currentLevel.levelName}");
        levelSelectorUI.SetActive(false);
        goalImageUI.sprite = currentLevel.goalImage;
        completeGoalImageUI.sprite = currentLevel.goalImage;
        levelTitleText.text = $"Nivel {currentLevel.levelNumber}";

        // Mostrar diálogo previo al minijuego
        if (currentLevel.preGameDialogue != null)
        {
            dialogueManager.StartDialogue(currentLevel.preGameDialogue);
            backgroundImage.sprite = currentLevel.levelDialogueImage;
        }
        else
        {
            // Si no hay diálogo, ir directo al minijuego
            StartCurrentMinigame();
            
        }
    }

    /// <summary>
    /// Llamar este método al finalizar el diálogo previo (desde un botón "Continuar" o evento)
    /// </summary>
    public void OnPreGameDialogueFinished()
    {
        StartCurrentMinigame();
    }

    public void OnMainMenuButtonClicked()
    {
        levelSelectorUI.SetActive(true);
        backgroundImage.sprite = mainMenuBackground;
    }
    private void StartCurrentMinigame()
    {
        if (currentLevel == null)
        {
            Debug.LogError("No current level set!");
            return;
        }

        backgroundImage.sprite = currentLevel.levelImage;
        
        switch (currentLevel.minigameType)
        {
            case MinigameType.Match3:
                if (match3Manager != null)
                {
                    match3Manager.StartMatch3Game(currentLevel);
                }
                else
                {
                    Debug.LogError("Match3Manager not found!");
                }
                break;

            case MinigameType.Trivia:
                if (triviaManager != null)
                {
                    triviaManager.StartTriviaGame(currentLevel);
                }
                else
                {
                    Debug.LogError("TriviaManager not found!");
                }
                break;

            default:
                Debug.LogError($"Unknown minigame type: {currentLevel.minigameType}");
                break;
        }
    }

    private void UnlockNextLevel()
    {
        int currentProgress = PlayerPrefs.GetInt(PROGRESS_KEY, 1);
        if (currentLevelIndex + 1 >= currentProgress)
        {
            PlayerPrefs.SetInt(PROGRESS_KEY, currentLevelIndex + 2); // +2 porque los índices empiezan en 0
            PlayerPrefs.Save();
            LoadAndApplyProgress(); // Actualizar la UI
        }
    }

    private void OnMinigameWin()
    {
        UnlockNextLevel();

        // Mostrar diálogo de victoria si existe
        if (currentLevel.postGameWinDialogue != null)
        {
            dialogueManager.StartDialogue(currentLevel.postGameWinDialogue);
        }

        // Avanzar al siguiente nivel después de un delay o al terminar el diálogo
        
        //Invoke(nameof(LoadNextLevel), 2f);
    }

    private void OnMinigameLose()
    {
        Debug.Log("Minigame Lost!");

        // Mostrar diálogo de derrota si existe
        if (currentLevel.postGameLoseDialogue != null)
        {
            dialogueManager.StartDialogue(currentLevel.postGameLoseDialogue);
        }

        // Opción: reintentar el nivel o volver al menú
        // Por ahora, permitimos reintentar después de un delay
    }

    private void LoadNextLevel()
    {
        int nextLevelIndex = currentLevelIndex + 1;

        if (nextLevelIndex < allLevels.Length)
        {
            StartLevel(nextLevelIndex);
        }
        else
        {
            Debug.Log("¡Todos los niveles completados!");
            levelSelectorUI.SetActive(false);
            dialogueManager.StartDialogue(dialogoFinal);
            // Aquí podrías mostrar una pantalla de victoria final o créditos
        }
    }

    /// <summary>
    /// Permite reintentar el nivel actual (llamar desde UI de Game Over)
    /// </summary>
    public void RetryCurrentLevel()
    {
        StartLevel(currentLevelIndex);
    }

    /// <summary>
    /// Salta al siguiente nivel (para testing)
    /// </summary>
    public void SkipToNextLevel()
    {
        LoadNextLevel();
    }

    public void ResetProgress()
    {
        PlayerPrefs.SetInt(PROGRESS_KEY, 1);
        PlayerPrefs.Save();
        LoadAndApplyProgress();
    }
}
