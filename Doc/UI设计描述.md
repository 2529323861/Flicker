# Flicker UI 设计描述

> 用途：喂给 AI 生成概念图，再拆图落地到 Unity。
> 参考分辨率：**1920 × 1080**（Canvas Scaler 按高度匹配）
> 现有底图：`UI_frame.png`，54×54 九宫格，深藏青底 + 棕褐描边

---

## 零、风格决策：为什么不用像素

游戏内美术是**低分辨率像素**：

| 资源 | 实际分辨率 | 放大到 1920 的倍数 |
|---|---|---|
| `Background.png` | 428 × 240 | 4.5× |
| `Character/idle.png` | 37 × 22 | 约 50× |
| `UI_frame.png` | 54 × 54 | — |

AI 生成像素图有三个难控的问题：

1. **粒度不统一**：面板边框 2px、按钮边框 3px、图标 1px，混在同一张图里
2. **边缘抗锯齿**：输出的图带半透明过渡像素，放大后是糊的
3. **无法九宫格拉伸**：拉伸时像素会被拉变形

**因此 UI 采用「硬边扁平」（hard-edge flat）**，它同时满足：

- 没有「像素粒度」这个 AI 难控的变量
- 纯色块 + 硬边缘 + 极小调色板，细节密度与像素画一致，不会风格冲突
- 边缘是直线，可以正常九宫格拉伸，省掉大量对齐工作

这个组合已有先例：像素场景 + 硬边扁平 UI（如 Dead Cells、Celeste 的部分界面）。人眼判断风格冲突主要看**细节密度与饱和度**，而不是边缘是否锯齿。

---

## 一、共享风格前缀（每次生成都先粘这段）

**所有界面必须共用这段，否则出不了一致的图。**

```
Flat vector game UI, 16:9, 1920x1080, dark fantasy cave theme.

Style: hard-edge flat design. Solid colour fills only.
NO pixel art, NO pixel grid, NO dithering, NO gradients, NO shadows,
NO glossy highlights, NO glassmorphism, NO 3D, NO textures.

Colour palette:
- panel background: deep navy-purple #2B2B3D
- panel border: tan brown #A08464, 3px solid stroke
- primary text: cream white #F0EBDD
- secondary text: muted grey-lavender #9A97A8
- accent glow: cyan-teal #4FD0C0
- danger: warm red #C0392B
- success: leaf green #5FA845
- primary action: muted blue #3A6EA5

Geometry rules:
- crisp straight edges, sharp corners with at most 6px radius
- every panel uses the same 3px border stroke
- generous inner padding, generous negative space
- high contrast, prioritise readability over decoration
- only ONE thin inner highlight line along the top edge of a panel

Typography: clean sans-serif with slightly squared terminals, medium weight.
```

**中文要点：**

| 项 | 值 |
|---|---|
| 面板底 | `#2B2B3D` 深藏青紫 |
| 面板描边 | `#A08464` 棕褐，**3px 实线** |
| 主文字 | `#F0EBDD` 米白 |
| 次文字 | `#9A97A8` 灰紫 |
| 强调 | `#4FD0C0` 青绿 |
| 危险 | `#C0392B` 红 |
| 正向 | `#5FA845` 绿 |
| 主操作 | `#3A6EA5` 蓝 |

**禁止项（必须写进 prompt）**：像素风、像素网格、抖动、渐变、阴影、高光、玻璃质感、3D、贴图纹理。

---

## 二、界面清单

| # | 界面 | 类型 | 现状问题 |
|---|---|---|---|
| 1 | 主菜单 | 全屏 | 无游戏名，4 个按钮无层级 |
| 2 | 大厅 | 全屏 | 三栏，空白多，字号混乱（7 种） |
| 3 | 对局 HUD | 覆盖层 | 底部元素常被裁切 |
| 4 | 设置 | 弹窗 | 无标题，无遮罩 |
| 5 | 加入房间 | 弹窗 | 无标题，无遮罩 |
| 6 | 加入中 | 小弹窗 | 只有一个文本 + 取消 |
| 7 | 提示 | 弹窗 | 标题/正文/确认三段 |
| 8 | 倒计时 | 覆盖层 | 3 → 2 → 1 → GO |
| 9 | 结算 | 全屏 | 几乎空白 |

