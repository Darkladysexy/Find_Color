using UnityEngine;

/// <summary>
/// Grid movement (stage 1–3): mỗi lần bấm đi đúng 1 ô — by-design cho puzzle,
/// không áp dụng game feel của platformer. Đọc input qua VirtualInput để
/// bàn phím, nút touch và swipe dùng chung một path.
/// </summary>
public class PlayerMovement : MonoBehaviour
{
    private const string k_WalkSound = "Walk";

    private SpriteRenderer m_spriteRenderer;
    private AudioManager m_audioManager;

    private void Awake()
    {
        m_spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (Menu.instant.isPaused)
        {
            return;
        }

        if (VirtualInput.GetKeyDown(KeyCode.W) || VirtualInput.GetKeyDown(KeyCode.UpArrow))
        {
            MoveInDirection(Vector2.up);
        }
        else if (VirtualInput.GetKeyDown(KeyCode.S) || VirtualInput.GetKeyDown(KeyCode.DownArrow))
        {
            MoveInDirection(Vector2.down);
        }
        else if (VirtualInput.GetKeyDown(KeyCode.A) || VirtualInput.GetKeyDown(KeyCode.LeftArrow))
        {
            m_spriteRenderer.flipX = true;
            MoveInDirection(Vector2.left);
        }
        else if (VirtualInput.GetKeyDown(KeyCode.D) || VirtualInput.GetKeyDown(KeyCode.RightArrow))
        {
            m_spriteRenderer.flipX = false;
            MoveInDirection(Vector2.right);
        }
    }

    private void MoveInDirection(Vector2 direction)
    {
        PlaySound(k_WalkSound);
        GameManager.Instance.AttemptMove(direction);
    }

    private void PlaySound(string soundName)
    {
        if (m_audioManager == null)
        {
            m_audioManager = FindAnyObjectByType<AudioManager>();
        }

        if (m_audioManager != null)
        {
            m_audioManager.Play(soundName);
        }
    }
}
