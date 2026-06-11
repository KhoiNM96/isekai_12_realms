# SPEC GAME UNITY — ISEKAI 12 REALMS REBORN

## 0. Tên dự án

Tên tạm thời: **Isekai 12 Realms Reborn**
Tên Việt gợi ý: **Loạn 12 Dị Giới** / **Chuyển Sinh 12 Vương Vực**
Thể loại: **Offline RPG + Match-3 Turn-Based Combat + 2D Adventure**
Nền tảng: **Android trước, iOS sau**
Màn hình: **Mobile portrait, 9:16**
Phong cách: **Pixel/chibi fantasy, vui vẻ, nhẹ nhàng, không máu me**

## 1. Mục tiêu sản phẩm

Game lấy cảm hứng từ gameplay cổ điển của dòng RPG match-3 Java mobile: người chơi đi map 2D, gặp NPC, nhận nhiệm vụ, đánh quái bằng bàn xếp hình theo lượt, lên cấp, mặc trang bị, mở khu vực mới.

Bản mới không dùng bối cảnh lịch sử gốc, không dùng tên/asset/map/icon y hệt bản cũ. Toàn bộ được chuyển thành thế giới **isekai**: nhân vật chính chuyển sinh sang một lục địa bị chia cắt bởi 12 lãnh địa ma thuật. Mục tiêu là thống nhất lại 12 vương vực bằng cách giúp từng vùng giải quyết khủng hoảng, thu phục đồng minh và đánh bại các thủ lĩnh bị tha hóa.

## 2. Trụ cột thiết kế

### 2.1. Giữ lại cảm giác gốc

Game cần giữ các cảm giác sau:

* Màn hình dọc kiểu game Java/mobile cổ.
* Nhân vật chibi đứng trong map 2D nhiều tầng.
* NPC có tên trên đầu.
* Có thanh máu, năng lượng, EXP.
* Khi đánh nhau chuyển sang bàn match-3.
* Xếp biểu tượng tạo hành động chiến đấu.
* Xếp 4+ có thưởng thêm hoặc tạo ô đặc biệt.
* Có vàng, EXP, trang bị, kỹ năng, nhiệm vụ.
* Có cảm giác đi cảnh, luyện cấp, săn đồ.

### 2.2. Thay đổi để tránh sao chép nguyên trạng

Không dùng:

* Tên “Loạn 12 Sứ Quân”.
* Tên Đinh Bộ Lĩnh, Hoa Lư, Ngọc Nga, Lưu Cơ, Ma Ngưu nếu không được phép.
* Asset pixel, icon, UI, map giống hệt.
* Logo, âm thanh, câu thoại gốc.
* Cốt truyện lịch sử nguyên bản.

Thay bằng:

* 12 vương vực isekai.
* Nhân vật chuyển sinh.
* Quái dễ thương/fantasy.
* NPC pháp sư, thương nhân, học giả, thợ rèn.
* Icon mới: kiếm gỗ, tim pha lê, táo mana, đồng xu, sách phép, tinh thể sấm, khiên, sao.
* UI mới nhưng gợi nhớ game mobile cổ điển.

## 3. Game loop chính

```text
Mở game
→ Login guest hoặc Google
→ Load local save
→ Sync Firebase nếu có mạng
→ Vào Main Town
→ Nhận nhiệm vụ / chọn map / nâng cấp
→ Vào màn phiêu lưu 2D
→ Gặp quái / boss / event
→ Vào Battle Match-3
→ Thắng nhận EXP, gold, item, material
→ Lên cấp / nâng skill / mặc trang bị
→ Mở stage mới
→ Lặp lại
```

## 4. Bối cảnh isekai

### 4.1. Tóm tắt cốt truyện

Nhân vật chính là một người bình thường từ thế giới hiện đại. Sau một tai nạn kỳ lạ, nhân vật được chuyển sinh sang lục địa **Asteria**, nơi từng được bảo vệ bởi 12 viên **Realm Core**. Khi các Realm Core bị nứt vỡ, 12 vương vực tách rời nhau, quái vật xuất hiện, còn các lãnh chúa bị ảnh hưởng bởi năng lượng hỗn loạn.

Người chơi không phải “chinh phạt bằng bạo lực”, mà là **du hành, giải cứu, thanh tẩy và kết nối lại 12 vương vực**.

### 4.2. 12 vương vực

| ID  | Tên vùng        | Chủ đề           | Boss / thủ lĩnh | Cơ chế đặc biệt            |
| --- | --------------- | ---------------- | --------------- | -------------------------- |
| R01 | Meadow Gate     | Đồng cỏ nhập môn | Slime King      | Dạy match-3                |
| R02 | Ember Village   | Làng lửa         | Cinder Boar     | Ô lửa gây damage           |
| R03 | Tide Shrine     | Đền nước         | Bubble Serpent  | Hồi máu, khiên nước        |
| R04 | Thunder Peak    | Núi sấm          | Storm Roc       | Extra turn, chain          |
| R05 | Rootwood Forest | Rừng cổ thụ      | Elder Treant    | Poison/regen               |
| R06 | Crystal Mine    | Mỏ pha lê        | Gem Golem       | Ô đá cản đường             |
| R07 | Moon Bazaar     | Chợ mặt trăng    | Mask Merchant   | Cược/đổi vật phẩm bằng NPC |
| R08 | Snow Lantern    | Tuyết đăng       | Frost Kitsune   | Đóng băng ô                |
| R09 | Clock Ruin      | Phế tích đồng hồ | Time Warden     | Giới hạn lượt              |
| R10 | Candy Citadel   | Thành kẹo        | Sugar Dragon    | Combo nhiều tầng           |
| R11 | Sky Library     | Thư viện trời    | Rune Owl        | Câu đố / sách EXP          |
| R12 | Eclipse Throne  | Cung nhật thực   | Void Prince     | Boss cuối nhiều phase      |

## 5. Nhân vật người chơi

### 5.1. Tạo nhân vật

Người chơi tạo avatar chibi:

* Giới tính: không bắt buộc, chọn body type A/B/C.
* Tóc: 12 kiểu.
* Màu tóc: 12 màu.
* Mắt: 8 kiểu.
* Màu da: nhiều tone trung tính, không gắn định kiến.
* Trang phục ban đầu: Traveler, Apprentice, Farmer, Student.
* Tên nhân vật: 3–16 ký tự.

### 5.2. Class khởi đầu

Có 3 class chính, lấy cảm hứng từ Hỏa/Thủy/Lôi nhưng đổi sang fantasy isekai.

#### Flame Squire

Vai trò: sát thương ổn định.
Tài nguyên chính: Fire Mana.
Ưu tiên token: Sword, Flame Star.

Kỹ năng:

| Skill       | Loại     | Mô tả                           |
| ----------- | -------- | ------------------------------- |
| Spark Slash | Active   | Gây sát thương nhỏ lên địch     |
| Ember Combo | Active   | Tăng damage nếu vừa match Sword |
| Flame Burst | Ultimate | Gây sát thương lớn, phá 3x3 ô   |
| Warm Heart  | Passive  | Match Heart hồi thêm 5%         |

#### Tide Acolyte

Vai trò: sống lâu, hồi máu, khiên.
Tài nguyên chính: Water Mana.
Ưu tiên token: Heart, Shield.

Kỹ năng:

| Skill        | Loại     | Mô tả                                             |
| ------------ | -------- | ------------------------------------------------- |
| Aqua Heal    | Active   | Hồi máu theo Magic Power                          |
| Bubble Guard | Active   | Tạo khiên trong 2 lượt                            |
| Moon Tide    | Ultimate | Hồi máu lớn và xóa debuff                         |
| Gentle Flow  | Passive  | Match 4+ Heart nhận thêm lượt nhỏ hoặc bonus heal |

#### Storm Scout

