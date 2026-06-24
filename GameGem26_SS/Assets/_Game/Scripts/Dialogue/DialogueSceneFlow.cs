using UnityEngine;

public enum DialogueEndFlowAction
{
    None,
    EnterSisterRoom,
    FinishNoonDialogue,
    FinishNightDeduction
}

[RequireComponent(typeof(DialogueManager))]
public class DialogueSceneFlow : MonoBehaviour
{
    [SerializeField] private DialogueEndFlowAction action = DialogueEndFlowAction.None;

    private DialogueManager dialogueManager;

    private void Awake()
    {
        dialogueManager = GetComponent<DialogueManager>();
        dialogueManager.onDialogueEnded.AddListener(Run);
    }

    private void OnDestroy()
    {
        if (dialogueManager != null)
        {
            dialogueManager.onDialogueEnded.RemoveListener(Run);
        }
    }

    public void Run()
    {
        if (GameFlowManager.Instance == null)
        {
            Debug.LogWarning("Dialogue flow ended, but GameFlowManager is missing.");
            return;
        }

        switch (action)
        {
            case DialogueEndFlowAction.EnterSisterRoom:
                GameFlowManager.Instance.EnterSisterRoom();
                break;
            case DialogueEndFlowAction.FinishNoonDialogue:
                GameFlowManager.Instance.FinishNoonDialogue();
                break;
            case DialogueEndFlowAction.FinishNightDeduction:
                GameFlowManager.Instance.FinishNightDeduction();
                break;
        }
    }
}
