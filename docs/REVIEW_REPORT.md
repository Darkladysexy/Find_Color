# REVIEW REPORT — Find Color refactor (theo skill unity-dev)

- **Source gốc:** `/tmp/findcolor/Assets/Scripts/` — 26 file, 1352 dòng, Unity 2022.3.39f1
- **Output:** `Scripts/` (26 file, giữ nguyên tên file/class)
- **Nguyên tắc áp dụng:** SKILL.md → Workflow "Review / refactor code có sẵn": đọc `solid.md` + `clean-code.md`, liệt kê vi phạm cụ thể; Output Contract: mỗi vi phạm nêu rõ nguyên tắc SOLID/code smell + code xấu → tốt.

---

## 1. Vi phạm đã sửa (theo file)

### GameManager.cs (233 dòng — file lớn nhất)
| # | Vi phạm | Vị trí gốc (ước lượng) | Sửa |
|---|---------|------------------------|-----|
| 1 | **Performance:** `FindAnyObjectByType<AudioManager>()` trong `AttemptPushMove` — scene-wide search chạy mỗi lần đẩy block (input handler). Skill `performance.md`: cache trong Awake/Start, không gọi trong Update/input. | ~dòng 141, 149 | Lazy-cache vào `m_audioManager`, lookup tối đa 1 lần rồi reuse. |
| 2 | **Clean code:** `#region` (Yellow_Level_Logic, Red_Orange_Logic_And_Helpers). Style guide: không dùng `#region` — class cần region thì nên tách class. | dòng 116, 161 | Thay bằng section comment thường. (Tách class thật cần file mới + rewiring — xem mục 3.) |
| 3 | **SRP (ghi nhận, chưa tách):** một class ôm 3 loại màn (Red/Orange/Yellow) + win sequence + spawn goal + tile counting. | toàn file | Trong giới hạn "không đổi public API / không thêm file", chỉ tổ chức lại. Tách thành `YellowLevelController` / `PushLevelController` là manual step (mục 3). |
| 4 | **Excessive commentary:** comment tiếng Việt từng dòng ("Biến không còn dùng đến", "<<< SỬA LỖI ĐẾM TILE CUỐI CÙNG >>>"...). | khắp file | Xóa hết; chỉ giữ comment "tại sao" (ví dụ: vì sao đếm tile thủ công qua `cellBounds`). |
| 5 | **Naming:** private field không prefix (`player`, `isLevelCompleted`, `paintedTiles`...). Style guide: private → `m_`. | khắp file | `m_player`, `m_isLevelCompleted`, `m_paintedTiles`, `m_totalFloorTiles`, `m_playerCellPosition`, `m_allSwitches`. |
| 6 | **Magic string/number:** `"Pushed"`, `"RedBlock"`, `"OrangeBlock"`, `"Player"`, `1.5f`. | khắp file | `private const string k_...`, `k_WinDelaySeconds`. |
| 7 | **Formatting:** 8 helper method viết một dòng dài 200+ ký tự, không braces. Style guide: luôn giữ braces, một statement một dòng, 80–120 ký tự/dòng. | dòng 165–174 | Tách multi-line, Allman braces. |
| 8 | **Debug.Log trong gameplay** (7 chỗ, có string concat `"Tổng số ô sàn: " + totalFloorTiles` → alloc). Skill: xóa `Debug.Log` trước build. | nhiều nơi | Xóa log thường, giữ `Debug.LogError` cho trường hợp gán thiếu Tilemap. |
| 9 | **Null-safety:** `wallTilemap` không được check ở nhánh Red/Orange (chỉ Yellow check `floorTilemap`) → `IsWallAt` sẽ NRE nếu quên gán. | `Start()` | Thêm guard `wallTilemap == null → LogError + disable`, đồng nhất với nhánh Yellow. |

Minh họa (xấu → tốt):
```csharp
// XẤU: search toàn scene mỗi lần đẩy block + magic string
FindAnyObjectByType<AudioManager>().Play("Pushed");

// TỐT: lazy-cache, lookup 1 lần
private AudioManager m_audioManager;
private void PlaySound(string soundName)
{
    if (m_audioManager == null)
    {
        m_audioManager = FindAnyObjectByType<AudioManager>();
    }
    if (m_audioManager != null)
    {
        m_audioManager.Play(soundName);
    }
}
```

