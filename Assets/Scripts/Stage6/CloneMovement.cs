using UnityEngine;

/// <summary>
/// Clone mirror theo input của player (đi ngược hướng ngang).
/// Dùng cùng hệ game feel với PlayerMovementPlatform (buffer/coyote/jump cut,
/// gia tốc) để hai nhân vật luôn đồng bộ.
///
/// Khác bản gốc 1 điểm: tôn trọng pause (bản gốc Update không check pause).
/// Xem GAMEPLAY_AUDIT.md.
/// </summary>
public class CloneMovement : MonoBehaviour
{
    public static CloneMovement instant;

    [Header("Movement")]
    [SerializeField]
    private float speed = 5f;
    [Tooltip("Tốc độ tăng tốc ngang (đơn vị/giây²).")]
    [SerializeField]
    private float acceleration = 50f;
    [Tooltip("Tốc độ giảm tốc khi thả phím (đơn vị/giây²).")]
    [SerializeField]
    private float deceleration = 60f;

    [Header("Jump")]
    [Tooltip("Impulse applied when jumping.")]
    public float jumpForce = 7f;
    [Tooltip("Bấm nhảy hơi sớm trước khi chạm đất vẫn nhảy (giây). 0 = tắt.")]
    [SerializeField]
    private float jumpBufferTime = 0.12f;
    [Tooltip("Chạy khỏi mép platform rồi bấm nhảy ngay vẫn nhảy (giây). 0 = tắt.")]
    [SerializeField]
    private float coyoteTime = 0.1f;
    [Tooltip("Giữ lại bao nhiêu % vận tốc lên khi nhả phím nhảy sớm. 1 = luôn nhảy full.")]
    [SerializeField, Range(0f, 1f)]
    private float jumpCutMultiplier = 0.5f;

    private static readonly int k_IsRunHash = Animator.StringToHash("isRun");

    private Rigidbody2D m_rigidbody;
    private SpriteRenderer m_spriteRenderer;
    private Animator m_animator;

    private float m_horizontalInput;
    private float m_jumpBufferTimer;
    private float m_coyoteTimer;
    private bool m_jumpCutRequested;

    private void Awake()
    {
        instant = this;
        m_rigidbody = GetComponent<Rigidbody2D>();
        m_spriteRenderer = GetComponent<SpriteRenderer>();
        m_animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (Menu.instant != null && Menu.instant.isPaused)
        {
            return;
        }

        // Mirror: đi ngược hướng ngang của player.
        m_horizontalInput = -VirtualInput.GetAxisRaw("Horizontal");

        if (VirtualInput.GetKeyDown(KeyCode.Space))
        {
            m_jumpBufferTimer = jumpBufferTime;
        }

        if (VirtualInput.GetKeyUp(KeyCode.Space))
        {
            m_jumpCutRequested = true;
        }

        if (m_horizontalInput > 0f)
        {
            m_spriteRenderer.flipX = false;
        }
        else if (m_horizontalInput < 0f)
        {
            m_spriteRenderer.flipX = true;
        }

        m_animator.SetBool(k_IsRunHash, !Mathf.Approximately(m_horizontalInput, 0f));
    }

    private void FixedUpdate()
    {
        if (Menu.instant != null && Menu.instant.isPaused)
        {
            return;
        }

        ApplyHorizontalMovement();
        UpdateJumpTimers();
        TryJump();
        ApplyJumpCut();
    }

    private void ApplyHorizontalMovement()
    {
        float targetSpeed = m_horizontalInput * speed;
        float rate = Mathf.Approximately(m_horizontalInput, 0f) ? deceleration : acceleration;

        float newSpeedX = Mathf.MoveTowards(
            m_rigidbody.velocity.x, targetSpeed, rate * Time.fixedDeltaTime);

        m_rigidbody.velocity = new Vector2(newSpeedX, m_rigidbody.velocity.y);
    }

    private void UpdateJumpTimers()
    {
        if (m_jumpBufferTimer > 0f)
        {
            m_jumpBufferTimer -= Time.fixedDeltaTime;
        }

        if (IsGrounded())
        {
            m_coyoteTimer = coyoteTime;
        }
        else if (m_coyoteTimer > 0f)
        {
            m_coyoteTimer -= Time.fixedDeltaTime;
        }
    }

    private void TryJump()
    {
        if (m_jumpBufferTimer <= 0f)
        {
            return;
        }

        if (!IsGrounded() && m_coyoteTimer <= 0f)
        {
            return;
        }

        m_jumpBufferTimer = 0f;
        m_coyoteTimer = 0f;

        m_rigidbody.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    private void ApplyJumpCut()
    {
        if (!m_jumpCutRequested)
        {
            return;
        }

        m_jumpCutRequested = false;

        if (m_rigidbody.velocity.y > 0.01f)
        {
            m_rigidbody.velocity = new Vector2(
                m_rigidbody.velocity.x,
                m_rigidbody.velocity.y * jumpCutMultiplier);
        }
    }

    private bool IsGrounded()
    {
        return FootClone.instant != null && FootClone.instant.onGround;
    }
}
