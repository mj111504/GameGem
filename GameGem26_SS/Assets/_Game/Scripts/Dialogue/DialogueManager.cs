using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

// ★ [추가] 악보 하나하나의 독립된 데이터를 담는 클래스
[System.Serializable]
public class MusicSheetData
{
    public string sheetName;           // 악보 스프라이트를 검색할 이름
    public Sprite customSprite;        // (선택) 인스펙터에서 직접 이미지를 드래그 앤 드롭하고 싶을 때 사용
    public string solvedWord;          // 이 악보의 정답 단어
    public AudioClip dissonanceClip;   // 이 악보의 불협화음 사운드
    public AudioClip harmonyClip;      // 이 악보의 화음 사운드
}

[System.Serializable]
public class DialogueLine
{
    [TextArea(3, 5)]
    public string sentence;
    public string emotion;
    public string speakerName;

    [Header("여동생 악보 설정")]
    public bool isMusicSheet;
    // ★ 기존 단일 변수들을 제거하고, 여러 개의 악보를 넣을 수 있도록 리스트로 변경
    public List<MusicSheetData> musicSheets;

    [Header("선택지 설정")]
    public bool isChoice;
    public string choice1Text;
    public int choice1TargetIndex;
    public string choice2Text;
    public int choice2TargetIndex;
    public string choice3Text;
    public int choice3TargetIndex;

    [Header("시나리오 강제 종료 설정")]
    public bool isEndLine;
}

[System.Serializable]
public class DayData
{
    public string dayName;
    public List<DialogueLine> dialogues;
}

public class DialogueManager : MonoBehaviour
{
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
    public List<DayData> allDaysData;

    [Header("대사 종료 이벤트")]
    public UnityEvent onDialogueEnded;

    private List<DialogueLine> currentDayLines = new List<DialogueLine>();
    private int currentLineIndex = 0;

    // ★ [추가] 하나의 대사(DialogueLine) 안에서 현재 몇 번째 악보를 보여주고 있는지 기억하는 인덱스
    private int currentSheetIndex = 0;

    private DialogueLine currentLine;
    private bool isTyping = false;
    private bool isWaitingForChoice = false;
    private bool isStoryEnded = false;
    private bool hasRaisedEndEvent = false;

    void Start()
    {
        if (choiceButton1 != null) choiceButton1.onClick.AddListener(() => OnChoiceSelected(1));
        if (choiceButton2 != null) choiceButton2.onClick.AddListener(() => OnChoiceSelected(2));
        if (choiceButton3 != null) choiceButton3.onClick.AddListener(() => OnChoiceSelected(3));

        LoadCurrentDayDialogue();
    }

