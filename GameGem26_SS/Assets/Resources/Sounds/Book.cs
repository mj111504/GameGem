using UnityEngine;
using UnityEngine.UI; // Image 컴포넌트 제어를 위해 필요합니다.
using TMPro;
using UnityEngine.SceneManagement;

public class BookManager : MonoBehaviour
{   
    // 여기서 배열 접근
    public static BookManager Instance { get; private set; }

    public struct BookItem
    {
        public string name;
        public string content;

        public BookItem(string name, string content)
        {
            this.name = name;
            this.content = content;
        }
    }

    public BookItem[,] bookData = new BookItem[3, 4];

    /*
    외부에서 데이터 접근하는 소스코드
    using UnityEngine;
    public class DisplayBookInfo : MonoBehaviour
    {
        private void Start()
        {
            // 싱글톤 인스턴스가 존재하는지 확인 후 접근
            if (BookManager.Instance != null)
            {
                // 0행 0열의 BookItem 데이터 가져오기
                BookManager.BookItem item = BookManager.Instance.bookData[0, 0];
            
                // 데이터 출력
                Debug.Log($"책 이름: {item.name}, 설명: {item.content}");
            }
        }
    }
    */
    [HideInInspector] public TMP_InputField[] bookInputFields = new TMP_InputField[12];

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitBookData();
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void InitBookData()
    {
        bookData[0, 0] = new BookItem("", "1번 설명");
        bookData[0, 1] = new BookItem("", "2번 설명");
        bookData[0, 2] = new BookItem("", "3번 설명");
        bookData[0, 3] = new BookItem("", "4번 설명");

        bookData[1, 0] = new BookItem("", "5번 설명");
        bookData[1, 1] = new BookItem("", "6번 설명");
        bookData[1, 2] = new BookItem("", "7번 설명");
        bookData[1, 3] = new BookItem("", "8번 설명");

        bookData[2, 0] = new BookItem("", "9번 설명");
        bookData[2, 1] = new BookItem("", "10번 설명");
        bookData[2, 2] = new BookItem("", "11번 설명");
        bookData[2, 3] = new BookItem("", "12번 설명");
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        GameObject canvas = GameObject.Find("Canvas");
        if (canvas == null) return;

        // 12개의 인풋 필드를 이름 기반으로 자동 검색 및 할당
        bookInputFields[0] = FindComponentInChild<TMP_InputField>(canvas, "InputField (TMP) (1)");
        bookInputFields[1] = FindComponentInChild<TMP_InputField>(canvas, "InputField (TMP) (2)");
        bookInputFields[2] = FindComponentInChild<TMP_InputField>(canvas, "InputField (TMP) (3)");
        bookInputFields[3] = FindComponentInChild<TMP_InputField>(canvas, "InputField (TMP) (4)");
        bookInputFields[4] = FindComponentInChild<TMP_InputField>(canvas, "InputField (TMP) (5)");
        bookInputFields[5] = FindComponentInChild<TMP_InputField>(canvas, "InputField (TMP) (6)");
        bookInputFields[6] = FindComponentInChild<TMP_InputField>(canvas, "InputField (TMP) (7)");
        bookInputFields[7] = FindComponentInChild<TMP_InputField>(canvas, "InputField (TMP) (8)");
        bookInputFields[8] = FindComponentInChild<TMP_InputField>(canvas, "InputField (TMP) (9)");
        bookInputFields[9] = FindComponentInChild<TMP_InputField>(canvas, "InputField (TMP) (10)");
        bookInputFields[10] = FindComponentInChild<TMP_InputField>(canvas, "InputField (TMP) (11)");
        bookInputFields[11] = FindComponentInChild<TMP_InputField>(canvas, "InputField (TMP) (12)");

        // UI 데이터 동기화 및 스타일/이벤트 일괄 세팅
        RefreshBookUI();
        SetupInputFieldEvents();
    }

    private T FindComponentInChild<T>(GameObject parent, string name) where T : Component
    {
        Transform target = parent.transform.Find(name);
        if (target != null) return target.GetComponent<T>();
        return null;
    }

