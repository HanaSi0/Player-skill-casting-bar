## 玩家头顶咏唱条

- 默认只显示其他队友，不包括自己。
- 显示技能名称、咏唱进度及可选剩余时间。
- 瞬发技能不显示，咏唱完成或中断后隐藏。
- `/pcbar` 可设置显示范围、大小、位置偏移和最大距离。

需要支持 Dalamud API 15 的客户端。下载并解压 `PlayerCastBars.zip`，在 Dalamud 开发插件设置中添加 `PlayerCastBars.dll` 路径后加载。

也可以在 `/xlsettings` → 实验性 → 自定义插件仓库中添加订阅地址，再通过 `/xlplugins` 搜索 **Player Cast Bars** 安装：

https://github.com/HanaSi0/Player-skill-casting-bar/releases/latest/download/pluginmaster.json

构建和 14 项逻辑检查通过；实际游戏内渲染与客户端咏唱同步仍需验收。完整说明见仓库 README。
