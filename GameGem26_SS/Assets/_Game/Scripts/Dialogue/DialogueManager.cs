using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class DialogueLine
{
    [TextArea(3, 5)]
    public string sentence;
    public string emotion;
    public string speakerName;

    [Header("여동생 악보 설정")]
    public bool isMusicSheet;
    public string solvedWord;
    public AudioClip dissonanceClip;
    public AudioClip harmonyClip;

    [Header("선택지 설정")]
    public bool isChoice;
    public string choice1Text;
    public int choice1TargetIndex; // ★ 1번 버튼을 눌렀을 때 점프할 대사 번호
    public string choice2Text;
    public int choice2TargetIndex; // ★ 2번 버튼을 눌렀을 때 점프할 대사 번호
    public string choice3Text;
    public int choice3TargetIndex; // ★ 3번 버튼을 눌렀을 때 점프할 대사 번호

    [Header("시나리오 강제 종료 설정")]
    public bool isEndLine;
}

[System.Serializable]
public class DayData
{
    public string dayName; // 예: "Day 1 스토리"
    public List<DialogueLine> dialogues;
}

public class DialogueManager : MonoBehaviour
{
    // ★ [전역 기억 장치] 게임 전체에서 "현재 몇 일 차"인지 기억하는 변수 (씬이 바뀌어도 유지됨)
    // 0 = Day 1, 1 = Day 2, 2 = Day 3 ...
    public static int GameDayIndex = 0;

    [Header("일반 UI 연결")]
    public GameObject dialogueBoxObject;
    public TextMeshProUGUI dialogueText;
    public GameObject clickHintObject;

    [Header("여동생 악보 UI 연결")]
    public GameObject chordObject;
    public Image sheetImage;
    public TextMeshProUGUI solvedText;

    [Header("선택지 UI 연결")]
    public GameObject choiceObject;
    public Button choiceButton1;
    public Button choiceButton2;
    public Button choiceButton3;

    [Header("설정")]
    public float typingSpeed = 0.05f;
    public List<Sprite> musicSheetSprites;
    public AudioSource sisterVoiceSource;

    [Header("데이별 데이터 (인스펙터에서 채우기)")]
    public List<DayData> allDaysData; // Element 0에 Day1 대사, Element 1에 Day2 대사...

    [Header("대사 종료 이벤트")]
    public UnityEvent onDialogueEnded;

    private List<DialogueLine> currentDayLines = new List<DialogueLine>();
    private int currentLineIndex = 0;

    private DialogueLine currentLine;
    private bool isTyping = false;
    private bool isWaitingForChoice = false;
    private bool isStoryEnded = false;
    private bool hasRaisedEndEvent = false;

    void Start()
    {
        // 버튼 이벤트 연결
        if (choiceButton1 != null) choiceButton1.onClick.AddListener(() => OnChoiceSelected(1));
        if (choiceButton2 != null) choiceButton2.onClick.AddListener(() => OnChoiceSelected(2));
        if (choiceButton3 != null) choiceButton3.onClick.AddListener(() => OnChoiceSelected(3));

        // ★ [핵심] 현재 기억된 GameDayIndex에 맞는 데이터를 자동으로 로드합니다.
        LoadCurrentDayDialogue();
    }

    void LoadCurrentDayDialogue()
    {
        int dayIndex = GameFlowManager.Instance != null ? GameFlowManager.Instance.CurrentDay - 1 : GameDayIndex;

        // 인덱스 오버플로우 방지 안전장치
        if (dayIndex < 0 || dayIndex >= allDaysData.Count)
        {
            dialogueText.text = "[더 이상 준비된 스토리 데이터가 없습니다.]";
            return;
        }

        currentDayLines = allDaysData[dayIndex].dialogues;
        currentLineIndex = 0;
        isStoryEnded = false;
        hasRaisedEndEvent = false;

        DisplayNextSentence();
    }

