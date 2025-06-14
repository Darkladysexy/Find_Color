using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps; // Thêm thư viện Tilemap

public class GreenLevelManager : MonoBehaviour
{
    // Singleton pattern để các script khác dễ dàng truy cập
    public static GreenLevelManager Instance;

    [Header("Cấu hình màn chơi")]
    public int startingSeeds = 0; // Số hạt mầm có sẵn khi bắt đầu
    public int nextSceneBuildIndex; // Màn tiếp theo để tải

    [Header("Tham chiếu trong Scene")]
    public GameObject player;
    public Tilemap wallTilemap; // Tham chiếu đến Tilemap của tường

    [Header("Prefabs")]
    public GameObject greenBlockPrefab; // Prefab của khối Lục

    // --- Biến nội bộ ---
    private int currentSeedCount;
    private bool isLevelCompleted = false;

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
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }
        if (player == null)
        {
            Debug.LogError("LỖI: Không tìm thấy GameObject có tag 'Player' trong Scene!");
            this.enabled = false; // Tắt script này nếu không có Player
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
        if (currentSeedCount <= 0)
        {
            Debug.Log("Không có hạt mầm!");
            return;
        }

        // 1. Xác định vị trí muốn trồng
        SpriteRenderer playerSprite = player.GetComponent<SpriteRenderer>();
        if (playerSprite == null)
        {
            Debug.LogError("Player không có SpriteRenderer!");
            return;
        }

        // Lấy hướng nhìn của người chơi dựa vào việc sprite có bị lật hay không
        float xOffset = playerSprite.flipX ? -1.2f : 1.2f; // Nếu lật (nhìn sang trái) thì offset âm
        Vector3 potentialPlantPosition = player.transform.position + new Vector3(xOffset, 0.5f, 0);

        // 2. Kiểm tra xem vị trí đó có hợp lệ không
        if (wallTilemap != null)
        {
            Vector3Int cellToPlantIn = wallTilemap.WorldToCell(potentialPlantPosition);
            if (wallTilemap.HasTile(cellToPlantIn))
            {
                Debug.Log("Không thể trồng cây trong tường!");
                return; // Dừng lại nếu có tường
            }
        }
        
        // 3. Nếu vị trí hợp lệ, tiến hành trồng cây
        currentSeedCount--;
        Debug.Log("Đã trồng! Hạt mầm còn lại: " + currentSeedCount);
        // TODO: Cập nhật UI

        if (greenBlockPrefab != null)
        {
            Instantiate(greenBlockPrefab, potentialPlantPosition, Quaternion.identity);
        }
        else
        {
            Debug.LogError("Chưa gán Green Block Prefab vào GreenLevelManager!");
        }
    }

    // Hàm được gọi bởi FinishLine
    public void CompleteLevel()
    {
        if (isLevelCompleted) return;
        isLevelCompleted = true; 
        
        Debug.Log("ĐÃ VỀ ĐÍCH! Chuẩn bị qua màn...");
        
        if (nextSceneBuildIndex > 0)
        {
            SceneManager.LoadScene(nextSceneBuildIndex);
        }
    }
}