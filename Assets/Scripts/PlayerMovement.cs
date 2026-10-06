using UnityEngine;

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

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            MoveInDirection(Vector2.up);
        }
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            MoveInDirection(Vector2.down);
        }
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            m_spriteRenderer.flipX = true;
            MoveInDirection(Vector2.left);
        }
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
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
