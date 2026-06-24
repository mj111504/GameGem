using UnityEngine;
using TMPro;
using System.Collections;

public class ClickToTalk : MonoBehaviour
{
    [Header("UI 연결")]
    public GameObject dialogueCanvas;
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI subInfoText;

    [Header("나올 대사 입력")]
    [TextArea(3, 5)]
    public string clickDialogue;

    [Header("타이핑 속도")]
    public float typingSpeed = 0.05f;

    private Coroutine typingCoroutine;
    private bool isTalking = false;

    private void OnMouseDown()
    {
        if (isTalking || dialogueCanvas.activeSelf) return;

        Debug.Log("마우스 클릭 성공!");

        isTalking = true;
        dialogueCanvas.SetActive(true);
        subInfoText.text = "space▼";

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        typingCoroutine = StartCoroutine(TypeText(clickDialogue));
    }

    private void Update()
    {
        if (!isTalking) return;

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("대화창 닫기!");

            isTalking = false;
            dialogueCanvas.SetActive(false);
        }
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