Vai trò: combo, extra turn, tốc độ.
Tài nguyên chính: Thunder Mana.
Ưu tiên token: Star, Mana Orb.

Kỹ năng:

| Skill         | Loại     | Mô tả                               |
| ------------- | -------- | ----------------------------------- |
| Quick Jab     | Active   | Đánh nhẹ, tăng crit                 |
| Static Step   | Active   | Đổi vị trí 2 ô bất kỳ               |
| Thunder Chain | Ultimate | Gây damage theo số combo trong lượt |
| Lucky Spark   | Passive  | Xếp 4+ có cơ hội nhận thêm Mana Orb |

### 5.3. Đồng minh mở khóa sau

Người chơi có thể mở đồng minh để thay đổi phong cách chơi. Mỗi đồng minh không cần online, chỉ là party companion.

| Nhân vật               | Vai trò                      | Cách mở   |
| ---------------------- | ---------------------------- | --------- |
| Mira, Mail Cat         | Hỗ trợ nhặt vật phẩm         | Quest R01 |
| Brann, Tiny Blacksmith | Nâng cấp trang bị            | R02       |
| Nami, Shrine Healer    | Heal bonus                   | R03       |
| Lumo, Star Sprite      | Tăng EXP                     | R04       |
| Taro, Food Golem       | Tăng lương thực trong battle | R05       |
| Yuki, Lantern Fox      | Giảm hiệu ứng đóng băng      | R08       |

## 6. Chỉ số nhân vật

### 6.1. Chỉ số chính

| Stat  | Mô tả                              |
| ----- | ---------------------------------- |
| Level | Cấp nhân vật                       |
| EXP   | Kinh nghiệm                        |
| HP    | Máu tối đa                         |
| ATK   | Sát thương vật lý                  |
| MAG   | Sát thương/hồi phục phép           |
| DEF   | Giảm sát thương                    |
| SPD   | Ảnh hưởng thứ tự lượt, crit, extra |
| LUCK  | Tỷ lệ rơi đồ, crit nhẹ             |
| FOOD  | Lương thực trong battle            |
| MANA  | Năng lượng dùng skill              |

### 6.2. Công thức đề xuất

```text
MaxHP = 100 + Level * 18 + VITBonus + EquipmentHP
ATK = BaseATK + Level * 2 + WeaponATK
DEF = BaseDEF + ArmorDEF
Damage = max(1, ATK * SkillMultiplier - EnemyDEF * 0.6)
Heal = MAG * HealMultiplier + Level * 2
FoodDrainPerTurn = 1
```

### 6.3. Level curve

```text
EXPRequired(level) = floor(50 * level^1.45)
```

Level cap bản đầu: 60.
Sau update có thể mở 80, 100 qua config.

## 7. Hệ thống battle match-3

### 7.1. Mục tiêu

Mỗi trận là cuộc đấu theo lượt giữa player và enemy. Người chơi đổi vị trí 2 ô kề nhau trên bàn 8x8 để tạo hàng/cột từ 3 biểu tượng giống nhau trở lên. Biểu tượng được match sẽ chuyển thành hành động RPG.

### 7.2. Kích thước board

Mặc định:

```text
Board: 8 x 8
Tile size reference: 96 px trên canvas 1080x1920
Board width: 768 px
Board height: 768 px
```

Có thể có stage đặc biệt:

```text
R01-R03: 8x8
R04-R06: 8x8 + obstacle
R07-R09: 7x8 hoặc 8x8 + locked tile
R10-R12: 8x9 boss board
```

### 7.3. Token cơ bản

| Token    | Tên           | Tác dụng                        |
| -------- | ------------- | ------------------------------- |
| Sword    | Kiếm          | Gây sát thương                  |
| Heart    | Tim pha lê    | Hồi máu                         |
| Coin     | Đồng vàng     | Nhận gold sau battle            |
| Food     | Táo/bánh mana | Tăng food, chống cạn lương thực |
| Book     | Sách phép     | Tăng EXP bonus                  |
| Mana Orb | Cầu mana      | Nạp thanh skill                 |
| Shield   | Khiên         | Tạo giáp                        |
| Star     | Sao dị giới   | Tăng class resource             |

### 7.4. Token đặc biệt

| Điều kiện tạo       | Token đặc biệt | Tác dụng                    |
| ------------------- | -------------- | --------------------------- |
| Match 4 hàng ngang  | Row Rune       | Xóa cả hàng                 |
| Match 4 hàng dọc    | Column Rune    | Xóa cả cột                  |
| Match 5 thẳng       | Realm Crystal  | Xóa toàn bộ token cùng loại |
| Match L/T           | Bomb Rune      | Nổ vùng 3x3                 |
| Combo 3 lần trở lên | Combo Spark    | Gây thêm damage nhỏ         |

### 7.5. Luật lượt

```text
1. Player chọn 2 ô kề nhau.
2. Nếu swap không tạo match, swap trả lại.
3. Nếu tạo match, resolve token.
4. Token rơi xuống, sinh token mới.
5. Nếu cascade tiếp tục match, tiếp tục resolve.
6. Tổng hợp reward/action của lượt.
7. Nếu match 4+ hoặc dùng skill tạo extraTurn, player được đi tiếp.
8. Nếu không, chuyển lượt enemy.
9. Enemy dùng AI chọn nước đi.
10. Lặp đến khi player HP <= 0 hoặc enemy HP <= 0 hoặc food <= 0.
```

### 7.6. Lương thực / Food

Food là cơ chế chiến thuật để tránh trận kéo dài.

```text
Bắt đầu battle: Food = PlayerBaseFood + ItemBonus
Mỗi lượt player: Food -= 1
Match Food token: Food += số token * 2
Food <= 0: player nhận ExhaustionDamage mỗi lượt
ExhaustionDamage = 5% MaxHP
```

Không nên cho thua ngay khi hết Food, vì sẽ gây khó chịu. Thay vào đó dùng sát thương kiệt sức.

### 7.7. Enemy AI

AI bản đầu dùng heuristic, không cần machine learning.

Điểm nước đi:

```text
score = 0
+ SwordMatch * enemyNeedAttackWeight
+ HeartMatch * enemyLowHpWeight
+ ManaMatch * enemySkillWeight
+ FoodMatch * foodWeight
+ SpecialCreated * 50
+ ExtraTurnPotential * 80
+ CascadePotential * 20
```

AI có 3 độ khó:

| Difficulty | Mô tả                                       |
| ---------- | ------------------------------------------- |
| Easy       | Chọn trong top 5 move ngẫu nhiên            |
| Normal     | Chọn trong top 3 move                       |
| Hard       | Chọn move điểm cao nhất, ưu tiên extra turn |

### 7.8. Board modifier

| Modifier    | Vùng dùng    | Mô tả                           |
| ----------- | ------------ | ------------------------------- |
| Locked Tile | Crystal Mine | Ô bị khóa, cần match cạnh để mở |
| Ice Tile    | Snow Lantern | Ô bị đóng băng trong 1–2 lượt   |
| Thorn Tile  | Rootwood     | Match gần gai nhận damage nhỏ   |
| Time Tile   | Clock Ruin   | Không phá trong X lượt sẽ nổ    |
| Bless Tile  | Shrine       | Match qua ô này tăng heal       |
| Wild Tile   | Late game    | Có thể thay thế bất kỳ token    |

## 8. Skill system

### 8.1. Skill slot trong battle

Mỗi nhân vật có:

* 1 Basic Skill.
* 2 Active Skill.
* 1 Ultimate.
* 1 Passive.

Trong UI battle hiển thị 3 nút skill:

```text
[Skill 1] [Skill 2] [Ultimate]
```

Passive không cần nút.

### 8.2. Mana cost

| Skill loại | Cost                   |
| ---------- | ---------------------- |
| Basic      | 0 hoặc cooldown 2 lượt |
| Active     | 30–60 mana             |
| Ultimate   | 100 mana               |

