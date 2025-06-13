using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
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
    
    [Header("Prefabs")]
    public GameObject redGoalPrefab;
    public GameObject orangeGoalPrefab;
    public GameObject yellowGoalPrefab;
    public GameObject yellowTrailPrefab; 

    // Biến không còn dùng đến
    [Header("Legacy (No longer used)")]
    public Transform goalSpawnPoint; 

    // --- Private State Variables ---
    private GameObject player;
    private bool isLevelCompleted = false; 
    
    private SwitchController[] allSwitches; 
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
        else 
        {
            InitializeRedOrangeLevel();
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
            case LevelType.Red:
            case LevelType.Orange:
                CheckRedOrangeWin();
                break;
            case LevelType.Yellow:
                CheckYellowWin();
                break;
        }
    }
    
    private IEnumerator WinSequence()
    {
        isLevelCompleted = true; 
        
        SpawnGoal(); 
        
        yield return new WaitForSeconds(1.5f); 
        
        Debug.Log("LEVEL COMPLETE! Loading next scene...");
        if (nextSceneBuildIndex > 0 && nextSceneBuildIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneBuildIndex);
        }
        else
        {
            Debug.Log("Đây là màn cuối! Hoặc bạn chưa cài đặt 'Next Scene Build Index'.");
        }
    }
    
    private void SpawnGoal()
    {
        GameObject goalToSpawn = null;
        switch (currentLevelType)
        {
            case LevelType.Red:    goalToSpawn = redGoalPrefab;    break;
            case LevelType.Orange: goalToSpawn = orangeGoalPrefab; break;
            case LevelType.Yellow: goalToSpawn = yellowGoalPrefab; break;
        }

        if (goalToSpawn != null && player != null)
        {
            Instantiate(goalToSpawn, player.transform.position, Quaternion.identity);
        }
    }

    #region Yellow_Level_Logic

    private void InitializeYellowLevel()
    {
        if (floorTilemap == null) { Debug.LogError("Chưa gán Floor Tilemap!"); this.enabled = false; return; }
        
        paintedTiles = new HashSet<Vector3Int>();
        totalFloorTiles = 0;

        // <<< SỬA LỖI ĐẾM TILE CUỐI CÙNG >>>
        // Dùng vòng lặp `foreach` để đếm thủ công. Đây là cách đáng tin cậy nhất.
        floorTilemap.CompressBounds();
        foreach (Vector3Int position in floorTilemap.cellBounds.allPositionsWithin)
        {
            if (floorTilemap.HasTile(position))
            {
                totalFloorTiles++;
            }
        }
        
        Debug.Log("Tổng số ô sàn đã đếm được: " + totalFloorTiles);

        playerCellPosition = floorTilemap.WorldToCell(player.transform.position);
        player.transform.position = floorTilemap.GetCellCenterWorld(playerCellPosition);
        
        PaintTile(playerCellPosition);
        CheckConditions();
    }

    private void AttemptPaintMove(Vector2 direction)
    {
        Vector3Int targetCell = playerCellPosition + new Vector3Int((int)direction.x, (int)direction.y, 0);

        if (floorTilemap.HasTile(targetCell) && !paintedTiles.Contains(targetCell))
        {
            playerCellPosition = targetCell;
            player.transform.position = floorTilemap.GetCellCenterWorld(playerCellPosition);
            PaintTile(targetCell);
            CheckConditions();
        }
    }

    private void PaintTile(Vector3Int cell)
    {
        if (!paintedTiles.Contains(cell) && yellowTrailPrefab != null)
        {
            paintedTiles.Add(cell);
            Instantiate(yellowTrailPrefab, floorTilemap.GetCellCenterWorld(cell), Quaternion.identity);
        }
    }

    private void CheckYellowWin()
    {
        if (totalFloorTiles > 0 && paintedTiles.Count == totalFloorTiles)
        {
            Debug.Log("BẠN ĐÃ TÔ MÀU HẾT CÁC Ô! Bắt đầu chuỗi hành động thắng!");
            StartCoroutine(WinSequence());
        }
    }

    #endregion

    #region Red_Orange_Logic_And_Helpers

    private void InitializeRedOrangeLevel()
    {
        allSwitches = FindObjectsOfType<SwitchController>();
    }

    private void AttemptPushMove(Vector2 direction)
    {
        Vector2 currentPos = player.transform.position; Vector2 targetPos = currentPos + direction;
        if (IsWallAt(targetPos)) return; Collider2D blockCollider = GetPushableObjectAt(targetPos);
        if (blockCollider != null)
        {
            if (blockCollider.CompareTag("RedBlock"))
            {
                Vector2 posAfterBlock = (Vector2)blockCollider.transform.position + direction;
                if (IsPositionFree(posAfterBlock, null))
                {
                    blockCollider.transform.position = posAfterBlock;
                    player.transform.position = targetPos;
                    FindAnyObjectByType<AudioManager>().Play("Pushed");

                }
            }
            else if (blockCollider.CompareTag("OrangeBlock"))
            {
                List<GameObject> connectedBlocks = FindConnectedBlocks(blockCollider.gameObject);
                if (CanClusterMove(connectedBlocks, direction))
                {
                    FindAnyObjectByType<AudioManager>().Play("Pushed");
                    MoveCluster(connectedBlocks, direction); player.transform.position = targetPos;
                }
            }
        }
        else { player.transform.position = targetPos; }
    }
    private void CheckRedOrangeWin() { foreach (var s in allSwitches) { if (!s.isActivated) return; } Debug.Log("Tất cả công tắc đã được kích hoạt! Bắt đầu chuỗi hành động thắng!"); if (!isLevelCompleted) StartCoroutine(WinSequence()); }
    private bool IsWallAt(Vector2 position) { return wallTilemap.HasTile(wallTilemap.WorldToCell(position)); }
    private Collider2D GetPushableObjectAt(Vector2 position) { Collider2D[] colliders = Physics2D.OverlapPointAll(position); foreach (var col in colliders) { if (col.gameObject.CompareTag("RedBlock") || col.gameObject.CompareTag("OrangeBlock")) { return col; } } return null; }
    private bool IsPositionFree(Vector2 position, List<GameObject> clusterToIgnore) { if (IsWallAt(position)) return false; Collider2D[] colliders = Physics2D.OverlapPointAll(position); foreach (var col in colliders) { if (clusterToIgnore != null && clusterToIgnore.Contains(col.gameObject)) continue; if (col.CompareTag("RedBlock") || col.CompareTag("OrangeBlock")) return false; } return true; }
    private List<GameObject> FindConnectedBlocks(GameObject startBlock) { List<GameObject> connectedCluster = new List<GameObject>(); Queue<GameObject> queue = new Queue<GameObject>(); queue.Enqueue(startBlock); connectedCluster.Add(startBlock); while (queue.Count > 0) { GameObject currentBlock = queue.Dequeue(); Vector2[] directions = { Vector2.up, Vector2.down, Vector2.left, Vector2.right }; foreach (var dir in directions) { CheckNeighbor((Vector2)currentBlock.transform.position + dir, queue, connectedCluster); } } return connectedCluster; }
    private void CheckNeighbor(Vector2 position, Queue<GameObject> queue, List<GameObject> cluster) { Collider2D[] colliders = Physics2D.OverlapPointAll(position); foreach (var col in colliders) { if (col.CompareTag("OrangeBlock") && !cluster.Contains(col.gameObject)) { cluster.Add(col.gameObject); queue.Enqueue(col.gameObject); } } }
    private bool CanClusterMove(List<GameObject> cluster, Vector2 direction) { foreach (var block in cluster) { Vector2 newPos = (Vector2)block.transform.position + direction; if (!IsPositionFree(newPos, cluster)) return false; } return true; }
    private void MoveCluster(List<GameObject> cluster, Vector2 direction) { foreach (var block in cluster) { block.transform.position += (Vector3)direction; } }
    
    #endregion
}