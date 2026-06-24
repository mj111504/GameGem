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
    [SerializeField] private List<WordbookEntryView> entryViews = new List<WordbookEntryView>();

    [Header("Entries")]
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

    public void ConfirmWord(string word)
    {
        for (int i = 0; i < entries.Count; i++)
        {
            if (entries[i].meaning == word)
            {
                entries[i].confirmed = true;
                RefreshAll();
                return;
            }
        }
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

        for (int i = 0; i < entryViews.Count; i++)
        {
            int index = i;
            WordbookEntryView view = entryViews[i];

            if (view.button != null)
            {
                view.button.onClick.RemoveAllListeners();
                view.button.onClick.AddListener(delegate { SelectEntry(index); });
            }

            if (view.memoInput != null)
            {
                view.memoInput.onEndEdit.RemoveAllListeners();
                view.memoInput.onEndEdit.AddListener(delegate(string text)
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
        selectedIndex = Mathf.Clamp(index, 0, Mathf.Max(0, entries.Count - 1));
        RefreshAll();
    }

    private void RefreshAll()
    {
        for (int i = 0; i < entryViews.Count; i++)
        {
            WordbookEntryView view = entryViews[i];
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
