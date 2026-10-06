using UnityEngine;

public class CloneMovement : MonoBehaviour
{
    public static CloneMovement instant;

    [SerializeField]
    private float speed = 5f;
    [Tooltip("Impulse applied when jumping.")]
    public float jumpForce = 7f;

    private static readonly int k_IsRunHash = Animator.StringToHash("isRun");

    private Rigidbody2D m_rigidbody;
    private SpriteRenderer m_spriteRenderer;
    private Animator m_animator;

    private void Awake()
    {
        instant = this;
        m_rigidbody = GetComponent<Rigidbody2D>();
        m_spriteRenderer = GetComponent<SpriteRenderer>();
        m_animator = GetComponent<Animator>();
    }

    private void Update()
    {
        MovePlayer();
        HandleJump();
    }

    private void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && FootClone.instant.onGround)
        {
            m_rigidbody.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }
    }

    private void MovePlayer()
    {
        // The clone mirrors the player's horizontal input.
        float moveX = Input.GetAxisRaw("Horizontal");
        m_rigidbody.velocity = new Vector2(moveX * speed * -1f, m_rigidbody.velocity.y);

        if (moveX < 0f)
        {
            m_spriteRenderer.flipX = false;
        }
        else if (moveX > 0f)
        {
            m_spriteRenderer.flipX = true;
        }

        m_animator.SetBool(k_IsRunHash, moveX != 0f);
    }
}
