# MOBILE SETUP — Find Color (UI responsive + touch controls)

## Đã làm gì trong đợt này (code)

**Input abstraction** (`Assets/Scripts/Input/`):
- `VirtualInput.cs` — static class mirror API `UnityEngine.Input`
  (`GetKeyDown/GetKey/GetKeyUp/GetAxisRaw/GetAxis`): bàn phím chạy y như cũ,
  nút touch + swipe nạp vào cùng API. Không cần GameObject, không phụ thuộc Awake order.
- `TouchButton.cs` — gắn vào UI Image, map 1 KeyCode ảo
  (trái/phải/nhảy/phím C). Xử lý cả trường hợp ngón tay trượt khỏi nút.
- `SwipeDetector.cs` — vuốt 4 hướng cho màn grid, đi đúng path keyboard
  (âm thanh + `GameManager.AttemptMove`). Tự tắt ở scene không có `PlayerMovement`.
  Trong editor có thể kéo chuột để test swipe.

**UI** (`Assets/Scripts/UI/`):
- `SafeArea.cs` — co panel theo `Screen.safeArea` (tránh tai thỏ), tự cập nhật khi xoay màn hình.
- `TouchControlsVisibility.cs` — cụm nút touch chỉ hiện trên mobile thật.
  Muốn xem trước trong editor: tick "Show In Editor" trong Inspector.

**Editor one-click setup** (`Assets/Editor/SetupResponsiveUI.cs`):
- Menu `Find Color/Setup Responsive UI + Touch Controls`, chạy idempotent
  (chạy lại không trùng): gắn `CanvasScaler` (Scale With Screen Size, 1920×1080,
  match 0.5) vào Canvas prefab → dựng cụm nút touch (D-pad + nhảy + action,
  neo 2 góc dưới) → gắn `SwipeDetector` → gắn `SafeArea` vào panel Menu.

**Script đã sửa để đọc qua VirtualInput** (tên file/class/public API giữ nguyên):
- `Assets/Scripts/PlayerMovement.cs` (grid: 8 phím WASD/mũi tên)
- `Assets/Scripts/Player/PlayerMovementPlatform.cs` (platformer: trục ngang + Space)
- `Assets/Scripts/Player/Player.cs` (trục ngang/dọc)
- `Assets/Scripts/Stage6/CloneMovement.cs` (mirror input)
- `Assets/Scripts/GreenLevelManager.cs` (phím C trồng cây)

---

## Checklist trong Unity editor

### Bước 1 — Chép files
Chép toàn bộ nội dung `stage2/Assets/` đè vào `Assets/` của project
(giữ đúng cấu trúc thư mục). Mở project → **Console phải 0 error, 0 warning liên quan**.

### Bước 2 — Chạy setup UI
1. Menu **Find Color → Setup Responsive UI + Touch Controls**.
2. Đọc log Console: mỗi bước báo `đã cập nhật` / `đã chuẩn, bỏ qua` / `lỗi`.
3. Mở prefab `Assets/Prefabs/Canvas.prefab` kiểm tra:
   - Có `Canvas Scaler` (Ui Scale Mode = Scale With Screen Size).
   - Có child `TouchControls` (4 nút BtnLeft/BtnRight/BtnJump/BtnAction) —
     tick **Show In Editor** trên `TouchControlsVisibility` để xem layout, xong nhớ untick.
   - Có component `SwipeDetector` trên Canvas root.
   - Panel `Menu` có `SafeArea`.
4. Nút touch mặc định là hình chữ nhật trắng mờ — muốn đẹp: gán sprite trong
   `Assets/Buttons/` vào ô Source Image của từng nút trong prefab.

### Bước 3 — Test trong editor
- **Bàn phím:** chơi thử 7 màn như bình thường — phải y hệt bản cũ
  (trừ platformer mượt hơn nhờ game feel mới).
- **Touch giả lập:** cài package **Device Simulator**
  (Package Manager → Unity Registry → Device Simulator), mở
  `Window → General → Device Simulator`, chọn một máy Android/iOS → cụm nút
  touch hiện ra, bấm thử di chuyển/nhảy/nút C.
- **Swipe:** ở màn grid (stage 1–3), kéo chuột để test vuốt 4 hướng.

### Bước 4 — Player Settings cho Android
1. `File → Build Settings → Android → Switch Platform` (đợi convert).
2. `Player Settings`:
   - **Resolution and Presentation:** Orientation = Landscape Left (giữ như cũ);
     bật **Render Outside Safe Area** (SafeArea.cs đã lo phần tai thỏ).
   - **Other Settings:** Minimum API Level ≥ 24; Target API = Automatic (highest installed);
     Scripting Backend = **IL2CPP**; Target Architectures = **ARM64**
     (+ ARMv7 nếu muốn hỗ trợ máy cũ); Target SDK = Device SDK.
   - **Configuration:** Install Location = Automatic.
3. `Build And Run` lên máy thật, test: D-pad, nhảy (giữ/nhả sớm để thấy jump cut),
   nút action ở màn Lục, vuốt ở màn grid, xoay máy, máy có tai thỏ.

### Bước 5 — Merge
- Mở pull request hiện tại (`refactor/unity-dev-cleanup`), kiểm tra Console 0 error
  rồi merge. Khuyên test tay đủ 7 màn trên máy thật trước khi release.

## Lưu ý đã biết
- Không có Unity editor ở phía build script nên code **chưa được compile kiểm tra** —
  nếu Console báo lỗi, gửi nguyên văn lỗi để sửa.
- `SwipeDetector` bật theo heuristic "scene có PlayerMovement" — nếu sau này có
  màn grid không dùng `PlayerMovement`, báo để đổi điều kiện.
- Nút touch cần **EventSystem** trong scene (các scene game đã có sẵn).
