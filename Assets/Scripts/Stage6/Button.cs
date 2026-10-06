using UnityEngine;

public class Button : MonoBehaviour
{
    [Tooltip("Ground object to enable when a clone steps on this button.")]
    public GameObject ground;

    private const string k_CloneTag = "Clone";

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.gameObject.CompareTag(k_CloneTag))
        {
            return;
        }

        if (ground != null)
        {
            ground.SetActive(true);
        }

        Destroy(collision.gameObject);
        PlayerCollision.instant.onGround = true;
    }
}