### PlayerMovement.cs (grid, stage 1–3)
| # | Vi phạm | Sửa |
|---|---------|-----|
| 1 | **DRY/WET:** 4 nhánh `if/else` copy-paste `FindAnyObjectByType<AudioManager>().Play("Walk")` + `GameManager.Instance.AttemptMove(...)`. | Gộp thành `MoveInDirection(Vector2)` — một chỗ gọi sound + move. |
| 2 | Comment tiếng Việt từng dòng ("Dùng GetKeyDown để chỉ nhận tín hiệu một lần..."). | Xóa; code tự giải thích. |
| 3 | `FindAnyObjectByType` trong input handler (4 lần/nhấn). | Reuse qua `PlaySound()` cache như trên. |

### Player.cs
| # | Vi phạm | Sửa |
|---|---------|-----|
| 1 | **WET:** `if (playerInput != Vector2.zero \|\| playerInput != Vector2.zero)` — điều kiện lặp y hệt. | `playerInput != Vector2.zero` (tương đương logic). |
| 2 | `Update()` trống `Start()`, `using System.Collections[Generic]` thừa. | Xóa. |
| 3 | `animator.SetBool("isRun", ...)` — string lookup mỗi frame. Skill performance: dùng `Animator.StringToHash` cache lúc init. | `private static readonly int k_IsRunHash = Animator.StringToHash("isRun");` |
| 4 | **BUG NGHIÊM TRỌNG — KHÔNG SỬA (chờ xác nhận):** `if (Menu.instant.isPaused) { MovePlayer(); }` — ngược với mọi script khác (`if paused → return`). Player này chỉ di chuyển khi game đang pause. Giữ nguyên 100% theo ràng buộc, đánh dấu NOTE trong code. | Manual step: xác nhận với designer rồi đảo thành `if (!Menu.instant.isPaused)`. |

### PlayerMovementPlatform.cs
| # | Vi phạm | Sửa |
|---|---------|-----|
| 1 | `Debug.Log(PlayerCollision.instant.onGround)` **mỗi frame** trong `Jump()` — string alloc + I/O log mỗi frame = GC pressure trong hot path. | Xóa. |
| 2 | `FindAnyObjectByType<AudioManager>()` trong `Jump()` (input handler). | Cache qua `PlaySound()`. |
| 3 | `FixedUpdate()` trống — Unity vẫn gọi event rỗng mỗi physics step. Skill: xóa Unity event rỗng. | Xóa. |
| 4 | `GetComponent` trong `Start()` thay vì `Awake()`. Skill: cache mọi thứ trong Awake. | Chuyển vào `Awake()`. |
| 5 | `"isRun"` string mỗi frame. | `k_IsRunHash` cached. |
| 6 | Method `Jump()` tên chung chung. | `HandleJump()` (private nên an toàn). |

### PlayerCollision.cs / BodyPlayer.cs / FootClone.cs
| # | Vi phạm | Sửa |
|---|---------|-----|
| 1 | **DRY:** 3 class gần như giống hệt nhau (ground sensor). Không merge được vì ràng buộc giữ tên file/class + `instant`/`onGround` đang được reference chéo. | Dọn từng file; merge là manual step (mục 3). |
| 2 | `.tag == "Ground"` — property `.tag` **allocate string mới mỗi lần đọc** (skill performance.md: dùng `CompareTag`). | `CompareTag(k_GroundTag)`. |
| 3 | Code bị comment-out (`OnCollisionStay2D`, `OnTriggerEnter2D`). Style guide: xóa, source control giữ lịch sử. | Xóa. |
| 4 | `Start()`/`Update()` trống. | Xóa. |
| 5 | `using JetBrains.Annotations;` thừa (BodyPlayer). | Xóa. |

Tổng cộng **12 chỗ `.tag ==`** trên toàn project đã chuyển sang `CompareTag` (Trap, Teleport, Button, Button2, NextLevel, FinishLine, Seed, ButtonManager, ChangeGravity, 3 ground sensor).

### Menu.cs
| # | Vi phạm | Sửa |
|---|---------|-----|
| 1 | `instant = this;` trong **cả** `Awake()` và `Start()` — WET. | Giữ trong `Awake()`, xóa `Start()`. |
| 2 | `GameObject.Find("Canvas").transform.Find("Menu")` chạy **mỗi lần** bấm Resume/Pause — string-based hierarchy search lặp lại. | Cache `m_menuPanel` một lần trong `Awake()` (null-safe). |
| 3 | `TogglePause()` bị comment-out. | Xóa. |
| 4 | `Update()` trống. | Xóa. |

