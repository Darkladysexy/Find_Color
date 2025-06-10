using UnityEngine;
using UnityEngine.Tilemaps;

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
                    CheckWinCondition(); // Kiểm tra thắng sau khi đẩy khối
                }
            }
        }
        else 
        {
            player.transform.position = targetPos;
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

    // --- CÁC HÀM HỖ TRỢ ---
    private bool IsWallAt(Vector2 position)
    {
        return wallTilemap.HasTile(wallTilemap.WorldToCell(position));
    }

    // Hàm này chỉ tìm các vật thể có thể đẩy được, bỏ qua công tắc
    private Collider2D GetPushableObjectAt(Vector2 position)
    {
        Collider2D[] colliders = Physics2D.OverlapPointAll(position);
        foreach (var col in colliders)
        {
            if (col.gameObject.CompareTag("RedBlock"))
            {
                return col;
            }
        }
        return null;
    }
    
    // Ô trống là ô không có tường và không có khối di động
    private bool IsPositionFree(Vector2 position)
    {
        return !IsWallAt(position) && GetPushableObjectAt(position) == null;
    }
}