Mana nạp từ:

```text
Match Mana Orb: +8 mana/token
Match Star đúng class: +10 mana/token
Combo >= 2: +5 mana
Bị đánh: +3 mana
```

### 8.3. Skill data structure

```csharp
public enum SkillTargetType
{
    Enemy,
    Player,
    BoardTile,
    RandomTiles,
    AllEnemies,
    Self
}

public enum SkillEffectType
{
    Damage,
    Heal,
    Shield,
    ConvertTiles,
    DestroyTiles,
    ExtraTurn,
    Buff,
    Debuff,
    Cleanse
}

[CreateAssetMenu(menuName = "Game/Skill Definition")]
public class SkillDefinition : ScriptableObject
{
    public string id;
    public string displayName;
    public Sprite icon;
    public int manaCost;
    public int cooldown;
    public SkillTargetType targetType;
    public List<SkillEffectData> effects;
}
```

## 9. Trang bị

### 9.1. Slot trang bị

| Slot   | Ví dụ                                    |
| ------ | ---------------------------------------- |
| Weapon | Wooden Sword, Spark Wand, Thunder Dagger |
| Armor  | Traveler Coat, Shell Armor               |
| Head   | Leaf Hood, Mage Hat                      |
| Boots  | Cloud Shoes                              |
| Ring   | Lucky Ring                               |
| Charm  | Realm Charm                              |

### 9.2. Rarity

| Rarity    | Màu UI     | Số substat |
| --------- | ---------- | ---------- |
| Common    | Xám        | 0–1        |
| Uncommon  | Xanh lá    | 1          |
| Rare      | Xanh dương | 2          |
| Epic      | Tím        | 3          |
| Legendary | Vàng       | 4          |

### 9.3. Stat trang bị

| Stat      | Tác dụng                |
| --------- | ----------------------- |
| HPFlat    | Tăng HP                 |
| ATKFlat   | Tăng ATK                |
| DEFFlat   | Tăng DEF                |
| MAGFlat   | Tăng MAG                |
| CritRate  | Tăng crit               |
| HealBonus | Tăng hồi máu            |
| GoldBonus | Tăng gold               |
| ExpBonus  | Tăng EXP                |
| FoodBonus | Tăng food đầu trận      |
| DropRate  | Tăng tỷ lệ rơi đồ       |
| ManaGain  | Tăng mana nhận từ token |

### 9.4. Nâng cấp trang bị

Trang bị nâng bằng gold + material.

```text
Max level Common: +5
Max level Uncommon: +8
Max level Rare: +12
Max level Epic: +16
Max level Legendary: +20
```

Không nên cho IAP mua trực tiếp trang bị mạnh. Người chơi có thể dùng premium currency đổi gold/material giới hạn ngày, nhưng trang bị chính vẫn rơi từ stage/boss/craft.

### 9.5. Craft

Người chơi thu nguyên liệu:

* Slime Jelly.
* Ember Dust.
* Tide Pearl.
* Thunder Feather.
* Crystal Shard.
* Moon Ticket.
* Star Ink.

Craft dùng recipe trong Blacksmith.

## 10. Vật phẩm tiêu hao

| Item          | Tác dụng                  | Cách nhận   |
| ------------- | ------------------------- | ----------- |
| Small Potion  | Hồi 20% HP ngoài battle   | Drop/shop   |
| Food Basket   | +10 food trận kế          | Quest/shop  |
| Shuffle Bell  | Xáo board 1 lần           | Reward/shop |
| Lucky Cookie  | +10% drop 5 trận          | Daily/shop  |
| Repair Hammer | Mở locked tile đầu battle | Craft       |
| Skill Scroll  | Nâng skill                | Boss/chest  |

IAP không bán item trực tiếp. Shop có thể bán bằng premium currency, nhưng giao dịch là dùng tiền tệ trong game, không phải product IAP riêng.

## 11. Tiền tệ và IAP

### 11.1. Các loại tiền

| Currency    | Nguồn                                | Dùng cho                                |
| ----------- | ------------------------------------ | --------------------------------------- |
| Gold        | Stage, quest, coin token             | Upgrade, craft, mua item thường         |
| Soul Gem    | IAP, achievement hiếm, event offline | Đổi gold, mua cosmetic, mua convenience |
| Material    | Drop                                 | Craft, upgrade                          |
| Realm Token | Boss reward                          | Mở vùng, craft đồ đặc biệt              |

### 11.2. IAP chỉ mua tiền tệ

Product IAP đều là consumable, chỉ cấp **Soul Gem**.

| Product ID  | Giá gợi ý | Soul Gem | Bonus |
| ----------- | --------: | -------: | ----: |
| gems_tiny   |     $0.99 |      120 |     0 |
| gems_small  |     $2.99 |      400 |   10% |
| gems_medium |     $4.99 |      750 |   25% |
| gems_large  |     $9.99 |    1,650 |   38% |
| gems_mega   |    $19.99 |    3,600 |   50% |

Có thể đổi theo thị trường sau khi test.

### 11.3. Nguyên tắc chống pay-to-win quá mạnh

Soul Gem được dùng cho:

* Mua cosmetic.
* Mua thêm slot inventory.
* Đổi gold giới hạn ngày.
* Mua item tiện ích nhẹ.
* Reset skill build.
* Mua Battle Pass offline nếu sau này có, nhưng không bắt buộc.

Không dùng Soul Gem để:

* Mua trang bị Legendary trực tiếp.
* Mua level trực tiếp.
* Bỏ qua toàn bộ campaign.
* Mua sức mạnh độc quyền chỉ IAP mới có.

### 11.4. Luồng mua IAP

```text
Player mở Shop
→ Chọn gem pack
→ Unity IAP gọi store
→ Store trả receipt
→ IAPService kiểm tra receipt local
→ Nếu transactionId chưa grant:
    → cộng Soul Gem
    → ghi local PurchaseLedger
    → ghi cloud ledger nếu online
→ Nếu đã grant:
    → không cộng lại
→ Hiện popup thành công
```

### 11.5. Purchase ledger

Không chỉ lưu số dư gem. Phải lưu transaction log để tránh cộng trùng.

```json
{
  "transactionId": "GPA.xxx",
  "productId": "gems_small",
  "amount": 400,
  "bonus": 40,
  "totalGranted": 440,
  "platform": "GooglePlay",
  "purchasedAt": 1710000000,
  "grantedAt": 1710000010,
  "receiptHash": "sha256..."
}
```

## 12. Save game offline-first

### 12.1. Nguyên tắc

Game phải chơi được offline. Firebase dùng cho:

* Backup save.
* Đồng bộ khi đổi máy.
* Login guest/Google.
* Lưu purchase ledger.
* Lưu config/event nếu có mạng.

Không được phụ thuộc Firebase để đánh battle thường.

### 12.2. Local save

Lưu tại:

```text
Application.persistentDataPath/save_v1.dat
Application.persistentDataPath/settings_v1.json
Application.persistentDataPath/purchase_ledger_v1.dat
```

Local save cần:

* JSON serialize.
* Nén optional.
* Mã hóa nhẹ bằng AES.
* HMAC để phát hiện sửa file.
* Auto-save sau mỗi battle, upgrade, purchase.
* Backup 2 slot: current + previous.

### 12.3. Cloud save

Cloud chỉ là bản backup/sync.

Firestore path đề xuất:

```text
/users/{uid}/profile/main
/users/{uid}/saves/default
/users/{uid}/purchases/{transactionId}
/users/{uid}/settings/main
```

### 12.4. Save conflict

Mỗi save có:

```json
{
  "saveVersion": 12,
  "updatedAt": 1710000000,
  "deviceId": "local-device-id",
  "playerLevel": 24,
  "mainQuest": "R04_Q08",
  "totalPlaySeconds": 40520,
  "checksum": "..."
}
```

Khi login:

