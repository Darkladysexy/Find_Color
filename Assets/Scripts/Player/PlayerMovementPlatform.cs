using UnityEngine;

public class PlayerMovementPlatform : MonoBehaviour
{
    public static PlayerMovementPlatform instant;

    [SerializeField]
    private float speed = 5f;
    [Tooltip("Impulse applied when jumping.")]
    public float jumpForce = 7f;

    private const string k_JumpSound = "Jump";

    private static readonly int k_IsRunHash = Animator.StringToHash("isRun");

    private Rigidbody2D m_rigidbody;
    private SpriteRenderer m_spriteRenderer;
    private Animator m_animator;
    private AudioManager m_audioManager;

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

        MovePlayer();
        HandleJump();
    }

    private void HandleJump()
    {
        if (!Input.GetKeyDown(KeyCode.Space) || !PlayerCollision.instant.onGround)
        {
            return;
        }

        PlaySound(k_JumpSound);

        // Jump direction follows the stage-7 gravity orientation: 0 = normal, 180 = flipped.
        float zRotation = transform.eulerAngles.z;
        if (Mathf.Approximately(zRotation, 0f))
        {
            m_rigidbody.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }
        else if (Mathf.Approximately(zRotation, 180f))
        {
            m_rigidbody.AddForce(Vector2.up * -jumpForce, ForceMode2D.Impulse);
        }
    }

    private void MovePlayer()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        m_rigidbody.velocity = new Vector2(moveX * speed, m_rigidbody.velocity.y);

        if (moveX > 0f)
        {
            m_spriteRenderer.flipX = false;
        }
        else if (moveX < 0f)
        {
            m_spriteRenderer.flipX = true;
        }

        m_animator.SetBool(k_IsRunHash, moveX != 0f);
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
