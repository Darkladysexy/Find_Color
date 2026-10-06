using UnityEngine;

public class Teleport : MonoBehaviour
{
    [Tooltip("Destination portal.")]
    public GameObject portal2;

    private const string k_PlayerTag = "Player";

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag(k_PlayerTag)
            && PlayerMovementPlatform.instant != null
            && portal2 != null)
        {
            PlayerMovementPlatform.instant.transform.position = portal2.transform.position;
        }
    }
}
