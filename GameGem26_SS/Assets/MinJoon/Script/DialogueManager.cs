using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class DialogueLine
{
    [TextArea(3, 5)]
    public string sentence;     // 일반 대사 내용 (여동생일 경우 악보 이미지 파일명)
    public string emotion;      // 감정/표정 상태

    [Header("여동생 악보 설정")]
    public bool isMusicSheet;   // 이 대사가 여동생의 악보 대사인가요?
    public string solvedWord;   // 악보 위에 얹어질 유추된 단어 문구

    [Header("선택지 설정")]
    public bool isChoice;         // 이 타이밍에 선택지를 띄울 것인가?
    public string choice1Text;    // 1번 버튼 문구
    public string choice2Text;    // 2번 버튼 문구
    public string choice3Text;    // 3번 버튼 문구 (비워두면 버튼 비활성화)
}

public class DialogueManager : MonoBehaviour
{
    [Header("일반 UI 연결")]
    public TextMeshProUGUI dialogueText;   // 일반 대사 텍스트

    [Header("여동생 악보 UI 연결")]
    public GameObject chordObject;         // 여동생 오브젝트 묶음 (ChordObject)
    public Image sheetImage;               // 악보 이미지 컴포넌트
    public TextMeshProUGUI solvedText;     // 악보 위에 뜰 단어 텍스트

    [Header("선택지 UI 연결")]
    public GameObject choiceObject;       // ChoiceObject 묶음 패널
    public Button choiceButton1;
    public Button choiceButton2;
    public Button choiceButton3;

    [Header("설정")]
    public float typingSpeed = 0.05f;
    public List<DialogueLine> testDialogues;
    public List<Sprite> musicSheetSprites;

    private Queue<DialogueLine> dialogueLines = new Queue<DialogueLine>();
    private DialogueLine currentLine;
    private bool isTyping = false;
    private bool isWaitingForChoice = false;

    void Start()
    {
        foreach (DialogueLine line in testDialogues)
        {
            dialogueLines.Enqueue(line);
        }

        // 선택지 버튼 클릭 이벤트 연결
        if (choiceButton1 != null) choiceButton1.onClick.AddListener(() => OnChoiceSelected(1));
        if (choiceButton2 != null) choiceButton2.onClick.AddListener(() => OnChoiceSelected(2));
        if (choiceButton3 != null) choiceButton3.onClick.AddListener(() => OnChoiceSelected(3));

        DisplayNextSentence();
    }

    void Update()
    {
        // 선택지 대기 중일 때는 마우스 클릭으로 넘어가는 것을 막음
        if (isWaitingForChoice) return;

        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            if (isTyping)
            {
                StopAllCoroutines();
                dialogueText.text = currentLine.sentence;
                isTyping = false;
            }
            else
            {
                DisplayNextSentence();
            }
        }
    }

    public void DisplayNextSentence()
    {
        if (dialogueLines.Count == 0)
        {
            EndDialogue();
            return;
        }

        currentLine = dialogueLines.Dequeue();

        // 1. 선택지 처리 분기
        if (currentLine.isChoice)
        {
            dialogueText.gameObject.SetActive(false);
            chordObject.SetActive(false);
            choiceObject.SetActive(true);

            choiceButton1.gameObject.SetActive(true);
            choiceButton1.GetComponentInChildren<TextMeshProUGUI>().text = currentLine.choice1Text;

            choiceButton2.gameObject.SetActive(true);
            choiceButton2.GetComponentInChildren<TextMeshProUGUI>().text = currentLine.choice2Text;

            if (!string.IsNullOrEmpty(currentLine.choice3Text))
            {
                choiceButton3.gameObject.SetActive(true);
                choiceButton3.GetComponentInChildren<TextMeshProUGUI>().text = currentLine.choice3Text;
            }
            else
            {
                choiceButton3.gameObject.SetActive(false);
            }

            isWaitingForChoice = true;
            return;
        }

        // 2. 여동생 악보 처리 분기
        if (currentLine.isMusicSheet)
        {
            dialogueText.gameObject.SetActive(false);
            choiceObject.SetActive(false);
            chordObject.SetActive(true);

            // 악보 이미지 변경
            Sprite targetSprite = musicSheetSprites.Find(s => s.name == currentLine.sentence);
            if (targetSprite != null) sheetImage.sprite = targetSprite;

            // 악보 위에 유추된 텍스트 바로 표시
            solvedText.text = currentLine.solvedWord;

            isTyping = false;
        }
        // 3. 일반 대사 처리 분기
        else
        {
            chordObject.SetActive(false);
            choiceObject.SetActive(false);
            dialogueText.gameObject.SetActive(true);

            StopAllCoroutines();
            StartCoroutine(TypeSentence(currentLine.sentence));
        }
    }

    public void OnChoiceSelected(int choiceIndex)
    {
        Debug.Log($"{choiceIndex}번 선택지를 골랐습니다.");
        choiceObject.SetActive(false);
        isWaitingForChoice = false;
        DisplayNextSentence();
    }

    IEnumerator TypeSentence(string sentence)
    {
        dialogueText.text = "";
        isTyping = true;
        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
        isTyping = false;
    }

    void EndDialogue()
    {
        dialogueText.text = "[이야기가 끝났습니다.]";
        dialogueText.gameObject.SetActive(true);
        chordObject.SetActive(false);
        choiceObject.SetActive(false);
    }
}