using System.Collections.Generic;
using UnityEngine;

namespace TldHacks;

// 三列布局,按"麻瓜修改器" / 用户参考截图风格。全部用 GUI.Xxx(Rect,..) 不用 GUILayout(被 strip)。
// Rect 通过 R(x,y,w,h) 乘 _scale。
internal static class Menu
{
    public static bool Open;

    private const float W = 1200f;   // 加宽给三列更多空间
    private const float H = 720f;
    // v2.7.28 ContentH 动态:tab0 主 cheat + uConsole 1400,tab1 spawner 只到 850
    //   之前固定 2200 → tab1 底部有 ~1300 空白可滚但无内容
    private const float ContentH_Main = 1400f;
    private const float ContentH_Spawner = 850f;
    private static Vector2 _mainScroll;
    private static Rect _window = new Rect(30f, 30f, W, H);
    private const int WindowId = 0x71D4_AC;

    private static float _scale = 1f;

    private static bool _prevCursorVisible;
    private static CursorLockMode _prevCursorLock;
    private static bool _cursorSaved;

    // Spawner + presets
    private static int _selectedCategory = 0;
    private static int _page = 0;
    private const int PageSize = 18;  // v2.7.28: 6 → 18 (3 col × 6 row),页数从 60 → 20
    private static int _quantity = 1;
    private static readonly int[] QuantityPresets = { 1, 5, 10, 50, 100 };
    private static readonly float[] SpeedPresets = { 0.5f, 1f, 2f, 5f };
    private static readonly int[] HourPresets = { 8, 12, 18, 22 };
    private static readonly string[] HourLabels = { "晨8", "午12", "暮18", "夜22" };
    // 修正映射:WeatherStage enum 实际值 DenseFog=0/LightSnow=1/HeavySnow=2/PartlyCloudy=3/Clear=4/Cloudy=5
    //                              LightFog=6/Blizzard=7/ClearAurora=8/ToxicFog=9/ElectrostaticFog=10
    private static readonly int[] WeatherStages = { 4, 3, 5, 1, 2, 7, 6, 0, 8, 9, 10 };
    private static readonly string[] WeatherLabels = { "晴", "局部多云", "多云", "小雪", "大雪", "暴风雪", "薄雾", "浓雾", "极光", "毒雾", "电磁雾" };
    private static List<ItemEntry> _filtered = new List<ItemEntry>();
    private static int _lastCat = -1;

    // 标签页
    private static readonly string[] Tabs = { "主要 + uConsole", "物品 & 传送" };
    private static int _activeTab = 0;

    private static Rect R(float x, float y, float w, float h)
        => new Rect(x * _scale, y * _scale, w * _scale, h * _scale);

    // v2.7.5 统一 spacing 常量 —— 防高缩放下行距不够、Section 压到 toggle
    private const float SEC_H = 22f;        // section 标题框高度
    private const float SEC_ADV = 30f;      // section 标题后 y 推进
    private const float ROW_H = 24f;        // 单个 toggle/button 高度
    private const float ROW_ADV = 28f;      // 同类型元素之间 y 推进
    private const float SECTION_END_ADV = 4f; // 一个 section 结束后额外留白

    public static void Toggle() { if (Open) Close(); else OpenMenu(); }

    private static void OpenMenu()
    {
        if (!_cursorSaved)
        {
            _prevCursorVisible = Cursor.visible;
            _prevCursorLock = Cursor.lockState;
            _cursorSaved = true;
        }
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        Open = true;
    }

    private static void Close()
    {
        Open = false;
        if (_cursorSaved)
        {
            Cursor.visible = _prevCursorVisible;
            Cursor.lockState = _prevCursorLock;
            _cursorSaved = false;
        }
    }

    public static void Draw()
    {
        if (!Open) return;

        var s = ModMain.Settings;
        if (s != null) _scale = Mathf.Clamp(s.MenuScale, 0.6f, 3.0f);

        _window.width = W * _scale;
        _window.height = H * _scale;
        _window.x = Mathf.Clamp(_window.x, 0f, Screen.width - 120f);
        _window.y = Mathf.Clamp(_window.y, 0f, Screen.height - 60f);

        ApplyFontScale();
        _window = GUI.Window(WindowId, _window, (GUI.WindowFunction)DrawContents, "TldHacks v2.7.36");
    }