---

## 三、逐个界面

### 1. 主菜单 `MainPanel`

**结构描述：**

- 背景：洞穴场景（已有素材，不用 AI 重画）
- 居中偏左放置一个**竖向长面板**，约占画面高 72%，宽 24%（约 460×780）
- 面板内**垂直居中**排列 4 个按钮，按钮宽 300、高 72，间距 40
- **面板顶部留出 120px 给游戏名 / 标题**
- 按钮文字：单人游戏 / 主持多人 / 加入多人 / 设置
- 按钮样式：米白底 + 深色文字，无边框，轻微圆角

**改进点（相比现状）：**

| 现状 | 建议 |
|---|---|
| 无标题 | 面板顶部放 `FLICKER` 粗体字 LOGO，副标题一行小字 |
| 4 个按钮一律同权重 | 第一个（单人游戏）可加重，或与其他三个之间加分隔线 |
| 按钮间距 90px 过散 | 收到 40px，让面板更紧凑 |

**英文 prompt：**

```
[共享风格前缀]

Screen: main menu. Vertical center-left panel, 460x780, 
dark navy-purple fill with tan 2px border and 4px rounded corners.

Panel contents top to bottom:
- game logo: the word "FLICKER" in chunky heavy-weight sans-serif lettering,
  cream white with a soft cyan glow, 96px tall
- a thin horizontal divider line in tan brown
- four identical menu buttons stacked vertically, each 300x72, 
  cream white fill, dark navy text, 22px sans-serif, 40px gap between them
- lines read: "single player", "host", "join", "settings"

The cavern background is visible around the panel, slightly darkened.
No shadows, no gloss, no pixel grid.
```

---

### 2. 大厅 `LobbyPanel`

**结构描述（三栏布局）：**

```
┌──────────────┬────────────────────────┬──────────────┐
│ 玩家名        │  道具池网格             │ 对手名        │
│ [未准备徽章]  │  （6 列，13 格）        │ [未准备徽章]  │
│              │                        │              │
│ [槽1][槽2][槽3]│                        │              │
│              │                        │              │
│              ├────────────────────────┤              │
│              │ [图标] 名称            │              │
│              │        描述文字         │              │
│ [准备][退出]  │        [选择]          ├──────────────┤
│ [开始游戏]    │                        │ [添加机器人]  │
└──────────────┴────────────────────────┴──────────────┘
```

- 左栏宽 ~460（占 24%），中栏弹性，右栏宽 ~460
- 左栏：顶部玩家名（40px）+ 右上角「未准备」小徽章；中部 3 个卡组槽（各 127×127，横排，间距 16）；底部准备（蓝）+ 退出（红）并排，下方开始游戏（绿）通栏
- 中栏上：道具池，6 列网格，格子 127×127，间距 16
- 中栏下：详情区，左侧图标 100×100，右侧名称（32px）+ 描述（20px），底部「选择」按钮
- 右栏：顶部对手名 + 徽章；底部「添加机器人」通栏

**改进点：**

| 现状 | 建议 |
|---|---|
| 字号有 7 种（含 63.4/64.3/65.3） | 收敛到 3 种：40 / 24 / 18 |
| 左栏下方大块空白 | 卡组槽下移，或让空白分区带一个「已选 3/3」提示 |
| 「添加机器人」比其他按钮大 | 统一为 M 尺寸 |
| 13 个道具全部蓝框 | 区分「可选 / 已选 / 不可选」三种状态 |

**英文 prompt：**

```
[共享风格前缀]

Screen: multiplayer lobby, three-column layout on top of the cavern background.

Left column (460 wide):
- player name in 40px cream text, top-left
- small status badge "unprepared" pinned top-right of the name area
- three empty loadout slots below, 127x127 each, tan border, dark fill, horizontal
- bottom: two buttons side by side, 200x56 each — blue "ready" and red "leave"
- below them one full-width green button "start game", 460x64

Center column (flexible):
- upper part: item pool grid, 6 columns, each cell 127x127 with 
  tan border and dark fill, small item icons inside
- lower part: a detail card — 100x100 icon on the left, 
  item name in 32px and two lines of description in 20px on the right,
  a small "select" button at the bottom left of the card

Right column (460 wide):
- opponent name in 40px greyed text with an "unprepared" badge
- a large empty area
- bottom: one full-width blue button "add bot", 460x64

All panels use the same tan border and rounded corners.
```

