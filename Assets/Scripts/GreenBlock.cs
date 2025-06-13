using UnityEngine;

public class GreenBlock : MonoBehaviour
{
    [Tooltip("Thời gian tồn tại của khối (giây). Đặt là 0 để tồn tại vĩnh viễn.")]
    public float lifetime = 0f;

    void Start()
    {
        // Nếu có thời gian tồn tại, khối sẽ tự hủy sau khoảng thời gian đó
        if (lifetime > 0)
        {
            Destroy(gameObject, lifetime);
        }
    }
}