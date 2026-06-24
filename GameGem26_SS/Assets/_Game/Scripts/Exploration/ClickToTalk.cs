using UnityEngine;
using TMPro;
using System.Collections;

public class ClickToTalk : MonoBehaviour
{
    [Header("Shared dialogue UI")]
    public GameObject dialogueCanvas;
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI subInfoText;
    public GameObject clickHintObject;

    [Header("Dialogue")]
    [TextArea(3, 5)] public string clickDialogue;
    [TextArea(3, 5)] public string firstDialogue;
    [TextArea(3, 5)] public string secondDialogue;

    [Header("Typing")]
    public float typingSpeed = 0.05f;

    private Coroutine typingCoroutine;
    private int dialogueIndex;
    private bool isTalking;

    private void OnMouseDown()
    {
        if (isTalking || dialogueCanvas == null || dialogueCanvas.activeSelf) return;

        dialogueIndex = 0;
        ShowCurrentDialogue();
    }

    private void Update()
    {
        if (!isTalking) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            dialogueIndex++;
            if (dialogueIndex >= GetDialogueCount())
            {
                CloseDialogue();
            }
            else
            {
                ShowCurrentDialogue();
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseDialogue();
        }
    }

    private void ShowCurrentDialogue()
    {
        isTalking = true;
        dialogueCanvas.SetActive(true);

        if (subInfoText != null)
        {
            subInfoText.text = "Space / Click";
        }

        if (clickHintObject != null)
        {
            clickHintObject.SetActive(false);
        }

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        typingCoroutine = StartCoroutine(TypeText(GetDialogue(dialogueIndex)));
    }

    private void CloseDialogue()
    {
        isTalking = false;
        dialogueIndex = 0;
        dialogueCanvas.SetActive(false);
    }

    private int GetDialogueCount()
    {
        int count = 0;
        if (!string.IsNullOrWhiteSpace(clickDialogue)) count++;
        if (!string.IsNullOrWhiteSpace(firstDialogue)) count++;
        if (!string.IsNullOrWhiteSpace(secondDialogue)) count++;
        return Mathf.Max(1, count);
    }

    private string GetDialogue(int index)
    {
        if (!string.IsNullOrWhiteSpace(firstDialogue))
        {
            return index == 0 ? firstDialogue : secondDialogue;
        }

        return clickDialogue;
    }

    private IEnumerator TypeText(string targetText)
    {
        if (dialogueText == null) yield break;

        dialogueText.text = "";
        foreach (char letter in targetText.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        typingCoroutine = null;

        if (clickHintObject != null)
        {
            clickHintObject.SetActive(true);
        }
    }
}
