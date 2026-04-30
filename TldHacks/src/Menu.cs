using System.Collections.Generic;
using UnityEngine;

namespace TldHacks;

// Cursor 风格暗色 IMGUI。全部用 GUI.Xxx(Rect,..) 不用 GUILayout(被 strip)。
// Rect 通过 R(x,y,w,h) 乘 _scale。
internal static class Menu
{
    public static bool Open;

    private const float W = 1280f;
    private const float H = 760f;
    // v2.7.28 ContentH 动态:tab0 主 cheat + uConsole 1400,tab1 spawner 只到 850
    //   之前固定 2200 → tab1 底部有 ~1300 空白可滚但无内容
    // v2.7.46:CT 复刻加了 16 toggle 分布三列,列最长 ~900 + Console 区 ~500 → 1600f 防截断
    private const float ContentH_Main = 1600f;
    // v2.7.55: Spawner content = viewport 等高,无滚动;物品行数动态填满
    private const float ContentH_Spawner = H - 108f;
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
    // v2.7.55: PageSize 改成动态,按 UI 框可用高度自适应(cols 固定 3)
    private static int _pageSize = 18;
    private static int _pageRows = 6;
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

    // v2.7.72 技能详细列表默认折叠,只显示"全部满级"
    private static bool _skillsExpanded = false;

    // 标签页
    private static readonly string[] Tabs = { "主要 + uConsole", "物品 & 传送" };
    private static int _activeTab = 0;

    private static Rect R(float x, float y, float w, float h)
        => new Rect(x * _scale, y * _scale, w * _scale, h * _scale);

    // Cursor Ink 配色
    private static readonly Color CursorBG = new Color32(0x1E, 0x1F, 0x22, 0xFF);
    private static readonly Color CursorPanel = new Color32(0x2B, 0x2D, 0x31, 0xFF);
    private static readonly Color CursorPanelHi = new Color32(0x35, 0x37, 0x3C, 0xFF);
    private static readonly Color CursorAccent = new Color32(0x59, 0x9C, 0xFF, 0xFF);
    private static readonly Color CursorAccentDim = new Color(0.35f, 0.61f, 1f, 0.25f);
    private static readonly Color CursorText = new Color32(0xF2, 0xF3, 0xF5, 0xFF);
    private static readonly Color CursorTextMuted = new Color32(0x9B, 0xA1, 0xAB, 0xFF);
    private static readonly Color CursorDanger = new Color32(0xFA, 0x79, 0x70, 0xFF);

    // v2.7.73 Cursor 风格 spacing / 三列栅格
    private const float OUTER_PAD = 16f;
    private const float COL_GAP = 16f;
    private const float SEC_H = 30f;
    private const float SEC_ADV = 38f;
    private const float ROW_H = 26f;
    private const float ROW_ADV = 30f;
    private const float SECTION_END_ADV = 14f;
    private const float COL_W = 405f;
    private const float BOX_W = 405f;
    private const float TOG_W = 182f;
    private const float TOG2_OFF = 198f;
    private const float TOG_WIDE = 380f;

    private static bool _stylesReady;
    private static Texture2D _bgTex;
    private static Texture2D _panelTex;
    private static Texture2D _panelHiTex;
    private static Texture2D _accentTex;
    private static Texture2D _accentDimTex;
    private static Texture2D _dangerTex;
    private static GUIStyle _windowStyle;
    private static GUIStyle _labelStyle;
    private static GUIStyle _mutedLabelStyle;
    private static GUIStyle _titleStyle;
    private static GUIStyle _sectionStyle;
    private static GUIStyle _sectionTitleStyle;
    private static GUIStyle _buttonStyle;
    private static GUIStyle _buttonActiveStyle;
    private static GUIStyle _buttonDangerStyle;
    private static GUIStyle _toggleStyle;
    private static GUIStyle _statusStyle;

    private static Texture2D Tex(Color color)
    {
        var tex = new Texture2D(1, 1);
        // v2.7.73 场景切换/uConsole 开启后 UI 变透明 —— Unity 把没挂树的 Texture2D 当可回收资源清掉,
        //   残留的 GUIStyle.background 指向 dead texture → 渲染透明。HideAndDontSave 锁死生命周期
        tex.hideFlags = HideFlags.HideAndDontSave;
        tex.SetPixel(0, 0, color);
        tex.Apply();
        return tex;
    }

    private static GUIStyleState State(Texture2D bg, Color text)
        => new GUIStyleState { background = bg, textColor = text };

