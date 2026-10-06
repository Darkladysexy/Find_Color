using UnityEngine;

public class PosControlClone : MonoBehaviour
{
    [Tooltip("Clone prefab to spawn.")]
    public GameObject clonePrefab;
    [Tooltip("Spawn position for the clone.")]
    public GameObject PosClone;

    private const string k_PlayerTag = "Player";
    private const string k_CloneTag = "Clone";

    private GameObject m_spawnedClone;
    private bool m_hasSpawned;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.gameObject.CompareTag(k_PlayerTag) || m_hasSpawned)
        {
            return;
        }

        if (m_spawnedClone == null && clonePrefab != null && PosClone != null)
        {
            m_spawnedClone = Instantiate(clonePrefab, PosClone.transform.position, Quaternion.identity);
            m_spawnedClone.tag = k_CloneTag;
        }

        m_hasSpawned = true;
    }
}