    // v2.7.5:基准字号降到 13pt(原 16 在 1.8x 时 29px 超过 22*1.8=39.6 的行距,section 重叠 toggle)
    // 13 * 1.8 = 23.4px,行距 39.6px,明显够 —— 4K 大缩放下也不再挤。
    private static void ApplyFontScale()
    {
        try
        {
            int fs = Mathf.Max(10, Mathf.RoundToInt(13f * _scale));
            var sk = GUI.skin;
            if (sk == null) return;
            if (sk.label != null) sk.label.fontSize = fs;
            if (sk.button != null) sk.button.fontSize = fs;
            if (sk.toggle != null) sk.toggle.fontSize = fs;
            if (sk.box != null) sk.box.fontSize = fs;
            if (sk.window != null) sk.window.fontSize = fs;
            if (sk.textField != null) sk.textField.fontSize = fs;
        }
        catch { }
    }

    private static void DrawContents(int id)
    {
        var s = ModMain.Settings;
        if (s == null)
        {
            GUI.Label(R(10f, 30f, W - 20f, 20f), "Settings not initialized.");
            GUI.DragWindow(R(0f, 0f, W, 24f));
            return;
        }

        // 标题栏右侧:scale - / + / Close
        if (GUI.Button(R(W - 192f, 3f, 22f, 20f), "-"))
        { s.MenuScale = Mathf.Max(0.6f, s.MenuScale - 0.1f); s.Save(); }
        GUI.Label(R(W - 166f, 3f, 50f, 20f), $"x{s.MenuScale:F1}");
        if (GUI.Button(R(W - 114f, 3f, 22f, 20f), "+"))
        { s.MenuScale = Mathf.Min(3.0f, s.MenuScale + 0.1f); s.Save(); }
        if (GUI.Button(R(W - 86f, 3f, 72f, 20f), "Close")) Close();

        // 标签页
        float tabW = 120f;
        for (int i = 0; i < Tabs.Length; i++)
        {
            string lbl = i == _activeTab ? $"[{Tabs[i]}]" : Tabs[i];
            if (GUI.Button(R(8f + i * (tabW + 4f), 26f, tabW, 22f), lbl)) _activeTab = i;
        }

        // ScrollView viewport:顶部 tabs 下(52 px)到 底部状态栏上(25 px)
        Rect viewport = R(4f, 52f, W - 8f, H - 80f);
        float ch = _activeTab == 0 ? ContentH_Main : ContentH_Spawner;
        Rect content  = R(0f, 0f, W - 30f, ch);  // 减 30 给滚动条
        _mainScroll = GUI.BeginScrollView(viewport, _mainScroll, content, false, true);

        if (_activeTab == 0)
        {
            float mainBottomY = DrawMainTab(s);
            DrawConsoleSection(s, mainBottomY + 14f);
        }
        else DrawSpawnerTab(s);

        GUI.EndScrollView();

        // 底部状态栏(在 scroll 区外,始终显示)
        GUI.Label(R(8f, H - 24f, W - 16f, 20f),
            $"Pos: {(string.IsNullOrEmpty(CheatState.PositionText) ? "-" : CheatState.PositionText)}   |   Last: {CheatState.LastActionLog}");

        GUI.DragWindow(R(0f, 0f, W - 200f, 24f));
    }

