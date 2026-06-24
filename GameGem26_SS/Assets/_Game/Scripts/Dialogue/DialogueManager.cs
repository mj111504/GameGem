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

    [Space(10)]
    public string choice1Text;
    public int choice1TargetIndex;
    public int choice1StabilityChange; // ★ 1번 선택 시 여동생 안정도(스탯) 변화량 (예: 5, -10 등)

    [Space(10)]
    public string choice2Text;
    public int choice2TargetIndex;
    public int choice2StabilityChange; // ★ 2번 선택 시 여동생 안정도(스탯) 변화량

    [Space(10)]
    public string choice3Text;
    public int choice3TargetIndex;
    public int choice3StabilityChange; // ★ 3번 선택 시 여동생 안정도(스탯) 변화량

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

    private DialogueLine currentLine;
    private bool isTyping = false;
    private bool isWaitingForChoice = false;
    private bool isStoryEnded = false;
    private bool hasRaisedEndEvent = false;

    private int buttonClickedFrame = -1;

    void Start()
    {
        if (choiceButton1 != null) { choiceButton1.onClick.RemoveAllListeners(); choiceButton1.onClick.AddListener(() => OnChoiceSelected(1)); }
        if (choiceButton2 != null) { choiceButton2.onClick.RemoveAllListeners(); choiceButton2.onClick.AddListener(() => OnChoiceSelected(2)); }
        if (choiceButton3 != null) { choiceButton3.onClick.RemoveAllListeners(); choiceButton3.onClick.AddListener(() => OnChoiceSelected(3)); }

        LoadCurrentDayDialogue();
    }

    void LoadCurrentDayDialogue()
    {
        int dayIndex = GameFlowManager.Instance != null ? GameFlowManager.Instance.CurrentDay - 1 : GameDayIndex;

        if (dayIndex < 0 || dayIndex >= allDaysData.Count)
        {
            if (dialogueText != null) dialogueText.text = "[더 이상 준비된 스토리 데이터가 없습니다.]";
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
        if (Time.frameCount == buttonClickedFrame) return;

        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            if (isTyping)
            {
                StopAllCoroutines();
                if (dialogueText != null) dialogueText.text = currentLine.sentence;
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
        Debug.Log($"<color=white>[대사 로드]</color> 인덱스 번호: {currentLineIndex} | 내용: {currentLine.sentence}");

        currentLineIndex++;

        // --- 1. 선택지 분기 영역 ---
        if (currentLine.isChoice)
        {
            SetDialogueBoxVisible(false);
            SetMusicBoxVisible(false);
            if (choiceObject != null) choiceObject.SetActive(true);

            if (choiceButton1 != null)
            {
                choiceButton1.gameObject.SetActive(true);
                choiceButton1.GetComponentInChildren<TextMeshProUGUI>().text = currentLine.choice1Text;
            }
            if (choiceButton2 != null)
            {
                choiceButton2.gameObject.SetActive(true);
                choiceButton2.GetComponentInChildren<TextMeshProUGUI>().text = currentLine.choice2Text;
            }

            if (choiceButton3 != null)
            {
                if (!string.IsNullOrEmpty(currentLine.choice3Text))
                {
                    choiceButton3.gameObject.SetActive(true);
                    choiceButton3.GetComponentInChildren<TextMeshProUGUI>().text = currentLine.choice3Text;
                }
                else choiceButton3.gameObject.SetActive(false);
            }

            isWaitingForChoice = true;
            return;
        }

        // --- 2. 악보 기믹 영역 ---
        if (currentLine.isMusicSheet)
        {
            if (choiceObject != null) choiceObject.SetActive(false);
            SetDialogueBoxVisible(false);
            SetMusicBoxVisible(true);

            if (musicSheetSprites != null && sheetImage != null)
            {
                Sprite targetSprite = musicSheetSprites.Find(s => s.name == currentLine.sentence);
                if (targetSprite != null) sheetImage.sprite = targetSprite;
            }

            if (solvedText != null) solvedText.text = currentLine.solvedWord;

            PlaySisterVoice(currentLine);
            isTyping = false;
            SetClickHintVisible(true);
        }
        // --- 3. 일반 대사 출력 영역 ---
        else
        {
            SetMusicBoxVisible(false);
            if (choiceObject != null) choiceObject.SetActive(false);

            if (dialogueBoxObject != null)
            {
                dialogueBoxObject.SetActive(true);
                Transform p = dialogueBoxObject.transform.parent;
                while (p != null)
                {
                    p.gameObject.SetActive(true);
                    p = p.parent;
                }
            }

            SetClickHintVisible(false);

            StopAllCoroutines();
            StartCoroutine(TypeSentence(currentLine.sentence));
        }
    }

    public void OnChoiceSelected(int choiceButtonNumber)
    {
        buttonClickedFrame = Time.frameCount;
        isWaitingForChoice = false;

        if (choiceObject != null) choiceObject.SetActive(false);

        int stabilityChangeAmount = 0;

        if (choiceButtonNumber == 1)
        {
            currentLineIndex = currentLine.choice1TargetIndex;
            stabilityChangeAmount = currentLine.choice1StabilityChange;
        }
        else if (choiceButtonNumber == 2)
        {
            currentLineIndex = currentLine.choice2TargetIndex;
            stabilityChangeAmount = currentLine.choice2StabilityChange;
        }
        else if (choiceButtonNumber == 3)
        {
            currentLineIndex = currentLine.choice3TargetIndex;
            stabilityChangeAmount = currentLine.choice3StabilityChange;
        }

        // ★ [스탯 반영 핵심 로직] GameFlowManager가 존재하면 안정도 스탯을 증감시킵니다.
        if (GameFlowManager.Instance != null)
        {
            GameFlowManager.Instance.AddStability(stabilityChangeAmount);
            Debug.Log($"<color=lime>[스탯 반영]</color> 여동생 안정도 변동: {stabilityChangeAmount} | 현재 총 안정도: {GameFlowManager.Instance.Stability}");
        }
        else
        {
            Debug.LogWarning("[DialogueManager] GameFlowManager가 씬에 없어 스탯을 반영하지 못했습니다.");
        }

        Debug.Log($"<color=cyan>[선택지 완료]</color> {choiceButtonNumber}번 분기 선택 -> 타겟 인덱스 {currentLineIndex}번으로 완전히 이동함.");

        SetMusicBoxVisible(false);

        if (dialogueBoxObject != null)
        {
            dialogueBoxObject.SetActive(true);
            Transform p = dialogueBoxObject.transform.parent;
            while (p != null)
            {
                p.gameObject.SetActive(true);
                p = p.parent;
            }
        }

        DisplayNextSentence();
    }

    IEnumerator TypeSentence(string sentence)
    {
        if (dialogueText == null) yield break;

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

    void PlaySisterVoice(DialogueLine line)
    {
        if (sisterVoiceSource == null)
        {
            sisterVoiceSource = GetComponent<AudioSource>();
            if (sisterVoiceSource == null) sisterVoiceSource = gameObject.AddComponent<AudioSource>();
        }

        AudioClip clip = !string.IsNullOrEmpty(line.solvedWord) && line.harmonyClip != null
            ? line.harmonyClip
            : line.dissonanceClip;

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