```text
Nếu local mới hơn cloud → hỏi upload local lên cloud.
Nếu cloud mới hơn local → hỏi tải cloud về.
Nếu khác device và cùng version → hiện popup so sánh.
Purchase ledger luôn merge theo transactionId.
Currency premium tính theo ledger + earnedGemLog, không tin tuyệt đối vào balance nếu có conflict.
```

### 12.5. Guest login

Luồng guest:

```text
Lần đầu mở game:
- Tạo localGuestId nếu chưa có.
- Nếu có mạng, sign in Firebase Anonymous.
- Lưu uid vào local auth cache.
- Save cloud dưới uid đó.

Khi người chơi bấm Login Google:
- Đăng nhập Google.
- Link Google credential vào anonymous account nếu còn hợp lệ.
- Giữ nguyên uid nếu link thành công.
- Nếu đã có Google account cũ:
    - Hỏi người chơi chọn save local hay cloud.
```

## 13. Firebase architecture

### 13.1. Firebase modules

Dùng:

* Firebase Auth.
* Cloud Firestore hoặc Realtime Database.
* Firebase Analytics optional.
* Firebase Crashlytics optional.
* Firebase Remote Config optional.
* Cloud Functions optional cho verify purchase nâng cao.

Khuyến nghị bản đầu:

```text
Auth + Firestore + Crashlytics
```

Sau MVP mới thêm Remote Config/Functions.

### 13.2. Firestore schema

```text
/users/{uid}
    createdAt
    lastLoginAt
    loginType
    banned
    appVersion

/users/{uid}/profile/main
    playerName
    avatar
    level
    currentRealm
    createdAt
    updatedAt

/users/{uid}/saves/default
    saveVersion
    dataBlob
    checksum
    updatedAt
    deviceId

/users/{uid}/purchases/{transactionId}
    productId
    amount
    platform
    receiptHash
    purchasedAt
    grantedAt

/users/{uid}/settings/main
    music
    sfx
    language
    cloudSync
```

### 13.3. Security rules mẫu

```js
rules_version = '2';

service cloud.firestore {
  match /databases/{database}/documents {
    function signedIn() {
      return request.auth != null;
    }

    function isOwner(uid) {
      return signedIn() && request.auth.uid == uid;
    }

    match /users/{uid} {
      allow read, write: if isOwner(uid);

      match /{document=**} {
        allow read, write: if isOwner(uid);
      }
    }

    match /publicConfig/{docId} {
      allow read: if true;
      allow write: if false;
    }
  }
}
```

### 13.4. Cloud sync service

Interface:

```csharp
public interface ICloudSaveService
{
    Task<AuthResult> SignInGuestAsync();
    Task<AuthResult> SignInGoogleAsync();
    Task<CloudSaveMeta> GetCloudMetaAsync();
    Task UploadSaveAsync(PlayerSaveData save);
    Task<PlayerSaveData> DownloadSaveAsync();
    Task MergePurchaseLedgerAsync(List<PurchaseRecord> localRecords);
}
```

## 14. Unity technical design

### 14.1. Unity version

Khuyến nghị:

```text
Unity 6 LTS nếu bắt đầu dự án mới.
Unity 2022 LTS nếu cần tương thích nhiều asset cũ.
```

Target ban đầu:

```text
Android min API: 23+
Orientation: Portrait
Scripting Backend: IL2CPP
Architecture: ARM64
Managed Stripping: Medium
```

### 14.2. Package cần dùng

Bắt buộc:

* TextMeshPro.
* Unity UI/uGUI.
* Unity IAP.
* Firebase Unity SDK: Auth, Firestore.
* Addressables.
* 2D Sprite.
* 2D Tilemap.
* Input System hoặc EventSystem mặc định.

Optional:

* DOTween cho UI animation.
* Newtonsoft Json nếu cần.
* Spine/DragonBones nếu dùng animation xương.
* Firebase Crashlytics.
* Firebase Remote Config.

### 14.3. Scene structure

Tối ưu cho mobile: ít scene, nhiều popup layer.

```text
BootScene
- Init services
- Load config
- Check local save
- Auto login guest nếu có mạng
- Chuyển MainScene

MainScene
- Town/Map/Adventure UI
- Hero UI
- Inventory UI
- Quest UI
- Shop UI
- Popup layer

BattleScene hoặc BattleSubScene
- Board
- Character battle sprites
- Enemy
- Skill buttons
- Result popup
```

Có thể làm chỉ 2 scene:

```text
BootScene
GameScene
```

Trong GameScene dùng state machine:

```text
GameState.MainTown
GameState.WorldMap
GameState.Adventure
GameState.Battle
GameState.Result
```

### 14.4. Folder structure

```text
Assets/
  _Game/
    Art/
      Characters/
      Enemies/
      UI/
      Icons/
      Tilesets/
      VFX/
    Audio/
      BGM/
      SFX/
    Prefabs/
      UI/
      Characters/
      Battle/
      Map/
      Popups/
    Scenes/
      BootScene.unity
      GameScene.unity
    Scripts/
      Core/
      Services/
      Data/
      Battle/
      Board/
      Character/
      Inventory/
      Quest/
      UI/
      Firebase/
      IAP/
      Editor/
    ScriptableObjects/
      Characters/
      Skills/
      Items/
      Stages/
      Enemies/
      DropTables/
      Economy/
    Addressables/
      Remote/
      Local/
```

### 14.5. Code architecture

Dùng kiểu service + state machine + ScriptableObject data.

```text
GameManager
├── ServiceLocator
├── GameStateMachine
├── SaveService
├── CloudSaveService
├── AuthService
├── IAPService
├── EconomyService
├── InventoryService
├── QuestService
├── BattleService
├── BoardService
├── AudioService
└── PopupService
```

Không để UI trực tiếp xử lý logic battle/save/IAP. UI chỉ gọi service.

### 14.6. Data-driven design

Static data dùng ScriptableObject:

* CharacterDefinition.
* SkillDefinition.
* ItemDefinition.
* EquipmentDefinition.
* EnemyDefinition.
* StageDefinition.
* DropTableDefinition.
* RealmDefinition.
* EconomyDefinition.

Runtime data lưu JSON:

* PlayerSaveData.
* InventorySaveData.
* QuestSaveData.
* EquipmentInstanceData.
* PurchaseLedgerData.
* SettingsData.

## 15. Model dữ liệu chính

### 15.1. PlayerSaveData

```csharp
[Serializable]
public class PlayerSaveData
{
    public int schemaVersion = 1;
    public long saveVersion;
    public string playerId;
    public string firebaseUid;
    public string localGuestId;
    public string playerName;
    public AvatarData avatar;

    public int level;
    public long exp;
    public int gold;
    public int soulGem;

    public string currentRealmId;
    public string currentStageId;
    public List<string> unlockedRealmIds;
    public List<string> completedStageIds;

    public CharacterRuntimeData character;
    public InventorySaveData inventory;
    public QuestSaveData quests;
    public List<PurchaseRecord> purchases;

    public long createdAt;
    public long updatedAt;
    public string deviceId;
    public string checksum;
}
```

### 15.2. StageDefinition

```csharp
[CreateAssetMenu(menuName = "Game/Stage Definition")]
public class StageDefinition : ScriptableObject
{
    public string id;
    public string realmId;
    public string displayName;
    public int recommendedLevel;
    public Sprite background;
    public EnemyDefinition enemy;
    public BoardConfig boardConfig;
    public DropTableDefinition dropTable;
    public int goldReward;
    public int expReward;
    public List<string> requiredCompletedStages;
    public List<DialogueLine> preBattleDialogue;
    public List<DialogueLine> postBattleDialogue;
}
```

### 15.3. Item instance

```csharp
[Serializable]
public class EquipmentInstanceData
{
    public string instanceId;
    public string definitionId;
    public int level;
    public Rarity rarity;
    public List<StatModifier> modifiers;
    public bool locked;
    public long acquiredAt;
}
```

