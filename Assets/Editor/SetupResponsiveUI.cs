using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// One-click setup UI responsive + touch controls cho game Find Color.
///
/// Chạy: menu "Find Color/Setup Responsive UI + Touch Controls".
/// Tác động duy nhất: prefab Assets/Prefabs/Canvas.prefab (được mọi scene dùng chung).
/// Chạy lại nhiều lần an toàn — các bước đều kiểm tra "đã có chưa" trước khi thêm.
///
/// Các bước:
/// 1. Thêm CanvasScaler (Scale With Screen Size, 1920x1080, match 0.5) nếu chưa có.
/// 2. Dựng cụm "TouchControls": D-pad trái/phải + nút nhảy + nút action (phím C),
///    neo 2 góc dưới, chỉ hiện trên mobile (TouchControlsVisibility).
/// 3. Gắn SwipeDetector lên Canvas (tự tắt ở scene không phải grid).
/// 4. Gắn SafeArea vào panel "Menu" (tránh tai thỏ).
///
/// Không throw giữa chừng: mỗi bước bọc try/catch, log rõ pass/skip/fail.
/// </summary>
public static class SetupResponsiveUI
{
    private const string k_MenuItem = "Find Color/Setup Responsive UI + Touch Controls";
    private const string k_CanvasPrefabPath = "Assets/Prefabs/Canvas.prefab";
    private const string k_TouchControlsName = "TouchControls";
    private const string k_MenuPanelName = "Menu";

    private const float k_ReferenceWidth = 1920f;
    private const float k_ReferenceHeight = 1080f;
    private const float k_MatchWidthOrHeight = 0.5f;

    private const float k_ButtonSize = 150f;
    private const float k_ButtonMargin = 40f;

    [MenuItem(k_MenuItem)]
    public static void RunSetup()
    {
        int changed = 0;
        int alreadyOk = 0;
        int failed = 0;

        GameObject prefabRoot = PrefabUtility.LoadPrefabContents(k_CanvasPrefabPath);
        if (prefabRoot == null)
        {
            Debug.LogError($"[FindColor] Không tìm thấy Canvas prefab tại {k_CanvasPrefabPath}. Dừng.");
            return;
        }

        try
        {
            RunStep("CanvasScaler", () => EnsureCanvasScaler(prefabRoot), ref changed, ref alreadyOk, ref failed);
            RunStep("TouchControls", () => EnsureTouchControls(prefabRoot), ref changed, ref alreadyOk, ref failed);
            RunStep("SwipeDetector", () => EnsureSwipeDetector(prefabRoot), ref changed, ref alreadyOk, ref failed);
            RunStep("SafeArea", () => EnsureSafeArea(prefabRoot), ref changed, ref alreadyOk, ref failed);

            PrefabUtility.SaveAsPrefabAsset(prefabRoot, k_CanvasPrefabPath);
            Debug.Log($"[FindColor] Setup xong — đổi: {changed}, đã chuẩn: {alreadyOk}, lỗi: {failed}.");
            if (failed > 0)
            {
                Debug.LogWarning("[FindColor] Có bước lỗi — xem log đỏ ở trên, các bước còn lại vẫn được lưu.");
            }
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(prefabRoot);
        }
    }

    private delegate bool SetupStep();

    private static void RunStep(string name, SetupStep step, ref int changed, ref int alreadyOk, ref int failed)
    {
        try
        {
            if (step())
            {
                changed++;
                Debug.Log($"[FindColor] [{name}] đã cập nhật.");
            }
            else
            {
                alreadyOk++;
                Debug.Log($"[FindColor] [{name}] đã chuẩn, bỏ qua.");
            }
        }
        catch (System.Exception e)
        {
            failed++;
            Debug.LogError($"[FindColor] [{name}] lỗi: {e.Message}");
        }
    }

    // 1. CanvasScaler: scale UI theo kích thước màn hình.
    private static bool EnsureCanvasScaler(GameObject canvasRoot)
    {
        CanvasScaler scaler = canvasRoot.GetComponent<CanvasScaler>();
        if (scaler == null)
        {
            scaler = canvasRoot.AddComponent<CanvasScaler>();
        }

        bool changed = false;
        changed |= SetScalerMode(scaler);
        return changed;
    }

    private static bool SetScalerMode(CanvasScaler scaler)
    {
        bool changed = false;

        if (scaler.uiScaleMode != CanvasScaler.ScaleMode.ScaleWithScreenSize)
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            changed = true;
        }

        Vector2 referenceResolution = new Vector2(k_ReferenceWidth, k_ReferenceHeight);
        if (scaler.referenceResolution != referenceResolution)
        {
            scaler.referenceResolution = referenceResolution;
            changed = true;
        }

