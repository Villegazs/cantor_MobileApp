using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using TMPro;

// This script must be attached to an object in the scene and configured
// with all the necessary UI references.
public class DialogueManager : MonoBehaviour
{
    // References to the containers of the different templates (Panels/GameObjects)
    [Header("Template Containers")]
    [Tooltip("Panel for Narrator dialogue type (simple text).")]
    public GameObject narratorContainer;
    [Tooltip("Panel for dialogues with character, name, and portrait.")]
    public GameObject characterContainer;
    [Tooltip("Panel for game/goal messages (e.g., Level 1).")]
    public GameObject goalContainer;

    // UI References within the Character Template
    [Header("Character Template Elements")]
    public TextMeshProUGUI characterText;
    public TextMeshProUGUI characterName;
    public Image characterPortrait;
    public RectTransform portraitLeftAnchor; // Left position of the portrait
    public RectTransform portraitRightAnchor; // Right position of the portrait

    // UI References within the Narrator Template
    [Header("Narrator Template Elements")]
    public TextMeshProUGUI narratorText;

    // UI References within the Goal Template (Assuming it has a title/description)
    [Header("Goal Template Elements")]
    public TextMeshProUGUI characterTextDescription;
    public TextMeshProUGUI goalDescriptionText;
    public Image goalPortraitImage;

    [Header("Flow Configuration")]
    [Tooltip("Speed of the typing effect (letters per second).")]
    public float textSpeed = 50f;
    private DialogueData currentDialogue;
    private int lineIndex;
    private bool isTyping = false;

    [Header("Events")]
    public UnityEvent OnDialogueEnd;

    // Static property for easy access to the Manager from any script (Singleton)
    public static DialogueManager Instance { get; private set; }

    void Awake()
    {
        // Implementation of the Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Hide all containers at the start
    void Start()
    {
        DeactivateAllContainers();
    }

    // Main function to start the conversation
    public void StartDialogue(DialogueData dialogue)
    {
        if (dialogue == null)
        {
            Debug.LogError("Provided DialogueData is null.");
            return;
        }

        currentDialogue = dialogue;
        lineIndex = 0;
        ShowNextLine();
    }

    // Called by a "Continue" button or screen tap.
    public void OnContinueClicked()
    {
        if (isTyping)
        {
            // If currently typing, skip the effect and show the full text
            StopAllCoroutines();
            isTyping = false;
            // The text was already assigned in ShowNextLine(), we just complete it
            if (currentDialogue.lines[lineIndex].template == TemplateType.NarratorBox)
            {
                narratorText.text = currentDialogue.lines[lineIndex].text;
            }
            else if (currentDialogue.lines[lineIndex].template == TemplateType.CharacterDialogue)
            {
                characterText.text = currentDialogue.lines[lineIndex].text;
            }
            // No typing effect for GoalMessage, so no action needed here
            return;
        }

        // If the text was already fully displayed, advance to the next line
        lineIndex++;
        if (lineIndex < currentDialogue.lines.Count)
        {
            ShowNextLine();
        }
        else
        {
            EndDialogue();
        }
    }

    private void ShowNextLine()
    {
        DialogueLine line = currentDialogue.lines[lineIndex];

        // 1. Deactivate all containers first.
        DeactivateAllContainers();

        // 2. Activate and configure the appropriate container.
        switch (line.template)
        {
            case TemplateType.NarratorBox:
                narratorContainer.SetActive(true);
                StartCoroutine(TypeText(narratorText, line.text));
                break;

            case TemplateType.CharacterDialogue:
                characterContainer.SetActive(true);

                // Configure name and portrait
                characterName.text = line.characterName;
                if (line.portrait != null)
                {
                    characterPortrait.gameObject.SetActive(true);
                    characterPortrait.sprite = line.portrait;
                }
                else
                {
                    characterPortrait.gameObject.SetActive(false);
                }

                // Configure portrait position (layout scalability)
                if (line.position == CharacterPosition.Right)
                {
                    // Assuming you toggle anchor points or parent objects
                    portraitLeftAnchor.gameObject.SetActive(false);
                    portraitRightAnchor.gameObject.SetActive(true);
                }
                else // Left by default
                {
                    portraitLeftAnchor.gameObject.SetActive(true);
                    portraitRightAnchor.gameObject.SetActive(false);
                }

                // Start the typing effect
                StartCoroutine(TypeText(characterText, line.text));
                break;

            case TemplateType.GameGoalMessage:
                goalContainer.SetActive(true);
                // NOTE: For the goal message, you would typically use specific
                // fields in DialogueLine (e.g., goalTitle, goalDescription) 
                // and assign them here. We omit the typing effect for this template.
                // goalTitleText.text = line.goalTitle; 
                // goalDescriptionText.text = line.goalDescription; 

                // For the purpose of this example, we'll just use the main text field
                // as the description.
                characterTextDescription.text = line.characterName;
                goalDescriptionText.text = line.text;
                goalPortraitImage.sprite = line.portrait;
                
                break;
        }
    }

    // Coroutine for the typing effect
    IEnumerator TypeText(TextMeshProUGUI textField, string fullText)
    {
        isTyping = true;
        textField.text = ""; // Clear the text
        int visibleCharacters = 0;
        float timeBetweenCharacters = 1f / textSpeed;

        while (visibleCharacters < fullText.Length)
        {
            textField.text += fullText[visibleCharacters];
            visibleCharacters++;
            // You can add voice/typing sound playback here
            yield return new WaitForSeconds(timeBetweenCharacters);
        }

        isTyping = false;
    }

    private void EndDialogue()
    {
        DeactivateAllContainers();
        currentDialogue = null;
        Debug.Log("Dialogue finished. Resuming game action.");
        // Here you can fire events or enable gameplay
    }

    private void DeactivateAllContainers()
    {
        narratorContainer.SetActive(false);
        characterContainer.SetActive(false);
        goalContainer.SetActive(false);
        // Ensure any active coroutines are stopped when changing lines
        StopAllCoroutines();
        isTyping = false;
    }
}