    void LoadCurrentDayDialogue()
    {
        int dayIndex = GameFlowManager.Instance != null ? GameFlowManager.Instance.CurrentDay - 1 : GameDayIndex;

        if (dayIndex < 0 || dayIndex >= allDaysData.Count)
        {
            dialogueText.text = "[더 이상 준비된 스토리 데이터가 없습니다.]";
            return;
        }

        currentDayLines = allDaysData[dayIndex].dialogues;
        currentLineIndex = 0;
        currentSheetIndex = 0; // 초기화 [추가]
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
                // 악보 연속 재생 중일 때는 'isEndLine' 체크로 넘어가기 전에 다음 악보를 먼저 보여줌
                if (currentLine != null && currentLine.isMusicSheet)
                {
                    DisplayNextSentence();
                    return;
                }

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
        // 더 이상 대사가 없고, 악보 진행도 끝났다면 종료
        if (currentLineIndex >= currentDayLines.Count && currentSheetIndex == 0)
        {
            EndDialogue();
            return;
        }

        // 현재 진행할 대사 라인 참조
        currentLine = currentDayLines[currentLineIndex];

        // --- [악보 기믹 처리 영역] ---
        if (currentLine.isMusicSheet)
        {
            // 방어 코드: 악보 리스트가 비어있다면 그냥 다음 대사로 패스
            if (currentLine.musicSheets == null || currentLine.musicSheets.Count == 0)
            {
                currentLineIndex++;
                currentSheetIndex = 0;
                DisplayNextSentence();
                return;
            }

            choiceObject.SetActive(false);
            SetDialogueBoxVisible(false);
            SetMusicBoxVisible(true);

            // 현재 순서의 악보 데이터 가져오기
            MusicSheetData currentSheet = currentLine.musicSheets[currentSheetIndex];

            // 1. 이미지 설정 (customSprite가 있으면 우선 적용, 없으면 이름으로 검색)
            if (currentSheet.customSprite != null)
            {
                sheetImage.sprite = currentSheet.customSprite;
            }
            else
            {
                Sprite targetSprite = musicSheetSprites.Find(s => s.name == currentSheet.sheetName);
                if (targetSprite != null) sheetImage.sprite = targetSprite;
            }

            // 2. 텍스트 설정
            if (solvedText != null)
            {
                solvedText.text = currentSheet.solvedWord;
            }

            // 3. 고유 사운드 재생
            PlaySisterVoice(currentSheet);

            isTyping = false;
            SetClickHintVisible(true);

            // 다음 클릭을 위해 악보 인덱스 증가
            currentSheetIndex++;

            // 이 대사 라인의 모든 악보를 다 보여주었다면?
            if (currentSheetIndex >= currentLine.musicSheets.Count)
            {
                currentSheetIndex = 0; // 악보 인덱스 초기화
                currentLineIndex++;    // 다음 대사 라인으로 넘어가도록 증가
            }
            return;
        }

        // --- [일반 대사 및 선택지 처리 영역] ---
        // 일반 대사로 넘어왔으므로 악보 인덱스는 안전하게 0으로 세팅
        currentSheetIndex = 0;
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

        SetMusicBoxVisible(false);
        choiceObject.SetActive(false);
        SetDialogueBoxVisible(true);
        SetClickHintVisible(false);

        StopAllCoroutines();
        StartCoroutine(TypeSentence(currentLine.sentence));
    }

    public void OnChoiceSelected(int choiceButtonNumber)
    {
        choiceObject.SetActive(false);
        isWaitingForChoice = false;

        if (choiceButtonNumber == 1) currentLineIndex = currentLine.choice1TargetIndex;
        else if (choiceButtonNumber == 2) currentLineIndex = currentLine.choice2TargetIndex;
        else if (choiceButtonNumber == 3) currentLineIndex = currentLine.choice3TargetIndex;

        currentSheetIndex = 0; // 선택지 점프 시 악보 인덱스 초기화
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

    void SetDialogueBoxVisible(bool visible) { if (dialogueBoxObject != null) dialogueBoxObject.SetActive(visible); }
    void SetMusicBoxVisible(bool visible) { if (chordObject != null) chordObject.SetActive(visible); }
    void SetClickHintVisible(bool visible) { if (clickHintObject != null) clickHintObject.SetActive(visible); }

    // ★ [수정] 개별 악보 데이터를 직접 매개변수로 받도록 변경
    void PlaySisterVoice(MusicSheetData sheet)
    {
        if (sisterVoiceSource == null)
        {
            sisterVoiceSource = GetComponent<AudioSource>();
            if (sisterVoiceSource == null)
            {
                sisterVoiceSource = gameObject.AddComponent<AudioSource>();
            }
        }

        // 매개변수로 넘어온 악보(sheet)의 클립 데이터 활용
        AudioClip clip = !string.IsNullOrEmpty(sheet.solvedWord) && sheet.harmonyClip != null
            ? sheet.harmonyClip
            : sheet.dissonanceClip;

        if (clip != null)
        {
            sisterVoiceSource.Stop();
            sisterVoiceSource.PlayOneShot(clip);
        }
    }

    void EndDialogue()
    {
        if (hasRaisedEndEvent) return;

        hasRaisedEndEvent = true;
        isStoryEnded = true;
        SetDialogueBoxVisible(false);
        SetMusicBoxVisible(false);
        if (choiceObject != null) choiceObject.SetActive(false);
        SetClickHintVisible(false);
        onDialogueEnded?.Invoke();
    }
}