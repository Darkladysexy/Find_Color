using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public enum LevelType { Red, Orange }

    [Header("Level Configuration")]
    public LevelType currentLevelType;
    public int nextSceneBuildIndex;

    [Header("Object References")]
    public Tilemap wallTilemap;
    public Transform goalSpawnPoint;
    public GameObject redGoalPrefab;
    public GameObject orangeGoalPrefab;

    private GameObject player;
    private bool areSwitchesActivated = false;
    private bool isLevelCompleted = false;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    public void AttemptMove(Vector2 direction)
    {
        if (isLevelCompleted) return;

        Vector2 currentPos = player.transform.position;
        Vector2 targetPos = currentPos + direction;

        if (IsWallAt(targetPos)) return;

        Collider2D blockCollider = GetPushableObjectAt(targetPos);
        if (blockCollider != null)
        {
            if (blockCollider.CompareTag("RedBlock"))
            {
                PushRedBlock(blockCollider, direction, targetPos);
            }
            else if (blockCollider.CompareTag("OrangeBlock"))
            {
                PushOrangeCluster(blockCollider, direction, targetPos);
            }
        }
        else
        {
            player.transform.position = targetPos;
        }
    }

    private void PushRedBlock(Collider2D blockCollider, Vector2 direction, Vector2 playerTargetPos)
    {
        Vector2 posAfterBlock = (Vector2)blockCollider.transform.position + direction;
        if (IsPositionFree(posAfterBlock))
        {
            blockCollider.transform.position = posAfterBlock;
            player.transform.position = playerTargetPos;
            // XÓA LỆNH GỌI TẠI ĐÂY
        }
    }

    private void PushOrangeCluster(Collider2D blockCollider, Vector2 direction, Vector2 playerTargetPos)
    {
        List<GameObject> connectedBlocks = FindConnectedBlocks(blockCollider.gameObject);
        if (CanClusterMove(connectedBlocks, direction))
        {
            MoveCluster(connectedBlocks, direction);
            player.transform.position = playerTargetPos;
            // XÓA LỆNH GỌI TẠI ĐÂY
        }
    }

    public void CheckWinCondition()
    {
        if (areSwitchesActivated) return;

        SwitchController[] switches = FindObjectsOfType<SwitchController>();
        foreach (var s in switches)
        {
            if (!s.isActivated) return;
        }

        Debug.Log("Tất cả công tắc đã được kích hoạt! Mục tiêu xuất hiện!");
        areSwitchesActivated = true;
        SpawnGoal();
    }
    
    private void SpawnGoal()
    {
        GameObject goalToSpawn = (currentLevelType == LevelType.Red) ? redGoalPrefab : orangeGoalPrefab;

        if (goalToSpawn != null && goalSpawnPoint != null)
        {
            Instantiate(goalToSpawn, goalSpawnPoint.position, Quaternion.identity);
        }
        else
        {
            Debug.LogError("Chưa thiết lập Goal Prefab hoặc Goal Spawn Point trong GameManager!");
        }
    }

    public void CompleteLevel()
    {
        if (isLevelCompleted) return;

        isLevelCompleted = true;
        Debug.Log("BẠN ĐÃ QUA MÀN!");
        
        if (nextSceneBuildIndex > 0 && nextSceneBuildIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneBuildIndex);
        }
        else
        {
            Debug.Log("Đây là màn cuối! Hoặc bạn chưa cài đặt 'Next Scene Build Index'.");
        }
    }

    // --- CÁC HÀM HỖ TRỢ (GIỮ NGUYÊN) ---

    private bool IsWallAt(Vector2 position) { /* ... */ return wallTilemap.HasTile(wallTilemap.WorldToCell(position)); }
    private Collider2D GetPushableObjectAt(Vector2 position) { /* ... */ Collider2D[] colliders = Physics2D.OverlapPointAll(position); foreach (var col in colliders) { if (col.gameObject.CompareTag("RedBlock") || col.gameObject.CompareTag("OrangeBlock")) { return col; } } return null; }
    private bool IsPositionFree(Vector2 position) { /* ... */ return !IsWallAt(position) && GetPushableObjectAt(position) == null; }
    private List<GameObject> FindConnectedBlocks(GameObject startBlock) { /* ... */ List<GameObject> connectedCluster = new List<GameObject>(); Queue<GameObject> queue = new Queue<GameObject>(); queue.Enqueue(startBlock); connectedCluster.Add(startBlock); while (queue.Count > 0) { GameObject currentBlock = queue.Dequeue(); Vector2 currentPos = currentBlock.transform.position; CheckNeighbor(currentPos + Vector2.up, queue, connectedCluster); CheckNeighbor(currentPos + Vector2.down, queue, connectedCluster); CheckNeighbor(currentPos + Vector2.left, queue, connectedCluster); CheckNeighbor(currentPos + Vector2.right, queue, connectedCluster); } return connectedCluster; }
    private void CheckNeighbor(Vector2 position, Queue<GameObject> queue, List<GameObject> cluster) { /* ... */ Collider2D[] colliders = Physics2D.OverlapPointAll(position); foreach (var col in colliders) { if (col.CompareTag("OrangeBlock") && !cluster.Contains(col.gameObject)) { cluster.Add(col.gameObject); queue.Enqueue(col.gameObject); } } }
    private bool CanClusterMove(List<GameObject> cluster, Vector2 direction) { /* ... */ foreach (var block in cluster) { Vector2 newPos = (Vector2)block.transform.position + direction; if (IsWallAt(newPos)) return false; Collider2D[] collidersAtNewPos = Physics2D.OverlapPointAll(newPos); foreach (var col in collidersAtNewPos) { if ((col.CompareTag("RedBlock") || col.CompareTag("OrangeBlock")) && !cluster.Contains(col.gameObject)) { return false; } } } return true; }
    private void MoveCluster(List<GameObject> cluster, Vector2 direction) { /* ... */ foreach (var block in cluster) { block.transform.position += (Vector3)direction; } }
}