## 16. UI/UX tổng thể

### 16.1. Reference resolution

Thiết kế theo:

```text
Canvas reference: 1080 x 1920
Canvas Scaler: Scale With Screen Size
Match: 0.5
Safe Area: có
Orientation: Portrait only
```

### 16.2. Layer UI

```text
RootCanvas
├── SafeAreaRoot
│   ├── BackgroundLayer
│   ├── MainLayer
│   ├── HudLayer
│   ├── NavigationLayer
│   ├── PopupLayer
│   ├── ToastLayer
│   └── LoadingLayer
```

Popup luôn nằm trên MainLayer.
Toast nằm trên Popup nhưng dưới Loading.
Loading che toàn màn hình khi chuyển scene/sync.

### 16.3. Style UI

Phong cách:

* Pixel/chibi nhưng UI rõ trên mobile hiện đại.
* Panel bo góc lớn.
* Viền fantasy vàng/kem.
* Background pastel.
* Icon lớn, dễ đọc.
* Button cao tối thiểu 88 px.
* Text chính tối thiểu 28–32 px ở 1080x1920.
* Dùng shadow/outline để đọc tốt.

Màu chủ đạo:

```text
Primary: xanh ngọc / cyan fantasy
Secondary: vàng mật ong
Danger: đỏ cam
Success: xanh lá
Panel: kem nhạt / xanh đêm trong suốt
```

## 17. UI màn hình chi tiết

### 17.1. Boot / Splash Screen

Layout:

```text
[Full background isekai sky]
[Center] Game logo
[Below logo] Loading bar
[Bottom] Version text
```

Nội dung:

* Logo game.
* Dòng “Loading Realm Gate...”
* Progress bar.
* Version: v0.1.0.

Logic:

```text
- Init Firebase dependency.
- Load local config.
- Load local save.
- Try silent guest auth nếu có mạng.
- Load addressables local catalog.
- Chuyển MainScene.
```

### 17.2. Title Screen

Layout:

```text
Top: Logo
Middle: Character idle animation
Bottom:
    [Play as Guest]
    [Sign in with Google]
    [Settings]
```

Trạng thái:

* Nếu đã có save: nút “Continue”.
* Nếu chưa có save: “New Adventure”.
* Nếu có cloud conflict: popup chọn save.

### 17.3. Character Creation

Layout:

```text
Top: "Create Your Reborn Hero"
Center left/right: Character preview
Below preview: name input
Tabs:
    Hair
    Face
    Outfit
    Color
Bottom:
    [Back] [Start Journey]
```

Yêu cầu:

* Randomize button.
* Không khóa màu da/tóc bằng IAP.
* Outfit cosmetic có thể mở sau.

### 17.4. Main Town

Đây là màn chính sau login.

Layout portrait:

```text
┌────────────────────────────┐
│ Top HUD                    │
│ Avatar Lv Name HP/EXP      │
│ Gold SoulGem Settings      │
├────────────────────────────┤
│                            │
│  2D Town Scene             │
│  Player đứng giữa          │
│  NPC: Quest, Shop, Forge   │
│  Floating icons            │
│                            │
├────────────────────────────┤
│ Quest Tracker              │
├────────────────────────────┤
│ Bottom Navigation          │
│ Adventure Hero Bag Shop    │
└────────────────────────────┘
```

Top HUD height: 140 px.
Bottom nav height: 150 px.
Main view: còn lại.

Top HUD gồm:

* Avatar tròn.
* Tên + level.
* EXP mini bar.
* Gold.
* Soul Gem.
* Mail/Settings.

Bottom nav:

| Button    | Mở               |
| --------- | ---------------- |
| Adventure | World Map        |
| Hero      | Character screen |
| Bag       | Inventory        |
| Quest     | Quest popup      |
| Shop      | Shop/IAP         |

### 17.5. World Map

Layout:

```text
Top: Realm name + progress
Center: Scrollable vertical map
    Realm nodes R01–R12
    Stage nodes nối bằng đường sáng
Bottom: Selected stage card
```

Stage node states:

| State     | Visual                |
| --------- | --------------------- |
| Locked    | Xám, khóa             |
| Available | Sáng                  |
| Completed | Có dấu check          |
| Boss      | Icon lớn, viền đỏ/tím |
| Farmable  | Có biểu tượng chest   |

Stage card:

```text
Stage name
Recommended Lv
Enemy preview
Reward icons
[Enter]
[Auto Team] optional
```

### 17.6. Adventure 2D Map

Màn đi cảnh giống cảm giác Java cổ.

Layout:

```text
Top HUD nhỏ:
    HP bar, mana bar, map name
Center:
    2D side-scrolling tilemap
    Player chibi
    NPC/enemy
Bottom:
    Virtual joystick left
    Action button right
    Quest hint center
```

Điều khiển:

* Joystick trái: di chuyển ngang.
* Nút Jump nếu có platform nhẹ.
* Nút Action khi gần NPC/chest/enemy.
* Có thể bỏ Jump bản MVP để đơn giản, chỉ đi ngang + lên xuống điểm chuyển tầng.

NPC interaction:

```text
Đến gần NPC
→ hiện bubble "Talk"
→ tap Action
→ mở dialogue popup
```

Enemy interaction:

```text
Đụng enemy hoặc bấm Attack
→ mở Battle
```

### 17.7. Dialogue Popup

Layout:

```text
Bottom 35% màn hình
Left: NPC portrait
Right: NPC name
Below: Dialogue text
Bottom: [Skip] [Next]
```

Có lựa chọn:

```text
[Accept Quest]
[Shop]
[Leave]
```

### 17.8. Battle Screen

Quan trọng nhất.

Layout reference 1080x1920:

```text
┌────────────────────────────┐
│ Enemy Area 260 px          │
│ Enemy sprite + HP + status │
├────────────────────────────┤
│ Battle Info 90 px          │
│ Turn, Food, Combo, Mana    │
├────────────────────────────┤
│ Match-3 Board 900 px       │
│ 8x8 tiles                  │
├────────────────────────────┤
│ Player Area 260 px         │
│ Player sprite, HP, shield  │
├────────────────────────────┤
│ Skill Bar 180 px           │
│ Skill1 Skill2 Ultimate Bag │
└────────────────────────────┘
```

Chi tiết:

Enemy area:

* Enemy name.
* Enemy level.
* HP bar.
* Status icons.
* Enemy animation idle/hurt.

Battle info:

* Turn indicator: “Your Turn” / “Enemy Turn”.
* Food count.
* Combo count.
* Pause button.

Board:

* 8x8 square.
* Tile có viền rõ.
* Khi kéo swap có animation.
* Match nổ có VFX nhẹ.
* Cascade nhanh nhưng đọc được.

Player area:

* Player sprite.
* HP bar.
* Shield bar.
* Mana bar.
* Buff/debuff icons.

Skill bar:

```text
[Skill 1] [Skill 2] [Ultimate] [Item]
```

Skill button state:

* Đủ mana: sáng.
* Thiếu mana: xám + hiển thị cost.
* Cooldown: overlay số lượt.

### 17.9. Battle Result Popup

Win:

```text
Victory!
EXP +xxx
Gold +xxx
Drops:
    item icons
Buttons:
    [Next]
    [Replay]
    [Back to Map]
```

Lose:

```text
Try Again
Reason: HP depleted / Exhausted
Tips: Match Food to avoid exhaustion
Buttons:
    [Retry]
    [Upgrade]
    [Back]
```

Không trừ tiền khi thua ở bản đầu để tránh khó chịu.

### 17.10. Hero Screen

Layout:

```text
Top: Character name, level, class
Left/Center: Character full body
Right: Stats panel
Bottom tabs:
    Stats | Skills | Equipment | Cosmetic
```

Stats panel:

