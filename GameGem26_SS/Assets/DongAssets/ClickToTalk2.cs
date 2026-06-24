using UnityEngine;
using TMPro;
using System.Collections;

public class ClickToTalk2 : MonoBehaviour
{
    [Header("UI 연결")]
    public GameObject dialogueCanvas;
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI subInfoText;

    [Header("대사 입력")]
    [TextArea(3, 5)] public string firstDialogue;
    [TextArea(3, 5)] public string secondDialogue;

    [Header("타이핑 속도")]
    public float typingSpeed = 0.05f;

    private int dialogueState = 0;
    private Coroutine typingCoroutine;

    private void OnMouseDown()
    {
        if (dialogueState > 0 || dialogueCanvas.activeSelf) return;

        Debug.Log("오브젝트 클릭: 첫 번째 대화 시작!");

        dialogueState = 1;
        dialogueCanvas.SetActive(true);

        StartTyping(firstDialogue);
        subInfoText.text = "space▼";
    }

    private void Update()
    {
        if (dialogueState == 0) return;

        if (dialogueState == 1 && Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Space 누름: 두 번째 대화로 전환!");

            dialogueState = 2;

            StartTyping(secondDialogue);
            subInfoText.text = "space▼";
        }
        else if (dialogueState == 2 && Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Space 누름: 대화창 종료!");

            dialogueState = 0;
            dialogueCanvas.SetActive(false);
        }
    }

    private void StartTyping(string textToType)
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        typingCoroutine = StartCoroutine(TypeText(textToType));
    }

    private IEnumerator TypeText(string targetText)
    {
        dialogueText.text = "";

        foreach (char letter in targetText.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        typingCoroutine = null;
    }
}