    // ————————————— Tab 1:主 cheat 面板 —————————————
    // v2.7.28 三列重排:天气/时间 从列 1 挪到列 3 底部,让 3 列高度更均衡
    // 列起点 x:10, 410, 810(列宽 400,中间 10 空隙)
    // 返回三列中最大的 y(之后 uConsole 区块从这里起)
    private static float DrawMainTab(TldHacksSettings s)
    {
        // 列起点
        // 三列起点,列宽 400,总宽 1200-中间空隙
        float c1 = 10f, c2 = 410f, c3 = 810f;
        float y1 = 6f, y2 = 6f, y3 = 6f;

        // ========== 第一列 ==========
        y1 = Section(c1, y1, "设置");
        GUI.Label(R(c1, y1, 330f, ROW_H), $"菜单键: {s.MenuHotkey}   飞行键: {s.FlyHotkey}");
        y1 += ROW_ADV + SECTION_END_ADV;

        y1 = Section(c1, y1, "Stacking");
        bool st = GUI.Toggle(R(c1, y1, 200f, ROW_H), s.StackingEnabled, " UI 堆叠");
        if (st != s.StackingEnabled) { s.StackingEnabled = st; s.Save(); }
        y1 += ROW_ADV + SECTION_END_ADV;

        y1 = Section(c1, y1, "生命");
        bool god = GUI.Toggle(R(c1, y1, 150f, ROW_H), s.GodMode, " 无敌模式");
        bool nfall = GUI.Toggle(R(c1 + 170f, y1, 150f, ROW_H), s.NoFallDamage, " 无坠落伤害");
        if (god != s.GodMode || nfall != s.NoFallDamage)
        { s.GodMode = god; s.NoFallDamage = nfall; CheatState.GodMode = god; CheatState.NoFallDamage = nfall; s.Save(); }
        y1 += ROW_ADV + SECTION_END_ADV;

        y1 = Section(c1, y1, "状态");
        bool warm = GUI.Toggle(R(c1, y1, 150f, ROW_H), s.AlwaysWarm, " 始终温暖");
        bool nhun = GUI.Toggle(R(c1 + 170f, y1, 150f, ROW_H), s.NoHunger, " 无饥饿");
        y1 += ROW_ADV;
        bool nthir= GUI.Toggle(R(c1, y1, 150f, ROW_H), s.NoThirst, " 无口渴");
        bool nfat = GUI.Toggle(R(c1 + 170f, y1, 150f, ROW_H), s.NoFatigue, " 无疲劳");
        y1 += ROW_ADV + SECTION_END_ADV;
        if (warm != s.AlwaysWarm || nhun != s.NoHunger || nthir != s.NoThirst || nfat != s.NoFatigue)
        {
            s.AlwaysWarm = warm; s.NoHunger = nhun; s.NoThirst = nthir; s.NoFatigue = nfat;
            CheatState.AlwaysWarm = warm;
            CheatState.NoHunger = nhun; CheatState.NoThirst = nthir; CheatState.NoFatigue = nfat;
            s.Save();
        }

        y1 = Section(c1, y1, "移动速度");
        float bx = c1;
        foreach (var sp in SpeedPresets)
        {
            bool active = Mathf.Abs(s.SpeedMultiplier - sp) < 0.01f;
            if (GUI.Button(R(bx, y1, 64f, ROW_H), active ? $"[{sp:F1}x]" : $"{sp:F1}x"))
            { s.SpeedMultiplier = sp; CheatState.SpeedMultiplier = sp; s.Save(); }
            bx += 70f;
        }
        y1 += ROW_ADV + SECTION_END_ADV;

        y1 = Section(c1, y1, "技能");
        for (int i = 0; i < Skills.All.Length; i++)
        {
            var (lbl, t) = Skills.All[i];
            GUI.Label(R(c1, y1, 100f, ROW_H), lbl);
            GUI.Label(R(c1 + 104f, y1, 40f, ROW_H), Skills.GetTier(t).ToString());
            if (GUI.Button(R(c1 + 150f, y1, 34f, ROW_H), "+")) Skills.SetMax(t);
            y1 += ROW_ADV;
        }
        if (GUI.Button(R(c1, y1, 180f, ROW_H), "全部满级")) Skills.SetAllMax();
        y1 += ROW_ADV + SECTION_END_ADV;

        // ========== 第二列 ==========
        y2 = Section(c2, y2, "动物");
        bool kill = GUI.Toggle(R(c2, y2, 160f, ROW_H), s.InstantKillAnimals, " 一击必杀");
        bool frz  = GUI.Toggle(R(c2 + 180f, y2, 160f, ROW_H), s.FreezeAnimals, " 动物不能动");
        y2 += ROW_ADV;
        bool stealth = GUI.Toggle(R(c2, y2, 220f, ROW_H), s.Stealth, " 动物自动逃跑");
        y2 += ROW_ADV;
        bool tinv = GUI.Toggle(R(c2, y2, 260f, ROW_H), s.TrueInvisible, " 真·隐身(检测不到)");
        y2 += ROW_ADV + SECTION_END_ADV;
        if (kill != s.InstantKillAnimals || frz != s.FreezeAnimals || stealth != s.Stealth || tinv != s.TrueInvisible)
        {
            s.InstantKillAnimals = kill; s.FreezeAnimals = frz; s.Stealth = stealth; s.TrueInvisible = tinv;
            CheatState.InstantKillAnimals = kill; CheatState.FreezeAnimals = frz;
            CheatState.Stealth = stealth; CheatState.TrueInvisible = tinv;
            s.Save();
        }

        y2 = Section(c2, y2, "环境");
        bool ice = GUI.Toggle(R(c2, y2, 160f, ROW_H), s.ThinIceNoBreak, " 冰面不破");
        bool lok = GUI.Toggle(R(c2 + 180f, y2, 160f, ROW_H), s.IgnoreLock, " 忽略上锁");
        y2 += ROW_ADV;
        bool qc  = GUI.Toggle(R(c2, y2, 160f, ROW_H), s.QuickOpenContainer, " 快速打开容器");
        bool wnd = GUI.Toggle(R(c2 + 180f, y2, 160f, ROW_H), s.StopWind, " 停止刮风");
        y2 += ROW_ADV + SECTION_END_ADV;
        if (ice != s.ThinIceNoBreak || lok != s.IgnoreLock || qc != s.QuickOpenContainer || wnd != s.StopWind)
        {
            s.ThinIceNoBreak = ice; s.IgnoreLock = lok; s.QuickOpenContainer = qc; s.StopWind = wnd;
            CheatState.ThinIceNoBreak = ice; CheatState.IgnoreLock = lok; CheatState.QuickOpenContainer = qc; CheatState.StopWind = wnd;
            s.Save();
        }

        y2 = Section(c2, y2, "地图");
        if (GUI.Button(R(c2, y2, 180f, ROW_H), "全开地图")) Cheats.RevealFullMap();
        y2 += ROW_ADV + SECTION_END_ADV;

        y2 = Section(c2, y2, "物品 / 衣物");
        bool dur = GUI.Toggle(R(c2, y2, 160f, ROW_H), s.InfiniteDurability, " 物品不损耗");
        bool wet = GUI.Toggle(R(c2 + 180f, y2, 160f, ROW_H), s.NoWetClothes, " 衣物不潮湿");
        y2 += ROW_ADV;
        // 火焰无限时长已移除 —— 其他 mod(InfiniteFiresDLC)覆盖 / 游戏内 H 键也可
        GUI.Label(R(c2, y2, 360f, ROW_H), "火焰无限时长:由其他 mod 覆盖,此处不做");
        y2 += ROW_ADV + SECTION_END_ADV;
        if (dur != s.InfiniteDurability || wet != s.NoWetClothes)
        {
            s.InfiniteDurability = dur; s.NoWetClothes = wet;
            CheatState.InfiniteDurability = dur; CheatState.NoWetClothes = wet;
            s.Save();
        }

        y2 = Section(c2, y2, "一次性操作");
        if (GUI.Button(R(c2, y2, 260f, ROW_H), "清除所有负面")) Cheats.ClearAllAfflictions();
        y2 += ROW_ADV;
        if (GUI.Button(R(c2, y2, 170f, ROW_H), "恢复全部耐久")) Cheats.RestoreAllSceneGear();
        if (GUI.Button(R(c2 + 180f, y2, 170f, ROW_H), "修复背包物品")) QuickActions.RepairAllInventory();
        y2 += ROW_ADV;
        if (GUI.Button(R(c2, y2, 260f, ROW_H), "修复手持物品")) Cheats.RepairItemInHands();
        y2 += ROW_ADV + SECTION_END_ADV;

        y2 = Section(c2, y2, "解锁");
        if (GUI.Button(R(c2, y2, 170f, ROW_H), "解锁全部壮举")) Feats.UnlockAllFeats();
        // v2.7.34 UI 标记:蓝图解锁实测失效 —— FindObjectsOfType 找不到 Blueprint (ScriptableObject not MonoBehaviour)
        //   临时解法:开 "免费制作" toggle 等效 —— 所有蓝图都能 craft,无需解锁
        if (GUI.Button(R(c2 + 180f, y2, 170f, ROW_H), "解锁蓝图⚠(失效/用免费制作)")) QuickActions.UnlockAllBlueprints();
        y2 += ROW_ADV + SECTION_END_ADV;

        y2 = Section(c2, y2, "免疫");
        bool nspr  = GUI.Toggle(R(c2, y2, 160f, ROW_H), s.NoSprainRisk, " 免扭伤风险");
        bool immA  = GUI.Toggle(R(c2 + 180f, y2, 180f, ROW_H), s.ImmuneAnimalDamage, " 免动物伤害");
        y2 += ROW_ADV;
        bool nsuf  = GUI.Toggle(R(c2, y2, 160f, ROW_H), s.NoSuffocating, " 不会窒息");
        y2 += ROW_ADV + SECTION_END_ADV;
        if (nspr != s.NoSprainRisk || immA != s.ImmuneAnimalDamage || nsuf != s.NoSuffocating)
        {
            s.NoSprainRisk = nspr; s.ImmuneAnimalDamage = immA; s.NoSuffocating = nsuf;
            CheatState.NoSprainRisk = nspr; CheatState.ImmuneAnimalDamage = immA; CheatState.NoSuffocating = nsuf;
            s.Save();
        }

        // ========== 第三列 ==========
        y3 = Section(c3, y3, "制作 / 节约时间");
        bool fc = GUI.Toggle(R(c3, y3, 160f, ROW_H), s.FreeCraft, " 免费制作");
        bool qk = GUI.Toggle(R(c3 + 180f, y3, 160f, ROW_H), s.QuickCraft, " 快速制作");
        y3 += ROW_ADV;
        bool qf = GUI.Toggle(R(c3, y3, 160f, ROW_H), s.QuickFire, " 生火 100%");
        bool qcl= GUI.Toggle(R(c3 + 180f, y3, 160f, ROW_H), s.QuickClimb, " 爬绳 ×5");
        y3 += ROW_ADV;
        bool qa = GUI.Toggle(R(c3, y3, 340f, ROW_H), s.QuickAction, " 采集/修理/拆解 自动加速");
        y3 += ROW_ADV + SECTION_END_ADV;
        if (fc != s.FreeCraft || qk != s.QuickCraft || qf != s.QuickFire || qcl != s.QuickClimb || qa != s.QuickAction)
        {
            s.FreeCraft = fc; s.QuickCraft = qk; s.QuickFire = qf; s.QuickClimb = qcl; s.QuickAction = qa;
            CheatState.FreeCraft = fc; CheatState.QuickCraft = qk; CheatState.QuickFire = qf; CheatState.QuickClimb = qcl;
            CheatState.QuickAction = qa;
            s.Save();
        }

        y3 = Section(c3, y3, "武器");
        bool amm = GUI.Toggle(R(c3, y3, 160f, ROW_H), s.InfiniteAmmo, " 无限弹药");
        bool jam = GUI.Toggle(R(c3 + 180f, y3, 160f, ROW_H), s.NoJam, " 永不卡壳");
        y3 += ROW_ADV;
        bool rec = GUI.Toggle(R(c3, y3, 160f, ROW_H), s.NoRecoil, " 无后坐力");
        bool ff  = GUI.Toggle(R(c3 + 180f, y3, 160f, ROW_H), s.FastFire, " 快速射击");
        y3 += ROW_ADV + SECTION_END_ADV;
        if (amm != s.InfiniteAmmo || jam != s.NoJam || rec != s.NoRecoil || ff != s.FastFire)
        { s.InfiniteAmmo = amm; s.NoJam = jam; s.NoRecoil = rec; s.FastFire = ff;
          CheatState.InfiniteAmmo = amm; CheatState.NoJam = jam; CheatState.NoRecoil = rec; CheatState.FastFire = ff; s.Save(); }

        // 一键获取武器
        if (GUI.Button(R(c3, y3, 110f, ROW_H), "获取弓")) QuickActions.GiveWeapon("GEAR_Bow");
        if (GUI.Button(R(c3 + 120f, y3, 110f, ROW_H), "获取信号枪")) QuickActions.GiveWeapon("GEAR_FlareGun");
        if (GUI.Button(R(c3 + 240f, y3, 110f, ROW_H), "获取步枪")) QuickActions.GiveWeapon("GEAR_Rifle");
        y3 += ROW_ADV;
        if (GUI.Button(R(c3, y3, 110f, ROW_H), "获取左轮")) QuickActions.GiveWeapon("GEAR_Revolver");
        if (GUI.Button(R(c3 + 120f, y3, 110f, ROW_H), "获取猎熊矛")) QuickActions.GiveWeapon("GEAR_BearSpear");
        if (GUI.Button(R(c3 + 240f, y3, 110f, ROW_H), "获取猎刀")) QuickActions.GiveWeapon("GEAR_Knife");
        y3 += ROW_ADV + SECTION_END_ADV;

        y3 = Section(c3, y3, "瞄准");
        bool sway = GUI.Toggle(R(c3, y3, 160f, ROW_H), s.NoAimSway, " 关闭瞄准晃动");
        bool shk  = GUI.Toggle(R(c3 + 180f, y3, 160f, ROW_H), s.NoAimShake, " 关闭瞄准抖动");
        y3 += ROW_ADV;
        bool brth = GUI.Toggle(R(c3, y3, 160f, ROW_H), s.NoBreathSway, " 关闭呼吸晃动");
        bool nstam= GUI.Toggle(R(c3 + 180f, y3, 200f, ROW_H), s.NoAimStamina, " 关闭瞄准体力消耗");
        y3 += ROW_ADV;
        bool ndof = GUI.Toggle(R(c3, y3, 160f, ROW_H), s.NoAimDOF, " 关闭瞄准景深");
        y3 += ROW_ADV + SECTION_END_ADV;
        if (sway != s.NoAimSway || shk != s.NoAimShake || brth != s.NoBreathSway
            || nstam != s.NoAimStamina || ndof != s.NoAimDOF)
        {
            s.NoAimSway = sway; s.NoAimShake = shk; s.NoBreathSway = brth;
            s.NoAimStamina = nstam; s.NoAimDOF = ndof;
            CheatState.NoAimSway = sway; CheatState.NoAimShake = shk; CheatState.NoBreathSway = brth;
            CheatState.NoAimStamina = nstam; CheatState.NoAimDOF = ndof;
            s.Save();
        }

        // 天气 / 时间 —— v2.7.28 从列 1 挪到列 3 底部让三列更均衡
        y3 = Section(c3, y3, "天气 / 时间");
        float bx3 = c3;
        for (int i = 0; i < WeatherStages.Length; i++)
        {
            if (GUI.Button(R(bx3, y3, 58f, ROW_H), WeatherLabels[i])) Cheats.SetWeatherStage(WeatherStages[i]);
            bx3 += 62f;
            if ((i + 1) % 5 == 0 && i < WeatherStages.Length - 1) { bx3 = c3; y3 += ROW_ADV; }
        }
        y3 += ROW_ADV;
        bx3 = c3;
        for (int i = 0; i < HourPresets.Length; i++)
        {
            if (GUI.Button(R(bx3, y3, 62f, ROW_H), HourLabels[i])) Cheats.SetTimeOfDay(HourPresets[i]);
            bx3 += 66f;
        }
        y3 += ROW_ADV + SECTION_END_ADV;

        return Mathf.Max(y1, Mathf.Max(y2, y3));
    }