---

### 3. 对局 HUD `PlayerHUD`

**结构描述：**

- 左下角一列（距屏幕左 40、下 40）：
  - 跳跃图标 + 「SPACE」标签（约 90×90）
  - 下方 3 个道具槽横排，各 127×127，间距 16
  - 每个槽下方有数字键提示 1 / 2 / 3
- 每个道具槽：圆角方框 + 道具图标 + **环形冷却遮罩**（360° 填充）+ 冷却中整体灰化
- 左上角：自己的名字（小字，属可选）
- 顶部或左右：目前**无血量**

**改进点：**

| 现状 | 建议 |
|---|---|
| 玩家名被屏幕左边缘裁切 | 名字改为左上角带 40px 边距，或移除（自己不需要看自己名字） |
| 底部一条 822×9 的白色细线 | 那可能是跳跃 CD 条，需确认是否有背景轨道 |
| 无血量信息 | 建议左右两侧竖排小血点（3 点） |

**英文 prompt：**

```
[共享风格前缀]

Screen: in-game HUD overlay, transparent background, only UI elements.

Bottom-left cluster:
- a 90x90 rounded square showing a small slime jump icon, 
  with a small dark label "SPACE" pinned below it
- to its right, three item slots in a horizontal row, 127x127 each, 16px gap
- each slot: tan border, dark fill, item icon centred,
  a small dark circle with a number "1" "2" "3" at the bottom-centre of each slot
- the first slot shows a radial cooldown sweep covering 40 percent of the icon, 
  the covered part is desaturated grey

Top-left: small player name label in 18px muted text, with 40px screen margin.

Top-right: three small red diamond pips in a vertical column, 
representing remaining health.

Everything else is transparent so the game world shows through.
```

---

### 4. 设置 `SettingsPanel`（弹窗）

**结构描述：**

- 弹窗尺寸约 720×420，居中
- 顶部标题「设置」，40px，下留 32px
- 三行，每行左侧标签（24px）+ 右侧控件：
  - 音乐 → 水平滑条
  - 音效 → 水平滑条
  - 玩家昵称 → 输入框
- 底部居中「确定」按钮，200×56
- **弹窗背后需要一层半透明黑色遮罩**（当前没有）

**英文 prompt：**

```
[共享风格前缀]

Screen: settings modal dialog, 720x420, centred.
Behind it, a flat 60 percent black overlay dims the whole screen.

Dialog contents:
- title "settings" in 40px cream text, top-centre, 32px below the top border
- three label-and-control rows, left aligned, 24px labels, 40px vertical gap:
  row 1: label "music", horizontal slider with a round knob
  row 2: label "effects", horizontal slider
  row 3: label "player name", a rectangular input field with a caret
- bottom-centre: one button 200x56, cream fill, dark text, reading "confirm"

No close X button. Same tan border and rounded corners as every other panel.
```

---

### 5. 加入房间 `JoinInputPanel`（弹窗）

**结构描述：**

- 弹窗约 760×320，居中 + 黑色遮罩
- 标题「加入多人」，40px
- 一行输入：标签「服务器地址：」24px + 输入框（占剩余宽度）
- 输入框内占位文字「127.0.0.1:7777」，灰紫色
- 底部两个按钮：左「加入」（正向绿）、右「取消」（中性白），各 240×56，间距 32

**英文 prompt：**

```
[共享风格前缀]

Screen: "join game" modal dialog, 760x320, centred, 
on top of a flat 60 percent black overlay.

Contents top to bottom:
- title "join multiplayer" in 40px cream text
- one row: label "server address" in 24px on the left, 
  and a wide input field filling the rest, containing grey placeholder text "127.0.0.1:7777"
- bottom row, two buttons 240x56 with 32px gap:
  left one green fill (primary), right one cream fill (neutral)

Same tan border, rounded corners, generous padding.
```

