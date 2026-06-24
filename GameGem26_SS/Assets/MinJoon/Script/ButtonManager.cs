using UnityEngine;
using UnityEngine.SceneManagement; // ◀ 씬 전환을 위해 필수!

public class MoveSceneButton : MonoBehaviour
{
    [Header("이동할 다른 장소 씬 이름")]
    public string targetSceneName;

    // 버튼을 누르면 실행될 함수
    public void ChangeScene()
    {
        if (!string.IsNullOrEmpty(targetSceneName))
        {
            SceneManager.LoadScene(targetSceneName);
        }
        else
        {
            Debug.LogError("이동할 씬 이름이 비어있습니다!");
        }
    }
}