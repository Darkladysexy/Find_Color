using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public enum LevelType { Red, Orange, Yellow }

    [Header("Level Configuration")]
    public LevelType currentLevelType;
    public int nextSceneBuildIndex;

    [Header("Object References")]
    public Tilemap wallTilemap;
    public Tilemap floorTilemap;
    public GameObject yellowTrailPrefab;

    [Header("Red/Orange Level Objects")]
    public Transform goalSpawnPoint;
    public GameObject redGoalPrefab;
    public GameObject orangeGoalPrefab;

    private GameObject player;
    private bool isLevelCompleted = false;
    private bool areSwitchesActivated = false;
    private HashSet<Vector3Int> paintedTiles;
    private int totalFloorTiles = 0;
    private Vector3Int playerCellPosition;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        if (currentLevelType == LevelType.Yellow)
        {
            InitializeYellowLevel();
        }
    }

    public void AttemptMove(Vector2 direction)
    {
        if (isLevelCompleted) return;

        switch (currentLevelType)
        {
            case LevelType.Red:
            case LevelType.Orange:
                AttemptPushMove(direction);
                break;
            case LevelType.Yellow:
                AttemptPaintMove(direction);
                break;
        }
    }

    public void CheckConditions()
    {
        if (isLevelCompleted) return;
        switch (currentLevelType)
        {
            case LevelType.Red: case LevelType.Orange: CheckRedOrangeWin(); break;
            case LevelType.Yellow: CheckYellowWin(); break;
        }
    }

    #region Yellow_Level_Logic

    private void InitializeYellowLevel()
    {
        if (floorTilemap == null || yellowTrailPrefab == null)
        {
            Debug.LogError("Cần gán Floor Tilemap và Yellow Trail Prefab cho GameManager!");
            return;
        }

        paintedTiles = new HashSet<Vector3Int>();
        
        // <<< SỬA LỖI ĐẾM TILE >>>
        // Chúng ta sẽ lấy danh sách các tile có thật thay vì duyệt cả một vùng chữ nhật
        totalFloorTiles = floorTilemap.GetUsedTilesCount(); 
        
        // Dòng Debug này giờ sẽ luôn cho ra số ô chính xác bạn đã vẽ
        Debug.Log("Initialization: Tổng số ô sàn cần tô: " + totalFloorTiles);

        playerCellPosition = floorTilemap.WorldToCell(player.transform.position);
        player.transform.position = floorTilemap.GetCellCenterWorld(playerCellPosition);
        
        PaintTile(playerCellPosition);
    }

    private void AttemptPaintMove(Vector2 direction)
    {
        Vector3Int targetCell = playerCellPosition + new Vector3Int((int)direction.x, (int)direction.y, 0);

        // --- Kiểm tra di chuyển ---
        // 1. Ô đích phải là sàn
        if (!floorTilemap.HasTile(targetCell))
        {
            return;
        }
        
        // 2. Ô đích không được có tường (kiểm tra lại cho chắc)
        if (wallTilemap.HasTile(targetCell))
        {
            return;
        }
        
        // 3. Ô đích chưa được tô
        if (paintedTiles.Contains(targetCell))
        {
            return;
        }

        // --- Thực hiện di chuyển ---
        playerCellPosition = targetCell;
        player.transform.position = floorTilemap.GetCellCenterWorld(playerCellPosition);

        PaintTile(targetCell);
        CheckConditions();
    }

    private void PaintTile(Vector3Int cell)
    {
        if (!paintedTiles.Contains(cell))
        {
            paintedTiles.Add(cell);
            Instantiate(yellowTrailPrefab, floorTilemap.GetCellCenterWorld(cell), Quaternion.identity);
        }
    }

    private void CheckYellowWin()
    {
        if (totalFloorTiles > 0 && paintedTiles.Count == totalFloorTiles)
        {
            isLevelCompleted = true;
            Debug.Log("BẠN ĐÃ TÔ MÀU HẾT CÁC Ô! BẠN THẮNG!");
            CompleteLevel();
        }
    }

    #endregion

    public void CompleteLevel()
    {
        if (!isLevelCompleted) isLevelCompleted = true;
        
        if (nextSceneBuildIndex > 0 && nextSceneBuildIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneBuildIndex);
        }
        else
        {
            Debug.Log("Đây là màn cuối! Hoặc bạn chưa cài đặt 'Next Scene Build Index'.");
        }
    }
    
    #region Other Levels and Helpers (Unchanged)
    private void AttemptPushMove(Vector2 direction) { Vector2 currentPos = player.transform.position; Vector2 targetPos = currentPos + direction; if (IsWallAt(targetPos)) return; Collider2D blockCollider = GetPushableObjectAt(targetPos); if (blockCollider != null) { if (blockCollider.CompareTag("RedBlock")) { Vector2 posAfterBlock = (Vector2)blockCollider.transform.position + direction; if (IsPositionFree(posAfterBlock, null)) { blockCollider.transform.position = posAfterBlock; player.transform.position = targetPos; } } else if (blockCollider.CompareTag("OrangeBlock")) { List<GameObject> connectedBlocks = FindConnectedBlocks(blockCollider.gameObject); if (CanClusterMove(connectedBlocks, direction)) { MoveCluster(connectedBlocks, direction); player.transform.position = targetPos; } } } else { player.transform.position = targetPos; } }
    private void CheckRedOrangeWin() { if (areSwitchesActivated) return; foreach (var s in FindObjectsOfType<SwitchController>()) { if (!s.isActivated) return; } Debug.Log("Tất cả công tắc đã được kích hoạt! Mục tiêu xuất hiện!"); areSwitchesActivated = true; SpawnGoal(); }
    private void SpawnGoal() { GameObject goalToSpawn = null; if (currentLevelType == LevelType.Red) goalToSpawn = redGoalPrefab; else if (currentLevelType == LevelType.Orange) goalToSpawn = orangeGoalPrefab; if (goalToSpawn != null && goalSpawnPoint != null) { Instantiate(goalToSpawn, goalSpawnPoint.position, Quaternion.identity); } }
    private bool IsWallAt(Vector2 position) { return wallTilemap.HasTile(wallTilemap.WorldToCell(position)); }
    private Collider2D GetPushableObjectAt(Vector2 position) { Collider2D[] colliders = Physics2D.OverlapPointAll(position); foreach (var col in colliders) { if (col.gameObject.CompareTag("RedBlock") || col.gameObject.CompareTag("OrangeBlock")) { return col; } } return null; }
    private bool IsPositionFree(Vector2 position, List<GameObject> clusterToIgnore) { if (IsWallAt(position)) return false; Collider2D[] colliders = Physics2D.OverlapPointAll(position); foreach (var col in colliders) { if (clusterToIgnore != null && clusterToIgnore.Contains(col.gameObject)) continue; if (col.CompareTag("RedBlock") || col.CompareTag("OrangeBlock")) return false; } return true; }
    private List<GameObject> FindConnectedBlocks(GameObject startBlock) { List<GameObject> connectedCluster = new List<GameObject>(); Queue<GameObject> queue = new Queue<GameObject>(); queue.Enqueue(startBlock); connectedCluster.Add(startBlock); while (queue.Count > 0) { GameObject currentBlock = queue.Dequeue(); Vector2[] directions = { Vector2.up, Vector2.down, Vector2.left, Vector2.right }; foreach (var dir in directions) { CheckNeighbor((Vector2)currentBlock.transform.position + dir, queue, connectedCluster); } } return connectedCluster; }
    private void CheckNeighbor(Vector2 position, Queue<GameObject> queue, List<GameObject> cluster) { Collider2D[] colliders = Physics2D.OverlapPointAll(position); foreach (var col in colliders) { if (col.CompareTag("OrangeBlock") && !cluster.Contains(col.gameObject)) { cluster.Add(col.gameObject); queue.Enqueue(col.gameObject); } } }
    private bool CanClusterMove(List<GameObject> cluster, Vector2 direction) { foreach (var block in cluster) { Vector2 newPos = (Vector2)block.transform.position + direction; if (!IsPositionFree(newPos, cluster)) return false; } return true; }
    private void MoveCluster(List<GameObject> cluster, Vector2 direction) { foreach (var block in cluster) { block.transform.position += (Vector3)direction; } }
    #endregion
}