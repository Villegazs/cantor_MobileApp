using Match3;
using UnityEngine;

// ScriptableObject para configurar cada nivel
[CreateAssetMenu(fileName = "NewLevel", menuName = "Game/Level Data", order = 2)]
public class LevelData : ScriptableObject
{
    [Header("Level Info")]
    public int levelNumber;
    public string levelName;
    public Sprite levelImage;
    public Sprite levelDialogueImage;
    public Sprite goalImage;

    [Header("Dialogue References")]
    [Tooltip("Diálogo antes del minijuego")]
    public DialogueData preGameDialogue;
    [Tooltip("Diálogo después de ganar")]
    public DialogueData winDialogue;
    [Tooltip("Diálogo después de perder")]
    public DialogueData loseDialogue;

    [Header("Minigame Configuration")]
    public MinigameType minigameType;

    // --- Match 3 Configuration ---
    [Header("Match 3 Settings (if applicable)")]
    public ObjectiveType objectiveType;
    public GemType gemType;
    public int maxMoves;

    // --- Trivia Configuration ---
    [Header("Trivia Settings (if applicable)")]
    public TriviaQuestion[] triviaQuestions;

    public DialogueData postGameWinDialogue;
    public DialogueData postGameLoseDialogue;
}

// Enum para tipos de minijuegos
public enum MinigameType
{
    Match3,
    Trivia
}

// Enum para tipos de objetivos en Match 3
public enum ObjectiveType
{
    Wire,   // Alambre - Nivel 1
    Corn,   // Maíz - Nivel 2
    Gold    // Oro - Nivel 3
    ,
    Special
}

// Estructura para preguntas de trivia
[System.Serializable]
public class TriviaQuestion
{
    [TextArea(2, 4)]
    public string question;
    public string[] answers;
    public int correctAnswerIndex;
}
