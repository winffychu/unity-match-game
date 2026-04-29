# Unity 对对消除小游戏 - 可直接生成可玩场景

## 项目概述

这是一个 Unity 对对消除（Memory Match）小游戏最小可玩项目骨架，已包含：
- 核心脚本（多关卡、翻牌限制、撤销、计分、连击）
- `Packages/manifest.json`（含 TextMeshPro 所需包）
- `Assets/Editor/MemoryMatchAutoBuilder.cs` 一键自动搭建场景工具
- 自动生成 `MainScene`、基础 UI、`Card.prefab`、`ComboEffect.prefab`、占位彩色卡面 Sprite

## 当前目录结构

```
Assets/
├── Editor/
│   └── MemoryMatchAutoBuilder.cs
├── Scripts/
│   ├── Core/
│   ├── Gameplay/
│   ├── Data/
│   └── Utils/
├── Scenes/
├── Prefabs/
└── Art/
Packages/
└── manifest.json
```

## 已实现功能

1. **基础玩法**：翻开两张牌，相同则配对成功，不同则翻回
2. **三关设计**：
   - 第1关：2x2，6次翻牌限制
   - 第2关：2x4，12次翻牌限制
   - 第3关：4x4，24次翻牌限制
3. **撤销操作**：支持撤销最近一次单张翻牌
4. **暂停/继续**：支持暂停与恢复
5. **结果展示**：胜利、失败、全部通关面板
6. **计分与连击**：包含连击文本与简易连击特效
7. **存档代码**：包含基础数据与存档脚本

## 快速开始

### 1. 打开项目
- 使用 Unity 2022.3 LTS 或更新版本打开该目录
- 等待 Unity 完成脚本编译与包导入

### 2. 一键生成可玩场景
在 Unity 顶部菜单执行：

`Tools > Memory Match > Build Playable Scene`

该操作会自动：
1. 创建 `Assets/Scenes/MainScene.unity`
2. 创建 `Canvas / EventSystem / GameManager / LevelManager / UIManager / AudioManager`
3. 创建主菜单、游戏UI、结算面板、暂停面板、全通关面板
4. 创建 `Assets/Prefabs/Card.prefab`
5. 创建 `Assets/Prefabs/ComboEffect.prefab`
6. 创建 `Assets/Art/CardColor_*.png` 占位卡牌图片
7. 将 `MainScene` 加入 Build Settings

### 3. 运行
- 打开 `MainScene`
- 点击 Play
- 在主菜单点击“开始游戏”即可开始

## 当前资源说明

- 当前自动生成的是**占位美术资源**，可直接运行验证逻辑
- 音频引用已留接口，但默认不会自动生成音频文件
- 如需正式发布，建议自行替换：
  - 卡牌正反面图片
  - 按钮样式
  - 背景图
  - BGM / SFX
  - Animator / 粒子特效

## 注意事项

1. 若首次打开 Unity 报 TMP 相关导入提示，正常导入即可
2. 若你修改了脚本私有字段名，需要同步更新 `MemoryMatchAutoBuilder.cs`
3. 重新执行自动构建工具会覆盖当前打开场景内容，适合在空场景下重建

## 操作说明

- **点击卡牌**：翻牌
- **回退按钮**：撤销上一步操作
- **暂停按钮**：暂停游戏
- **主菜单**：开始新游戏或退出

## 后续可扩展方向

1. 加入真正的卡面图案资源
2. 增加设置面板（音量、分辨率等）
3. 增加更多关卡与难度曲线
4. 增加排行榜/计时模式
5. 补充动画控制器与音效资源
