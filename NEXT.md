# 明天开工指南

## DLL 已部署，直接开游戏测

路径：`D:\Steam\steamapps\common\TheLongDark\Mods\TldHacks.dll`

---

## 测试优先级

### 必测（上次报告"全部没用"的功能）

1. **ESP 透视** — Tab菜单 → ESP & 自动瞄准 → 勾"透视(ESP)"→ 应该看到红色方框围住动物
2. **自动瞄准** — 勾"自动瞄准" → 按住右键 → 准星自动追踪动物 → 屏幕中心有白色FOV圆圈
3. **后坐力** — 
   - "无后坐力"勾上 → 开枪准星不跳
   - 或者不勾无后坐力，把"后坐力强度"滑块拉到 50% → 后坐力减半

### 次测

4. 魔法子弹 — 勾上后开枪自动命中最近/锁定的动物
5. 设置持久化 — 关游戏重开，ESP/自瞄设置还在
6. 墙后动物 — 框变半透明 + [WALL] 标签

---

## 如果有问题

| 现象 | 原因 | 快速修法 |
|------|------|----------|
| ESP 仍无显示 | FindObjectsOfType 可能返回空 | 看 MelonLoader/Latest.log 有无异常 |
| 游戏崩溃 | Physics.Raycast IL2Cpp 不兼容 | ESP.cs:307 HasLOS 改成 `return true;` |
| 后坐力仍全量 | 窗口仍不够长 | CheatsPatches.cs:725 改 `0.8f` |
| 自瞄锁死亡目标 | HP检查失效 | 告诉我，我查 m_CurrentHP getter |
| FOV圆圈不显示 | AutoAim toggle 没开 | 菜单里先勾"自动瞄准" |

---

## 构建（如果我改了代码）

```powershell
dotnet build "D:\TLD-Mods\tld--2.55\TldHacks\src\TldHacks.csproj" --no-incremental
```

游戏跑着时编译会失败（DLL被锁），关游戏再编译。

---

## 文档位置

- `HANDOFF.md` — 完整交接历史（所有session的改动记录）
- `CT_vs_Mod_Compare.md` — CT表 vs Mod 功能对比
- `NEXT.md` — 本文件（明天开工用）
