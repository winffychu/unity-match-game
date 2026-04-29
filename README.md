# Unity 对对消除小游戏 - 完整项目

## 项目概述

这是一个完整的 Unity 对对消除（Memory Match）小游戏，支持多关卡、翻牌次数限制、回退操作等功能。

## 项目结构

```
Assets/
├── Scripts/
│   ├── Core/
│   │   ├── GameManager.cs      # 游戏主控制器
│   │   ├── LevelManager.cs     # 关卡管理器
│   │   ├── UIManager.cs        # UI 管理器
│   │   └── AudioManager.cs     # 音频管理器
│   ├── Gameplay/
│   │   └── Card.cs             # 卡牌逻辑
│   ├── Data/
│   │   ├── GameData.cs         # 基础数据模型
│   │   ├── ScriptableObjects.cs # ScriptableObject 定义
│   │   └── SaveSystem.cs       # 存档系统
│   └── Utils/
│       └── ShuffleUtility.cs   # 洗牌工具
├── Prefabs/
│   └── Card.prefab             # 卡牌预制体
├── Scenes/
│   └── Main.unity              # 主场景
└── Resources/
    ├── Cards/                  # 卡牌图案
    └── Audio/                  # 音频资源
```

## 功能特性

1. **基础玩法**：点击两张卡牌，相同则消除，不同则翻回
2. **三关设计**：
   - 第1关：2x2 网格，4张牌，6次翻牌限制
   - 第2关：2x4 网格，8张牌，12次翻牌限制
   - 第3关：4x4 网格，16张牌，24次翻牌限制
3. **入口功能**：开始挑战、重新挑战
4. **翻牌次数**：限制次数内通关，次数用完失败
5. **回退操作**：支持撤销上一步翻牌
6. **得分系统**：连击加成、通关评分
7. **数据存档**：保存玩家进度和最佳成绩

## 快速开始

### 1. 创建 Unity 项目
- 使用 Unity 2022.3 LTS 或更新版本
- 创建 2D 项目

### 2. 导入代码
1. 在 `Assets` 下创建 `Scripts` 文件夹
2. 将 `Scripts` 下的所有代码文件复制到对应目录

### 3. 创建卡牌预制体
1. 在场景中创建一个空物体，命名为 `Card`
2. 添加 `Card.cs` 脚本
3. 创建子物体：
   - `Front`：包含 Image 组件（显示正面图案）
   - `Back`：包含 Image 组件（显示背面图案）
   - `MatchedEffect`：匹配成功时的特效（可选）
4. 将 Card 拖入 Prefabs 文件夹创建预制体

### 4. 场景设置
1. 创建空物体 `GameManager`，挂载 `GameManager.cs`
2. 创建空物体 `LevelManager`，挂载 `LevelManager.cs`
3. 创建空物体 `UIManager`，挂载 `UIManager.cs`
4. 创建空物体 `AudioManager`，挂载 `AudioManager.cs`
5. 在 Canvas 中创建：
   - MainMenuPanel（主菜单）
   - GameUIPanel（游戏界面）
   - ResultPanel（结果面板）
   - PausePanel（暂停面板）

### 5. 配置资源
1. 准备卡牌正面图案（至少8种不同图案）
2. 准备卡牌背面图案
3. 准备音效文件（可选）
4. 在 LevelManager 中配置卡牌图案列表

### 6. 运行游戏
点击 Play 按钮即可运行！

## 操作说明

- **点击卡牌**：翻牌
- **回退按钮**：撤销上一步操作
- **暂停按钮**：暂停游戏
- **主菜单**：开始新游戏或退出

## 扩展建议

1. 添加更多关卡和难度
2. 实现时间挑战模式
3. 添加排行榜功能
4. 实现卡牌收集系统
5. 添加更多动画和特效