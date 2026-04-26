# tld 自用改造 2.55

**The Long Dark 2.55** 个人定制层 —— 基于 117-mod 整合包之上的配置快照 + 自制 mod。

> ⚠️ **只含配置和我写的 mod,不含第三方 mod 本体。** 要用这个仓库,你得先装好你用的那个 117-mod 整合包,然后把这里的 `configs/Mods/*` 覆盖过去。第三方 mod 的版权属于各自作者。

## 包含什么

```
├── BunkerDefaults/         ← 我原创的 mod,可自由使用/修改
│   ├── BunkerDefaults.dll      编译好的,直接丢 Mods/ 就行
│   └── src/                    C# 源码 + 工程,dotnet build 即可
├── configs/                ← 所有 mod 的配置快照(203 个文件)
│   ├── Mods/                   Mods/ 下所有 *.json / *.txt(不含 .dll)
│   └── UserData/               MelonLoader 全局偏好
├── tools/
│   └── PopulateFastTravel.ps1  PS 脚本:手动批量注入 FastTravel 传送点
├── CHANGELOG.md            ← 对原 mod 做了哪些具体改动
└── LICENSE
```

## 不包含什么(为什么没带)

| 类型 | 原因 |
|---|---|
| 117 个整合包的 `.dll` / `.modcomponent` | 都是别人的代码,没作者许可不能分发 |
| MelonLoader 本体 | 从 [LavaGang/MelonLoader](https://github.com/LavaGang/MelonLoader) 官方下 |
| 游戏本体 | 买 |
| 存档 (`sandbox*` / `*.moddata`) | 私人数据 |

## BunkerDefaults — 自制 mod

**解决什么问题:** Pathoschild 的 FastTravel mod 要你**先走到**才能保存一个传送点。我这个 mod 在游戏每次启动时扫描 `Mods/ModData/*.moddata`,自动往 FastTravel 存档里**预填 9 个大地图地堡坐标**,新开存档也不用自己跑。

**热键(继承 FastTravel 的):**

| 键 | 目的地 | 场景名 |
|---|---|---|
| `5` | 神秘湖地堡 | LakeRegion |
| `6` | 怡人山谷地堡 | RuralRegion |
| `7` | 林狼雪岭地堡 | MountainPassRegion |
| `8` | 孤寂沼泽地堡 | MarshRegion |
| `9` | 山间小镇地堡 | MountainTownRegion |
| `F2` | 寂静河谷地堡 | RiverValleyRegion |
| `F3` | 荒凉水湾地堡 | CanneryRegion |
| `F4` | 灰烬峡谷地堡 | AshCanyonRegion |
| `F7` | 黑岩地区地堡 | BlackrockRegion |

坐标取自 CheatEngine 社区表。

**新开存档时的流程(重要):**
1. 建新沙盒,玩到触发一次存档(睡觉 / `ESC→Save` / 过场景)
2. BunkerDefaults 会在下次切场景时自动往 `<save>.moddata` 注入 9 个传送点
3. **`ESC → Main Menu → 重新载入存档`** —— 让 FastTravel 刷新内存缓存
4. 按 `5` 直接传神秘湖地堡

第 3 步的重载**每个新存档只需做一次**,之后永远好用。

## 快速换机恢复

```powershell
# 1. 装好你的整合包到 <TLD>/Mods/
# 2. clone 这个仓库
git clone https://github.com/Jcxu97/tld-自用改造2.55.git

# 3. 覆盖配置
cp -r tld-自用改造2.55/configs/Mods/*       <TLD>/Mods/
cp -r tld-自用改造2.55/configs/UserData/*   <TLD>/UserData/
cp    tld-自用改造2.55/BunkerDefaults/BunkerDefaults.dll <TLD>/Mods/
```

## 已知问题

- **整合包切场景偶发闪退 + 重启卡启动**:117-mod 整合包通病。切场景前按 `F5` 快存,闪退后反复重启/重启电脑即可。详见 `CHANGELOG.md`。
- **DarkerNights.dll (v1.3 by Xpazeman)**:对 TLD 2.55 已失效,加载会让启动死循环。**必须不装它**。该作者其他 mod(AmbientLights / PlacingAnywhere / HouseLights / GearDecayModifier)还能用。
- **StackManager 的 `AddStackableComponent` 只影响新生成的物品**:已经在你背包里的 soda/jerky 还是不能叠,要去新容器捡。

## License

- `BunkerDefaults/` 下面我写的一切:MIT(见 `LICENSE`)
- `configs/` 下面的是我的设置快照 —— 各 mod 配置格式归原作者,这些只是"我填了什么值"。你随便拿去改。
