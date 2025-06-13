using UnityEngine;

public class Seed : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Gọi đến GreenLevelManager để cộng hạt mầm
            if (GreenLevelManager.Instance != null)
            {
                GreenLevelManager.Instance.CollectSeed(1);
            }
            Destroy(gameObject);
        }
    }
}