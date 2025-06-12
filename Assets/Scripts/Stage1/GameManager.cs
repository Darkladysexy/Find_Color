using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    public Tilemap wallTilemap;
    
    private GameObject player;
    private bool isLevelComplete = false;

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
        if (isLevelComplete) return;

        Vector2 currentPos = player.transform.position;
        Vector2 targetPos = currentPos + direction;

        if (IsWallAt(targetPos)) return;

        Collider2D blockCollider = GetPushableObjectAt(targetPos);
        
        if (blockCollider != null)
        {
            if (blockCollider.CompareTag("RedBlock"))
            {
                Vector2 posAfterBlock = (Vector2)blockCollider.transform.position + direction;
                if (IsPositionFree(posAfterBlock))
                {
                    blockCollider.transform.position = posAfterBlock;
                    player.transform.position = targetPos;
                    CheckWinCondition();
                }
            }
            else if (blockCollider.CompareTag("OrangeBlock"))
            {
                List<GameObject> connectedBlocks = FindConnectedBlocks(blockCollider.gameObject);
                
                // Thay đổi logic kiểm tra ở hàm CanClusterMove
                if (CanClusterMove(connectedBlocks, direction))
                {
                    MoveCluster(connectedBlocks, direction);
                    player.transform.position = targetPos;
                    CheckWinCondition();
                }
            }
        }
        else 
        {
            player.transform.position = targetPos;
        }
    }

    private List<GameObject> FindConnectedBlocks(GameObject startBlock)
    {
        List<GameObject> connectedCluster = new List<GameObject>();
        Queue<GameObject> queue = new Queue<GameObject>();

        queue.Enqueue(startBlock);
        connectedCluster.Add(startBlock);

        while (queue.Count > 0)
        {
            GameObject currentBlock = queue.Dequeue();
            Vector2 currentPos = currentBlock.transform.position;

            CheckNeighbor(currentPos + Vector2.up, queue, connectedCluster);
            CheckNeighbor(currentPos + Vector2.down, queue, connectedCluster);
            CheckNeighbor(currentPos + Vector2.left, queue, connectedCluster);
            CheckNeighbor(currentPos + Vector2.right, queue, connectedCluster);
        }
        
        return connectedCluster;
    }

    private void CheckNeighbor(Vector2 position, Queue<GameObject> queue, List<GameObject> cluster)
    {
        Collider2D[] colliders = Physics2D.OverlapPointAll(position);
        foreach (var col in colliders)
        {
            if (col.CompareTag("OrangeBlock") && !cluster.Contains(col.gameObject))
            {
                cluster.Add(col.gameObject);
                queue.Enqueue(col.gameObject);
            }
        }
    }

    /// <summary>
    /// --- LOGIC ĐÃ SỬA LỖI ---
    /// Kiểm tra xem cụm khối có thể di chuyển không.
    /// Chỉ coi tường và các khối khác (không thuộc cụm) là vật cản.
    /// </summary>
    private bool CanClusterMove(List<GameObject> cluster, Vector2 direction)
    {
        foreach (var block in cluster)
        {
            Vector2 newPos = (Vector2)block.transform.position + direction;

            // 1. Kiểm tra tường
            if (IsWallAt(newPos)) return false;

            // 2. Kiểm tra các vật thể khác tại vị trí mới
            Collider2D[] collidersAtNewPos = Physics2D.OverlapPointAll(newPos);
            foreach (var col in collidersAtNewPos)
            {
                // Nếu vật thể đó là một khối đẩy (Đỏ hoặc Cam) VÀ nó không nằm trong cụm ta đang xét
                // thì đó là vật cản.
                if ((col.CompareTag("RedBlock") || col.CompareTag("OrangeBlock")) && !cluster.Contains(col.gameObject))
                {
                    return false;
                }
            }
        }
        
        // Nếu không có khối nào trong cụm bị cản, cho phép di chuyển
        return true;
    }

    private void MoveCluster(List<GameObject> cluster, Vector2 direction)
    {
        foreach (var block in cluster)
        {
            block.transform.position += (Vector3)direction;
        }
    }
    
    public void CheckWinCondition()
    {
        SwitchController[] switches = FindObjectsOfType<SwitchController>();
        foreach (var s in switches)
        {
            if (!s.isActivated) return;
        }

        isLevelComplete = true;
        Debug.Log("BẠN ĐÃ THẮNG!");
    }

    private bool IsWallAt(Vector2 position)
    {
        Vector3Int cellPosition = wallTilemap.WorldToCell(position);
        return wallTilemap.HasTile(cellPosition);
    }

    private Collider2D GetPushableObjectAt(Vector2 position)
    {
        Collider2D[] colliders = Physics2D.OverlapPointAll(position);
        foreach (var col in colliders)
        {
            if (col.gameObject.CompareTag("RedBlock") || col.gameObject.CompareTag("OrangeBlock"))
            {
                return col;
            }
        }
        return null;
    }
    
    private bool IsPositionFree(Vector2 position)
    {
        return !IsWallAt(position) && GetPushableObjectAt(position) == null;
    }
}