using UnityEngine;

/// <summary>
/// Gắn ở root của cụm nút touch ("TouchControls"): chỉ hiện nút cảm ứng trên
/// mobile thật. Trên PC/editor cụm tự tắt để không che màn hình.
///
/// Muốn xem trước layout trong editor: tick "Show In Editor" trong Inspector
/// (chỉ có tác dụng trong editor, không ảnh hưởng build).
/// </summary>
public class TouchControlsVisibility : MonoBehaviour
{
    [Tooltip("Hiện cụm nút touch ngay trong editor để kiểm tra layout (không ảnh hưởng build).")]
    [SerializeField]
    private bool showInEditor;

    /// <summary>Test hook: ép hiện/ẩn từ code hoặc test, bất kể platform.</summary>
    public static bool ForceVisible;

    private void Awake()
    {
        bool visible = Application.isMobilePlatform || ForceVisible;

#if UNITY_EDITOR
        visible |= showInEditor;
#endif

        gameObject.SetActive(visible);
    }
}