    private static void InitStyles()
    {
        // v2.7.73 兜底:即使加了 HideAndDontSave,某些 Il2Cpp 下的 texture 仍可能失效。
        //   Unity 的 == null 对 destroyed Object 返回 true,触发重建
        if (_stylesReady && _bgTex == null) _stylesReady = false;

        if (!_stylesReady)
        {
            _bgTex = Tex(CursorBG);
            _panelTex = Tex(CursorPanel);
            _panelHiTex = Tex(CursorPanelHi);
            _accentTex = Tex(CursorAccent);
            _accentDimTex = Tex(CursorAccentDim);
            _dangerTex = Tex(CursorDanger);

            _windowStyle = new GUIStyle(GUI.skin.window);
            _labelStyle = new GUIStyle(GUI.skin.label);
            _mutedLabelStyle = new GUIStyle(GUI.skin.label);
            _titleStyle = new GUIStyle(GUI.skin.label);
            _sectionStyle = new GUIStyle(GUI.skin.box);
            _sectionTitleStyle = new GUIStyle(GUI.skin.label);
            _buttonStyle = new GUIStyle(GUI.skin.button);
            _buttonActiveStyle = new GUIStyle(GUI.skin.button);
            _buttonDangerStyle = new GUIStyle(GUI.skin.button);
            _toggleStyle = new GUIStyle(GUI.skin.button);
            _statusStyle = new GUIStyle(GUI.skin.label);
            _stylesReady = true;
        }

        int body = Mathf.Max(10, Mathf.RoundToInt(12f * _scale));
        int h1 = Mathf.Max(12, Mathf.RoundToInt(15f * _scale));
        int h2 = Mathf.Max(11, Mathf.RoundToInt(13f * _scale));

        _windowStyle.normal.background = _bgTex;
        _windowStyle.normal.textColor = CursorText;
        _windowStyle.padding = new RectOffset(0, 0, 0, 0);
        _windowStyle.border = new RectOffset(0, 0, 0, 0);
        _windowStyle.fontSize = h1;

        ConfigureLabel(_labelStyle, body, CursorText);
        ConfigureLabel(_mutedLabelStyle, body, CursorTextMuted);
        ConfigureLabel(_titleStyle, h1, CursorText);
        ConfigureLabel(_sectionTitleStyle, h2, CursorText);
        ConfigureLabel(_statusStyle, body, CursorTextMuted);

        ConfigureBox(_sectionStyle, _panelTex, CursorText);
        ConfigureButton(_buttonStyle, _panelHiTex, CursorText, body);
        ConfigureButton(_buttonActiveStyle, _accentTex, CursorText, body);
        ConfigureButton(_buttonDangerStyle, _dangerTex, CursorText, body);
        ConfigureButton(_toggleStyle, _panelHiTex, CursorText, body);
        _toggleStyle.onNormal = State(_panelHiTex, CursorAccent);
        _toggleStyle.onHover = State(_panelHiTex, CursorAccent);
        _toggleStyle.onActive = State(_accentDimTex, CursorText);
        _toggleStyle.onFocused = State(_panelHiTex, CursorAccent);

        var skin = GUI.skin;
        if (skin == null) return;
        skin.window = _windowStyle;
        skin.label = _labelStyle;
        skin.box = _sectionStyle;
        skin.button = _buttonStyle;
        skin.toggle = _toggleStyle;
    }

    private static void ConfigureLabel(GUIStyle style, int size, Color color)
    {
        style.fontSize = size;
        style.normal = State(null, color);
        style.hover = State(null, color);
        style.active = State(null, color);
        style.focused = State(null, color);
        style.padding = new RectOffset(6, 6, 0, 0);
    }

    private static void ConfigureBox(GUIStyle style, Texture2D bg, Color text)
    {
        style.normal = State(bg, text);
        style.hover = State(bg, text);
        style.active = State(bg, text);
        style.focused = State(bg, text);
        style.border = new RectOffset(1, 1, 1, 1);
        style.padding = new RectOffset(0, 0, 0, 0);
    }

    private static void ConfigureButton(GUIStyle style, Texture2D bg, Color text, int size)
    {
        style.fontSize = size;
        style.normal = State(bg, text);
        style.hover = State(bg == _accentTex || bg == _dangerTex ? bg : _accentDimTex, CursorText);
        style.active = State(bg == _accentTex || bg == _dangerTex ? bg : _accentDimTex, CursorText);
        style.focused = State(bg, text);
        style.padding = new RectOffset(8, 8, 0, 0);
        style.border = new RectOffset(1, 1, 1, 1);
    }

    private static bool CBtn(float x, float y, float w, float h, string text, bool active = false, bool danger = false)
        => GUI.Button(R(x, y, w, h), text, danger ? _buttonDangerStyle : active ? _buttonActiveStyle : _buttonStyle);

    private static bool CTog(float x, float y, float w, float h, bool value, string text)
        => GUI.Toggle(R(x, y, w, h), value, text, _toggleStyle);

    private static void CLabel(float x, float y, float w, float h, string text, bool muted = false)
        => GUI.Label(R(x, y, w, h), text, muted ? _mutedLabelStyle : _labelStyle);

    private static void Divider(float x, float y, float w)
        => GUI.Box(R(x, y, w, 1f), "", _buttonStyle);

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

        InitStyles();
        _window = GUI.Window(WindowId, _window, (GUI.WindowFunction)DrawContents, "");
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

        GUI.Box(R(0f, 0f, W, H), "", _windowStyle);
        GUI.Label(R(16f, 8f, 260f, 24f), "TldHacks v2.7.74", _titleStyle);
        GUI.Label(R(16f, 30f, 500f, 18f), "Cursor Ink UI / IL2CPP IMGUI", _mutedLabelStyle);

