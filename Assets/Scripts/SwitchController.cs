using UnityEngine;

public class SwitchController : MonoBehaviour
{
    public bool isActivated = false;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color; // Lưu lại màu gốc
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("RedBlock"))
        {
            isActivated = true;
            spriteRenderer.color = Color.red; // Đổi sang màu đỏ khi được kích hoạt
            GameManager.Instance.CheckWinCondition();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("RedBlock"))
        {
            isActivated = false;
            spriteRenderer.color = originalColor; // Trở về màu gốc khi khối rời đi
        }
    }
}