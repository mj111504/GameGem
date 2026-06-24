using UnityEngine;
using UnityEngine.SceneManagement;

public enum TimePhase
{
    Morning,
    Noon,
    Night
}

public class GameFlowManager : MonoBehaviour
{
    public static GameFlowManager Instance { get; private set; }

    [Header("Progress")]
    [SerializeField] private int currentDay = 1;
    [SerializeField] private TimePhase currentPhase = TimePhase.Morning;
    [SerializeField] private int stability = 50;

    [Header("Scene Names")]
    [SerializeField] private string livingRoomScene = "LivingRoom";
    [SerializeField] private string sisterRoomScene = "SisterRoom";
    [SerializeField] private string brotherRoomScene = "BrotherRoom";
    [SerializeField] private string night = "Night";

    public int CurrentDay => currentDay;
    public TimePhase CurrentPhase => currentPhase;
    public int Stability => stability;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void AddStability(int amount)
    {
        stability = Mathf.Clamp(stability + amount, 0, 100);
    }

    public void EnterSisterRoom()
    {
        currentPhase = TimePhase.Noon;
        LoadScene(sisterRoomScene);
    }

    public void FinishNoonDialogue()
    {
        currentPhase = TimePhase.Night;
        LoadScene(night);
    }

    public void FinishNightDeduction()
    {
        if (currentDay >= 7)
        {
            LoadEndingByStability();
            return;
        }

        currentDay++;
        currentPhase = TimePhase.Morning;
        LoadScene(livingRoomScene);
    }

    public void MoveToScene(string sceneName)
    {
        if (!string.IsNullOrWhiteSpace(sceneName))
        {
            LoadScene(sceneName);
        }
    }

    private void LoadScene(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName)) return;
        SceneManager.LoadScene(sceneName);
    }

    private void LoadEndingByStability()
    {
        Debug.Log($"Ending branch by stability: {stability}");
    }
}
