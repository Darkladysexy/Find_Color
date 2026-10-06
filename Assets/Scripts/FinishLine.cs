using UnityEngine;

public class FinishLine : MonoBehaviour
{
    private const string k_PlayerTag = "Player";

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(k_PlayerTag) && GreenLevelManager.Instance != null)
        {
            GreenLevelManager.Instance.CompleteLevel();
        }
    }
}
