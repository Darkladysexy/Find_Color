using UnityEngine;

/// <summary>
/// Co RectTransform theo vùng an toàn của màn hình (tránh tai thỏ, camera đục lỗ,
/// thanh gesture). Gắn vào panel UI full-stretch (VD: panel Menu trong Canvas prefab).
///
/// Khi không có notch, safeArea = full màn hình nên anchors giữ nguyên.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class SafeArea : MonoBehaviour
{
    private RectTransform m_rectTransform;
    private Rect m_lastSafeArea;

    private void Awake()
    {
        m_rectTransform = GetComponent<RectTransform>();
        ApplySafeArea();
    }

    private void Update()
    {
        // Xoay màn hình hoặc đổi thiết bị có thể làm safeArea đổi lúc runtime.
        if (Screen.safeArea != m_lastSafeArea)
        {
            ApplySafeArea();
        }
    }

    private void ApplySafeArea()
    {
        m_lastSafeArea = Screen.safeArea;

        Vector2 screenSize = new Vector2(Screen.width, Screen.height);
        if (screenSize.x <= 0f || screenSize.y <= 0f)
        {
            return;
        }

        m_rectTransform.anchorMin = m_lastSafeArea.position / screenSize;
        m_rectTransform.anchorMax = (m_lastSafeArea.position + m_lastSafeArea.size) / screenSize;
    }
}
