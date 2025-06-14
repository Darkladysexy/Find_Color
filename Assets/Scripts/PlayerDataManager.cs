using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerDataManager : MonoBehaviour
{
    // Singleton Pattern: Giúp chúng ta có thể truy cập script này từ bất kỳ đâu
    public static PlayerDataManager Instance;

    [Header("Player Stats")]
    public int maxLives = 3;
    private int currentLives;

    private void Awake()
    {
        // Thiết lập Singleton và không phá hủy đối tượng này khi chuyển scene
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Bắt đầu một game mới với số mạng tối đa
            currentLives = maxLives;
        }
        else
        {
            // Nếu đã có một PlayerDataManager khác, hủy đối tượng này đi
            Destroy(gameObject);
        }
    }

    public int GetCurrentLives()
    {
        return currentLives;
    }

    // Hàm này sẽ được gọi khi người chơi chết
    public void LoseLife()
    {
        currentLives--;
        Debug.Log("Mất một mạng! Số mạng còn lại: " + currentLives);

        // TODO: Cập nhật UI hiển thị số mạng ở đây

        if (currentLives > 0)
        {
            // Nếu vẫn còn mạng, tải lại màn chơi hiện tại
            RestartCurrentLevel();
        }
        else
        {
            // Nếu hết mạng, quay về màn chơi đầu tiên
            RestartGame();
        }
    }

    private void RestartCurrentLevel()
    {
        // Lấy build index của scene hiện tại và tải lại nó
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex);
    }

    private void RestartGame()
    {
        Debug.Log("Hết mạng! Trở về màn chơi đầu tiên.");
        // Reset lại số mạng cho lần chơi tiếp theo
        currentLives = maxLives;
        
        // Tải lại màn chơi đầu tiên (build index là 0)
        // Hãy chắc chắn màn đầu tiên của bạn có build index là 0
        SceneManager.LoadScene(2);
    }
}