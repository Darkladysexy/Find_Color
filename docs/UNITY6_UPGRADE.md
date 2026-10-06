# UNITY6_UPGRADE.md — Nâng project Find Color từ 2022.3.39f1 lên Unity 6

> Không tự sửa file version trong task này theo ràng buộc an toàn. Tài liệu này là checklist để user tự thực hiện trong Unity Hub / Editor.

## 0. Tình trạng project hiện tại (đọc từ source)

- `ProjectSettings/ProjectVersion.txt`: `m_EditorVersion: 2022.3.39f1`
- Packages đáng chú ý trong `Packages/manifest.json`:
  - `com.unity.cinemachine`: **2.10.3** → Unity 6 dùng Cinemachine **3.x** (breaking API changes — chỉ ảnh hưởng nếu code dùng API Cinemachine; 26 script refactor **không** dùng)
  - `com.unity.feature.2d`: 2.0.1 → có bản tương thích Unity 6
  - `com.unity.textmeshpro`: 3.0.6, `com.unity.timeline`: 1.7.6, `com.unity.ugui`: 1.0.0, `com.unity.visualscripting`: 1.9.4, `com.unity.test-framework`: 1.1.33
  - `com.unity.ide.rider`: 3.0.31 / `com.unity.ide.visualstudio`: 2.0.22
- API đã dùng trong 26 script refactor: `FindAnyObjectByType` (có từ 2022.1, **giữ nguyên trong Unity 6**), `Tilemap`, `Physics2D`, `SceneManager`, `Animator.StringToHash`, `UnityEngine.Pool` (không dùng) → **không có API nào bị xóa/deprecate trong Unity 6**.

## 1. Chuẩn bị (bắt buộc)

1. **Backup:** commit toàn bộ project vào git (hoặc copy cả thư mục project sang ổ khác). Kiểm tra `git status` sạch trước khi tiếp tục.
2. Đóng Unity Editor hoàn toàn.
3. Cài **Unity 6 LTS** qua Unity Hub (khuyến nghị bản LTS mới nhất dòng `6000.0.x`, ví dụ 6000.0.x LTS; tránh bản Tech Stream nếu muốn ổn định).
4. (Khuyến nghị) Xóa thư mục `Library/` trong project — ép reimport sạch, tránh cache cũ gây lỗi vặt khi nhảy version lớn. (`Library/` sẽ được build lại tự động; không xóa `Assets/`, `Packages/`, `ProjectSettings/`.)

## 2. Mở project bằng Unity 6 (thứ tự an toàn)

1. Mở **Unity Hub → Projects → Add** (nếu chưa có) → chọn thư mục project → mở bằng bản **Unity 6** đã cài.
   - **Không sửa tay `ProjectVersion.txt`** trừ khi Hub không nhận project; để Hub/Editor tự ghi version mới.
2. Editor sẽ hiện dialog **"Upgrade project"** → xác nhận. Unity tự chạy **API Updater** quét và sửa các API obsolete (project này dự kiến không có gì cần sửa).
3. Chờ reimport toàn bộ Assets (lần đầu lâu — bình thường).
4. Mở **Window → Package Manager** → kiểm tra các package có update cho Unity 6:
   - `2D` (feature.2d), `TextMeshPro`, `Timeline`, `UGUI`, `Visual Scripting`, `Test Framework` → **Update** lên bản tương thích.
   - **Cinemachine 2.10.3:** Unity 6 đề xuất Cinemachine 3.x nhưng 3.x **đổi breaking API**. Vì game không dùng API Cinemachine trong code, có 2 lựa chọn an toàn:
     - (a) Giữ 2.10.3 nếu Package Manager vẫn cho cài trên Unity 6, hoặc
     - (b) Update lên 3.x rồi mở lại các scene dùng Cinemachine kiểm tra component (không cần sửa code).
   - `IDE Rider / Visual Studio` → update theo IDE đang dùng.
5. Vào **Edit → Project Settings → Player**: kiểm tra lại
   - `Scripting Backend` (IL2CPP/Mono) và `Api Compatibility Level` theo target build cũ;
   - `Active Input Handling` (game dùng `Input.GetKeyDown`/`GetAxis` — old Input Manager, giữ nguyên);
   - Color Space / Render Pipeline (project 2D nhiều khả năng dùng Built-in — giữ nguyên, đừng đổi URP lúc upgrade).
6. **Edit → Project Settings → Quality / Physics 2D:** đối chiếu với bản 2022 nếu gameplay vật lý khác lạ (Fixed Timestep mặc định 0.02 — Unity 6 giữ nguyên).

## 3. Kiểm tra sau upgrade

1. Mở Console (Ctrl+Shift+C): phải **0 error**. Warning về package thì xử lý theo gợi ý của Package Manager.
2. Chạy Play Mode và test **đủ 7 stage** (đỏ đẩy, cam dính, vàng vệt, lục trồng cây, lam teleport, chàm clone, tím trọng lực) + menu pause/resume/restart.
3. Kiểm tra các điểm đã refactor: âm thanh Walk/Pushed/Switch/Jump còn phát, switch đổi màu, clone spawn, gravity zone xoay player.
4. Nếu build ra thiết bị: Build Settings → kiểm tra target platform, rồi Build And Run thử.

## 4. Những gì KHÔNG cần động tới

- **File `.meta` / GUID:** giữ nguyên tuyệt đối — script refactor giữ nguyên tên file/class nên GUID khớp, không mất reference trong scene/prefab.
- **Scene/prefab:** không cần sửa gì cho đợt refactor này (public field giữ nguyên tên).
- **`ProjectVersion.txt`:** để Editor tự ghi sau khi mở bằng Unity 6 lần đầu.

## 5. Rollback nếu có sự cố

- Đóng Editor → restore project từ backup (git `reset --hard` / copy thư mục backup đè lại) → mở lại bằng Unity **2022.3.39f1**.
- Không bao giờ mở project đã upgrade bằng bản Unity cũ hơn rồi save — sẽ corrupt version metadata.