* HP.
* ATK.
* MAG.
* DEF.
* SPD.
* LUCK.
* Food.
* Drop bonus.

Buttons:

* Upgrade skill.
* Change class nếu mở sau.
* Details.

### 17.11. Skill Screen

Layout:

```text
Top: Class name + mana type
Center: Skill tree
Bottom: Selected skill detail
    Icon
    Description
    Current level
    Next level
    Cost
    [Upgrade]
```

Skill tree dạng 3 nhánh:

* Attack.
* Survival.
* Utility.

Cost:

```text
Gold + Skill Scroll
```

### 17.12. Equipment Screen

Layout:

```text
Center: Character preview
Around character:
    Head
    Weapon
    Armor
    Boots
    Ring
    Charm
Right/Bottom: Selected equipment details
Bottom:
    [Equip] [Upgrade] [Enhance] [Lock]
```

Equipment detail:

```text
Name
Rarity
Level
Main stat
Sub stats
Set bonus nếu có
```

### 17.13. Inventory Screen

Tabs:

```text
All | Equipment | Material | Consumable | Quest
```

Layout:

```text
Top: Capacity 45/80
Center: Grid 5 columns
Bottom: Item detail panel
```

Actions:

* Use.
* Equip.
* Upgrade.
* Sell.
* Lock.
* Sort.

### 17.14. Shop Screen

Tabs:

```text
Daily | Gold Shop | Gem Shop | IAP
```

Daily Shop:

* Mua bằng Gold.
* Reset mỗi ngày local time.
* Không yêu cầu online.

Gem Shop:

* Mua bằng Soul Gem.
* Cosmetic/convenience.

IAP tab:

```text
Soul Gem Packs only
[gems_tiny]
[gems_small]
[gems_medium]
[gems_large]
[gems_mega]
```

Hiển thị rõ:

```text
IAP only grants Soul Gems. No direct equipment purchase.
```

### 17.15. Cloud Sync Popup

Layout:

```text
Title: Cloud Save Found
Left card: Local Save
Right card: Cloud Save
Each card:
    Level
    Realm
    Gold
    Soul Gem
    Updated time
Buttons:
    [Use Local]
    [Use Cloud]
    [Cancel]
```

### 17.16. Settings Screen

Sections:

* Account.
* Audio.
* Graphics.
* Language.
* Cloud Save.
* Privacy.
* Restore Purchases.
* Support.

Buttons:

```text
[Sign in with Google]
[Sync Now]
[Restore Purchases]
[Delete Local Save]
```

Delete save cần confirm 2 bước.

## 18. Art direction

### 18.1. Nhân vật

Style:

* Chibi 2D.
* Đầu hơi lớn.
* Mắt rõ.
* Animation đơn giản.
* Không máu, không gore.
* Hit effect là sao, khói, ánh sáng.

Animation tối thiểu:

| Animation | Frame |
| --------- | ----: |
| Idle      |     4 |
| Walk      |     6 |
| Attack    |     4 |
| Cast      |     4 |
| Hurt      |     2 |
| Victory   |     4 |
| Defeat    |     2 |

### 18.2. Enemy

Enemy dễ thương/fantasy:

* Slime.
* Mushroom.
* Tiny Boar.
* Crystal Golem.
* Lantern Fox.
* Cloud Bird.
* Book Owl.
* Candy Dragon.

Không thiết kế quái quá kinh dị.

### 18.3. Tile icon

Mỗi token cần rõ ở size nhỏ.

| Token    | Shape          |
| -------- | -------------- |
| Sword    | Kiếm ngắn sáng |
| Heart    | Tim pha lê     |
| Coin     | Đồng xu tròn   |
| Food     | Táo/bánh       |
| Book     | Sách mở        |
| Mana Orb | Cầu xanh       |
| Shield   | Khiên          |
| Star     | Sao vàng       |

Icon phải phân biệt bằng cả hình, không chỉ màu.

### 18.4. VFX

* Match 3: pop nhẹ.
* Match 4: line shine.
* Match 5: burst.
* Heal: hạt xanh/tim bay lên.
* Damage: số damage nảy.
* Shield: vòng sáng.
* Ultimate: full-screen flash nhẹ 0.2s.

## 19. Audio

### 19.1. BGM

| Màn           | Nhạc                       |
| ------------- | -------------------------- |
| Title         | Fantasy soft               |
| Town          | Vui, nhẹ                   |
| World Map     | Phiêu lưu                  |
| Battle thường | Nhịp nhanh vừa             |
| Boss          | Kịch tính nhưng không nặng |
| Victory       | Jingle ngắn                |
| Shop          | Dễ thương                  |

### 19.2. SFX

* Button tap.
* Popup open/close.
* Tile swap.
* Match.
* Combo.
* Skill cast.
* Hit.
* Heal.
* Coin.
* Item drop.
* Level up.

## 20. Quest system

### 20.1. Loại quest

| Type           | Mô tả                  |
| -------------- | ---------------------- |
| Main Quest     | Mở cốt truyện/vùng mới |
| Side Quest     | NPC phụ                |
| Daily Quest    | Offline daily          |
| Achievement    | Thành tựu dài hạn      |
| Tutorial Quest | Dạy hệ thống           |

### 20.2. Quest objective

```csharp
public enum QuestObjectiveType
{
    CompleteStage,
    DefeatEnemy,
    CollectItem,
    UpgradeEquipment,
    ReachLevel,
    TalkToNpc,
    MatchTokenCount,
    UseSkill
}
```

### 20.3. Daily quest

Daily offline không nên quá phức tạp:

* Win 3 battles.
* Match 50 Sword.
* Collect 500 Gold.
* Upgrade 1 item.
* Complete 1 old stage.

Reward: Gold, Soul Gem nhỏ, material.

## 21. Stage replay và drop đồ

Người chơi nên được đánh lại level cũ để farm đồ.

### 21.1. Replay rule

* Stage đã clear có nút Replay.
* Replay tốn không tốn energy.
* Reward EXP giảm 30% nếu farm quá nhiều.
* Boss stage có daily bonus 1 lần/ngày.
* Drop rare vẫn có nhưng tỷ lệ thấp.

### 21.2. Drop table

Ví dụ R02 boss:

```text
Gold: 120–180
EXP: 90
Common material: 80%
Rare material: 25%
Rare weapon: 8%
Epic weapon: 1.5%
Skill Scroll: 10%
```

Có pity nhẹ cho material, không cần pity cho equipment.

## 22. Economy balance bản đầu

### 22.1. Mục tiêu

Người chơi free vẫn hoàn thành campaign. IAP giúp nhanh hơn, đẹp hơn, tiện hơn, không khóa nội dung chính.

### 22.2. Gold sink

Gold dùng cho:

* Upgrade equipment.
* Craft.
* Skill upgrade.
* Mua consumable.
* Reroll shop bằng gold số lần giới hạn.

### 22.3. Soul Gem sink

Soul Gem dùng cho:

* Mở inventory slot.
* Mua cosmetic.
* Đổi gold giới hạn ngày.
* Mua convenience pack.
* Reset skill.
* Mua battle background/cosmetic board skin.

### 22.4. Không có energy bắt buộc

Không dùng energy giới hạn lượt chơi trong bản đầu. Game offline nên để người chơi chơi thoải mái.

## 23. Tutorial

### 23.1. Tutorial flow

```text
T01: Di chuyển trong town.
T02: Nói chuyện NPC.
T03: Vào battle đầu.
T04: Swap 3 Sword để attack.
T05: Match Heart để heal.
T06: Match Food để tăng lương thực.
T07: Match Mana Orb để nạp skill.
T08: Dùng skill.
T09: Nhận reward.
T10: Mặc trang bị.
```

### 23.2. Tutorial UX

* Dùng hand pointer.
* Highlight ô cần kéo.
* Không khóa quá lâu.
* Có nút Skip sau lần chơi đầu.
* Tutorial state lưu trong save.

