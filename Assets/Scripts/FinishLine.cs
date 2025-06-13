using UnityEngine;

public class FinishLine : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Gọi đến GreenLevelManager để hoàn thành màn chơi
            if(GreenLevelManager.Instance != null)
            {
                GreenLevelManager.Instance.CompleteLevel();
            }
        }
    }
}