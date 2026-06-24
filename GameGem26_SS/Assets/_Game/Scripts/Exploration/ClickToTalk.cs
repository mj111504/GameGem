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
    [TextArea(3, 5)]
    public string[] dialogues;

    [Header("Typing")]
    public float typingSpeed = 0.05f;

    private Coroutine typingCoroutine;
    private int dialogueIndex;
    private bool isTalking;
    private bool isTyping;

    // 클릭이 동시에 두 번 먹히는 것을 방지하는 시간 변수
    private float lastClickTime;

    private void OnMouseDown()
    {
        if (isTalking || dialogueCanvas == null || dialogueCanvas.activeSelf) return;
        if (dialogues == null || dialogues.Length == 0) return;

        // 대화를 시작한 시간을 기록합니다.
        lastClickTime = Time.time;

        dialogueIndex = 0;
        ShowCurrentDialogue();
    }

    private void Update()
    {
        if (!isTalking) return;

        // 대화를 시작한 직후(0.1초 이내)에는 스킵/넘어가기 클릭을 무시합니다.
        if (Time.time - lastClickTime < 0.1f) return;

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            lastClickTime = Time.time; // 다음 클릭을 위해 시간을 다시 갱신

            if (isTyping)
            {
                if (typingCoroutine != null)
                {
                    StopCoroutine(typingCoroutine);
                }

                dialogueText.text = dialogues[dialogueIndex];
                isTyping = false;

                if (clickHintObject != null)
                {
                    clickHintObject.SetActive(true);
                }
            }
            else
            {
                dialogueIndex++;

                if (dialogueIndex >= dialogues.Length)
                {
                    CloseDialogue();
                }
                else
                {
                    ShowCurrentDialogue();
                }
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

        if (clickHintObject != null)
        {
            clickHintObject.SetActive(false);
        }

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        typingCoroutine = StartCoroutine(TypeText(dialogues[dialogueIndex]));
    }

    private void CloseDialogue()
    {
        isTalking = false;
        isTyping = false;
        dialogueIndex = 0;

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        dialogueCanvas.SetActive(false);
    }

    private IEnumerator TypeText(string targetText)
    {
        isTyping = true;
        if (dialogueText == null) yield break;

        dialogueText.text = "";
        foreach (char letter in targetText.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
        typingCoroutine = null;

        if (clickHintObject != null)
        {
            clickHintObject.SetActive(true);
        }
    }
}