using UnityEngine;

public class GoalController : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Kiểm tra xem đối tượng va chạm có phải là người chơi không
        if (other.CompareTag("Player"))
        {
            Debug.Log("Người chơi đã chạm vào Goal! Chuẩn bị qua màn!");
            
            // Gọi hàm hoàn thành màn chơi từ GameManager
            GameManager.Instance.CompleteLevel();

            // Ẩn hoặc phá hủy Goal để người chơi không thể chạm vào lần nữa
            gameObject.SetActive(false);
        }
    }
}