    // ————————————— Tab 2:物品刷出 + 传送 —————————————
    // v2.7.28 重排:15 个传送目的地改 5 列 × 3 行;Item Spawner 2 列 × 3 行(6/页)→ 3 列 × 6 行(18/页)
    private static void DrawSpawnerTab(TldHacksSettings s)
    {
        float y = 6f;

        // ========== 快速传送(5 列 × 3 行,紧凑) ==========
        GUI.Label(R(10f, y, 400f, ROW_H), $"—— 快速传送({Teleport.Destinations.Count} 个目的地) ——");
        y += ROW_ADV;
        const int teleCols = 5;
        float teleW = (W - 30f) / teleCols;    // 每列宽度
        for (int i = 0; i < Teleport.Destinations.Count; i++)
        {
            var d = Teleport.Destinations[i];
            float tx = 10f + (i % teleCols) * teleW;
            float ty = y + (i / teleCols) * ROW_ADV;
            if (GUI.Button(R(tx, ty, teleW - 4f, ROW_H), $"→ {d.Label}"))
                Teleport.TravelTo(d);
        }
        y += ROW_ADV * ((Teleport.Destinations.Count + teleCols - 1) / teleCols) + SECTION_END_ADV;

        // 位置控制行
        if (GUI.Button(R(10f, y, 180f, ROW_H), "打印位置")) ConsoleBridge.Run("pos");
        if (GUI.Button(R(200f, y, 180f, ROW_H), "反射刷新位置")) Cheats.UpdatePlayerPosition();
        GUI.Label(R(400f, y, W - 410f, ROW_H),
            $"当前:{(string.IsNullOrEmpty(CheatState.PositionText) ? "(未获取)" : CheatState.PositionText)}");
        y += ROW_ADV + SECTION_END_ADV;

        GUI.Box(R(10f, y, W - 20f, 1f), "");
        y += 8f;

        // ========== 物品刷出 ==========
        GUI.Label(R(10f, y, 400f, ROW_H), $"Item Spawner ({ItemDatabase.All.Count} 条)");
        y += ROW_ADV;

        // 类别 tabs (全 9 个平铺)
        float catW = (W - 20f) / ItemDatabase.Categories.Length;
        for (int i = 0; i < ItemDatabase.Categories.Length; i++)
        {
            string lbl = i == _selectedCategory ? $"[{ItemDatabase.Categories[i]}]" : ItemDatabase.Categories[i];
            if (GUI.Button(R(10f + i * catW, y, catW - 2f, ROW_H), lbl))
            {
                if (_selectedCategory != i) { _selectedCategory = i; _page = 0; }
            }
        }
        y += ROW_ADV + SECTION_END_ADV;

        // 数量预设
        GUI.Label(R(10f, y, 60f, ROW_H), "数量:");
        float bx = 70f;
        for (int i = 0; i < QuantityPresets.Length; i++)
        {
            string lbl = _quantity == QuantityPresets[i] ? $"[×{QuantityPresets[i]}]" : $"×{QuantityPresets[i]}";
            if (GUI.Button(R(bx, y, 70f, ROW_H), lbl)) _quantity = QuantityPresets[i];
            bx += 75f;
        }
        GUI.Label(R(bx + 10f, y, 140f, ROW_H), $"当前: {_quantity}");
        y += ROW_ADV + SECTION_END_ADV;

        if (_lastCat != _selectedCategory) { RebuildFilter(); _lastCat = _selectedCategory; }

        int totalPages = Mathf.Max(1, (_filtered.Count + PageSize - 1) / PageSize);
        _page = Mathf.Clamp(_page, 0, totalPages - 1);
        int start = _page * PageSize;
        int end = Mathf.Min(start + PageSize, _filtered.Count);

        // 3 列 × 6 行 = 18 个/页,每列宽 (W-40)/3 ≈ 387
        const int cols = 3, rows = 6;
        float colW = (W - 40f) / cols;
        float btnW = 56f;  // + ×N 按钮宽度缩小,留更多空间给名字
        for (int idx = start; idx < end; idx++)
        {
            int i = idx - start;
            int col = i % cols, row = i / cols;
            float x = 10f + col * colW;
            float yy = y + row * ROW_ADV;
            var e = _filtered[idx];
            if (GUI.Button(R(x, yy, btnW, ROW_H), $"+×{_quantity}"))
                Cheats.SpawnItem(e.PrefabName, _quantity);
            GUI.Label(R(x + btnW + 6f, yy, colW - btnW - 14f, ROW_H),
                $"{e.Name}" + (e.Calories > 0 ? $" {e.Calories}k" : ""));
        }
        y += ROW_ADV * rows + SECTION_END_ADV;

        // 分页控制
        if (GUI.Button(R(10f, y, 100f, ROW_H), "◀ 上一页")) { if (_page > 0) _page--; }
        GUI.Label(R(120f, y, 350f, ROW_H), $"页 {_page + 1}/{totalPages}   共 {_filtered.Count} 个");
        if (GUI.Button(R(480f, y, 100f, ROW_H), "下一页 ▶")) { if (_page < totalPages - 1) _page++; }
        // 快速跳页
        if (GUI.Button(R(600f, y, 80f, ROW_H), "⏮ 首页")) _page = 0;
        if (GUI.Button(R(690f, y, 80f, ROW_H), "末页 ⏭")) _page = totalPages - 1;
    }

