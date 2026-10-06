using UnityEngine;

public class GreenBlock : MonoBehaviour
{
    [Tooltip("Lifetime in seconds. 0 = permanent.")]
    public float lifetime = 0f;

    private void Start()
    {
        if (lifetime > 0f)
        {
            Destroy(gameObject, lifetime);
        }
    }
}
