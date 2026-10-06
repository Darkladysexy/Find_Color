using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField]
    private float speed = 5f;

    private static readonly int k_IsRunHash = Animator.StringToHash("isRun");

    private Rigidbody2D m_rigidbody;
    private SpriteRenderer m_spriteRenderer;
    private Animator m_animator;

    private void Awake()
    {
        m_rigidbody = GetComponent<Rigidbody2D>();
        m_spriteRenderer = GetComponent<SpriteRenderer>();
        m_animator = GetComponent<Animator>();
    }

    private void Update()
    {
        // NOTE: preserved exactly as the original — movement runs only while paused.
        // This condition looks inverted (other scripts use "if paused, return").
        // Confirm with the level designer before "fixing"; see REVIEW_REPORT.md.
        if (Menu.instant.isPaused)
        {
            MovePlayer();
        }
    }

    private void MovePlayer()
    {
        float moveX = VirtualInput.GetAxis("Horizontal");
        float moveY = VirtualInput.GetAxis("Vertical");

        Vector2 playerInput = new Vector2(moveX, moveY).normalized * speed;
        m_rigidbody.velocity = playerInput;

        if (playerInput.x > 0f)
        {
            m_spriteRenderer.flipX = false;
        }
        else if (playerInput.x < 0f)
        {
            m_spriteRenderer.flipX = true;
        }

        m_animator.SetBool(k_IsRunHash, playerInput != Vector2.zero);
    }
}