---

### 6. 加入中 `JoiningPanel`（小弹窗）

**结构描述：**

- 小弹窗约 420×180，居中 + 遮罩
- 一行文字「加入中...」，32px，居中偏上
- 可加一个**三点循环动画**（小方块依次高亮，纯 UI 图实现，不需要美术）
- 底部「取消」按钮，160×48

**英文 prompt：**

```
[共享风格前缀]

Screen: small "connecting" modal, 420x180, centred, 
on a flat 60 percent black overlay.

Contents:
- text "connecting" in 32px cream, upper-centre
- below it, three small square pips arranged horizontally as a loading indicator,
  the middle one highlighted in cyan, the others dark grey
- bottom: one button 160x48 labelled "cancel", cream fill dark text
```

---

### 7. 提示 `TipPanel`（弹窗）

**结构描述：**

- 弹窗约 640×300，居中 + 遮罩
- 标题 40px（如「注意」），顶部
- 正文 24px，可多行，行高 36
- 底部居中「确认」按钮 200×56
- 支持**队列**：连续弹多个时依次出现，因此样式要足够轻

**英文 prompt：**

```
[共享风格前缀]

Screen: message modal, 640x300, centred on a flat 60 percent black overlay.

Contents:
- title "notice" in 40px cream text, top-centre
- body text below, 24px, up to three wrapped lines, left aligned with 32px side padding
- bottom-centre: one button 200x56 labelled "ok", cream fill dark text

Minimal, no icon needed, same border and corner radius.
```

---

### 8. 倒计时 `CountDown`

**结构描述：**

- **无面板底板**，直接覆盖在画面上
- 中央显示巨大数字 3 → 2 → 1，字号约 240px，超粗无衬线体，深色描边保证在背景上可读
- 最后一个是「GO」
- 数字颜色：米白 + 深色描边（保证在洞穴背景上可读）
- 出现时带缩放弹入（放大 → 回弹）

**英文 prompt：**

```
[共享风格前缀]

Screen: countdown overlay, no panel, no background.
Only a single giant character centred on screen: the digit "2"
in 240px ultra-bold sans-serif, cream white fill with a 4px dark navy outline.
Slightly rotated, with a subtle cyan rim light on the top-left edge.

Fully transparent everywhere else.
```

---

### 9. 结算 `MultiPlaySettlementPanel`

**结构描述：**

- 面板约 1000×600，居中
- 顶部：结果文字「游戏胜利！」/「游戏失败！」48px，胜利用米白 + 青绿光，失败用灰紫
- 中部：**目前完全空白**，建议加统计行：
  - 对局时长
  - 命中次数
  - 使用道具次数
  - 胜负原因（如「被对手踩中」）
- 底部：「返回大厅」按钮 240×56

**英文 prompt：**

```
[共享风格前缀]

Screen: match result panel, 1000x600, centred on a dark overlay.

Contents:
- top: bold result text "victory" in 48px cream with a cyan glow
- a thin tan divider line below it
- middle: four stat rows, each with a 20px grey label on the left 
  and a 24px cream value on the right, evenly spaced:
  "duration" / "hits" / "items used" / "cause"
- bottom-centre: one button 240x56, cream fill dark text, reading "back to lobby"

The panel should feel spacious but not empty — the stat rows fill the middle third.
```

---

## 四、拆图建议（生成后如何落地）

**不要一次生成整张图就拆。** 按组件分三次出图，比拆一张大图更快：

| 批次 | 出什么 | 用途 |
|---|---|---|
| **1. 底图** | 一张 54×54 或 96×96 的九宫格面板底图（可拉伸） | 替换 `UI_frame.png` |
| **2. 按钮** | 4 张：中性 / 正向 / 危险 / 禁用，各 3 态（normal / pressed / disabled） | 替换 `UI_button_*` |
| **3. 控件** | 滑条轨道、滑条 knob、输入框、道具槽（空/选中/冷却） | 新增或替换 |

**理由：**

- 整图拆件需要精确对齐，且每个面板都要重做一遍
- 组件出图后可以**在 Unity 里用九宫格拉伸**，一套组件服务所有面板