        if (scaler.screenMatchMode != CanvasScaler.ScreenMatchMode.MatchWidthOrHeight)
        {
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            changed = true;
        }

        if (!Mathf.Approximately(scaler.matchWidthOrHeight, k_MatchWidthOrHeight))
        {
            scaler.matchWidthOrHeight = k_MatchWidthOrHeight;
            changed = true;
        }

        return changed;
    }

    // 2. Cụm nút touch: D-pad + nhảy + action, neo 2 góc dưới.
    private static bool EnsureTouchControls(GameObject canvasRoot)
    {
        bool changed = false;

        Transform existing = canvasRoot.transform.Find(k_TouchControlsName);
        GameObject root;
        if (existing == null)
        {
            root = new GameObject(k_TouchControlsName);
            root.transform.SetParent(canvasRoot.transform, false);
            StretchFull(root);
            changed = true;
        }
        else
        {
            root = existing.gameObject;
        }

        if (root.GetComponent<TouchControlsVisibility>() == null)
        {
            root.AddComponent<TouchControlsVisibility>();
            changed = true;
        }

        // Cụm trái: D-pad.
        changed |= EnsureTouchButton(root.transform, "BtnLeft", KeyCode.LeftArrow,
            new Vector2(0f, 0f), new Vector2(k_ButtonSize + k_ButtonMargin, k_ButtonSize + k_ButtonMargin));
        changed |= EnsureTouchButton(root.transform, "BtnRight", KeyCode.RightArrow,
            new Vector2(0f, 0f), new Vector2(k_ButtonSize * 2f + k_ButtonMargin * 2f, k_ButtonSize + k_ButtonMargin));

        // Cụm phải: nhảy + action (phím C trồng cây ở màn Lục).
        changed |= EnsureTouchButton(root.transform, "BtnJump", KeyCode.Space,
            new Vector2(1f, 0f), new Vector2(-(k_ButtonSize * 2f + k_ButtonMargin * 2f), k_ButtonSize + k_ButtonMargin));
        changed |= EnsureTouchButton(root.transform, "BtnAction", KeyCode.C,
            new Vector2(1f, 0f), new Vector2(-(k_ButtonSize + k_ButtonMargin), k_ButtonSize + k_ButtonMargin));

        return changed;
    }

    private static bool EnsureTouchButton(Transform parent, string name, KeyCode key, Vector2 anchor, Vector2 anchoredPosition)
    {
        bool changed = false;

        Transform existing = parent.Find(name);
        GameObject go;
        if (existing == null)
        {
            go = new GameObject(name);
            go.transform.SetParent(parent, false);

            RectTransform rect = go.AddComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.sizeDelta = new Vector2(k_ButtonSize, k_ButtonSize);
            rect.anchoredPosition = anchoredPosition;

            // Nút mặc định: hình chữ nhật trắng mờ. Muốn đẹp hơn thì gán sprite
            // trong thư mục Assets/Buttons vào ô Source Image trong Inspector.
            Image image = go.AddComponent<Image>();
            image.color = new Color(1f, 1f, 1f, 0.25f);
            image.raycastTarget = true;

            changed = true;
        }
        else
        {
            go = existing.gameObject;
        }

        TouchButton button = go.GetComponent<TouchButton>();
        if (button == null)
        {
            button = go.AddComponent<TouchButton>();
            changed = true;
        }

        if (button.key != key)
        {
            button.key = key;
            changed = true;
        }

        return changed;
    }

    private static void StretchFull(GameObject go)
    {
        RectTransform rect = go.GetComponent<RectTransform>();
        if (rect == null)
        {
            rect = go.AddComponent<RectTransform>();
        }

        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    // 3. SwipeDetector: tự tắt ở scene không phải grid.
    private static bool EnsureSwipeDetector(GameObject canvasRoot)
    {
        if (canvasRoot.GetComponent<SwipeDetector>() != null)
        {
            return false;
        }

        canvasRoot.AddComponent<SwipeDetector>();
        return true;
    }

    // 4. SafeArea cho panel Menu (pause): tránh tai thỏ che nút.
    // Lưu ý: nếu muốn nền mờ của Menu vẫn full-bleed, hãy di chuyển component
    // SafeArea từ "Menu" sang object con chứa các nút trong Inspector.
    private static bool EnsureSafeArea(GameObject canvasRoot)
    {
        Transform menu = canvasRoot.transform.Find(k_MenuPanelName);
        if (menu == null)
        {
            Debug.LogWarning($"[FindColor] [SafeArea] không thấy panel '{k_MenuPanelName}' trong Canvas prefab — bỏ qua bước này.");
            return false;
        }

        if (menu.GetComponent<SafeArea>() != null)
        {
            return false;
        }

        menu.gameObject.AddComponent<SafeArea>();
        return true;
    }
}