    void Update()
    {
        if (isWaitingForChoice) return;

        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            if (isTyping)
            {
                StopAllCoroutines();
                dialogueText.text = currentLine.sentence;
                isTyping = false;
                SetClickHintVisible(true);
            }
            else
            {
                if (currentLine != null && currentLine.isEndLine)
                {
                    EndDialogue();
                    return;
                }

                DisplayNextSentence();
            }
        }
    }

    public void DisplayNextSentence()
    {
        if (currentLineIndex >= currentDayLines.Count)
        {
            EndDialogue();
            return;
        }

        currentLine = currentDayLines[currentLineIndex];
        // 일단 다음 대사를 위해 인덱스를 1 올려둠 (점프가 일어나면 이 값은 무시됨)
        currentLineIndex++;

        if (currentLine.isChoice)
        {
            SetDialogueBoxVisible(false);
            SetMusicBoxVisible(false);
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
            else choiceButton3.gameObject.SetActive(false);

            isWaitingForChoice = true;
            return;
        }

        if (currentLine.isMusicSheet)
        {
            choiceObject.SetActive(false);
            SetDialogueBoxVisible(false);
            SetMusicBoxVisible(true);

            Sprite targetSprite = musicSheetSprites.Find(s => s.name == currentLine.sentence);
            if (targetSprite != null) sheetImage.sprite = targetSprite;

            if (solvedText != null)
            {
                solvedText.text = currentLine.solvedWord;
            }

            PlaySisterVoice(currentLine);
            isTyping = false;
            SetClickHintVisible(true);
        }
        else
        {
            SetMusicBoxVisible(false);
            choiceObject.SetActive(false);
            SetDialogueBoxVisible(true);
            SetClickHintVisible(false);

            StopAllCoroutines();
            StartCoroutine(TypeSentence(currentLine.sentence));
        }
    }

    public void OnChoiceSelected(int choiceButtonNumber)
    {
        choiceObject.SetActive(false);
        isWaitingForChoice = false;

        if (choiceButtonNumber == 1)
        {
            currentLineIndex = currentLine.choice1TargetIndex;
        }
        else if (choiceButtonNumber == 2)
        {
            currentLineIndex = currentLine.choice2TargetIndex;
        }
        else if (choiceButtonNumber == 3)
        {
            currentLineIndex = currentLine.choice3TargetIndex;
        }

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
        SetClickHintVisible(true);
    }

    void SetDialogueBoxVisible(bool visible)
    {
        if (dialogueBoxObject != null)
        {
            dialogueBoxObject.SetActive(visible);
        }
    }

    void SetMusicBoxVisible(bool visible)
    {
        if (chordObject != null)
        {
            chordObject.SetActive(visible);
        }
    }

    void SetClickHintVisible(bool visible)
    {
        if (clickHintObject != null)
        {
            clickHintObject.SetActive(visible);
        }
    }

    // --- [DialogueManager.cs 내부 PlaySisterVoice()] ---
    void PlaySisterVoice(DialogueLine line)
    {
        if (sisterVoiceSource == null)
        {
            sisterVoiceSource = GetComponent<AudioSource>();
            if (sisterVoiceSource == null)
            {
                sisterVoiceSource = gameObject.AddComponent<AudioSource>();
            }
        }

        // ★ [체인 연결] 도감(BookManager)에서 이 단어가 맞춤(confirmed) 상태인지 실시간 체크
        bool isConfirmedInBook = BookManager.Instance != null && BookManager.Instance.IsWordConfirmed(line.solvedWord);

        // 도감에서 맞췄고, harmonyClip이 있다면 맞는 화음(harmonyClip) 재생! 아니라면 불협화음 재생!
        AudioClip clip = isConfirmedInBook && line.harmonyClip != null
            ? line.harmonyClip
            : line.dissonanceClip;

        if (clip != null)
        {
            sisterVoiceSource.Stop();
            sisterVoiceSource.PlayOneShot(clip);
        }
    }

    // ★ 오늘의 스토리가 끝났을 때 실행되는 함수
    void EndDialogue()
    {
        if (hasRaisedEndEvent)
        {
            return;
        }

        hasRaisedEndEvent = true;
        isStoryEnded = true;
        SetDialogueBoxVisible(false);
        SetMusicBoxVisible(false);
        if (choiceObject != null) choiceObject.SetActive(false);
        SetClickHintVisible(false);
        onDialogueEnded?.Invoke();
    }
}