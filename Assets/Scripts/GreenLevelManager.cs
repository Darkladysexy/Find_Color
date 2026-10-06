using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class GreenLevelManager : MonoBehaviour
{
    public static GreenLevelManager Instance;

    [Header("Level Configuration")]
    [Tooltip("Seeds available when the level starts.")]
    public int startingSeeds = 0;
    [Tooltip("Build index of the scene to load after finishing.")]
    public int nextSceneBuildIndex;

    [Header("Scene References")]
    [Tooltip("Player object. Auto-found by tag if left empty.")]
    public GameObject player;
    public Tilemap wallTilemap;

    [Header("Prefabs")]
    public GameObject greenBlockPrefab;

    private const KeyCode k_PlantKey = KeyCode.C;
    private const string k_PlayerTag = "Player";
    private const float k_PlantXOffset = 1.2f;
    private const float k_PlantYOffset = 0.5f;

    private int m_currentSeedCount;
    private bool m_isLevelCompleted;
    private SpriteRenderer m_playerSprite;

    private void Awake()
    {
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
            player = GameObject.FindGameObjectWithTag(k_PlayerTag);
        }

        if (player == null)
        {
            Debug.LogError("No GameObject with tag 'Player' found in the scene.");
            enabled = false;
            return;
        }

        m_playerSprite = player.GetComponent<SpriteRenderer>();
        m_currentSeedCount = startingSeeds;
    }

    private void Update()
    {
        if (m_isLevelCompleted)
        {
            return;
        }

        // Đọc qua VirtualInput để nút "action" trên mobile cũng trồng cây được.
        if (VirtualInput.GetKeyDown(k_PlantKey))
        {
            TryPlantBlock();
        }
    }

    public void CollectSeed(int amount)
    {
        m_currentSeedCount += amount;
    }

    private void TryPlantBlock()
    {
        if (m_currentSeedCount <= 0)
        {
            return;
        }

        if (m_playerSprite == null)
        {
            Debug.LogError("Player has no SpriteRenderer.");
            return;
        }

        // Plant on the side the player is facing; a flipped sprite means facing left.
        float xOffset = m_playerSprite.flipX ? -k_PlantXOffset : k_PlantXOffset;
        Vector3 plantPosition = player.transform.position + new Vector3(xOffset, k_PlantYOffset, 0);

        if (wallTilemap != null)
        {
            Vector3Int cellToPlantIn = wallTilemap.WorldToCell(plantPosition);
            if (wallTilemap.HasTile(cellToPlantIn))
            {
                return;
            }
        }

        m_currentSeedCount--;

        if (greenBlockPrefab != null)
        {
            Instantiate(greenBlockPrefab, plantPosition, Quaternion.identity);
        }
        else
        {
            Debug.LogError("Green Block Prefab is not assigned on GreenLevelManager.");
        }
    }

    public void CompleteLevel()
    {
        if (m_isLevelCompleted)
        {
            return;
        }

        m_isLevelCompleted = true;

        if (nextSceneBuildIndex > 0)
        {
            SceneManager.LoadScene(nextSceneBuildIndex);
        }
    }
}