### MainMenu.cs / TranformScene.cs / NextLevel.cs / FinishLine.cs / GreenBlock.cs / Seed.cs
- Xóa `Start()`/`Update()` trống và `using` thừa (MainMenu, TranformScene, NextLevel).
- Magic build index `2`, scene name `"MainMenu"` → `const` có tên (`k_FirstLevelBuildIndex`, `k_MainMenuScene`).
- `GreenBlock.lifetime` (public, giữ tên): thêm `private` cho `Start()`; `[Tooltip]` đã có từ trước — giữ.
- `Seed`: `CompareTag`; giữ null-check `GreenLevelManager.Instance`.

### PlayerDataManager.cs
| # | Vi phạm | Sửa |
|---|---------|-----|
| 1 | Private field `currentLives` không prefix. | `m_currentLives`. |
| 2 | Hai wrapper một dòng `RestartCurrentLevel()`/`RestartGame()` chỉ để gọi `LoadScene` — indirection không cần thiết (KISS). | Inline vào `LoseLife()`. |
| 3 | `Debug.Log("Mất một mạng!...")` + string concat mỗi lần chết. | Xóa. |
| 4 | Magic `2` và comment tiếng Việt. | `k_FirstLevelBuildIndex`; TODO UI xóa theo YAGNI (tính năng chưa có thì đừng để TODO treo). |

### GreenLevelManager.cs
| # | Vi phạm | Sửa |
|---|---------|-----|
| 1 | `player.GetComponent<SpriteRenderer>()` trong `TryPlantBlock()` — GetComponent mỗi lần nhấn C. | Cache `m_playerSprite` trong `Start()`. |
| 2 | `Debug.Log` + concat trong `CollectSeed`/`TryPlantBlock` (chạy mỗi lần nhặt/trồng). | Xóa log thường, giữ `LogError` khi thiếu prefab/component. |
| 3 | Magic `KeyCode.C`, offset `1.2f`/`0.5f`. | `k_PlantKey`, `k_PlantXOffset`, `k_PlantYOffset` + comment "tại sao" (flipX = hướng nhìn). |
| 4 | Comment tiếng Việt/TODO UI. | Xóa; `[Tooltip]` cho serialized field. |

### SwitchController.cs
| # | Vi phạm | Sửa |
|---|---------|-----|
| 1 | `FindAnyObjectByType<AudioManager>()` trong `OnTriggerEnter2D`/`OnTriggerExit2D`. | Cache qua `PlaySound()`. |
| 2 | Magic color `new Color(1f, 0.64f, 0f)` (instance readonly). | `private static readonly Color k_OrangeActivatedColor`. |
| 3 | Logic chọn màu lồng `if/else` + tag check lặp 2 lần. | Ternary + helper `IsPushableBlock()` dùng chung cho Enter/Exit (DRY). |

### Stage 5–7 (Teleport / Trap / Button / Button2 / CloneMovement / FootClone / PosControlClone / ButtonManager / ChangeGravity)
| File | Vi phạm chính | Sửa |
|------|---------------|-----|
| Teleport.cs | `.tag ==`, không null-check `portal2`/`instant` | `CompareTag` + null guards |
| Trap.cs | `.tag ==` x2 | `CompareTag` + const tags |
| Button.cs | `.tag ==`, code comment-out, `ground.SetActive` không guard | `CompareTag`, xóa comment, null guard |
| Button2.cs | `.tag ==`, `Destroy(groundDesTroy)` không guard | `CompareTag`, null guards |
| CloneMovement.cs | `Debug.Log` mỗi frame, `FixedUpdate` trống, GetComponent trong Start, `"isRun"` string | Như PlayerMovementPlatform; giữ mirror `* -1f` + comment "tại sao" |
| PosControlClone.cs | `int flag` (0/1) thay vì bool — Enigmatic naming | `bool m_hasSpawned`; `CompareTag`; null guards |
| ButtonManager.cs | Magic `-0.2F`/`0.15F`, `"IsPushed"` string, `.tag ==`, `door.GetComponent` không guard | `k_PressedGravityScale`/`k_ReleasedGravityScale` consts, `k_IsPushedHash`, `CompareTag`, null-safe `SetDoorGravity()` |
| ChangeGravity.cs | `this.gameObject.tag ==`, `GetComponent<Rigidbody2D>()` trong trigger handler, magic `-1`/`1`/`180f` | `CompareTag`, lazy-cache `EnsureCached()` (tránh lỗi thứ tự Awake), consts đặt tên |

