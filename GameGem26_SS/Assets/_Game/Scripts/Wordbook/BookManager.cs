using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class WordbookEntry
{
    public string id;
    public string meaning;
    public Sprite noteSprite;
    public bool confirmed;
    [NonSerialized] public string memo;
}

[Serializable]
public class WordbookEntryView
{
    public Button button;
    public Image background;
    public Image noteImage;
    public TextMeshProUGUI idLabel;
    public TextMeshProUGUI stateLabel;
    public TMP_InputField memoInput;
}

public class BookManager : MonoBehaviour
{
    public static BookManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Button toggleButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private TextMeshProUGUI detailTitle;
    [SerializeField] private TextMeshProUGUI detailBody;
    
    // ★ 12개로 맞춘 UI 슬롯들을 담을 리스트
    [SerializeField] private List<WordbookEntryView> entryViews = new List<WordbookEntryView>();

    [Header("Entries")]
    // ★ 12개의 데이터가 들어갈 리스트
    [SerializeField] private List<WordbookEntry> entries = new List<WordbookEntry>();

    [Header("Colors")]
    [SerializeField] private Color normalCardColor = new Color(0.28f, 0.23f, 0.18f, 0.95f);
    [SerializeField] private Color selectedCardColor = new Color(0.46f, 0.37f, 0.22f, 0.98f);
    [SerializeField] private Color confirmedTextColor = new Color(0.73f, 0.86f, 0.62f, 1f);
    [SerializeField] private Color unknownTextColor = new Color(0.83f, 0.76f, 0.62f, 1f);

    private int selectedIndex;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        BindUi();
        SetPanelVisible(false);
        RefreshAll();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleWordbook();
        }

        if (Input.GetKeyDown(KeyCode.Escape) && panelRoot != null && panelRoot.activeSelf)
        {
            SetPanelVisible(false);
        }
    }

    public void ToggleWordbook()
    {
        SetPanelVisible(panelRoot != null && !panelRoot.activeSelf);
    }

    public bool IsWordConfirmed(string word)
    {
        if (string.IsNullOrEmpty(word)) return false;

<<<<<<< HEAD
        // entries 리스트를 하나씩 순회하며 검사
=======
>>>>>>> main
        foreach (WordbookEntry entry in entries)
        {
            if (entry != null && entry.meaning == word)
            {
                return entry.confirmed;
            }
        }
        return false;
    }

    private void BindUi()
    {
        if (toggleButton != null)
        {
            toggleButton.onClick.RemoveListener(ToggleWordbook);
            toggleButton.onClick.AddListener(ToggleWordbook);
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(ToggleWordbook);
            closeButton.onClick.AddListener(ToggleWordbook);
        }

        // ★ [안전장치] 슬롯(UI) 개수와 실제 데이터 개수 중 더 작은 값에 맞추어 인덱스 바인딩을 진행합니다.
        int activeCount = Mathf.Min(entryViews.Count, entries.Count);

        for (int i = 0; i < entryViews.Count; i++)
        {
            int index = i;
            WordbookEntryView view = entryViews[i];

            if (view == null) continue;

            // 12개의 범위를 넘어가는 비활성 UI 예외 처리
            if (i >= activeCount)
            {
                if (view.button != null) view.button.gameObject.SetActive(false);
                continue;
            }

            if (view.button != null)
            {
                view.button.onClick.RemoveAllListeners();
                view.button.onClick.AddListener(delegate { SelectEntry(index); });
            }

            if (view.memoInput != null)
            {
                view.memoInput.onEndEdit.RemoveAllListeners();
                view.memoInput.onEndEdit.AddListener(delegate (string text)
                {
                    if (index < entries.Count)
                    {
                        entries[index].memo = text.Trim();
                        SelectEntry(index);
                    }
                });
            }
        }
    }

    private void SetPanelVisible(bool visible)
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(visible);
        }

        RefreshAll();
    }

    private void SelectEntry(int index)
    {
        // ★ 목록이 줄어듦에 따라 선택할 수 있는 최대 인덱스 범위를 안전하게 재조정합니다.
        int maxIndex = Mathf.Max(0, entries.Count - 1);
        selectedIndex = Mathf.Clamp(index, 0, maxIndex);
        RefreshAll();
    }

    private void RefreshAll()
    {
        for (int i = 0; i < entryViews.Count; i++)
        {
            WordbookEntryView view = entryViews[i];
            if (view == null) continue;

            bool hasEntry = i < entries.Count;
            if (view.button != null) view.button.gameObject.SetActive(hasEntry);
            if (!hasEntry) continue;

            WordbookEntry entry = entries[i];
            if (view.background != null) view.background.color = i == selectedIndex ? selectedCardColor : normalCardColor;
            if (view.noteImage != null)
            {
                view.noteImage.sprite = entry.noteSprite;
                view.noteImage.enabled = entry.noteSprite != null;
                view.noteImage.preserveAspect = true;
            }

            if (view.idLabel != null) view.idLabel.text = entry.id;
            if (view.stateLabel != null)
            {
                view.stateLabel.text = entry.confirmed ? "화음" : "불협";
                view.stateLabel.color = entry.confirmed ? confirmedTextColor : unknownTextColor;
            }
        }

        RefreshDetail();
    }

    private void RefreshDetail()
    {
        if (detailTitle == null || detailBody == null || entries.Count == 0) return;

        // ★ 선택된 인덱스가 현재 줄어든 리스트 크기(12개)를 벗어나지 않게 제어합니다.
        WordbookEntry entry = entries[Mathf.Clamp(selectedIndex, 0, entries.Count - 1)];
        string memo = string.IsNullOrWhiteSpace(entry.memo) ? "비어 있음" : entry.memo;

        detailTitle.text = entry.confirmed ? entry.meaning : "미확정 단어";
        detailBody.text =
            "ID: " + entry.id + "\n\n" +
            "상태: " + (entry.confirmed ? "화음으로 정리됨" : "불협화음") + "\n" +
            "의미: " + (entry.confirmed ? entry.meaning : "아직 모름") + "\n" +
            "내 메모: " + memo;
    }
}