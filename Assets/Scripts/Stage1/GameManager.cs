using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

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

    // Unused by code; kept so existing serialized scene data is not lost.
    [Header("Legacy (No longer used)")]
    [Tooltip("Legacy field. Kept for serialized-data compatibility only.")]
    public Transform goalSpawnPoint;

    private const string k_PushedSound = "Pushed";
    private const string k_PlayerTag = "Player";
    private const string k_RedBlockTag = "RedBlock";
    private const string k_OrangeBlockTag = "OrangeBlock";
    private const float k_WinDelaySeconds = 1.5f;

    private static readonly Vector2[] k_NeighborDirections =
    {
        Vector2.up,
        Vector2.down,
        Vector2.left,
        Vector2.right
    };

    private GameObject m_player;
    private bool m_isLevelCompleted;
    private SwitchController[] m_allSwitches;
    private HashSet<Vector3Int> m_paintedTiles;
    private int m_totalFloorTiles;
    private Vector3Int m_playerCellPosition;
    private AudioManager m_audioManager;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (wallTilemap == null)
        {
            Debug.LogError("Wall Tilemap is not assigned on GameManager.");
            enabled = false;
            return;
        }

        m_player = GameObject.FindGameObjectWithTag(k_PlayerTag);

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
        if (m_isLevelCompleted)
        {
            return;
        }

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
        if (m_isLevelCompleted)
        {
            return;
        }

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
        m_isLevelCompleted = true;

        SpawnGoal();

        yield return new WaitForSeconds(k_WinDelaySeconds);

        if (nextSceneBuildIndex > 0 && nextSceneBuildIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneBuildIndex);
        }
    }

    private void SpawnGoal()
    {
        GameObject goalToSpawn = null;
        switch (currentLevelType)
        {
            case LevelType.Red:
                goalToSpawn = redGoalPrefab;
                break;
            case LevelType.Orange:
                goalToSpawn = orangeGoalPrefab;
                break;
            case LevelType.Yellow:
                goalToSpawn = yellowGoalPrefab;
                break;
        }

        if (goalToSpawn != null && m_player != null)
        {
            Instantiate(goalToSpawn, m_player.transform.position, Quaternion.identity);
        }
    }

    // ------------------------------------------------------------------
    // Yellow level logic (paint all floor tiles)
    // ------------------------------------------------------------------

    private void InitializeYellowLevel()
    {
        if (floorTilemap == null)
        {
            Debug.LogError("Floor Tilemap is not assigned for the Yellow level.");
            enabled = false;
            return;
        }

        m_paintedTiles = new HashSet<Vector3Int>();
        m_totalFloorTiles = 0;

        // Manual count via cellBounds is the reliable way to count the last tile too.
        floorTilemap.CompressBounds();
        foreach (Vector3Int position in floorTilemap.cellBounds.allPositionsWithin)
        {
            if (floorTilemap.HasTile(position))
            {
                m_totalFloorTiles++;
            }
        }

        m_playerCellPosition = floorTilemap.WorldToCell(m_player.transform.position);
        m_player.transform.position = floorTilemap.GetCellCenterWorld(m_playerCellPosition);

        PaintTile(m_playerCellPosition);
        CheckConditions();
    }

    private void AttemptPaintMove(Vector2 direction)
    {
        Vector3Int targetCell = m_playerCellPosition + new Vector3Int((int)direction.x, (int)direction.y, 0);

        if (floorTilemap.HasTile(targetCell) && !m_paintedTiles.Contains(targetCell))
        {
            m_playerCellPosition = targetCell;
            m_player.transform.position = floorTilemap.GetCellCenterWorld(m_playerCellPosition);
            PaintTile(targetCell);
            CheckConditions();
        }
    }

    private void PaintTile(Vector3Int cell)
    {
        if (!m_paintedTiles.Contains(cell) && yellowTrailPrefab != null)
        {
            m_paintedTiles.Add(cell);
            Instantiate(yellowTrailPrefab, floorTilemap.GetCellCenterWorld(cell), Quaternion.identity);
        }
    }

    private void CheckYellowWin()
    {
        if (m_totalFloorTiles > 0 && m_paintedTiles.Count == m_totalFloorTiles && !m_isLevelCompleted)
        {
            StartCoroutine(WinSequence());
        }
    }

    // ------------------------------------------------------------------
    // Red / Orange level logic (push blocks onto switches)
    // ------------------------------------------------------------------

    private void InitializeRedOrangeLevel()
    {
        m_allSwitches = FindObjectsOfType<SwitchController>();
    }

    private void AttemptPushMove(Vector2 direction)
    {
        Vector2 currentPos = m_player.transform.position;
        Vector2 targetPos = currentPos + direction;

        if (IsWallAt(targetPos))
        {
            return;
        }

        Collider2D blockCollider = GetPushableObjectAt(targetPos);
        if (blockCollider == null)
        {
            m_player.transform.position = targetPos;
            return;
        }

        if (blockCollider.CompareTag(k_RedBlockTag))
        {
            Vector2 posAfterBlock = (Vector2)blockCollider.transform.position + direction;
            if (IsPositionFree(posAfterBlock, null))
            {
                blockCollider.transform.position = posAfterBlock;
                m_player.transform.position = targetPos;
                PlaySound(k_PushedSound);
            }
        }
        else if (blockCollider.CompareTag(k_OrangeBlockTag))
        {
            List<GameObject> connectedBlocks = FindConnectedBlocks(blockCollider.gameObject);
            if (CanClusterMove(connectedBlocks, direction))
            {
                PlaySound(k_PushedSound);
                MoveCluster(connectedBlocks, direction);
                m_player.transform.position = targetPos;
            }
        }
    }

    private void CheckRedOrangeWin()
    {
        foreach (SwitchController switchController in m_allSwitches)
        {
            if (!switchController.isActivated)
            {
                return;
            }
        }

        if (!m_isLevelCompleted)
        {
            StartCoroutine(WinSequence());
        }
    }

    private bool IsWallAt(Vector2 position)
    {
        return wallTilemap.HasTile(wallTilemap.WorldToCell(position));
    }

    private Collider2D GetPushableObjectAt(Vector2 position)
    {
        Collider2D[] colliders = Physics2D.OverlapPointAll(position);
        foreach (Collider2D collider in colliders)
        {
            if (IsPushableBlock(collider.gameObject))
            {
                return collider;
            }
        }

        return null;
    }

    private static bool IsPushableBlock(GameObject gameObject)
    {
        return gameObject.CompareTag(k_RedBlockTag) || gameObject.CompareTag(k_OrangeBlockTag);
    }

    private bool IsPositionFree(Vector2 position, List<GameObject> clusterToIgnore)
    {
        if (IsWallAt(position))
        {
            return false;
        }

        Collider2D[] colliders = Physics2D.OverlapPointAll(position);
        foreach (Collider2D collider in colliders)
        {
            if (clusterToIgnore != null && clusterToIgnore.Contains(collider.gameObject))
            {
                continue;
            }

            if (IsPushableBlock(collider.gameObject))
            {
                return false;
            }
        }

        return true;
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
            foreach (Vector2 direction in k_NeighborDirections)
            {
                CheckNeighbor((Vector2)currentBlock.transform.position + direction, queue, connectedCluster);
            }
        }

        return connectedCluster;
    }

    private void CheckNeighbor(Vector2 position, Queue<GameObject> queue, List<GameObject> cluster)
    {
        Collider2D[] colliders = Physics2D.OverlapPointAll(position);
        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag(k_OrangeBlockTag) && !cluster.Contains(collider.gameObject))
            {
                cluster.Add(collider.gameObject);
                queue.Enqueue(collider.gameObject);
            }
        }
    }

    private bool CanClusterMove(List<GameObject> cluster, Vector2 direction)
    {
        foreach (GameObject block in cluster)
        {
            Vector2 newPos = (Vector2)block.transform.position + direction;
            if (!IsPositionFree(newPos, cluster))
            {
                return false;
            }
        }

        return true;
    }

    private void MoveCluster(List<GameObject> cluster, Vector2 direction)
    {
        foreach (GameObject block in cluster)
        {
            block.transform.position += (Vector3)direction;
        }
    }

    private void PlaySound(string soundName)
    {
        if (m_audioManager == null)
        {
            m_audioManager = FindAnyObjectByType<AudioManager>();
        }

        if (m_audioManager != null)
        {
            m_audioManager.Play(soundName);
        }
    }
}
