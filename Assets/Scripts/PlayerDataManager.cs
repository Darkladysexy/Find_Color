using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerDataManager : MonoBehaviour
{
    public static PlayerDataManager Instance;

    [Header("Player Stats")]
    [Tooltip("Lives granted at the start of a new game.")]
    public int maxLives = 3;

    // Build index of the first playable level (see Build Settings).
    private const int k_FirstLevelBuildIndex = 2;

    private int m_currentLives;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            m_currentLives = maxLives;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public int GetCurrentLives()
    {
        return m_currentLives;
    }

    public void LoseLife()
    {
        m_currentLives--;

        if (m_currentLives > 0)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        else
        {
            m_currentLives = maxLives;
            SceneManager.LoadScene(k_FirstLevelBuildIndex);
        }
    }
}
