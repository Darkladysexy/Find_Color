using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // Biến để lưu trữ component SpriteRenderer
    private SpriteRenderer spriteRenderer;

    // Awake được gọi một lần khi script được khởi tạo, rất tốt để lấy các component
    private void Awake()
    {
        // Lấy component SpriteRenderer từ cùng GameObject và lưu vào biến để sử dụng
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // Dùng GetKeyDown để chỉ nhận tín hiệu một lần duy nhất khi phím được nhấn xuống
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            // Khi đi lên hoặc xuống, chúng ta không cần thay đổi hướng nhìn
            GameManager.Instance.AttemptMove(Vector2.up);
        }
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            GameManager.Instance.AttemptMove(Vector2.down);
        }
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {   
            // --- THÊM LOGIC LẬT HÌNH Ở ĐÂY ---
            // Đặt flipX = true để nhân vật nhìn sang trái
            spriteRenderer.flipX = true;
            
            GameManager.Instance.AttemptMove(Vector2.left);
        }
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            // --- THÊM LOGIC LẬT HÌNH Ở ĐÂY ---
            // Đặt flipX = false để nhân vật nhìn sang phải (hướng mặc định)
            spriteRenderer.flipX = false;
            
            GameManager.Instance.AttemptMove(Vector2.right);
        }
    }
}