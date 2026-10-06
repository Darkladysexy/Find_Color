using UnityEngine;

/// <summary>
/// Vuốt 4 hướng cho các màn grid (stage 1–3): mỗi lần vuốt qua ngưỡng sẽ giả lập
/// một lần bấm phím mũi tên qua VirtualInput, đi đúng path keyboard của
/// PlayerMovement (âm thanh + GameManager.AttemptMove) nên tự tôn trọng pause.
///
/// Tự vô hiệu hóa ở scene không phải grid (không tìm thấy PlayerMovement).
/// Đặt component này trên Canvas prefab — nó chỉ "sống" ở màn grid.
/// </summary>
public class SwipeDetector : MonoBehaviour
{
    [Tooltip("Quãng vuốt tối thiểu (pixel) để tính là một lần di chuyển.")]
    [SerializeField]
    private float minSwipeDistance = 60f;

    [Tooltip("Cho phép kéo chuột để test swipe ngay trong editor.")]
    [SerializeField]
    private bool enableMouse = true;

    private bool m_isGridScene;
    private int m_activeFingerId = -1;
    private Vector2 m_swipeStart;
    private bool m_mouseTracking;

    private void Start()
    {
        // Màn grid là màn có PlayerMovement (di chuyển rời rạc theo ô).
        m_isGridScene = FindAnyObjectByType<PlayerMovement>() != null;
        if (!m_isGridScene)
        {
            enabled = false;
        }
    }

    private void Update()
    {
        HandleTouch();
        if (enableMouse)
        {
            HandleMouse();
        }
    }

    private void HandleTouch()
    {
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    if (m_activeFingerId == -1)
                    {
                        m_activeFingerId = touch.fingerId;
                        m_swipeStart = touch.position;
                    }
                    break;

                case TouchPhase.Moved:
                case TouchPhase.Stationary:
                    if (touch.fingerId == m_activeFingerId)
                    {
                        TrySwipe(touch.position);
                    }
                    break;

                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    if (touch.fingerId == m_activeFingerId)
                    {
                        TrySwipe(touch.position);
                        m_activeFingerId = -1;
                    }
                    break;
            }
        }
    }

    private void HandleMouse()
    {
        if (Input.GetMouseButtonDown(0))
        {
            m_mouseTracking = true;
            m_swipeStart = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (m_mouseTracking)
            {
                TrySwipe(Input.mousePosition);
            }
            m_mouseTracking = false;
        }
        else if (m_mouseTracking)
        {
            TrySwipe(Input.mousePosition);
        }
    }

    private void TrySwipe(Vector2 currentPosition)
    {
        Vector2 delta = currentPosition - m_swipeStart;
        if (delta.magnitude < minSwipeDistance)
        {
            return;
        }

        // Reset điểm bắt đầu để một lần kéo dài có thể đi nhiều ô liên tiếp.
        m_swipeStart = currentPosition;

        // Touch/mouse dùng hệ tọa độ màn hình (y hướng lên) — khớp với mũi tên.
        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
        {
            VirtualInput.SimulateKeyPress(delta.x > 0f ? KeyCode.RightArrow : KeyCode.LeftArrow);
        }
        else
        {
            VirtualInput.SimulateKeyPress(delta.y > 0f ? KeyCode.UpArrow : KeyCode.DownArrow);
        }
    }
}
