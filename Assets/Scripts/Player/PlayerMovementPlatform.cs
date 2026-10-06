using UnityEngine;

/// <summary>
/// Platformer movement với game feel hiện đại:
/// - Input sample trong Update, apply physics trong FixedUpdate (ổn định mọi framerate).
/// - Gia tốc/giảm tốc mượt thay vì set velocity tức thì.
/// - Jump buffering + coyote time + variable jump height (jump cut).
///
/// Mọi thông số tune được trong Inspector; default giữ gần cảm giác cũ.
/// Muốn về y hệt cũ: acceleration/deceleration = 1000, jumpBufferTime/coyoteTime = 0,
/// jumpCutMultiplier = 1. Chi tiết xem GAMEPLAY_AUDIT.md.
/// </summary>
public class PlayerMovementPlatform : MonoBehaviour
{
    public static PlayerMovementPlatform instant;

    [Header("Movement")]
    [SerializeField]
    private float speed = 5f;
    [Tooltip("Tốc độ tăng tốc ngang (đơn vị/giây²). Để rất lớn (VD 1000) sẽ nhanh như bản cũ.")]
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

    private const string k_JumpSound = "Jump";

    private static readonly int k_IsRunHash = Animator.StringToHash("isRun");

    private Rigidbody2D m_rigidbody;
    private SpriteRenderer m_spriteRenderer;
    private Animator m_animator;
    private AudioManager m_audioManager;

    // Input được sample trong Update (đúng chỗ đọc GetKeyDown/GetKeyUp),
    // rồi FixedUpdate mới dùng để tính physics.
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
        if (Menu.instant.isPaused)
        {
            return;
        }

        m_horizontalInput = VirtualInput.GetAxisRaw("Horizontal");

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
        // Pause đã set timeScale = 0 nên physics đứng yên; guard thêm cho chắc.
        if (Menu.instant.isPaused)
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

        PlaySound(k_JumpSound);

        // Hướng nhảy ngược với trọng lực (màn 7 đổi trọng lực qua gravityScale).
        float jumpSign = m_rigidbody.gravityScale < 0f ? -1f : 1f;
        m_rigidbody.AddForce(Vector2.up * jumpSign * jumpForce, ForceMode2D.Impulse);
    }

    private void ApplyJumpCut()
    {
        if (!m_jumpCutRequested)
        {
            return;
        }

        m_jumpCutRequested = false;

        float jumpSign = m_rigidbody.gravityScale < 0f ? -1f : 1f;
        float upwardVelocity = m_rigidbody.velocity.y * jumpSign;

        // Chỉ cắt khi đang bay lên; đang rơi thì không đụng vào.
        if (upwardVelocity > 0.01f)
        {
            m_rigidbody.velocity = new Vector2(
                m_rigidbody.velocity.x,
                m_rigidbody.velocity.y * jumpCutMultiplier);
        }
    }

    private bool IsGrounded()
    {
        return PlayerCollision.instant != null && PlayerCollision.instant.onGround;
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