    /// <summary>
    /// 저장된 이름을 인풋 필드의 text 값으로 뿌려주는 함수
    /// </summary>
    public void RefreshBookUI()
    {
        if (bookInputFields[0] != null) bookInputFields[0].text = bookData[0, 0].name;
        if (bookInputFields[1] != null) bookInputFields[1].text = bookData[0, 1].name;
        if (bookInputFields[2] != null) bookInputFields[2].text = bookData[0, 2].name;
        if (bookInputFields[3] != null) bookInputFields[3].text = bookData[0, 3].name;

        if (bookInputFields[4] != null) bookInputFields[4].text = bookData[1, 0].name;
        if (bookInputFields[5] != null) bookInputFields[5].text = bookData[1, 1].name;
        if (bookInputFields[6] != null) bookInputFields[6].text = bookData[1, 2].name;
        if (bookInputFields[7] != null) bookInputFields[7].text = bookData[1, 3].name;

        if (bookInputFields[8] != null) bookInputFields[8].text = bookData[2, 0].name;
        if (bookInputFields[9] != null) bookInputFields[9].text = bookData[2, 1].name;
        if (bookInputFields[10] != null) bookInputFields[10].text = bookData[2, 2].name;
        if (bookInputFields[11] != null) bookInputFields[11].text = bookData[2, 3].name;
    }

    /// <summary>
    /// 각 인풋 필드에 데이터 갱신 및 선택 여부에 따른 스타일 제어 이벤트를 연결합니다.
    /// </summary>
    private void SetupInputFieldEvents()
    {
        // 0번째 행 매칭
        ConfigureInputField(bookInputFields[0], 0, 0);
        ConfigureInputField(bookInputFields[1], 0, 1);
        ConfigureInputField(bookInputFields[2], 0, 2);
        ConfigureInputField(bookInputFields[3], 0, 3);

        // 1번째 행 매칭
        ConfigureInputField(bookInputFields[4], 1, 0);
        ConfigureInputField(bookInputFields[5], 1, 1);
        ConfigureInputField(bookInputFields[6], 1, 2);
        ConfigureInputField(bookInputFields[7], 1, 3);

        // 2번째 행 매칭
        ConfigureInputField(bookInputFields[8], 2, 0);
        ConfigureInputField(bookInputFields[9], 2, 1);
        ConfigureInputField(bookInputFields[10], 2, 2);
        ConfigureInputField(bookInputFields[11], 2, 3);
    }

    /// <summary>
    /// 인풋 필드 하나하나의 이벤트와 투명도 스타일을 관리하는 헬퍼 함수
    /// </summary>
    private void ConfigureInputField(TMP_InputField inputField, int row, int col)
    {
        if (inputField == null) return;

        Image bgImage = inputField.GetComponent<Image>();

        // 시작할 때는 일반 텍스트처럼 보이도록 배경 이미지를 숨깁니다.
        if (bgImage != null) bgImage.enabled = false;

        // 기존 리스너 초기화 (씬 재로드 시 중복 등록 방지)
        inputField.onSelect.RemoveAllListeners();
        inputField.onEndEdit.RemoveAllListeners();

        // 1. 유저가 클릭하여 입력을 시작했을 때 -> 테두리 배경을 보여줌
        inputField.onSelect.AddListener((currentText) =>
        {
            if (bgImage != null) bgImage.enabled = true;
        });

        // 2. 유저가 엔터를 치거나 바깥을 눌러 입력을 끝냈을 때 -> 데이터 저장 및 배경 다시 숨김
        inputField.onEndEdit.AddListener((newText) =>
        {
            if (bgImage != null) bgImage.enabled = false;
            UpdateItemName(row, col, newText);
        });
    }

    public void UpdateItemName(int row, int col, string newName)
    {
        if (!string.IsNullOrEmpty(newName))
        {
            if (row >= 0 && row < 3 && col >= 0 && col < 4)
            {
                bookData[row, col].name = newName;
                RefreshBookUI();
            }
        }
    }
}