        // 标题栏右侧:scale - / + / Close
        if (CBtn(W - 192f, 8f, 28f, 24f, "-"))
        { s.MenuScale = Mathf.Max(0.6f, s.MenuScale - 0.1f); s.Save(); }
        // v2.7.73 x{scale} label 垂直居中 —— 不用 TextAnchor(csproj 没引 TextRenderingModule),
        //   用坐标下移:label 默认 UpperLeft 字体 top 在 y,按钮 MiddleCenter 字体 top 约 y+(h-fs)/2=14,
        //   label y: 8 → 14,高度缩到 18 避免溢出,和按钮 -/+ 文字基线对齐
        GUI.Label(R(W - 158f, 14f, 50f, 18f), $"x{s.MenuScale:F1}", _mutedLabelStyle);
        if (CBtn(W - 104f, 8f, 28f, 24f, "+"))
        { s.MenuScale = Mathf.Min(3.0f, s.MenuScale + 0.1f); s.Save(); }
        if (CBtn(W - 70f, 8f, 54f, 24f, "Close", false, true)) Close();

        // 标签页
        float tabW = 150f;
        for (int i = 0; i < Tabs.Length; i++)
        {
            float tx = 16f + i * (tabW + 8f);
            if (CBtn(tx, 52f, tabW, 28f, Tabs[i])) _activeTab = i;
            if (i == _activeTab) GUI.Box(R(tx, 82f, tabW, 2f), "", _buttonActiveStyle);
        }

        // ScrollView viewport:顶部 title/tab 下到 底部状态栏上
        Rect viewport = R(OUTER_PAD, 92f, W - OUTER_PAD * 2f, H - 128f);
        float ch = _activeTab == 0 ? ContentH_Main : ContentH_Spawner;
        Rect content  = R(0f, 0f, W - OUTER_PAD * 2f, ch);
        _mainScroll = GUI.BeginScrollView(viewport, _mainScroll, content, false, true);

        if (_activeTab == 0)
        {
            float mainBottomY = DrawMainTab(s);
            DrawConsoleSection(s, mainBottomY + 14f);
        }
        else DrawSpawnerTab(s);

        GUI.EndScrollView();

        // 底部状态栏(在 scroll 区外,始终显示)
        GUI.Box(R(0f, H - 34f, W, 34f), "");
        GUI.Label(R(16f, H - 28f, W - 32f, 22f),
            $"Pos: {(string.IsNullOrEmpty(CheatState.PositionText) ? "-" : CheatState.PositionText)}   |   Last: {CheatState.LastActionLog}",
            _statusStyle);