## 24. Editor tools cho Unity

### 24.1. Stage Editor

Tạo `IsekaiStageEditorWindow`.

Chức năng:

* Tạo realm.
* Tạo stage.
* Chọn enemy.
* Chọn background.
* Chọn board config.
* Chọn drop table.
* Nhập dialogue.
* Validate required fields.
* Export ScriptableObject.

Menu:

```text
Tools/Isekai 12 Realms/Stage Editor
```

### 24.2. Enemy Editor

Chức năng:

* Tạo enemy.
* Gán sprite/animation.
* Nhập HP/ATK/DEF.
* Chọn AI difficulty.
* Chọn skill enemy.
* Preview reward.

### 24.3. Item Editor

Chức năng:

* Tạo item/equipment.
* Chọn rarity.
* Chọn stat range.
* Chọn icon.
* Tạo recipe craft.

### 24.4. Economy Validator

Chức năng:

* Scan toàn bộ stage reward.
* Tính gold/EXP trung bình.
* Kiểm tra stage có drop table chưa.
* Kiểm tra upgrade cost có vượt quá reward curve.
* Báo lỗi nếu IAP product chưa map với gem amount.

Output:

```text
Economy Report:
- Average gold per stage
- Average EXP per stage
- Time to upgrade weapon +10
- Gem pack value ratio
- Possible broken configs
```

### 24.5. Board Test Window

Cho designer test board không cần vào game.

Chức năng:

* Generate board theo seed.
* Check có move hợp lệ không.
* Simulate 1000 board.
* Detect dead board.
* Preview special tile.

## 25. Addressables và cập nhật nội dung

### 25.1. Dùng Addressables cho

* Enemy sprites.
* Stage backgrounds.
* Dialogue data.
* Event config.
* Cosmetic skins.
* Audio lớn.
* New realm content.

### 25.2. Local vs Remote

Local Addressables:

* Core UI.
* Core character.
* Token icons.
* R01-R03 content.
* Essential audio.

Remote Addressables:

* R04+ optional content.
* Cosmetic.
* Seasonal event.
* Extra stage.
* New enemy.

### 25.3. Offline behavior

Nếu không có mạng:

* Game vẫn chơi được nội dung đã tải.
* Không chặn vào game.
* Remote content chưa tải thì hiện “Download when online”.
* Không tự tải lớn bằng mobile data nếu người chơi tắt.

## 26. Battle implementation detail

### 26.1. Board classes

```csharp
public class BoardController : MonoBehaviour
{
    public int width;
    public int height;
    public TileView tilePrefab;
    public Transform tileRoot;

    private TileData[,] tiles;

    public void Initialize(BoardConfig config);
    public Task<bool> TrySwapAsync(Vector2Int a, Vector2Int b);
    public List<MatchGroup> FindMatches();
    public Task ResolveMatchesAsync(List<MatchGroup> matches);
    public Task DropTilesAsync();
    public bool HasValidMove();
    public void ShuffleBoard();
}
```

### 26.2. Tile data

```csharp
public enum TileType
{
    Sword,
    Heart,
    Coin,
    Food,
    Book,
    Mana,
    Shield,
    Star
}

public enum SpecialTileType
{
    None,
    RowRune,
    ColumnRune,
    BombRune,
    RealmCrystal
}

public class TileData
{
    public TileType type;
    public SpecialTileType special;
    public bool locked;
    public int freezeTurns;
    public Vector2Int position;
}
```

### 26.3. Match resolve

```csharp
public class BattleResolver
{
    public BattleTurnResult Resolve(
        List<MatchGroup> groups,
        PlayerBattleState player,
        EnemyBattleState enemy,
        BattleContext context)
    {
        var result = new BattleTurnResult();

        foreach (var group in groups)
        {
            switch (group.TileType)
            {
                case TileType.Sword:
                    result.damage += CalculateSwordDamage(group.Count, player);
                    break;
                case TileType.Heart:
                    result.heal += CalculateHeal(group.Count, player);
                    break;
                case TileType.Coin:
                    result.gold += group.Count * context.goldPerCoinTile;
                    break;
                case TileType.Food:
                    result.food += group.Count * context.foodPerTile;
                    break;
                case TileType.Book:
                    result.expBonus += group.Count * context.expPerBookTile;
                    break;
                case TileType.Mana:
                    result.mana += group.Count * context.manaPerTile;
                    break;
                case TileType.Shield:
                    result.shield += CalculateShield(group.Count, player);
                    break;
                case TileType.Star:
                    result.classResource += group.Count;
                    break;
            }

            if (group.Count >= 4)
            {
                result.extraTurn = true;
            }
        }

        return result;
    }
}
```

## 27. UI implementation detail

### 27.1. Popup system

```csharp
public interface IPopup
{
    void Open(object payload);
    void Close();
}

public class PopupService : MonoBehaviour
{
    public Task<T> ShowAsync<T>(string popupId, object payload = null);
    public void CloseTop();
    public void CloseAll();
}
```

Popup registry:

```text
popup_login
popup_settings
popup_cloud_conflict
popup_battle_result
popup_item_detail
popup_equipment_upgrade
popup_shop_confirm
popup_iap_success
popup_error
```

### 27.2. Toast

Toast dùng cho thông báo nhỏ:

* “Saved”.
* “Not enough gold”.
* “Item equipped”.
* “Cloud sync complete”.
* “No internet connection”.

### 27.3. Loading overlay

Dùng cho:

* Firebase init.
* Cloud sync.
* IAP processing.
* Addressables download.
* Scene transition.

Có timeout và nút retry nếu lỗi mạng.

## 28. Error handling

### 28.1. Firebase lỗi

| Lỗi            | Xử lý                  |
| -------------- | ---------------------- |
| Không mạng     | Chơi offline, sync sau |
| Auth fail      | Dùng local guest       |
| Cloud conflict | Hiện popup chọn        |
| Upload fail    | Queue retry            |
| Download fail  | Giữ local save         |

### 28.2. IAP lỗi

| Lỗi                             | Xử lý                            |
| ------------------------------- | -------------------------------- |
| User cancelled                  | Đóng loading, không báo lỗi nặng |
| Purchase failed                 | Hiện lỗi ngắn                    |
| Receipt duplicate               | Không grant lại, báo đã xử lý    |
| Grant local success, cloud fail | Lưu pending sync                 |
| Store unavailable               | Hiện “Shop unavailable offline”  |

### 28.3. Save lỗi

Nếu current save hỏng:

```text
- Thử load previous backup.
- Nếu backup OK → restore.
- Nếu cả hai hỏng → hỏi tạo save mới.
```

## 29. Analytics optional

Không bắt buộc MVP, nhưng nên chuẩn bị event enum.

Event đề xuất:

```text
game_start
login_guest
login_google
stage_start
stage_clear
stage_fail
battle_turn_count
skill_used
equipment_upgraded
item_dropped
iap_started
iap_completed
iap_failed
cloud_sync_success
cloud_sync_failed
```

Không gửi dữ liệu cá nhân nhạy cảm.

## 30. Build config

### 30.1. Android

```text
Package name: com.yourstudio.isekai12realms
Minify: bật sau khi Firebase/IAP ổn định
IL2CPP: On
ARM64: On
Portrait only
Internet permission: cần cho Firebase/IAP
```

### 30.2. Quality

Mobile low/mid:

```text
Target FPS: 60 nếu máy ổn, fallback 30
Sprite Atlas: On
Compression: ASTC nếu target hỗ trợ, fallback ETC2
UI overdraw: hạn chế panel full alpha
Particle count: thấp
```

### 30.3. Save compatibility

Mọi save có `schemaVersion`.

Khi update:

```csharp
public interface ISaveMigration
{
    int FromVersion { get; }
    int ToVersion { get; }
    PlayerSaveData Migrate(PlayerSaveData oldSave);
}
```

