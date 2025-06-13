using UnityEngine;
using UnityEngine.SceneManagement;

public class GreenLevelManager : MonoBehaviour
{
    // Singleton pattern để các script khác dễ dàng truy cập
    public static GreenLevelManager Instance;

    [Header("Cấu hình màn chơi")]
    public int startingSeeds = 0; // Số hạt mầm có sẵn khi bắt đầu
    public int nextSceneBuildIndex; // Màn tiếp theo để tải

    [Header("Prefabs")]
    public GameObject greenBlockPrefab; // Prefab của khối Lục

    // --- Biến nội bộ ---
    private int currentSeedCount;
    private bool isLevelCompleted = false;
    private GameObject player;

    private void Awake()
    {
        // Khởi tạo Singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("LỖI: Không tìm thấy GameObject có tag 'Player' trong Scene!");
            return;
        }

        currentSeedCount = startingSeeds;
        Debug.Log("Màn Lục bắt đầu với " + currentSeedCount + " hạt mầm.");
        // TODO: Cập nhật UI hiển thị số hạt mầm tại đây
    }

    private void Update()
    {
        if (isLevelCompleted) return;

        // Nhấn phím 'C' để trồng cây
        if (Input.GetKeyDown(KeyCode.C))
        {
            TryPlantBlock();
        }
    }

    // Hàm này được gọi bởi các đối tượng Seed
    public void CollectSeed(int amount)
    {
        currentSeedCount += amount;
        Debug.Log("Hạt mầm hiện tại: " + currentSeedCount);
        // TODO: Cập nhật UI
    }

    // Hàm xử lý việc trồng cây
    private void TryPlantBlock()
    {
        if (currentSeedCount > 0)
        {
            currentSeedCount--;
            Debug.Log("Đã trồng! Hạt mầm còn lại: " + currentSeedCount);
            // TODO: Cập nhật UI

            // Lấy hướng nhìn của người chơi một cách an toàn
            // Dựa vào việc sprite có bị lật hay không (flipX)
            SpriteRenderer playerSprite = player.GetComponent<SpriteRenderer>();
            float xOffset = playerSprite.flipX ? -1.5f : 1.5f; // Nếu lật (nhìn sang trái) thì offset âm
            
            // Vị trí trồng cây sẽ ở phía trước và hơi cao hơn người chơi
            Vector3 plantPosition = player.transform.position + new Vector3(xOffset, 0.5f, 0);

            if (greenBlockPrefab != null)
            {
                Instantiate(greenBlockPrefab, plantPosition, Quaternion.identity);
            }
            else
            {
                Debug.LogError("Chưa gán Green Block Prefab vào GreenLevelManager!");
            }
        }
        else
        {
            Debug.Log("Không có hạt mầm để trồng!");
        }
    }

    // Hàm được gọi bởi FinishLine
    public void CompleteLevel()
    {
        if (isLevelCompleted) return;
        isLevelCompleted = true; 
        
        Debug.Log("ĐÃ VỀ ĐÍCH! Chuẩn bị qua màn...");
        
        // Tải màn chơi tiếp theo nếu có
        if (nextSceneBuildIndex > 0)
        {
            SceneManager.LoadScene(nextSceneBuildIndex);
        }
    }
}