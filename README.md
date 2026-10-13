# Player Cast Bars · 玩家头顶咏唱条

Dalamud API 15 插件。在其他队友头顶显示技能名称、咏唱进度和可选的剩余时间，默认不显示自己。瞬发技能、咏唱完成或中断时不显示；切图和过场时暂停绘制。

## 使用

### 第三方插件仓库安装（推荐）

打开 `/xlsettings` → 实验性（Experimental）→ 自定义插件仓库，添加以下地址并启用、保存：

```text
https://raw.githubusercontent.com/HanaSi0/Player-skill-casting-bar/main/pluginmaster.json
```

然后打开 `/xlplugins`，刷新列表，搜索 **Player Cast Bars** 并安装。插件要求 Dalamud API 15；不兼容的客户端需要先使用对应版本的 Dalamud。订阅地址固定，索引随每次成功发布的 Release 自动更新。

### 手动开发插件安装

1. 使用支持 Dalamud API 15 的 XIVLauncher / Dalamud。游戏地区与版本必须与对应 Dalamud 版本兼容。
2. 编译后，在游戏内 `/xlsettings` 的开发插件设置中添加 `bin/Release/PlayerCastBars.dll` 的绝对路径，然后通过 `/xlplugins` 的开发插件列表加载。
3. 输入 `/pcbar` 打开设置：启用、仅队伍、显示自己、剩余时间、宽高、世界高度偏移、屏幕垂直偏移、最大距离。

默认仅队伍其他成员。关闭“只显示队伍成员”会显示附近其他玩家，“显示自己”始终由独立开关决定。技能名称使用客户端语言。绘制不接收鼠标操作。

## 构建与检查

需要 .NET 10 SDK，以及对应 API 15 的 Dalamud 开发程序集。Windows 下 SDK 会自动寻找 `%APPDATA%/XIVLauncher/addon/Hooks/dev`；也可以设置 `DALAMUD_HOME` 指向包含 `Dalamud.dll` 的目录。Linux 可交叉编译，但不能在本环境运行 FF14 游戏内插件。

```sh
dotnet restore PlayerCastBars.csproj --locked-mode
dotnet build PlayerCastBars.csproj -c Release --no-restore
dotnet run --project tests/PlayerCastBars.Tests.csproj -c Release
```

输出 DLL、生成的 API 15 清单位于 `bin/Release/`，打包产物位于 `bin/Release/PlayerCastBars/`。检查程序覆盖队友/自己/附近玩家筛选、瞬发、完成、中断和异常时间数据。

## 游戏内验收（需实际客户端）

- 与另一名玩家组队，让队友施放有咏唱时间的技能：头顶显示名称与进度，自己默认不显示。
- 让队友移动取消、完成咏唱、使用瞬发技能（包括迅速魔后的技能）：条应立即消失或始终不出现。
- 旋转相机、移动和切换窗口位置：读条应跟随角色，屏幕外不显示；不同种族的高度应合理。
- 切图、播放过场、离队、卸载插件：不应残留咏唱条或回调。
- 开启“显示自己”、关闭队伍限制、调整大小和偏移；重载插件确认设置保留。

仅渲染客户端已加载、范围内且投影在屏幕内的玩家。不会检测墙体遮挡；重叠玩家的读条可能重叠。当前云端只验证编译及逻辑检查，实际渲染与客户端咏唱同步仍需按以上步骤验收。
