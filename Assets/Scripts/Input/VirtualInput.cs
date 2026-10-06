using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Drop-in thay thế cho UnityEngine.Input, gộp thêm điều khiển cảm ứng.
///
/// Bàn phím/mouse chạy đúng như cũ. Nút bấm trên màn hình (TouchButton) và
/// vuốt (SwipeDetector) nạp trạng thái vào đây qua PressKey/ReleaseKey/
/// SimulateKeyPress, rồi game đọc qua cùng một API.
///
/// Vì sao static class mà không phải Singleton MonoBehaviour (KISS):
/// không cần GameObject, không cần Awake order, API gọi y hệt Input.
/// </summary>
public static class VirtualInput
{
    // Phím đang được giữ bởi touch (TouchButton chưa nhả).
    private static readonly HashSet<KeyCode> s_heldKeys = new HashSet<KeyCode>();

    // Frame mà phím touch được bấm/nhả — để tái tạo ngữ nghĩa GetKeyDown/GetKeyUp.
    private static readonly Dictionary<KeyCode, int> s_pressedFrame = new Dictionary<KeyCode, int>();
    private static readonly Dictionary<KeyCode, int> s_releasedFrame = new Dictionary<KeyCode, int>();

    // Cho phép input mô phỏng "sống" thêm 1 frame: SwipeDetector/TouchButton có thể
    // chạy sau script đọc input trong cùng frame mà không bị mất sự kiện.
    private const int k_PressGraceFrames = 1;

    /// <summary>Gọi khi nút touch được nhấn: phím down trong frame này và giữ cho tới ReleaseKey.</summary>
    public static void PressKey(KeyCode key)
    {
        s_heldKeys.Add(key);
        s_pressedFrame[key] = Time.frameCount;
    }

    /// <summary>Gọi khi nút touch được nhả (hoặc ngón tay trượt khỏi nút).</summary>
    public static void ReleaseKey(KeyCode key)
    {
        s_heldKeys.Remove(key);
        s_releasedFrame[key] = Time.frameCount;
    }

    /// <summary>
    /// Giả lập một lần bấm phím duy nhất (không giữ) — dùng cho swipe.
    /// Không đưa vào s_heldKeys nên GetKey() sẽ không thấy phím này.
    /// </summary>
    public static void SimulateKeyPress(KeyCode key)
    {
        s_pressedFrame[key] = Time.frameCount;
    }

    public static bool GetKeyDown(KeyCode key)
    {
        return Input.GetKeyDown(key) || WasPressedRecently(key);
    }

    public static bool GetKey(KeyCode key)
    {
        return Input.GetKey(key) || s_heldKeys.Contains(key);
    }

    public static bool GetKeyUp(KeyCode key)
    {
        return Input.GetKeyUp(key) || WasReleasedRecently(key);
    }

    /// <summary>
    /// Ưu tiên phím cứng; nếu phím cứng = 0 thì dùng nút touch (D-pad).
    /// Nút touch map sang phím mũi tên nên suy ra trục từ các phím đang giữ.
    /// </summary>
    public static float GetAxisRaw(string axisName)
    {
        float keyboard = Input.GetAxisRaw(axisName);
        if (!Mathf.Approximately(keyboard, 0f))
        {
            return keyboard;
        }

        return GetTouchAxis(axisName);
    }

    public static float GetAxis(string axisName)
    {
        float touch = GetTouchAxis(axisName);
        if (!Mathf.Approximately(touch, 0f))
        {
            return touch;
        }

        return Input.GetAxis(axisName);
    }

    private static bool WasPressedRecently(KeyCode key)
    {
        return s_pressedFrame.TryGetValue(key, out int frame)
            && Time.frameCount - frame <= k_PressGraceFrames;
    }

    private static bool WasReleasedRecently(KeyCode key)
    {
        return s_releasedFrame.TryGetValue(key, out int frame)
            && Time.frameCount - frame <= k_PressGraceFrames;
    }

    private static float GetTouchAxis(string axisName)
    {
        if (axisName == "Horizontal")
        {
            float value = 0f;
            if (s_heldKeys.Contains(KeyCode.RightArrow) || s_heldKeys.Contains(KeyCode.D))
            {
                value += 1f;
            }
            if (s_heldKeys.Contains(KeyCode.LeftArrow) || s_heldKeys.Contains(KeyCode.A))
            {
                value -= 1f;
            }
            return value;
        }

        if (axisName == "Vertical")
        {
            float value = 0f;
            if (s_heldKeys.Contains(KeyCode.UpArrow) || s_heldKeys.Contains(KeyCode.W))
            {
                value += 1f;
            }
            if (s_heldKeys.Contains(KeyCode.DownArrow) || s_heldKeys.Contains(KeyCode.S))
            {
                value -= 1f;
            }
            return value;
        }

        return 0f;
    }
}
