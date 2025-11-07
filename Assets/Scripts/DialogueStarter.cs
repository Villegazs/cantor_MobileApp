using UnityEngine;

// This script can be attached to a character or a scene controller.
public class DialogueStarter : MonoBehaviour
{
    [Tooltip("Drag the Dialogue ScriptableObject asset here from the Unity Inspector.")]
    public DialogueData initialDialogue;

    // Use Awake to ensure DialogueManager is initialized (Singleton)
    void Awake()
    {
        if (initialDialogue == null)
        {
            Debug.LogError("ERROR! initialDialogue is not assigned in the Unity Inspector.");
        }
    }


    public void StartConversation()
    {
        // Check if the Manager is available and if a dialogue is assigned.
        if (DialogueManager.Instance != null && initialDialogue != null)
        {
            DialogueManager.Instance.StartDialogue(initialDialogue);
        }
        else
        {
            Debug.LogWarning("Could not start conversation. Verify that DialogueManager and DialogueData are correctly configured.");
        }
    }
}