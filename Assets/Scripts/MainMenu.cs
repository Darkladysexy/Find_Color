using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // Build index of the first playable level (see Build Settings).
    private const int k_FirstLevelBuildIndex = 2;

    public void StartGame()
    {
        SceneManager.LoadScene(k_FirstLevelBuildIndex);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
