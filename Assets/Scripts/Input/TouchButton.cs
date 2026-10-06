using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Nút bấm cảm ứng: gắn vào GameObject UI có Image (raycastTarget = true),
/// map 1 KeyCode ảo. Nhấn → VirtualInput.PressKey, nhả/trượt khỏi nút → ReleaseKey.
///
/// Yêu cầu: scene phải có EventSystem (các scene game đã có sẵn).
/// </summary>
public class TouchButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    [Tooltip("Phím ảo mà nút này mô phỏng (VD: LeftArrow, Space, C).")]
    public KeyCode key = KeyCode.Space;

    private bool m_isPressed;

    public void OnPointerDown(PointerEventData eventData)
    {
        m_isPressed = true;
        VirtualInput.PressKey(key);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Release();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Ngón tay trượt khỏi nút giữa chừng vẫn phải nhả phím.
        Release();
    }

    private void OnDisable()
    {
        Release();
    }

    private void Release()
    {
        if (!m_isPressed)
        {
            return;
        }

        m_isPressed = false;
        VirtualInput.ReleaseKey(key);
    }
}
