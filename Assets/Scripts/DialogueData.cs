using UnityEngine;
using System.Collections.Generic;

// 1. ScriptableObject to create dialogue assets in the Inspector.
// This decouples the data from the code.

[CreateAssetMenu(fileName = "NewDialogue", menuName = "Dialogue System/Complete Dialogue", order = 1)]
public class DialogueData : ScriptableObject
{
    // A list of all "Lines" or "Steps" that make up this conversation.
    public List<DialogueLine> lines = new List<DialogueLine>();
}

// 2. Structure for each individual line of the conversation.
// 'System.Serializable' is essential for Unity to display it in the Inspector.
[System.Serializable]
public class DialogueLine
{
    [Tooltip("The text to display.")]
    [TextArea(3, 10)] // Makes the text field larger in the Inspector
    public string text;

    [Tooltip("The name of the character speaking. Leave blank for narrator.")]
    public string characterName;

    [Tooltip("The character's portrait Sprite. Optional.")]
    public Sprite portrait;

    // --- Extensibility and Modularity ---

    [Header("Template Configuration (Layout)")]
    [Tooltip("Defines which UI template should be used for this line.")]
    public TemplateType template = TemplateType.NarratorBox;

    [Tooltip("Optional: Determines if the portrait should be on the Left or Right (only if the template uses it).")]
    public CharacterPosition position = CharacterPosition.Left;

    // You can add more fields here if needed:
    // public AudioClip voiceSound;
    // public string eventOnFinish; // To trigger a specific game function
}

// 3. Enum to define the types of dialogue templates observed in the Figma frames.
// This is key for scalability: if you add a new template (e.g., 'Decision'), just add it here.
public enum TemplateType
{
    NarratorBox,        // Simple text box at the bottom, no portrait (e.g., first intro screen)
    CharacterDialogue,  // Box with name and character portrait (e.g., Xiégua)
    GameGoalMessage     // Pop-up window for mission/level goals (e.g., "Level 1 - Goal")
}

public enum CharacterPosition
{
    Left,
    Right
}