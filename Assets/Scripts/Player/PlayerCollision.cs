using UnityEngine;

public class PlayerCollision : MonoBehaviour
{
    public static PlayerCollision instant;
    public bool onGround = false;

    private const string k_GroundTag = "Ground";

    private void Awake()
    {
        instant = this;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(k_GroundTag))
        {
            onGround = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        onGround = false;
    }
}