    private static float Section(float x, float y, string title)
    {
        GUI.Box(R(x, y, 390f, SEC_H), title);
        return y + SEC_ADV;
    }

    // —— 合并进 Tab 1 底部的 uConsole 命令区块 ——
    // 这些功能通过 TLD 内置 uConsole 命令调用。需要 DeveloperConsole.dll 已装。
    // v2.7.28:接收三列最大 y,动态起点,不再 hard-code
    private static void DrawConsoleSection(TldHacksSettings s, float y)
    {
        // 跨列分隔线
        GUI.Box(R(10f, y, W - 20f, 1f), "");
        y += 6f;
        GUI.Label(R(10f, y, W - 20f, ROW_H),
            "━━━━━ uConsole 命令区 ━━━━━ 需要 DeveloperConsole.dll;状态不持久,重启游戏要重开");
        y += ROW_ADV;

        // —— 状态 toggle ——
        // (uConsole 状态 toggle + 飞行 全部删除 —— set_invulnerable / set_invisible /
        //  force_no_jam / force_no_random_sprain / fly 在 release build 里都 no-op。
        //  对应功能用 Tab 1 的 GodMode / Stealth / NoJam / NoSprainRisk 替代。)
        // (天气锁定 UI 也去掉,用 Tab 1 第一列"天气 / 时间"即可)

        // —— 温度锁定 —— ⚠ 慎用 ——
        y = Section(10f, y, "温度锁定 ⚠ 慎用(解除后温度不会回到原值,必须重载存档)");
        int[] temps = { 20, 10, 0, -10, -30 };
        float bx = 10f;
        for (int i = 0; i < temps.Length; i++)
        {
            if (GUI.Button(R(bx, y, 95f, ROW_H), $"锁 {temps[i]}°C")) ConsoleBridge.Run($"lock_temperature {temps[i]}");
            bx += 100f;
        }
        if (GUI.Button(R(bx, y, 130f, ROW_H), "尝试解除锁定"))
        {
            ConsoleBridge.Run("unlock_temperature");
        }
        y += ROW_ADV;
        GUI.Label(R(10f, y, W - 20f, ROW_H),
            "⚠ 警告:一旦锁温度,游戏存的原始天气温度会丢,解除锁定后温度不会回原值。只用于测试,用完重载存档");
        y += ROW_ADV + SECTION_END_ADV;

        // —— 物品 / 动物 ——
        y = Section(10f, y, "一键操作");
        if (GUI.Button(R(10f, y, 180f, ROW_H), "添加全部物品")) ConsoleBridge.Run("add_all_gear");
        if (GUI.Button(R(200f, y, 180f, ROW_H), "秒杀所有动物")) ConsoleBridge.Run("kill_all_animals");
        if (GUI.Button(R(390f, y, 180f, ROW_H), "修复传输器")) ConsoleBridge.Run("repair_transmitters");
        y += ROW_ADV + SECTION_END_ADV;

        // —— 生成动物 ——
        y = Section(10f, y, "生成动物 spawn_*");
        // v2.7.34 UI 标记:母鹿实测失效(可能是 2.55 内部行为变了),加 ⚠ 提示
        string[] spawnCmds = { "spawn_wolf", "spawn_bear", "spawn_cougar", "spawn_moose", "spawn_doe", "spawn_stag", "spawn_rabbit", "spawn_ptarmigan" };
        string[] spawnLbls = { "狼", "熊", "美洲狮", "驼鹿", "母鹿⚠", "雄鹿", "兔", "松鸡" };
        bx = 10f;
        for (int i = 0; i < spawnCmds.Length; i++)
        {
            if (GUI.Button(R(bx, y, 95f, ROW_H), spawnLbls[i])) ConsoleBridge.Run(spawnCmds[i]);
            bx += 100f;
            if (i == 3) { bx = 10f; y += ROW_ADV; }
        }
        y += ROW_ADV + SECTION_END_ADV;

        // —— 商人 ——
        y = Section(10f, y, "商人 Trader");
        if (GUI.Button(R(10f, y, 200f, ROW_H), "秒完成交易")) ConsoleBridge.Run("trader_trade_force_completed");
        if (GUI.Button(R(220f, y, 200f, ROW_H), "刷新交易清单")) ConsoleBridge.Run("trader_reset_drawn_exchanges");
        if (GUI.Button(R(430f, y, 200f, ROW_H), "信任 +100")) ConsoleBridge.Run("trader_trust_add 100");
        y += ROW_ADV;
        if (GUI.Button(R(10f, y, 200f, ROW_H), "解锁所有对话")) ConsoleBridge.Run("trader_unlock_conversation_all");
        if (GUI.Button(R(220f, y, 200f, ROW_H), "解锁所有交易")) ConsoleBridge.Run("trader_unlock_exchange_all");
        if (GUI.Button(R(430f, y, 200f, ROW_H), "解锁所有改进")) ConsoleBridge.Run("trader_unlock_improvement_all");
        y += ROW_ADV + SECTION_END_ADV;
    }

    private static void RebuildFilter()
    {
        _filtered.Clear();
        string cat = ItemDatabase.Categories[_selectedCategory];
        for (int i = 0; i < ItemDatabase.All.Count; i++)
        {
            var e = ItemDatabase.All[i];
            if (_selectedCategory != 0 && e.Category != cat) continue;
            _filtered.Add(e);
        }
    }
}