### IntroSceneController.cs
- `volume = 1f` magic → `k_MusicVolume`; xóa `using UnityEngine.SceneManagement` thừa; comment "gọi từ Timeline" giữ lại dạng "tại sao" bằng tiếng Anh.

---

## 2. Thống kê nhanh

- `FindAnyObjectByType` trong hot path/input handler: **8 call sites → 0** (lazy-cache ở 4 class: GameManager, SwitchController, PlayerMovement, PlayerMovementPlatform).
- `.tag ==` → `CompareTag`: **12 chỗ**.
- `Debug.Log` thường trong gameplay: **~20 → 0** (giữ `LogError` cho misconfiguration).
- Unity event rỗng (`Start`/`Update`/`FixedUpdate`): **xóa ~15 method**.
- Code comment-out: **xóa toàn bộ**.
- Animator string param mỗi frame: **3 file → cached hash**.
- Public API: **giữ nguyên 100%** (tên class/file, public field/method, signature).

---

## 3. KHÔNG sửa được + lý do (manual steps cho user làm trong editor)

1. **Player.cs — điều kiện pause bị ngược (BUG).** `if (Menu.instant.isPaused) { MovePlayer(); }` trong khi mọi script khác đều `if (isPaused) return;`. Không sửa vì ràng buộc "gameplay giữ nguyên 100%". → Xác nhận với designer: nếu là bug, đổi thành `if (!Menu.instant.isPaused) { MovePlayer(); }`.
2. **GameManager SRP split.** Tách `YellowLevelController` / `PushLevelController` cần file mới + gán lại reference trong scene. → Làm trong editor khi có nhu cầu thêm loại màn mới (lúc đó vi phạm OCP sẽ rõ).
3. **PlayerCollision / BodyPlayer / FootClone trùng lặp (DRY).** Gộp thành một `GroundSensor` cần đổi tên class/file → vỡ GUID + reference trong scene. → Làm trong editor: tạo class mới, thay component, xóa 3 class cũ.
4. **Shared constants (tags, sound names).** Các tag `"Player"`, `"Ground"`, `"Clone"` và sound `"Walk"`, `"Pushed"` lặp ở nhiều file dưới dạng `private const`. Gộp vào một `static class GameTags` cần file mới (vượt giới hạn 26 file). → Cân nhắc khi refactor lần sau.
5. **`goalSpawnPoint` (GameManager, public, legacy).** Giữ lại để không mất serialized data trong scene. → Xóa trong Inspector/editor khi chắc không còn scene nào dùng.
6. **SwitchController logic nhiều block.** Enter block A → `isActivated=true`; exit block A trong khi block B vẫn đè → `isActivated=false` (sai). Giữ nguyên behavior. → Sửa đúng cần đếm reference (counter), là thay đổi gameplay — cần designer xác nhận.
7. **PlayerCollision.OnCollisionExit2D** set `onGround=false` cho *bất kỳ* collider nào rời đi (kể cả không phải Ground). Giữ nguyên behavior. → Sửa đúng cần check tag trong Exit — thay đổi gameplay, cần test.
8. **AudioManager.cs (ngoài phạm vi 26 file).** `Play(string)` dùng `Array.Find` rồi gọi `s.source.Play()` không null-check → NRE nếu tên sound không tồn tại (ví dụ gõ sai `"Walk"`). → Thêm guard trong `AudioManager`: `if (s == null) { Debug.LogWarning(...); return; }`.
9. **Menu.cs cache giả định Canvas/Menu tồn tại lúc Awake.** Đúng với scene hiện tại (code gốc cũng Find cùng object đó). Nếu Canvas được instantiate động sau Awake, `m_menuPanel` sẽ null và nút Resume/Pause không ẩn/hiện panel (không crash nhờ null-check). → Kiểm tra trong editor nếu menu được spawn động.

---

## 4. Ghi chú verify

- Đủ 26 file `.cs` trong `Scripts/`, tên file trùng tên class `public`.
- Không đổi tên bất kỳ public field/method nào; mọi rename chỉ ở private (`m_`, `k_`, `HandleJump`, `MoveInDirection`, helper static).
- Không sót reference tới method đã đổi tên (kiểm tra: `Jump()`→`HandleJump()` chỉ gọi nội bộ trong `Update()` của cùng class; `MovePlayer()` giữ tên).
- Không thêm file mới, không thêm dependency, không đổi behavior gameplay (trừ các null-guard chỉ kích hoạt khi misconfiguration — trước đây sẽ crash NRE).