**规格建议（非像素，尺寸需大于实际显示）：**

| 资源 | 出图尺寸 | 九宫格 border | 说明 |
|---|---|---|---|
| 面板底 | 256 × 256 | 40 | 可九宫格拉伸 |
| 按钮 | 384 × 112 | 32 | 可九宫格拉伸 |
| 道具槽 | 160 × 160 | — | 不拉伸 |
| 滑条轨道 | 256 × 32 | 12（左右） | 横向九宫格 |
| 滑条头 | 48 × 48 | — | 不拉伸 |
| 输入框 | 256 × 80 | 24 | 可九宫格拉伸 |
| 弹窗遮罩 | 纯色不用图 | — | 用 Unity 默认白图 + 黑色调 |

**为什么比显示尺寸大**：按钮实际显示 200×56，出图 384×112 是 1.9 倍。UI 缩放时每单位有 ~2 像素可用，边缘不会发虚。**非像素风格可以接受双线性缩放，这是它相对像素风的最大落地优势。**

**导入设置：**

| 项 | 值 | 理由 |
|---|---|---|
| Texture Type | Sprite (2D and UI) | |
| Filter Mode | **Bilinear** | 非像素必须用双线性；用 Point 会出锯齿 |
| Compression | None 或 High Quality | 纯色块压缩容易出色带 |
| Generate Mip Maps | 关闭 | UI 不需要 |
| Alpha Is Transparency | 开启 | 边缘干净 |

---

## 五、落地时的数值规范

避免再次出现「7 种字号」的问题，所有界面只从下表取值：

### 字号

| 角色 | 字号 | 用途 |
|---|---|---|
| 标题 | 40 | 面板标题 |
| 强调 | 32 | 玩家名、二级标题 |
| **正文** | **24** | 按钮、描述、列表 |
| 注释 | 18 | 徽章、提示、次要信息 |

### 按钮尺寸

| 角色 | 尺寸 | 用途 |
|---|---|---|
| S | 120×40 | 小操作（选择、取消） |
| **M** | **200×56** | 主按钮（准备、退出、确认、返回） |
| L | 320×72 | 强调（开始游戏、添加机器人） |

### 间距

| 项 | 值 |
|---|---|
| 面板内边距 | 32 |
| 元素间距 | 16 |
| 分组间距 | 40 |
| 按钮间距 | 16 |
| 屏幕安全边距 | 40 |

### 颜色使用规则

| 语义 | 颜色 | 适用 |
|---|---|---|
| 导航 / 中性 | 米白 `#F0EBDD` | 确认、取消、返回、主菜单全部按钮 |
| 正向动作 | 绿 `#5FA845` | 开始游戏、加入 |
| 主操作 | 蓝 `#3A6EA5` | 准备、添加机器人 |
| 破坏性 | 红 `#C0392B` | 退出 |
| 禁用 | 灰 `#55535F` | 条件不满足时 |

**规则：颜色对应语义，不对应界面。** 主菜单的 4 个按钮同级，所以全白；大厅有红蓝绿，因为它们语义不同。

---

## 六、AI 出图的三个注意事项

**① 中文字不要让它画**

AI 出中文几乎必错。**prompt 里一律用英文单词**占位，落地时在 Unity 里换成中文 TMP 文本。

**② 尺寸一致性靠共享前缀**

每次生成都必须粘贴同一段共享风格前缀。否则不同次生成的边框粗细、圆角半径都会不同。

**③ 必须显式写否定词**

非像素风格必须把下列词写进 prompt，否则 AI 很容易自行加像素网格或渐变：

```
NO pixel art, NO pixel grid, NO dithering, NO gradients,
NO shadows, NO glossy highlights, NO 3D
```

**④ 先出 2~3 张挑一张**

同一 prompt 出多次，挑比例最接近 1920×1080 且留白合理的。AI 经常把按钮画得太挤。

**⑤ 概念图只用来定布局，不直接拆**

概念图的边框粗细、圆角半径都不精确。用它确定**元素位置与比例**，实际素材按第四节的组件规格单独出。
