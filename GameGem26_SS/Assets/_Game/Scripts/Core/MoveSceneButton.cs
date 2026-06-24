using UnityEngine;

public class MoveSceneButton : MonoBehaviour
{
    [Header("이동할 다른 장소 씬 이름")]
    public string targetSceneName;

    // 버튼을 누르면 실행될 함수
    public void ChangeScene()
    {
        if (!string.IsNullOrEmpty(targetSceneName))
        {
            if (GameFlowManager.Instance != null)
            {
                GameFlowManager.Instance.MoveToScene(targetSceneName);
            }
            else
            {
                Debug.LogError("GameFlowManager가 씬에 없습니다!");
            }
        }
        else
        {
            Debug.LogError("이동할 씬 이름이 비어있습니다!");
        }
    }
}