        GUI.DragWindow(R(0f, 0f, W - 220f, 44f));
    }

    // ————————————— Tab 1:主 cheat 面板 —————————————
    // v2.7.54 重设:所有 2-toggle 行统一 TOG_W=180 + TOG2_OFF=200
    //   整列 width 400;Section box 宽 BOX_W=390;toggle 宽度一致防错位
    //   地图/天气/时间 合并到一个 "世界时钟" section
    //   Settings 字段一律保留,兼容旧存档
    private static float DrawMainTab(TldHacksSettings s)
    {
        float c1 = 0f, c2 = COL_W + COL_GAP, c3 = (COL_W + COL_GAP) * 2f;
        float y1 = 0f, y2 = 0f, y3 = 0f;

        // ═════════ 列 1:玩家状态 ═════════
        y1 = Section(c1, y1, "生存");
        bool god   = GUI.Toggle(R(c1,            y1, TOG_W, ROW_H), s.GodMode,          " 无敌模式");
        bool nfall = GUI.Toggle(R(c1 + TOG2_OFF, y1, TOG_W, ROW_H), s.NoFallDamage,     " 无坠落伤害");
        y1 += ROW_ADV;
        bool nspr  = GUI.Toggle(R(c1,            y1, TOG_W, ROW_H), s.NoSprainRisk,     " 免扭伤风险");
        bool immA  = GUI.Toggle(R(c1 + TOG2_OFF, y1, TOG_W, ROW_H), s.ImmuneAnimalDamage," 免动物伤害");
        y1 += ROW_ADV;
        bool nsuf  = GUI.Toggle(R(c1,            y1, TOG_W, ROW_H), s.NoSuffocating,    " 不会窒息");
        bool cdp   = GUI.Toggle(R(c1 + TOG2_OFF, y1, TOG_W, ROW_H), s.ClearDeathPenalty," 清除死亡惩罚");
        y1 += ROW_ADV + SECTION_END_ADV;
        if (god != s.GodMode || nfall != s.NoFallDamage || nspr != s.NoSprainRisk
            || immA != s.ImmuneAnimalDamage || nsuf != s.NoSuffocating || cdp != s.ClearDeathPenalty)
        {
            s.GodMode = god; s.NoFallDamage = nfall; s.NoSprainRisk = nspr;
            s.ImmuneAnimalDamage = immA; s.NoSuffocating = nsuf; s.ClearDeathPenalty = cdp;
            CheatState.GodMode = god; CheatState.NoFallDamage = nfall; CheatState.NoSprainRisk = nspr;
            CheatState.ImmuneAnimalDamage = immA; CheatState.NoSuffocating = nsuf; CheatState.ClearDeathPenalty = cdp;
            s.Save();
        }

        y1 = Section(c1, y1, "温饱");
        bool warm  = GUI.Toggle(R(c1,            y1, TOG_W, ROW_H), s.AlwaysWarm, " 始终温暖");
        bool nhun  = GUI.Toggle(R(c1 + TOG2_OFF, y1, TOG_W, ROW_H), s.NoHunger,   " 无饥饿");
        y1 += ROW_ADV;
        bool nthir = GUI.Toggle(R(c1,            y1, TOG_W, ROW_H), s.NoThirst,   " 无口渴");
        bool nfat  = GUI.Toggle(R(c1 + TOG2_OFF, y1, TOG_W, ROW_H), s.NoFatigue,  " 无疲劳");
        y1 += ROW_ADV;
        bool fcv   = GUI.Toggle(R(c1,            y1, TOG_WIDE, ROW_H), s.FreezeColdValue, " 冻结寒冷值(开启抓当前)");
        y1 += ROW_ADV + SECTION_END_ADV;
        if (warm != s.AlwaysWarm || nhun != s.NoHunger || nthir != s.NoThirst || nfat != s.NoFatigue || fcv != s.FreezeColdValue)
        {
            s.AlwaysWarm = warm; s.NoHunger = nhun; s.NoThirst = nthir; s.NoFatigue = nfat; s.FreezeColdValue = fcv;
            CheatState.AlwaysWarm = warm;
            CheatState.NoHunger = nhun; CheatState.NoThirst = nthir; CheatState.NoFatigue = nfat;
            CheatState.FreezeColdValue = fcv;
            s.Save();
        }

        y1 = Section(c1, y1, "移动速度");
        float bx1 = c1;
        foreach (var sp in SpeedPresets)
        {
            bool active = Mathf.Abs(s.SpeedMultiplier - sp) < 0.01f;
            if (CBtn(bx1, y1, 92f, ROW_H, $"{sp:F1}x", active))
            { s.SpeedMultiplier = sp; CheatState.SpeedMultiplier = sp; s.Save(); }
            bx1 += 96f;
        }
        y1 += ROW_ADV + SECTION_END_ADV;

        y1 = Section(c1, y1, "技能");
        // v2.7.72 默认折叠 —— 只显示"全部满级",详细按需展开
        if (GUI.Button(R(c1,               y1, TOG_W, ROW_H), "全部满级"))
            Skills.SetAllMax();
        if (GUI.Button(R(c1 + TOG_W + 8f,  y1, 80f, ROW_H), _skillsExpanded ? "收起 ▲" : "展开 ▼"))
            _skillsExpanded = !_skillsExpanded;
        y1 += ROW_ADV;
        if (_skillsExpanded)
        {
            for (int i = 0; i < Skills.All.Length; i++)
            {
                var (lbl, t) = Skills.All[i];
                GUI.Label (R(c1 + 8f,       y1, 120f, ROW_H), lbl);
                GUI.Label (R(c1 + 138f,     y1, 40f,  ROW_H), Skills.GetTier(t).ToString());
                if (GUI.Button(R(c1 + 186f, y1, 44f,  ROW_H), "满")) Skills.SetMax(t);
                y1 += ROW_ADV;
            }
        }
        y1 += SECTION_END_ADV;

        // —— 从列 3 搬来:制作 ——
        y1 = Section(c1, y1, "制作");
        bool fc = GUI.Toggle(R(c1,            y1, TOG_W, ROW_H), s.FreeCraft,  " 免费制作");
        bool qk = GUI.Toggle(R(c1 + TOG2_OFF, y1, TOG_W, ROW_H), s.QuickCraft, " 快速制作");
        y1 += ROW_ADV + SECTION_END_ADV;
        if (fc != s.FreeCraft || qk != s.QuickCraft)
        {
            s.FreeCraft = fc; s.QuickCraft = qk;
            CheatState.FreeCraft = fc; CheatState.QuickCraft = qk;
            s.Save();
        }

        // —— 从列 2 搬来:锁 & 容器 ——
        y1 = Section(c1, y1, "锁 & 容器");
        bool lok  = GUI.Toggle(R(c1,            y1, TOG_W, ROW_H), s.IgnoreLock,        " 忽略上锁");
        bool qc   = GUI.Toggle(R(c1 + TOG2_OFF, y1, TOG_W, ROW_H), s.QuickOpenContainer," 快速开容器");
        y1 += ROW_ADV;
        bool usaf = GUI.Toggle(R(c1,            y1, TOG_WIDE, ROW_H), s.UnlockSafes,   " 解锁保险箱/门");
        y1 += ROW_ADV + SECTION_END_ADV;
        if (lok != s.IgnoreLock || qc != s.QuickOpenContainer || usaf != s.UnlockSafes)
        {
            s.IgnoreLock = lok; s.QuickOpenContainer = qc; s.UnlockSafes = usaf;
            CheatState.IgnoreLock = lok; CheatState.QuickOpenContainer = qc; CheatState.UnlockSafes = usaf;
            s.Save();
        }

        // —— 从列 3 搬来:瞄准 ——
        y1 = Section(c1, y1, "瞄准");
        bool sway = GUI.Toggle(R(c1,            y1, TOG_W, ROW_H), s.NoAimSway,     " 瞄准无晃动");
        bool shk  = GUI.Toggle(R(c1 + TOG2_OFF, y1, TOG_W, ROW_H), s.NoAimShake,    " 瞄准无抖动");
        y1 += ROW_ADV;
        bool brth = GUI.Toggle(R(c1,            y1, TOG_W, ROW_H), s.NoBreathSway,  " 呼吸无晃动");
        bool nstam= GUI.Toggle(R(c1 + TOG2_OFF, y1, TOG_W, ROW_H), s.NoAimStamina,  " 瞄准不耗体力");
        y1 += ROW_ADV;
        bool ndof = GUI.Toggle(R(c1,            y1, TOG_WIDE, ROW_H), s.NoAimDOF,   " 关闭瞄准景深");
        y1 += ROW_ADV + SECTION_END_ADV;
        if (sway != s.NoAimSway || shk != s.NoAimShake || brth != s.NoBreathSway
            || nstam != s.NoAimStamina || ndof != s.NoAimDOF)
        {
            s.NoAimSway = sway; s.NoAimShake = shk; s.NoBreathSway = brth;
            s.NoAimStamina = nstam; s.NoAimDOF = ndof;
            CheatState.NoAimSway = sway; CheatState.NoAimShake = shk; CheatState.NoBreathSway = brth;
            CheatState.NoAimStamina = nstam; CheatState.NoAimDOF = ndof;
            s.Save();
        }

        // ═════════ 列 2:世界 & 环境 & 一次性 ═════════
        y2 = Section(c2, y2, "动物");
        bool kill    = GUI.Toggle(R(c2,            y2, TOG_W, ROW_H), s.InstantKillAnimals," 一击必杀");
        bool frz     = GUI.Toggle(R(c2 + TOG2_OFF, y2, TOG_W, ROW_H), s.FreezeAnimals,     " 动物冰冻");
        y2 += ROW_ADV;
        bool stealth = GUI.Toggle(R(c2,            y2, TOG_W, ROW_H), s.Stealth,           " 动物逃跑");
        bool tinv    = GUI.Toggle(R(c2 + TOG2_OFF, y2, TOG_W, ROW_H), s.TrueInvisible,     " 真·隐身");
        y2 += ROW_ADV + SECTION_END_ADV;
        if (kill != s.InstantKillAnimals || frz != s.FreezeAnimals || stealth != s.Stealth || tinv != s.TrueInvisible)
        {
            s.InstantKillAnimals = kill; s.FreezeAnimals = frz; s.Stealth = stealth; s.TrueInvisible = tinv;
            CheatState.InstantKillAnimals = kill; CheatState.FreezeAnimals = frz;
            CheatState.Stealth = stealth; CheatState.TrueInvisible = tinv;
            s.Save();
        }

        y2 = Section(c2, y2, "环境 / 篝火");
        bool ice  = GUI.Toggle(R(c2,            y2, TOG_W, ROW_H), s.ThinIceNoBreak, " 冰面不破");
        bool wnd  = GUI.Toggle(R(c2 + TOG2_OFF, y2, TOG_W, ROW_H), s.StopWind,       " 停止刮风");
        y2 += ROW_ADV;
        bool ftmp = GUI.Toggle(R(c2,            y2, TOG_W, ROW_H), s.FireTemp300,    " 篝火 300℃");
        bool fnev = GUI.Toggle(R(c2 + TOG2_OFF, y2, TOG_W, ROW_H), s.FireNeverDie,   " 篝火永不熄灭");
        y2 += ROW_ADV + SECTION_END_ADV;
        if (ice != s.ThinIceNoBreak || wnd != s.StopWind
            || ftmp != s.FireTemp300 || fnev != s.FireNeverDie)
        {
            s.ThinIceNoBreak = ice; s.StopWind = wnd; s.FireTemp300 = ftmp; s.FireNeverDie = fnev;
            CheatState.ThinIceNoBreak = ice; CheatState.StopWind = wnd;
            CheatState.FireTemp300 = ftmp; CheatState.FireNeverDie = fnev;
            s.Save();
        }

        // 世界时钟 — 地图 + 天气(3行) + 时间(1行)合并一块
        y2 = Section(c2, y2, "世界时钟 — 地图 / 天气 / 时间");
        if (GUI.Button(R(c2, y2, TOG_WIDE, ROW_H), "全开地图")) Cheats.RevealFullMap();
        y2 += ROW_ADV + 2f;
        // 天气 — 每行 5 个 + 最后一行剩余
        float bx2 = c2;
        for (int i = 0; i < WeatherStages.Length; i++)
        {
            if (GUI.Button(R(bx2, y2, 72f, ROW_H), WeatherLabels[i])) Cheats.SetWeatherStage(WeatherStages[i]);
            bx2 += 76f;
            if ((i + 1) % 5 == 0 && i < WeatherStages.Length - 1) { bx2 = c2; y2 += ROW_ADV; }
        }
        y2 += ROW_ADV + 2f;
        // 时间 — 4 个小时 preset
        bx2 = c2;
        for (int i = 0; i < HourPresets.Length; i++)
        {
            if (GUI.Button(R(bx2, y2, 92f, ROW_H), HourLabels[i])) Cheats.SetTimeOfDay(HourPresets[i]);
            bx2 += 96f;
        }
        y2 += ROW_ADV + SECTION_END_ADV;

        y2 = Section(c2, y2, "一次性操作");
        if (GUI.Button(R(c2,             y2, TOG_W, ROW_H), "清除所有负面"))   Cheats.ClearAllAfflictions();
        if (GUI.Button(R(c2 + TOG2_OFF,  y2, TOG_W, ROW_H), "解锁全部壮举"))   Feats.UnlockAllFeats();
        y2 += ROW_ADV;
        if (GUI.Button(R(c2,             y2, TOG_W, ROW_H), "恢复全部耐久"))   Cheats.RestoreAllSceneGear();
        if (GUI.Button(R(c2 + TOG2_OFF,  y2, TOG_W, ROW_H), "修复背包"))       QuickActions.RepairAllInventory();
        y2 += ROW_ADV;
        if (GUI.Button(R(c2,             y2, TOG_WIDE, ROW_H), "修复手持物品")) Cheats.RepairItemInHands();
        y2 += ROW_ADV + SECTION_END_ADV;

        // 商人 & 美洲狮(CT 复刻 v2.7.55)
        y2 = Section(c2, y2, "商人 & 美洲狮");
        bool tul = GUI.Toggle(R(c2,            y2, TOG_W, ROW_H), s.TraderUnlimitedList,   " 交易清单上限 64");
        bool tmt = GUI.Toggle(R(c2 + TOG2_OFF, y2, TOG_W, ROW_H), s.TraderMaxTrust,        " 信任值最大化");
        y2 += ROW_ADV;
        bool tix = GUI.Toggle(R(c2,            y2, TOG_W, ROW_H), s.TraderInstantExchange, " 交易秒完成");
        bool taa = GUI.Toggle(R(c2 + TOG2_OFF, y2, TOG_W, ROW_H), s.TraderAlwaysAvailable, " 随时可联系商人");
        y2 += ROW_ADV;
        bool cia = GUI.Toggle(R(c2,            y2, TOG_WIDE, ROW_H), s.CougarInstantActivate, " 美洲狮首次立即激活(进/出门、睡觉触发动画)");
        y2 += ROW_ADV + SECTION_END_ADV;
        if (tul != s.TraderUnlimitedList || tmt != s.TraderMaxTrust
            || tix != s.TraderInstantExchange || taa != s.TraderAlwaysAvailable
            || cia != s.CougarInstantActivate)
        {
            s.TraderUnlimitedList = tul; s.TraderMaxTrust = tmt;
            s.TraderInstantExchange = tix; s.TraderAlwaysAvailable = taa;
            s.CougarInstantActivate = cia;
            CheatState.TraderUnlimitedList = tul; CheatState.TraderMaxTrust = tmt;
            CheatState.TraderInstantExchange = tix; CheatState.TraderAlwaysAvailable = taa;
            CheatState.CougarInstantActivate = cia;
            s.Save();
        }

        // ═════════ 列 3:物品 & 武器 ═════════
        y3 = Section(c3, y3, "快速操作(CT 复刻)");
        bool qcook = GUI.Toggle(R(c3,            y3, TOG_W, ROW_H), s.QuickCook,      " 秒烤肉");
        bool qsrch = GUI.Toggle(R(c3 + TOG2_OFF, y3, TOG_W, ROW_H), s.QuickSearch,    " 秒搜刮/采摘");
        y3 += ROW_ADV;
        bool qhv   = GUI.Toggle(R(c3,            y3, TOG_W, ROW_H), s.QuickHarvest,   " 秒割肉");
        bool qbd   = GUI.Toggle(R(c3 + TOG2_OFF, y3, TOG_W, ROW_H), s.QuickBreakDown, " 秒打碎");
        y3 += ROW_ADV;
        bool qev   = GUI.Toggle(R(c3,            y3, TOG_W, ROW_H), s.QuickEvolve,    " 加工秒完成");
        bool qf    = GUI.Toggle(R(c3 + TOG2_OFF, y3, TOG_W, ROW_H), s.QuickFire,      " 生火 100%");
        y3 += ROW_ADV;
        bool qcl   = GUI.Toggle(R(c3,            y3, TOG_W, ROW_H), s.QuickClimb,     " 爬绳 ×5");
        y3 += ROW_ADV + SECTION_END_ADV;
        if (qcook != s.QuickCook || qsrch != s.QuickSearch || qhv != s.QuickHarvest
            || qbd != s.QuickBreakDown || qev != s.QuickEvolve || qf != s.QuickFire || qcl != s.QuickClimb)
        {
            s.QuickCook = qcook; s.QuickSearch = qsrch; s.QuickHarvest = qhv;
            s.QuickBreakDown = qbd; s.QuickEvolve = qev; s.QuickFire = qf; s.QuickClimb = qcl;
            CheatState.QuickCook = qcook; CheatState.QuickSearch = qsrch;
            CheatState.QuickHarvest = qhv; CheatState.QuickBreakDown = qbd;
            CheatState.QuickEvolve = qev; CheatState.QuickFire = qf; CheatState.QuickClimb = qcl;
            s.Save();
        }

        y3 = Section(c3, y3, "物品 & 装备");
        bool stk   = GUI.Toggle(R(c3,            y3, TOG_W, ROW_H), s.StackingEnabled,    " UI 堆叠");
        bool dur   = GUI.Toggle(R(c3 + TOG2_OFF, y3, TOG_W, ROW_H), s.InfiniteDurability, " 物品不损耗");
        y3 += ROW_ADV;
        bool wet   = GUI.Toggle(R(c3,            y3, TOG_W, ROW_H), s.NoWetClothes,       " 衣物不潮湿");
        bool lamp  = GUI.Toggle(R(c3 + TOG2_OFF, y3, TOG_W, ROW_H), s.LampFuelNoDrain,    " 油灯不耗油");
        y3 += ROW_ADV;
        bool flsk1 = GUI.Toggle(R(c3,            y3, TOG_W, ROW_H), s.FlaskNoHeatLoss,    " 保温杯不失温");
        bool flsk2 = GUI.Toggle(R(c3 + TOG2_OFF, y3, TOG_W, ROW_H), s.FlaskInfiniteVol,   " 保温杯无限容量");
        y3 += ROW_ADV;
        bool flsk3 = GUI.Toggle(R(c3,            y3, TOG_WIDE, ROW_H), s.FlaskAnyItem,    " 保温瓶装任意液体");
        y3 += ROW_ADV;
        bool bnop  = GUI.Toggle(R(c3,            y3, TOG_WIDE, ROW_H), s.BlockAutoPickupOwnDrops, " ItemPicker 不捡自己丢的物品");
        y3 += ROW_ADV + SECTION_END_ADV;
        if (stk != s.StackingEnabled || dur != s.InfiniteDurability || wet != s.NoWetClothes
            || lamp != s.LampFuelNoDrain || flsk1 != s.FlaskNoHeatLoss
            || flsk2 != s.FlaskInfiniteVol || flsk3 != s.FlaskAnyItem
            || bnop != s.BlockAutoPickupOwnDrops)
        {
            s.StackingEnabled = stk; s.InfiniteDurability = dur; s.NoWetClothes = wet;
            s.LampFuelNoDrain = lamp; s.FlaskNoHeatLoss = flsk1;
            s.FlaskInfiniteVol = flsk2; s.FlaskAnyItem = flsk3;
            s.BlockAutoPickupOwnDrops = bnop;
            CheatState.InfiniteDurability = dur; CheatState.NoWetClothes = wet;
            CheatState.LampFuelNoDrain = lamp; CheatState.FlaskNoHeatLoss = flsk1;
            CheatState.FlaskInfiniteVol = flsk2; CheatState.FlaskAnyItem = flsk3;
            CheatState.BlockAutoPickupOwnDrops = bnop;
            s.Save();
        }

        y3 = Section(c3, y3, "武器 / 射击");
        bool amm = GUI.Toggle(R(c3,            y3, TOG_W, ROW_H), s.InfiniteAmmo, " 无限弹药");
        bool jam = GUI.Toggle(R(c3 + TOG2_OFF, y3, TOG_W, ROW_H), s.NoJam,        " 永不卡壳");
        y3 += ROW_ADV;
        bool rec = GUI.Toggle(R(c3,            y3, TOG_WIDE, ROW_H), s.NoRecoil,  " 无后坐力");
        // v2.7.71 删除 FastFire —— 射击 state machine bug 难修
        y3 += ROW_ADV + SECTION_END_ADV;
        if (amm != s.InfiniteAmmo || jam != s.NoJam || rec != s.NoRecoil)
        {
            s.InfiniteAmmo = amm; s.NoJam = jam; s.NoRecoil = rec;
            CheatState.InfiniteAmmo = amm; CheatState.NoJam = jam; CheatState.NoRecoil = rec;
            s.Save();
        }

        // v2.7.71 一键获取武器重构 —— 4 列 × 2 行
        //   武器:弓 / 步枪 / 左轮 / 斧头 / 猎刀
        //   弹药:50× 箭 / 50× 步枪子弹 / 50× 左轮子弹
        y3 = Section(c3, y3, "一键获取武器");
        const float wbw = 92f, wgap = 4f;
        if (GUI.Button(R(c3,                      y3, wbw, ROW_H), "弓"))          QuickActions.GiveWeapon("GEAR_Bow");
        if (GUI.Button(R(c3 +   (wbw + wgap),     y3, wbw, ROW_H), "步枪"))        QuickActions.GiveWeapon("GEAR_Rifle");
        if (GUI.Button(R(c3 + 2*(wbw + wgap),     y3, wbw, ROW_H), "左轮"))        QuickActions.GiveWeapon("GEAR_Revolver");
        if (GUI.Button(R(c3 + 3*(wbw + wgap),     y3, wbw, ROW_H), "斧头"))        QuickActions.GiveWeapon("GEAR_Hatchet");
        y3 += ROW_ADV;
        if (GUI.Button(R(c3,                      y3, wbw, ROW_H), "猎刀"))        QuickActions.GiveWeapon("GEAR_Knife");
        if (GUI.Button(R(c3 +   (wbw + wgap),     y3, wbw, ROW_H), "箭 ×50"))      Cheats.SpawnItem("GEAR_Arrow", 50);
        if (GUI.Button(R(c3 + 2*(wbw + wgap),     y3, wbw, ROW_H), "步枪弹 ×50"))  Cheats.SpawnItem("GEAR_RifleAmmoSingle", 50);
        if (GUI.Button(R(c3 + 3*(wbw + wgap),     y3, wbw, ROW_H), "左轮弹 ×50"))  Cheats.SpawnItem("GEAR_RevolverAmmoSingle", 50);
        y3 += ROW_ADV + SECTION_END_ADV;

        // —— 从列 2 搬来:商人 uConsole 命令 ——
        // CT toggle 不覆盖的功能:刷新清单 / 解锁对话/交易/改进 —— 每按一次
        y3 = Section(c3, y3, "商人 uConsole 命令");
        const float cbw = 120f, cgap = 5f;
        if (GUI.Button(R(c3,                       y3, cbw, ROW_H), "秒完成交易")) ConsoleBridge.Run("trader_trade_force_completed");
        if (GUI.Button(R(c3 + cbw + cgap,          y3, cbw, ROW_H), "刷新清单"))   ConsoleBridge.Run("trader_reset_drawn_exchanges");
        if (GUI.Button(R(c3 + 2*(cbw + cgap),      y3, cbw, ROW_H), "信任 +100")) ConsoleBridge.Run("trader_trust_add 100");
        y3 += ROW_ADV;
        if (GUI.Button(R(c3,                       y3, cbw, ROW_H), "解锁对话"))   ConsoleBridge.Run("trader_unlock_conversation_all");
        if (GUI.Button(R(c3 + cbw + cgap,          y3, cbw, ROW_H), "解锁交易"))   ConsoleBridge.Run("trader_unlock_exchange_all");
        if (GUI.Button(R(c3 + 2*(cbw + cgap),      y3, cbw, ROW_H), "解锁改进"))   ConsoleBridge.Run("trader_unlock_improvement_all");
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

        // 位置控制行 —— v2.7.55 "打印位置到 log" 按钮:在 Latest.log 留 [POS-MARK],mod 作者可回读坐标
        if (GUI.Button(R(10f, y, 160f, ROW_H), "uConsole pos"))    ConsoleBridge.Run("pos");
        if (GUI.Button(R(180f, y, 160f, ROW_H), "刷新位置显示"))   Cheats.UpdatePlayerPosition();
        if (GUI.Button(R(350f, y, 200f, ROW_H), "★ 打印坐标到 log")) Cheats.PrintPositionToLog();
        GUI.Label(R(560f, y, W - 570f, ROW_H),
            $"当前:{(string.IsNullOrEmpty(CheatState.PositionText) ? "(未获取)" : CheatState.PositionText)}");
        y += ROW_ADV + SECTION_END_ADV;

        GUI.Box(R(10f, y, W - 20f, 1f), "");
        y += 8f;

        // ========== 物品刷出 ==========
        GUI.Label(R(10f, y, 400f, ROW_H), $"Item Spawner ({ItemDatabase.All.Count + ItemDatabaseMod.All.Count} 条)");
        y += ROW_ADV;

        // 类别 tabs (全 9 个平铺)
        float catW = (W - 20f) / ItemDatabase.Categories.Length;
        for (int i = 0; i < ItemDatabase.Categories.Length; i++)
        {
            string lbl = ItemDatabase.Categories[i];
            if (CBtn(10f + i * catW, y, catW - 2f, ROW_H, lbl, i == _selectedCategory))
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
            string lbl = $"×{QuantityPresets[i]}";
            if (CBtn(bx, y, 70f, ROW_H, lbl, _quantity == QuantityPresets[i])) _quantity = QuantityPresets[i];
            bx += 75f;
        }
        GUI.Label(R(bx + 10f, y, 140f, ROW_H), $"当前: {_quantity}");
        y += ROW_ADV + SECTION_END_ADV;

        if (_lastCat != _selectedCategory) { RebuildFilter(); _lastCat = _selectedCategory; }

        // v2.7.57: 动态 PageSize —— 根据剩余可视高度塞满物品
        //   v2.7.73 修分页消失:H-80 是 v2.7.72 前的 viewport,codex 改 ContentH_Spawner=H-108 后
        //   这里还按 H-80 算 → 物品多塞 ~28px → 分页按钮被裁出 scroll content 外看不见
        //   cols 从 3 改 4 —— 每页物品数 +33%,总页数减少
        const int cols = 4;
        float availH = ContentH_Spawner - y - (ROW_ADV + 8f);
        _pageRows = Mathf.Max(3, (int)(availH / ROW_ADV));
        _pageSize = _pageRows * cols;

        int totalPages = Mathf.Max(1, (_filtered.Count + _pageSize - 1) / _pageSize);
        _page = Mathf.Clamp(_page, 0, totalPages - 1);
        int start = _page * _pageSize;
        int end = Mathf.Min(start + _pageSize, _filtered.Count);

        float colW = (W - 40f) / cols;
        float btnW = 56f;
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
        y += ROW_ADV * _pageRows + SECTION_END_ADV;

        // 分页控制
        if (GUI.Button(R(10f, y, 100f, ROW_H), "◀ 上一页")) { if (_page > 0) _page--; }
        GUI.Label(R(120f, y, 500f, ROW_H),
            $"页 {_page + 1}/{totalPages}   共 {_filtered.Count} 个   每页 {_pageSize}");
        if (GUI.Button(R(630f, y, 100f, ROW_H), "下一页 ▶")) { if (_page < totalPages - 1) _page++; }
        if (GUI.Button(R(750f, y, 80f, ROW_H), "⏮ 首页"))   _page = 0;
        if (GUI.Button(R(840f, y, 80f, ROW_H), "末页 ⏭"))   _page = totalPages - 1;
    }

    private static float Section(float x, float y, string title)
    {
        GUI.Box(R(x, y, BOX_W, SEC_H), "");
        GUI.Box(R(x, y, 3f, SEC_H), "", _buttonActiveStyle);
        GUI.Label(R(x + 10f, y + 4f, BOX_W - 20f, SEC_H - 8f), title, _sectionTitleStyle);
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

        // v2.7.74 删除 uConsole 锁温度:unlock_temperature 游戏内损坏,锁完无法恢复;
        //   改用玩家列的 "冻结寒冷值" toggle(走 Freezing.m_CurrentFreezing 路径,toggle off 干净)
        float bx = 10f;

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
        for (int i = 0; i < ItemDatabaseMod.All.Count; i++)
        {
            var e = ItemDatabaseMod.All[i];
            if (_selectedCategory != 0 && e.Category != cat) continue;
            _filtered.Add(e);
        }
    }
}
