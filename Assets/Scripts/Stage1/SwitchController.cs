using UnityEngine;

public class SwitchController : MonoBehaviour
{
    public bool isActivated = false;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    // Định nghĩa sẵn màu cam để dễ sử dụng
    private readonly Color orangeColor = new Color(1f, 0.64f, 0f);

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Kiểm tra xem có phải là khối có thể đẩy không
        if (other.CompareTag("RedBlock") || other.CompareTag("OrangeBlock"))
        {
            isActivated = true;
            
            // Đổi màu công tắc dựa trên màu của khối
            if (other.CompareTag("OrangeBlock"))
            {
                spriteRenderer.color = orangeColor; // Đổi thành màu cam
            }
            else // Nếu là "RedBlock"
            {
                spriteRenderer.color = Color.red; // Đổi thành màu đỏ
            }
            
            // Lưu ý: Chúng ta không gọi CheckWinCondition() ở đây nữa để tránh lỗi
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("RedBlock") || other.CompareTag("OrangeBlock"))
        {
            isActivated = false;
            spriteRenderer.color = originalColor; // Trả về màu gốc
        }
    }
}