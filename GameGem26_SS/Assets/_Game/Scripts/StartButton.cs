using UnityEngine;
using UnityEngine.SceneManagement;

public class StartButton : MonoBehaviour
{
    [SerializeField] private string sceneToLoad = "BrotherRoom";

    public void OnStartClicked()
    {
        SceneManager.LoadScene(sceneToLoad);
    }
}