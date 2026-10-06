using UnityEngine;
using UnityEngine.SceneManagement;

public class TranformScene : MonoBehaviour
{
    private const string k_MainMenuScene = "MainMenu";

    private void Start()
    {
        SceneManager.LoadScene(k_MainMenuScene);
    }
}