## 31. MVP scope

### 31.1. MVP cần có

* BootScene.
* Guest login local + Firebase anonymous nếu có mạng.
* Google login basic.
* Local save.
* Cloud save upload/download.
* Main Town.
* World Map.
* 3 realm đầu.
* 20 stage.
* Battle board 8x8.
* 3 class.
* 12 enemy.
* 1 boss/realm.
* Equipment basic.
* Skill upgrade basic.
* Inventory.
* Shop Gold/Gem.
* IAP Soul Gem.
* Settings.
* Stage Editor basic.
* Crash/error logs.

### 31.2. MVP chưa cần

* PvP thật.
* Chat thật.
* Guild.
* Marketplace online.
* Realtime multiplayer.
* Server authoritative battle.
* Battle pass.
* Seasonal events.
* Voice.
* Complex cutscene.

## 32. Milestone phát triển

### Phase 1 — Core Foundation

Mục tiêu: chạy được game shell.

Tasks:

* Tạo Unity project.
* Setup folder.
* Setup BootScene/GameScene.
* Setup UI root + popup layer.
* Setup ServiceLocator.
* Setup SaveService local.
* Setup Firebase Auth mock interface.
* Setup main menu.

Deliverable:

```text
Mở game → tạo save local → vào Main Town placeholder.
```

### Phase 2 — Match-3 Battle

Tasks:

* Board 8x8.
* Swap tile.
* Find match.
* Cascade.
* Special tile.
* Player/enemy HP.
* Token effects.
* Turn system.
* Win/lose.
* Battle result.

Deliverable:

```text
Vào battle test → đánh slime → thắng nhận reward.
```

### Phase 3 — RPG Progression

Tasks:

* Player level/EXP.
* Inventory.
* Equipment.
* Skill.
* Drop table.
* Upgrade equipment.
* Stage reward.
* Quest simple.

Deliverable:

```text
Clear stage → nhận item → mặc đồ → tăng stat.
```

### Phase 4 — World/Adventure

Tasks:

* World Map.
* Stage select.
* 2D town.
* NPC dialogue.
* 3 realm đầu.
* Tutorial.

Deliverable:

```text
Người chơi đi từ town → chọn stage → battle → mở stage tiếp.
```

### Phase 5 — Firebase + IAP

Tasks:

* Firebase Auth anonymous.
* Google login.
* Cloud save.
* Conflict popup.
* Unity IAP.
* Purchase ledger.
* Restore purchases.
* Shop UI.

Deliverable:

```text
Người chơi guest chơi offline, online sync, mua gem và lưu ledger.
```

### Phase 6 — Polish + Release

Tasks:

* UI polish.
* VFX/SFX.
* Optimization.
* Crashlytics.
* Android build.
* Test devices.
* Store screenshots.
* Privacy text.
* Production config.

Deliverable:

```text
APK/AAB release candidate.
```

## 33. Acceptance criteria

### 33.1. Battle

* Không sinh board dead ngay từ đầu.
* Swap sai trả lại.
* Match 3 hoạt động.
* Match 4 tạo extra/special.
* Cascade ổn định.
* AI đi được.
* Win/lose đúng.
* Không softlock khi hết move.

### 33.2. Save

* Đóng app mở lại không mất progress.
* Battle win xong save ngay.
* Upgrade xong save ngay.
* IAP grant xong save ngay.
* Save hỏng thì restore backup.

### 33.3. Firebase

* Guest có uid khi online.
* Google login giữ progress nếu link thành công.
* Upload/download save được.
* Conflict hiện rõ.
* Không đọc/ghi được save user khác.

### 33.4. IAP

* Mua gem cộng đúng.
* Không cộng trùng transaction.
* Hủy mua không cộng.
* Mất mạng sau mua vẫn lưu pending.
* Restore/refresh không double grant.

### 33.5. UI

* Chơi được bằng một tay.
* Button đủ lớn.
* Không bị che bởi tai thỏ/safe area.
* Text đọc được trên 720x1280.
* Màn battle không rối.

## 34. Prompt triển khai cho Codex

### 34.1. Prompt tạo core architecture

```text
Read docs/spec.md. Implement the Unity C# core architecture for the game:
- ServiceLocator
- GameStateMachine
- SaveService with local JSON save, backup slot, checksum placeholder
- PopupService
- Basic BootScene flow
Do not implement battle yet. Keep code modular and testable.
```

### 34.2. Prompt tạo battle board

```text
Read docs/spec.md. Implement the match-3 battle board:
- 8x8 board
- Tile model and TileView
- Swap adjacent tiles
- Find horizontal/vertical matches
- Resolve matches
- Drop/cascade
- Generate new tiles
- Detect valid moves
- Shuffle when no moves
Use async animations but keep logic separated from view.
```

### 34.3. Prompt tạo RPG reward

```text
Read docs/spec.md. Implement RPG progression:
- Player stats
- EXP and level up
- Gold reward
- Inventory data
- Equipment instance
- Equip/unequip
- Equipment stat calculation
- DropTableDefinition ScriptableObject
```

### 34.4. Prompt tạo Firebase save

```text
Read docs/spec.md. Implement Firebase integration:
- AuthService for anonymous login and Google login abstraction
- CloudSaveService for upload/download save
- Cloud conflict detection by saveVersion and updatedAt
- Firestore path /users/{uid}/saves/default
Keep local-first behavior. If Firebase is unavailable, game must still work offline.
```

### 34.5. Prompt tạo IAP

```text
Read docs/spec.md. Implement Unity IAP:
- Consumable products for Soul Gem packs
- IAPService
- PurchaseRecord ledger
- Grant currency only once per transactionId
- Save immediately after grant
- Queue cloud sync if offline
No direct item/equipment IAP products.
```

## 35. Rủi ro và giải pháp

| Rủi ro                     | Giải pháp                                                   |
| -------------------------- | ----------------------------------------------------------- |
| Game bị xem là copy IP     | Đổi toàn bộ tên, art, story, icon, map, dialogue            |
| Offline save bị hack       | Chấp nhận một phần vì single-player, bảo vệ IAP bằng ledger |
| Firebase conflict mất save | Luôn hiện popup so sánh, không tự ghi đè                    |
| IAP double grant           | Transaction ledger theo transactionId                       |
| Battle lặp nhàm            | Thêm board modifier theo realm                              |
| UI quá chật                | Board ưu tiên trung tâm, popup dùng tab                     |
| Update content khó         | Dùng ScriptableObject + Addressables                        |
| Designer khó thêm stage    | Tạo Stage Editor                                            |

## 36. Định nghĩa bản “clone đúng tinh thần”

Bản này được coi là đạt mục tiêu nếu người chơi cảm thấy:

* Đây là RPG mobile dọc kiểu cổ điển.
* Có map 2D, NPC, nhiệm vụ.
* Battle là match-3 theo lượt.
* Mỗi token có ý nghĩa chiến thuật.
* Có hồi máu, đánh, vàng, EXP, lương thực, mana.
* Có class Hỏa/Thủy/Lôi phiên bản isekai.
* Có trang bị, skill, drop đồ, replay stage.
* Chơi offline được.
* Có cloud save khi đăng nhập.
* IAP chỉ mua tiền tệ, không phá game.

## 37. Ghi chú triển khai quan trọng

Ưu tiên làm core battle thật chắc trước khi làm nhiều content. Nếu battle chưa vui, thêm realm/stage cũng không cứu được game.

Thứ tự tốt nhất:

```text
1. Battle prototype
2. Reward/progression
3. UI battle polish
4. Save local
5. World map
6. Firebase
7. IAP
8. Content editor
9. Content expansion
```

Không nên làm PvP/chat/market ở bản đầu vì game đã xác định offline. Có thể giả lập cảm giác online cũ bằng NPC arena, ghost opponent, leaderboard local hoặc event offline.
