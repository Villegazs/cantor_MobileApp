using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

public class TriviaManager : MonoBehaviour
{
    [Header("Level Configuration")]
    public LevelData currentLevel;

    [Header("UI References")]
    public GameObject triviaUI;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI questionText;
    public Button[] answerButtons; // Array de botones para las respuestas
    public TextMeshProUGUI[] answerTexts; // Textos de los botones
    public Button continueButton;

    [Header("Feedback UI")]
    public GameObject correctFeedback;
    public GameObject incorrectFeedback;

    [Header("Game State")]
    private int currentQuestionIndex = 0;
    private bool isGameActive = false;

    [Header("Events")]
    public UnityEvent OnGameWin;
    public UnityEvent OnGameLose;

    // Singleton para fácil acceso
    public static TriviaManager Instance { get; private set; }

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
        triviaUI.SetActive(false);
        if (correctFeedback != null) correctFeedback.SetActive(false);
        if (incorrectFeedback != null) incorrectFeedback.SetActive(false);
        if (continueButton != null) continueButton.gameObject.SetActive(false);

        // Configurar listeners de los botones
        for (int i = 0; i < answerButtons.Length; i++)
        {
            int index = i; // Captura el índice para el closure
            answerButtons[i].onClick.AddListener(() => OnAnswerSelected(index));
        }
    }

    // Inicializa el minijuego de trivia con el nivel actual
    public void StartTriviaGame(LevelData level)
    {
        if (level == null || level.minigameType != MinigameType.Trivia)
        {
            Debug.LogError("Invalid level data for Trivia!");
            return;
        }

        if (level.triviaQuestions == null || level.triviaQuestions.Length == 0)
        {
            Debug.LogError("No trivia questions configured for this level!");
            return;
        }

        currentLevel = level;
        currentQuestionIndex = 0;
        isGameActive = true;

        triviaUI.SetActive(true);
        if (continueButton != null) continueButton.gameObject.SetActive(false);
        ShowQuestion(currentQuestionIndex);

        Debug.Log($"Trivia Game Started: {level.levelName}");
    }

    private void ShowQuestion(int questionIndex)
    {
        if (questionIndex >= currentLevel.triviaQuestions.Length)
        {
            Debug.LogError("Question index out of range!");
            return;
        }

        TriviaQuestion question = currentLevel.triviaQuestions[questionIndex];

        levelText.text = $"Pregunta de trivia - Etapa {currentLevel.levelNumber}";
        // Mostrar la pregunta
        questionText.text = question.question;

        // Configurar las respuestas
        for (int i = 0; i < answerButtons.Length; i++)
        {
            if (i < question.answers.Length)
            {
                answerButtons[i].gameObject.SetActive(true);
                answerTexts[i].text = question.answers[i];
                answerButtons[i].interactable = true;
                answerButtons[i].image.color = Color.white;
            }
            else
            {
                answerButtons[i].gameObject.SetActive(false);
            }
        }

        // Ocultar feedback
        if (correctFeedback != null) correctFeedback.SetActive(false);
        if (incorrectFeedback != null) incorrectFeedback.SetActive(false);
    }

    private void OnAnswerSelected(int answerIndex)
    {
        if (!isGameActive) return;

        TriviaQuestion currentQuestion = currentLevel.triviaQuestions[currentQuestionIndex];

        // Deshabilitar botones para evitar múltiples clics
        foreach (Button btn in answerButtons)
        {
            btn.interactable = false;
        }

        // Verificar si la respuesta es correcta
        if (answerIndex == currentQuestion.correctAnswerIndex)
        {
            // Respuesta correcta
            Debug.Log("¡Respuesta correcta!");
            
            answerButtons[answerIndex].image.color = Color.green;
            
            if (correctFeedback != null)
            {
                correctFeedback.SetActive(true);
            }

            // Esperar un momento y pasar a la siguiente pregunta o finalizar
            Invoke(nameof(NextQuestion), 1.5f);
        }
        else
        {
            // Respuesta incorrecta
            Debug.Log("Respuesta incorrecta.");
            answerButtons[answerIndex].image.color = Color.red;
            answerButtons[currentQuestion.correctAnswerIndex].image.color = Color.green;
            if (incorrectFeedback != null)
            {
                incorrectFeedback.SetActive(true);
            }

            // Esperar un momento y terminar el juego
            Invoke(nameof(GameLost), 1.5f);
        }
    }

    private void NextQuestion()
    {
        currentQuestionIndex++;

        // Verificar si hay más preguntas
        if (currentQuestionIndex < currentLevel.triviaQuestions.Length)
        {
            ShowQuestion(currentQuestionIndex);
        }
        else
        {
            // Todas las preguntas respondidas correctamente
            GameWon();
        }
    }

    private void GameWon()
    {
        isGameActive = false;
        if (continueButton != null) continueButton.gameObject.SetActive(true);
        Debug.Log("¡Trivia completada exitosamente!");
        OnGameWin?.Invoke();
    }

    private void GameLost()
    {
        isGameActive = false;
        if (continueButton != null) continueButton.gameObject.SetActive(true);
        Debug.Log("Trivia fallida. Respuesta incorrecta.");
        OnGameLose?.Invoke();
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
            StartTriviaGame(currentLevel);
        }
    }
}
