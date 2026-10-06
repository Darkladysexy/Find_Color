using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    public static Menu instant;
    public bool isPaused = false;

    private const string k_CanvasName = "Canvas";
    private const string k_MenuChildName = "Menu";
    private const string k_MainMenuScene = "MainMenu";

    private GameObject m_menuPanel;

    private void Awake()
    {
        instant = this;
        CacheMenuPanel();
    }

    public void Resume()
    {
        SetMenuVisible(false);
        Time.timeScale = 1f;
        isPaused = false;
    }

    public void PauseGame()
    {
        SetMenuVisible(true);
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        Time.timeScale = 1f;
        isPaused = false;
    }

    public void ExitToMainMenu()
    {
        SceneManager.LoadScene(k_MainMenuScene);
    }

    private void SetMenuVisible(bool visible)
    {
        if (m_menuPanel != null)
        {
            m_menuPanel.SetActive(visible);
        }
    }

    // The Canvas/Menu hierarchy lookup happens once instead of on every button click.
    private void CacheMenuPanel()
    {
        GameObject canvas = GameObject.Find(k_CanvasName);
        if (canvas == null)
        {
            return;
        }

        Transform menuTransform = canvas.transform.Find(k_MenuChildName);
        if (menuTransform != null)
        {
            m_menuPanel = menuTransform.gameObject;
        }
    }
}
