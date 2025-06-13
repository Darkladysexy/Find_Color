using UnityEngine;

public class SwitchController : MonoBehaviour
{
    public bool isActivated = false;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private readonly Color orangeActivatedColor = new Color(1f, 0.64f, 0f);

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("RedBlock") || other.CompareTag("OrangeBlock"))
        {
            isActivated = true;
            
            if (other.CompareTag("OrangeBlock"))
            {
                spriteRenderer.color = orangeActivatedColor;
            }
            else
            {
                spriteRenderer.color = Color.red;
            }
            
            // <<< SỬA: Gọi đến hàm kiểm tra điều kiện chung
            if (GameManager.Instance != null)
            {
                GameManager.Instance.CheckConditions();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("RedBlock") || other.CompareTag("OrangeBlock"))
        {
            isActivated = false;
            spriteRenderer.color = originalColor;

            // <<< SỬA: Gọi đến hàm kiểm tra điều kiện chung
             if (GameManager.Instance != null)
            {
                GameManager.Instance.CheckConditions();
            }
        }
    }
}