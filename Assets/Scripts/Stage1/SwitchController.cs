using UnityEngine;

public class SwitchController : MonoBehaviour
{
    public bool isActivated = false;

    private const string k_SwitchSound = "Switch";
    private const string k_RedBlockTag = "RedBlock";
    private const string k_OrangeBlockTag = "OrangeBlock";

    private static readonly Color k_OrangeActivatedColor = new Color(1f, 0.64f, 0f);

    private SpriteRenderer m_spriteRenderer;
    private Color m_originalColor;
    private AudioManager m_audioManager;

    private void Awake()
    {
        m_spriteRenderer = GetComponent<SpriteRenderer>();
        m_originalColor = m_spriteRenderer.color;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsPushableBlock(other))
        {
            return;
        }

        isActivated = true;
        m_spriteRenderer.color = other.CompareTag(k_OrangeBlockTag) ? k_OrangeActivatedColor : Color.red;
        PlaySound(k_SwitchSound);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.CheckConditions();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!IsPushableBlock(other))
        {
            return;
        }

        isActivated = false;
        m_spriteRenderer.color = m_originalColor;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.CheckConditions();
        }
    }

    private static bool IsPushableBlock(Collider2D other)
    {
        return other.CompareTag(k_RedBlockTag) || other.CompareTag(k_OrangeBlockTag);
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
