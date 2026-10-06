using UnityEngine;

public class ButtonManager : MonoBehaviour
{
    [Tooltip("Door whose gravity changes when a block rests on this button.")]
    public GameObject door;

    private const float k_PressedGravityScale = -0.2f;
    private const float k_ReleasedGravityScale = 0.15f;
    private const string k_GroundTag = "Ground";

    private static readonly int k_IsPushedHash = Animator.StringToHash("IsPushed");

    private Animator m_animator;
    private Rigidbody2D m_doorRigidbody;

    private void Start()
    {
        m_animator = GetComponent<Animator>();

        if (door != null)
        {
            m_doorRigidbody = door.GetComponent<Rigidbody2D>();
        }
        else
        {
            Debug.LogError("Door is not assigned on ButtonManager.");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag(k_GroundTag))
        {
            m_animator.SetBool(k_IsPushedHash, true);
            SetDoorGravity(k_PressedGravityScale);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag(k_GroundTag))
        {
            m_animator.SetBool(k_IsPushedHash, false);
            SetDoorGravity(k_ReleasedGravityScale);
        }
    }

    private void SetDoorGravity(float gravityScale)
    {
        if (m_doorRigidbody != null)
        {
            m_doorRigidbody.gravityScale = gravityScale;
        }
    }
}
