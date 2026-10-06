using UnityEngine;

public class Seed : MonoBehaviour
{
    private const string k_PlayerTag = "Player";

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(k_PlayerTag))
        {
            if (GreenLevelManager.Instance != null)
            {
                GreenLevelManager.Instance.CollectSeed(1);
            }

            Destroy(gameObject);
        }
    }
}
