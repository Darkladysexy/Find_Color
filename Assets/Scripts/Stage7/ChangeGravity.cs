using UnityEngine;

public class ChangeGravity : MonoBehaviour
{
    [Tooltip("Optional box affected by this gravity zone.")]
    public GameObject box;

    private const string k_GravityUpTag = "GravityUp";
    private const string k_GravityDownTag = "GravityDown";
    private const float k_UpGravityScale = -1f;
    private const float k_DownGravityScale = 1f;
    private const float k_FlippedRotationZ = 180f;
    private const float k_NormalRotationZ = 0f;

    private Rigidbody2D m_boxRigidbody;
    private Rigidbody2D m_playerRigidbody;
    private bool m_initialized;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        EnsureCached();

        if (CompareTag(k_GravityUpTag))
        {
            ApplyGravity(k_UpGravityScale, k_FlippedRotationZ);
        }
        else if (CompareTag(k_GravityDownTag))
        {
            ApplyGravity(k_DownGravityScale, k_NormalRotationZ);
        }
    }

    private void EnsureCached()
    {
        if (m_initialized)
        {
            return;
        }

        m_initialized = true;

        if (box != null)
        {
            m_boxRigidbody = box.GetComponent<Rigidbody2D>();
        }

        if (PlayerMovementPlatform.instant != null)
        {
            m_playerRigidbody = PlayerMovementPlatform.instant.GetComponent<Rigidbody2D>();
        }
    }

    private void ApplyGravity(float gravityScale, float rotationZ)
    {
        if (m_boxRigidbody != null)
        {
            m_boxRigidbody.gravityScale = gravityScale;
        }

        if (m_playerRigidbody == null || PlayerMovementPlatform.instant == null)
        {
            return;
        }

        m_playerRigidbody.gravityScale = gravityScale;

        Vector3 eulerAngles = PlayerMovementPlatform.instant.transform.rotation.eulerAngles;
        PlayerMovementPlatform.instant.transform.rotation =
            Quaternion.Euler(eulerAngles.x, eulerAngles.y, rotationZ);
    }
}
