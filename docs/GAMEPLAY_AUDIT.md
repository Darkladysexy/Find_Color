# GAMEPLAY AUDIT — Find Color (input & game feel)

Đọc kỹ các script movement bản refactor đợt 1, đối chiếu chuẩn platformer hiện đại
(jump buffering, coyote time, variable jump height, acceleration). Dòng tham chiếu
theo bản refactor đợt 1 trong `~/workspace/find-color-upgrade/Scripts/`.

**Kết luận ngắn:** input grid (stage 1–3) ổn, giữ nguyên by-design. Platformer
(PlayerMovementPlatform, CloneMovement) còn "cứng": 5 vấn đề dưới đây — **đã fix
hết trong đợt này**.

---

## Vấn đề đã fix

### 1. Không có jump buffering — PlayerMovementPlatform.cs:42
```csharp
// TRƯỚC: bấm Space sớm hơn thời điểm chạm đất vài chục ms → mất input
if (!Input.GetKeyDown(KeyCode.Space) || !PlayerCollision.instant.onGround) return;
```
Bấm nhảy hơi sớm trước khi tiếp đất là chuyện thường xuyên → cảm giác game "nuốt phím".
**Fix:** buffer 0.12s — phím nhảy được nhớ trong 0.12s, chạm đất trong lúc đó vẫn nhảy.
File mới: `Assets/Scripts/Player/PlayerMovementPlatform.cs` (TryJump + m_jumpBufferTimer).

### 2. Không có coyote time — PlayerMovementPlatform.cs:42
Chạy khỏi mép platform, bấm nhảy trong ~0.1s sau đó → rơi thẳng (dù tay bấm "kịp").
**Fix:** coyote 0.1s — rời mép đất trong 0.1s vẫn được nhảy.

### 3. Nhảy luôn full height — PlayerMovementPlatform.cs:53/57
`AddForce(up * jumpForce, Impulse)` — nhả Space sớm vẫn nhảy cao tối đa, khó
canh độ cao khi nhảy qua chướng ngại thấp.
**Fix:** jump cut — nhả phím khi đang bay lên thì vận tốc đứng × 0.5.

### 4. Di chuyển ngang set velocity tức thì — PlayerMovementPlatform.cs:64
```csharp
m_rigidbody.velocity = new Vector2(moveX * speed, m_rigidbody.velocity.y);
```
Bắt đầu/dừng đột ngột, không có cảm giác quán tính.
**Fix:** gia tốc 50 / giảm tốc 60 (đơn vị/giây²), `speed = 5` giữ làm tốc độ tối đa.

### 5. Đọc input và apply physics cùng trong Update — PlayerMovementPlatform.cs:36-37
Set `rigidbody.velocity` trong `Update()` → phụ thuộc framerate, có thể giật khi
fps ≠ physics timestep.
**Fix:** sample input trong `Update()` (đúng chỗ đọc GetKeyDown/GetKeyUp),
apply trong `FixedUpdate()`.

### 6. Clone không đồng bộ feel + không tôn trọng pause — CloneMovement.cs:28-46
- Clone đọc input trực tiếp, bản gốc không có game feel riêng (vấn đề nhỏ).
- **Nghi ngờ behavior:** `Update()` của Clone không check `Menu.instant.isPaused`
  trong khi player có → pause game clone vẫn di chuyển/nhảy.
**Fix:** Clone dùng cùng hệ feel mới (mirror input), thêm pause guard.
Ghi chú: đây là *thay đổi behavior* so với bản gốc (bản gốc clone chạy cả khi pause).
Nếu designer muốn giữ y hệt cũ, xóa 2 dòng check pause trong CloneMovement.

---

## Không đụng tới (by-design / ngoài phạm vi)

### 7. Grid movement — PlayerMovement.cs:22-41 — GIỮ NGUYÊN
Di chuyển rời rạc theo ô là luật chơi của puzzle (stage 1–3). `GetKeyDown` mỗi lần
đi 1 ô là đúng. Chỉ đổi `Input` → `VirtualInput` để touch/swipe đi chung path
(âm thanh + `GameManager.AttemptMove` giữ nguyên).

### 8. Player.cs dùng Input.GetAxis (smoothed) — GIỮ NGUYÊN feel
Không nhất quán với các script khác (dùng GetAxisRaw), nhưng file này đang trong
diện "chờ xác nhận bug pause ngược" (REVIEW_REPORT.md) nên chỉ swap sang
`VirtualInput.GetAxis`, không đổi cảm giác.

### 9. Ground detection (PlayerCollision/FootClone) — NGOÀI PHẠM VI
`OnCollisionExit2D` set `onGround = false` cho mọi collider (kể cả không phải đất)
— đã ghi nhận ở REVIEW_REPORT.md mục 7, cần test gameplay mới dám sửa.

---

## Bảng thông số mới (PlayerMovementPlatform & CloneMovement, Inspector)

| Thông số | Default | Ý nghĩa | Muốn về như cũ |
|---|---|---|---|
| speed | 5 | tốc độ ngang tối đa — giữ nguyên bản cũ | — |
| acceleration | 50 | gia tốc ngang (đv/giây²) | = 1000 ≈ set tức thì |
| deceleration | 60 | giảm tốc khi thả phím | = 1000 ≈ dừng tức thì |
| jumpForce | 7 | lực nhảy — giữ nguyên bản cũ | — |
| jumpBufferTime | 0.12 | nhớ phím nhảy (giây) | = 0 tắt |
| coyoteTime | 0.1 | nhảy trễ sau khi rời mép (giây) | = 0 tắt |
| jumpCutMultiplier | 0.5 | giữ 50% vận tốc đứng khi nhả Space sớm | = 1 luôn nhảy full |

## Thay đổi kỹ thuật đáng chú ý
- **Hướng nhảy khi đổi trọng lực (màn 7):** bản cũ check `transform.eulerAngles.z`
  (0/180). Bản mới dùng dấu của `rigidbody.gravityScale` — tương đương vì
  `ChangeGravity.ApplyGravity` luôn set cả hai cùng lúc, code gọn và ít magic hơn.
- **Âm thanh nhảy:** vẫn phát đúng lúc nhảy thật (kể cả khi nhảy nhờ buffer/coyote),
  không phát khi bấm hụt.
- **Animator/flipX:** chuyển sang dùng input đã sample trong `Update()` (visual),
  không còn đọc input trực tiếp trong logic physics.
