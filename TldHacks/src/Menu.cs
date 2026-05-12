using System;
using System.Collections.Generic;
using Il2Cpp;
using UnityEngine;

namespace TldHacks;

// Cursor 风格暗色 IMGUI。全部用 GUI.Xxx(Rect,..) 不用 GUILayout(被 strip)。
// Rect 通过 R(x,y,w,h) 乘 _scale。
internal static class Menu
{
    public static bool Open;

    private static float W = 1280f;
    private static float H = 760f;
    internal static float ContentW => W - 20f;
    private const float W_MIN = 600f, W_MAX = 2400f;
    private const float H_MIN = 400f, H_MAX = 1200f;
    private static float _measuredMainH = 800f;
    private static float _measuredSpawnerH = 800f;
    private static float _measuredTweaksH = 400f;
    private static readonly Func<float, float, float, float, Rect> RFunc = R;
    private static float ContentH_Spawner => H - 130f;
    private static Vector2 _mainScroll;
    private static Rect _window = new Rect(30f, 30f, W, H);
    private const int WindowId = 0x71D4_AC;

    private static float _scale = 1f;
    private static int _cols = 3;

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
    private static readonly string[] HourLabelsZh = { "晨8", "午12", "暮18", "夜22" };
    private static readonly string[] HourLabelsEn = { "8am", "Noon", "6pm", "10pm" };
    private static string[] HourLabels => I18n.IsEnglish ? HourLabelsEn : HourLabelsZh;
    // 修正映射:WeatherStage enum 实际值 DenseFog=0/LightSnow=1/HeavySnow=2/PartlyCloudy=3/Clear=4/Cloudy=5
    //                              LightFog=6/Blizzard=7/ClearAurora=8/ToxicFog=9/ElectrostaticFog=10
    private static readonly int[] WeatherStages = { 4, 3, 5, 1, 2, 7, 6, 0, 8, 9, 10 };
    private static readonly string[] WeatherLabelsZh = { "晴", "局部多云", "多云", "小雪", "大雪", "暴风雪", "薄雾", "浓雾", "极光", "毒雾", "电磁雾" };
    private static readonly string[] WeatherLabelsEn = { "Clear", "Partly", "Cloudy", "Lt Snow", "Hvy Snow", "Blizzard", "Mist", "Fog", "Aurora", "Toxic", "EM Fog" };
    private static string[] WeatherLabels => I18n.IsEnglish ? WeatherLabelsEn : WeatherLabelsZh;
    private static List<ItemEntry> _filtered = new List<ItemEntry>();
    private static int _lastCat = -1;
    private static bool _namesResolved;

    // v2.7.72 技能详细列表默认折叠,只显示"全部满级"
    private static bool _skillsExpanded = false;

    // v6.6 GC 优化:缓存每帧都渲染的字符串,仅在值变化时重建
    private static string _cachedStatusBar = "";
    private static string _cachedPosText = "";
    private static string _cachedLastAction = "";

    private static string _cachedScurvyLabel = "";
    private static float  _cachedVitC = -1f;
    private static string _cachedScurvyStatus = "";

    private static readonly string[] SpeedPresetLabels = { "0.5x", "1.0x", "2.0x", "5.0x" };

    // Spawner tab: 预计算 QuantityPreset 标签 + 当前数量标签缓存
    private static readonly string[] QuantityPresetLabels = { "×1", "×5", "×10", "×50", "×100" };
    private static string _cachedQtyLabel = "Now: 1";
    private static string _cachedQtyLabelZh = "当前: 1";
    private static int    _cachedQtyValue = 1;
    private static string _cachedSpawnBtnLabel = "+×1";
    private static string _cachedPageInfo = "";
    private static int    _cachedPageNum = -1;
    private static int    _cachedTotalPages = -1;
    private static int    _cachedFilteredCount = -1;

    // 标签页
    private static readonly string[] TabsZh = { "主页", "物品 & 传送", "人物 & 生存", "装备 & 世界", "光火 & 制作", "视觉 & 显示" };
    private static readonly string[] TabsEn = { "Main", "Items & Teleport", "Character", "Gear & World", "Light & Craft", "Visual & Display" };
    private static string[] Tabs => I18n.IsEnglish ? TabsEn : TabsZh;
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
    // v2.7.83:从 405 → 380,与 toggle 行(TOG2_OFF+TOG_W=380)右对齐
    private const float BOX_W = 380f;
    private const float TOG_W = 182f;
    private const float TOG2_OFF = 198f;
    private const float TOG_WIDE = 380f;

    private static bool _stylesReady;
    private static Texture2D _bgTex;
    private static Texture2D _transparentTex;
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
    private static GUIStyle _rowLabelStyle;       // v4.0 物品生成器行 Label 垂直居中,与按钮基线对齐

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
            _transparentTex = Tex(new Color(0f, 0f, 0f, 0f));
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
            _rowLabelStyle = new GUIStyle(GUI.skin.label);
            _stylesReady = true;
        }

        int body = Mathf.Max(10, Mathf.RoundToInt(12f * _scale));
        int h1 = Mathf.Max(12, Mathf.RoundToInt(15f * _scale));
        int h2 = Mathf.Max(11, Mathf.RoundToInt(13f * _scale));

        _windowStyle.normal = State(_bgTex, CursorText);
        _windowStyle.hover = State(_bgTex, CursorText);
        _windowStyle.active = State(_bgTex, CursorText);
        _windowStyle.focused = State(_bgTex, CursorText);
        _windowStyle.onNormal = State(_bgTex, CursorText);
        _windowStyle.onHover = State(_bgTex, CursorText);
        _windowStyle.onActive = State(_bgTex, CursorText);
        _windowStyle.onFocused = State(_bgTex, CursorText);
        _windowStyle.padding = new RectOffset(0, 0, 0, 0);
        _windowStyle.border = new RectOffset(0, 0, 0, 0);
        _windowStyle.margin = new RectOffset(0, 0, 0, 0);
        _windowStyle.overflow = new RectOffset(0, 0, 0, 0);
        _windowStyle.contentOffset = Vector2.zero;
        _windowStyle.fontSize = h1;

        ConfigureLabel(_labelStyle, body, CursorText);
        ConfigureLabel(_mutedLabelStyle, body, CursorTextMuted);
        ConfigureLabel(_titleStyle, h1, CursorText);
        ConfigureLabel(_sectionTitleStyle, h2, CursorText);
        ConfigureLabel(_statusStyle, body, CursorTextMuted);
        ConfigureLabel(_rowLabelStyle, body, CursorText);
        _rowLabelStyle.alignment = TextAnchor.MiddleLeft;

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

        MenuTweaks.SectionBox = _sectionStyle;
        MenuTweaks.AccentBar = _buttonActiveStyle;
        MenuTweaks.LabelStyle = _labelStyle;
        MenuTweaks.MutedLabel = _mutedLabelStyle;
        MenuTweaks.SectionTitle = _sectionTitleStyle;
        MenuTweaks.ToggleStyle = _toggleStyle;
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
        // v2.7.83:从 Settings 恢复窗口位置
        var s = ModMain.Settings;
        if (s != null)
        {
            _window.x = Mathf.Clamp(s.MenuX, 0f, Screen.width - 120f);
            _window.y = Mathf.Clamp(s.MenuY, 0f, Screen.height - 60f);
        }
        Open = true;
    }

    private static void Close()
    {
        // v2.7.83:关闭时保存窗口位置 + 缩放
        var s = ModMain.Settings;
        if (s != null)
        {
            s.MenuX = _window.x;
            s.MenuY = _window.y;
            s.MenuScale = Mathf.Round(_scale * 10f) / 10f;
            s.Save();
        }
        _resizing = false;
        Open = false;
        if (_cursorSaved)
        {
            Cursor.visible = _prevCursorVisible;
            Cursor.lockState = _prevCursorLock;
            _cursorSaved = false;
        }
    }

    private static float _prevWinX, _prevWinY;
    private static int _saveCooldown = 0; // 拖动停止后延迟保存,避免每帧写磁盘
    // v2.7.94:自定义拖动(屏幕坐标驱动,鼠标出界仍能拖)
    private static bool _dragging = false;
    private static Vector2 _dragStartScreenMouse;
    private static float _dragStartWinX, _dragStartWinY;
    // v2.7.91:窗口尺寸拖拽(宽高独立)
    private static bool _resizing = false;
    private static Vector2 _resizeStartMouse;
    private static Vector2 _resizeStartScreenMouse;
    private static float _resizeStartW, _resizeStartH;
    private enum ResizeEdge { None, Right, Bottom, Corner }
    private static ResizeEdge _resizeEdge = ResizeEdge.None;

    public static void Draw()
    {
        if (!Open) return;

        var s = ModMain.Settings;
        if (s != null)
        {
            _scale = Mathf.Clamp(s.MenuScale, 0.6f, 3.0f);
            W = Mathf.Clamp(s.MenuWidth, W_MIN, W_MAX);
            H = Mathf.Clamp(s.MenuHeight, H_MIN, H_MAX);
        }
        _cols = W >= 1100f ? 3 : W >= 700f ? 2 : 1;

        _window.width = W * _scale;
        _window.height = H * _scale;
        _window.x = Mathf.Clamp(_window.x, 0f, Screen.width - 120f);
        _window.y = Mathf.Clamp(_window.y, 0f, Screen.height - 60f);

        InitStyles();

        // v2.7.94:自定义拖动(屏幕坐标驱动,鼠标出界仍能丝滑拖动)
        if (_dragging)
        {
            Vector2 screenMouse = Input.mousePosition;
            screenMouse.y = Screen.height - screenMouse.y;
            _window.x = Mathf.Clamp(_dragStartWinX + (screenMouse.x - _dragStartScreenMouse.x), 0f, Screen.width - 120f);
            _window.y = Mathf.Clamp(_dragStartWinY + (screenMouse.y - _dragStartScreenMouse.y), 0f, Screen.height - 60f);
            if (!Input.GetMouseButton(0)) _dragging = false;
        }

        // resize 用 Input.mousePosition(屏幕坐标,不受窗口裁剪)驱动,
        // 鼠标跑出窗口边界也能丝滑拖拽
        if (_resizing)
        {
            Vector2 screenMouse = Input.mousePosition;
            screenMouse.y = Screen.height - screenMouse.y;
            float dx = (screenMouse.x - _resizeStartScreenMouse.x) / _scale;
            float dy = (screenMouse.y - _resizeStartScreenMouse.y) / _scale;
            if (_resizeEdge == ResizeEdge.Right || _resizeEdge == ResizeEdge.Corner)
                W = Mathf.Clamp(_resizeStartW + dx, W_MIN, W_MAX);
            if (_resizeEdge == ResizeEdge.Bottom || _resizeEdge == ResizeEdge.Corner)
                H = Mathf.Clamp(_resizeStartH + dy, H_MIN, H_MAX);
            _window.width = W * _scale;
            _window.height = H * _scale;
            if (!Input.GetMouseButton(0))
            {
                _resizing = false;
                _resizeEdge = ResizeEdge.None;
                W = Mathf.Round(W / 10f) * 10f;
                H = Mathf.Round(H / 10f) * 10f;
                if (s != null) { s.MenuWidth = W; s.MenuHeight = H; s.Save(); }
            }
        }

        _window = GUI.Window(WindowId, _window, (GUI.WindowFunction)DrawContents, "");

        // v2.7.83:检测拖动 → 延迟保存(位置稳定 30 帧后才写磁盘)
        if (s != null)
        {
            if (Mathf.Abs(_window.x - _prevWinX) > 0.5f || Mathf.Abs(_window.y - _prevWinY) > 0.5f)
            {
                _prevWinX = _window.x;
                _prevWinY = _window.y;
                _saveCooldown = 30; // 位置还在变,重置计时器
            }
            else if (_saveCooldown > 0 && --_saveCooldown == 0)
            {
                // 位置稳定 30 帧(0.5s),写一次磁盘
                s.MenuX = _window.x;
                s.MenuY = _window.y;
                s.Save();
            }
        }
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
            GUI.Label(R(10f, 30f, W - 20f, 20f), I18n.T("配置未初始化。", "Settings not initialized."));
            return;
        }

        GUI.Box(R(0f, 0f, W, H), "", _windowStyle);
        GUI.Label(R(16f, 8f, W - 200f, 24f), I18n.T(
            "TldHacks v6.6  本软件完全免费|作者:AKUL|最新版本&交流群:563092033",
            "TldHacks v6.6  Free software | Author: AKUL | Latest & QQ Group: 563092033"), _titleStyle);
        GUI.Label(R(16f, 30f, 500f, 18f), "Cursor Ink UI / IL2CPP IMGUI", _mutedLabelStyle);

        // 标题栏右侧:scale - / x1.0 / + / Close (v2.7.83 统一间距 4px)
        if (CBtn(W - 148f, 8f, 28f, 24f, "-"))
        { s.MenuScale = Mathf.Max(0.6f, s.MenuScale - 0.1f); s.Save(); }
        // label 居中在 - 和 + 之间
        var prevAlign = _mutedLabelStyle.alignment;
        _mutedLabelStyle.alignment = UnityEngine.TextAnchor.MiddleCenter;
        GUI.Label(R(W - 120f, 8f, 32f, 24f), $"x{s.MenuScale:F1}", _mutedLabelStyle);
        _mutedLabelStyle.alignment = prevAlign;
        if (CBtn(W - 88f, 8f, 28f, 24f, "+"))
        { s.MenuScale = Mathf.Min(3.0f, s.MenuScale + 0.1f); s.Save(); }
        if (CBtn(W - 56f, 8f, 44f, 24f, I18n.T("关闭", "Close"), false, true)) Close();

        // 标签页
        float tabW = Mathf.Min(150f, (W - 32f - (Tabs.Length - 1) * 6f) / Tabs.Length);
        for (int i = 0; i < Tabs.Length; i++)
        {
            float tx = 16f + i * (tabW + 6f);
            if (CBtn(tx, 52f, tabW, 28f, Tabs[i])) _activeTab = i;
            if (i == _activeTab) GUI.Box(R(tx, 82f, tabW, 2f), "", _buttonActiveStyle);
        }

        // ScrollView viewport:顶部 title/tab 下到 底部状态栏上
        float viewH = H - 128f;
        Rect viewport = R(OUTER_PAD, 92f, W - OUTER_PAD * 2f, viewH);
        float measuredH = _activeTab == 0 ? _measuredMainH + 20f
            : _activeTab == 1 ? _measuredSpawnerH + 20f
            : _measuredTweaksH + 40f;
        bool needScroll = measuredH > viewH + 4f;
        float ch = needScroll ? measuredH : viewH - 2f;
        Rect content  = R(0f, 0f, W - OUTER_PAD * 2f - (needScroll ? 16f : 0f), ch);
        if (!needScroll) _mainScroll = Vector2.zero;
        _mainScroll = GUI.BeginScrollView(viewport, _mainScroll, content, false, needScroll);

        switch (_activeTab)
        {
            case 0: _measuredMainH = DrawMainTab(s); break;
            case 1: _measuredSpawnerH = DrawSpawnerTab(s); break;
            case 2: _measuredTweaksH = MenuTweaks.DrawCharacterAndQoLTab(RFunc, s); break;
            case 3: _measuredTweaksH = MenuTweaks.DrawGearAndWorldTab(RFunc, s); break;
            case 4: _measuredTweaksH = MenuTweaks.DrawLightAndCraftTab(RFunc, s); break;
            case 5: _measuredTweaksH = MenuTweaks.DrawGfxTab(RFunc, s); break;
        }

        GUI.EndScrollView();

        // 底部状态栏(在 scroll 区外,始终显示)
        // v6.6 GC: 仅在 PositionText/LastActionLog 变化时重建字符串
        {
            var pt = CheatState.PositionText;
            var la = CheatState.LastActionLog;
            if (pt != _cachedPosText || la != _cachedLastAction)
            {
                _cachedPosText   = pt;
                _cachedLastAction = la;
                _cachedStatusBar = $"Pos: {(string.IsNullOrEmpty(pt) ? "-" : pt)}   |   Last: {la}";
            }
        }
        GUI.Box(R(0f, H - 34f, W, 34f), "");
        GUI.Label(R(16f, H - 28f, W - 200f, 22f), _cachedStatusBar, _statusStyle);
        GUI.Label(R(W - 130f, H - 28f, 120f, 22f), I18n.T("↘ 拖拽缩放", "↘ Drag resize"), _mutedLabelStyle);

        // v2.7.91:边缘+角落拖拽调整宽高(宽边缘便于抓取)
        {
            const float edgeW = 20f;
            var e = Event.current;
            float winW = W * _scale, winH = H * _scale;

            if (e.type == EventType.MouseDown && !_resizing)
            {
                bool onRight = e.mousePosition.x >= winW - edgeW && e.mousePosition.y > 50f;
                bool onBottom = e.mousePosition.y >= winH - edgeW;
                if (onRight || onBottom)
                {
                    _resizing = true;
                    _resizeStartMouse = e.mousePosition;
                    var sm = Input.mousePosition;
                    _resizeStartScreenMouse = new Vector2(sm.x, Screen.height - sm.y);
                    _resizeStartW = W;
                    _resizeStartH = H;
                    _resizeEdge = (onRight && onBottom) ? ResizeEdge.Corner
                                : onRight ? ResizeEdge.Right : ResizeEdge.Bottom;
                    e.Use();
                }
            }
            // 视觉:右下角 resize 手柄 + 右边缘/底边缘高亮线
            if (!_resizing)
            {
                GUI.color = new Color(1f, 1f, 1f, 0.15f);
                GUI.DrawTexture(R(W - 3f, 50f, 3f, H - 84f), Texture2D.whiteTexture);
                GUI.DrawTexture(R(0f, H - 3f, W, 3f), Texture2D.whiteTexture);
            }
            else
            {
                GUI.color = new Color(0.4f, 0.7f, 1f, 0.6f);
                if (_resizeEdge == ResizeEdge.Right || _resizeEdge == ResizeEdge.Corner)
                    GUI.DrawTexture(R(W - 3f, 50f, 3f, H - 84f), Texture2D.whiteTexture);
                if (_resizeEdge == ResizeEdge.Bottom || _resizeEdge == ResizeEdge.Corner)
                    GUI.DrawTexture(R(0f, H - 3f, W, 3f), Texture2D.whiteTexture);
            }
            float gx = W - 18f, gy = H - 18f;
            GUI.color = _resizing ? new Color(0.4f, 0.7f, 1f, 0.9f) : new Color(1f, 1f, 1f, 0.4f);
            for (int i = 0; i < 3; i++)
            {
                float o = i * 6f;
                GUI.DrawTexture(R(gx + o, gy + 14f - o, 3f, 3f), Texture2D.whiteTexture);
                GUI.DrawTexture(R(gx + 3f + o, gy + 11f - o, 3f, 3f), Texture2D.whiteTexture);
            }
            GUI.color = Color.white;
        }

        // v2.7.94:自定义拖动启动(标题栏区域点击启动,实际拖动在 Draw() 用屏幕坐标驱动)
        {
            var e = Event.current;
            if (e.type == EventType.MouseDown && !_dragging && !_resizing)
            {
                float mx = e.mousePosition.x / _scale;
                float my = e.mousePosition.y / _scale;
                if (mx < W - 220f && my < 44f)
                {
                    _dragging = true;
                    var sm = Input.mousePosition;
                    _dragStartScreenMouse = new Vector2(sm.x, Screen.height - sm.y);
                    _dragStartWinX = _window.x;
                    _dragStartWinY = _window.y;
                    e.Use();
                }
            }
        }
    }

    // ————————————— Tab 1:主 cheat 面板 —————————————
    // v2.7.54 重设:所有 2-toggle 行统一 TOG_W=180 + TOG2_OFF=200
    //   整列 width 400;Section box 宽 BOX_W=390;toggle 宽度一致防错位
    //   地图/天气/时间 合并到一个 "世界时钟" section
    //   Settings 字段一律保留,兼容旧存档
    private static float DrawMainTab(TldHacksSettings s)
    {
        float colW = _cols >= 3 ? COL_W : _cols == 2 ? (W - OUTER_PAD * 2f - COL_GAP) / 2f : (W - OUTER_PAD * 2f);
        float c1 = 0f;
        float c2 = _cols >= 2 ? colW + COL_GAP : 0f;
        float c3 = _cols >= 3 ? (colW + COL_GAP) * 2f : _cols == 2 ? 0f : 0f;
        float y1 = 0f, y2 = 0f, y3 = 0f;

        // ═════════ 列 1:玩家状态 ═════════
        y1 = Section(c1, y1, I18n.T("生存", "Survival"));
        bool god   = GUI.Toggle(R(c1,            y1, TOG_W, ROW_H), s.GodMode,          I18n.T(" 无敌模式", " God Mode"));
        bool nfall = GUI.Toggle(R(c1 + TOG2_OFF, y1, TOG_W, ROW_H), s.NoFallDamage,     I18n.T(" 无坠落伤", " No Fall Dmg"));
        y1 += ROW_ADV;
        bool nspr  = GUI.Toggle(R(c1,            y1, TOG_W, ROW_H), s.NoSprainRisk,     I18n.T(" 免扭伤险", " No Sprain Risk"));
        bool immA  = GUI.Toggle(R(c1 + TOG2_OFF, y1, TOG_W, ROW_H), s.ImmuneAnimalDamage,I18n.T(" 免动物伤", " No Animal Dmg"));
        y1 += ROW_ADV;
        bool nsuf  = GUI.Toggle(R(c1,            y1, TOG_W, ROW_H), s.NoSuffocating,    I18n.T(" 不会窒息", " No Suffocate"));
        bool cdp   = GUI.Toggle(R(c1 + TOG2_OFF, y1, TOG_W, ROW_H), s.ClearDeathPenalty, I18n.T(" 免死亡罚", " Clr Death Pen."));
        y1 += ROW_ADV + SECTION_END_ADV;
        if (god != s.GodMode || nfall != s.NoFallDamage || nspr != s.NoSprainRisk
            || immA != s.ImmuneAnimalDamage || nsuf != s.NoSuffocating || cdp != s.ClearDeathPenalty)
        {
            s.GodMode = god; s.NoFallDamage = nfall; s.NoSprainRisk = nspr;
            s.ImmuneAnimalDamage = immA; s.NoSuffocating = nsuf; s.ClearDeathPenalty = cdp;
            // v3.0.4r4: GodMode 隐含 NoFallDamage + NoSprainRisk
            CheatState.GodMode = god;
            CheatState.NoFallDamage = nfall || god;
            CheatState.NoSprainRisk = nspr || god;
            CheatState.ImmuneAnimalDamage = immA; CheatState.NoSuffocating = nsuf; CheatState.ClearDeathPenalty = cdp;
            s.Save();
        }

        y1 = Section(c1, y1, I18n.T("温饱", "Food & Warmth"));
        bool warm  = GUI.Toggle(R(c1,            y1, TOG_W, ROW_H), s.AlwaysWarm, I18n.T(" 始终温暖", " Always Warm"));
        bool nhun  = GUI.Toggle(R(c1 + TOG2_OFF, y1, TOG_W, ROW_H), s.NoHunger,   I18n.T(" 无饥饿", " No Hunger"));
        y1 += ROW_ADV;
        bool nthir = GUI.Toggle(R(c1,            y1, TOG_W, ROW_H), s.NoThirst,   I18n.T(" 无口渴", " No Thirst"));
        bool nfat  = GUI.Toggle(R(c1 + TOG2_OFF, y1, TOG_W, ROW_H), s.NoFatigue,  I18n.T(" 无疲劳", " No Fatigue"));
        y1 += ROW_ADV;
        bool ista  = GUI.Toggle(R(c1, y1, TOG_W, ROW_H), s.InfiniteStamina, I18n.T(" 无限体力(含冲刺)", " Infinite Stamina"));
        y1 += ROW_ADV + SECTION_END_ADV;
        if (warm != s.AlwaysWarm || nhun != s.NoHunger || nthir != s.NoThirst || nfat != s.NoFatigue
            || ista != s.InfiniteStamina)
        {
            s.AlwaysWarm = warm; s.NoHunger = nhun; s.NoThirst = nthir; s.NoFatigue = nfat;
            s.InfiniteStamina = ista;
            CheatState.AlwaysWarm = warm;
            CheatState.NoHunger = nhun; CheatState.NoThirst = nthir; CheatState.NoFatigue = nfat;
            CheatState.InfiniteStamina = ista;
            s.Save();
        }

        // —— Buff / 增益(CT 复刻)——
        y1 = Section(c1, y1, I18n.T("增益 Buff(CT)", "Buffs (CT)"));
        bool nfrost = GUI.Toggle(R(c1,            y1, TOG_W, ROW_H), s.NoFrostbiteRisk, I18n.T(" 免疫冻伤", " No Frostbite"));
        bool wfbuf  = GUI.Toggle(R(c1 + TOG2_OFF, y1, TOG_W, ROW_H), s.WellFedBuff,     I18n.T(" 吃得饱饱", " Well Fed"));
        y1 += ROW_ADV;
        bool fzbuf  = GUI.Toggle(R(c1,            y1, TOG_W, ROW_H), s.FreezingBuff,    I18n.T(" 加热", " Warming Buff"));
        bool ftbuf  = GUI.Toggle(R(c1 + TOG2_OFF, y1, TOG_W, ROW_H), s.FatigueBuff,     I18n.T(" 疲劳减缓", " Less Fatigue"));
        y1 += ROW_ADV;
        bool cfb    = GUI.Toggle(R(c1,            y1, TOG_W, ROW_H), s.CureFrostbite,   I18n.T(" 治愈永久冻伤", " Cure Frostbite"));
        y1 += ROW_ADV;
        // 坏血病状态显示(只读信息)
        // v6.6 GC: 仅在 VitaminC/Status 变化时重建格式化字符串
        ScurvyViewer.Poll();
        if (ScurvyViewer.VitaminC != _cachedVitC || ScurvyViewer.Status != _cachedScurvyStatus)
        {
            _cachedVitC = ScurvyViewer.VitaminC;
            _cachedScurvyStatus = ScurvyViewer.Status;
            _cachedScurvyLabel = I18n.IsEnglish
                ? $"  Scurvy: {ScurvyViewer.Status}  VitC={ScurvyViewer.VitaminC:F1}/{ScurvyViewer.Threshold:F1}"
                : $"  坏血病: {ScurvyViewer.Status}  维C={ScurvyViewer.VitaminC:F1}/{ScurvyViewer.Threshold:F1}";
        }
        CLabel(c1, y1, TOG_WIDE, ROW_H, _cachedScurvyLabel, true);
        y1 += ROW_ADV + SECTION_END_ADV;
        if (nfrost != s.NoFrostbiteRisk || wfbuf != s.WellFedBuff
            || fzbuf != s.FreezingBuff || ftbuf != s.FatigueBuff || cfb != s.CureFrostbite)
        {
            if (s.WellFedBuff && !wfbuf) Cheats.ClearWellFedBuff();
            if (s.FatigueBuff && !ftbuf) Cheats.ClearFatigueBuff();
            s.NoFrostbiteRisk = nfrost; s.WellFedBuff = wfbuf;
            s.FreezingBuff = fzbuf; s.FatigueBuff = ftbuf; s.CureFrostbite = cfb;
            CheatState.NoFrostbiteRisk = nfrost; CheatState.WellFedBuff = wfbuf;
            CheatState.FreezingBuff = fzbuf; CheatState.FatigueBuff = ftbuf;
            CheatState.CureFrostbite = cfb;
            s.Save();
        }

        y1 = Section(c1, y1, I18n.T("全局速度", "Global Speed"));
        float bx1 = c1;
        for (int spi = 0; spi < SpeedPresets.Length; spi++)
        {
            float sp = SpeedPresets[spi];
            bool active = Mathf.Abs(s.SpeedMultiplier - sp) < 0.01f;
            if (CBtn(bx1, y1, 92f, ROW_H, SpeedPresetLabels[spi], active))
            { s.SpeedMultiplier = sp; CheatState.SpeedMultiplier = sp; s.Save(); }
            bx1 += 96f;
        }
        y1 += ROW_ADV + SECTION_END_ADV;

        y1 = Section(c1, y1, I18n.T("技能", "Skills"));
        // v2.7.72 默认折叠 —— v2.7.82 "全部满级" + "展开" 同行
        if (GUI.Button(R(c1,               y1, 120f, ROW_H), I18n.T("全部满级", "All Max")))
            Skills.SetAllMax();
        if (GUI.Button(R(c1 + 128f,        y1, 92f, ROW_H), _skillsExpanded ? I18n.T("收起全部", "Hide All") : I18n.T("展开全部", "Show All")))
            _skillsExpanded = !_skillsExpanded;
        y1 += ROW_ADV;
        if (_skillsExpanded)
        {
            for (int i = 0; i < Skills.All.Length; i++)
            {
                var (lbl, t) = Skills.All[i];
                GUI.Label (R(c1 + 8f,       y1, 120f, ROW_H), lbl);
                GUI.Label (R(c1 + 138f,     y1, 40f,  ROW_H), Skills.GetTier(t).ToString());
                if (GUI.Button(R(c1 + 186f, y1, 44f,  ROW_H), I18n.T("满", "Max"))) Skills.SetMax(t);
                y1 += ROW_ADV;
            }
        }
        y1 += SECTION_END_ADV;

        // —— 物品 & 装备(v4.0r3 从 col 3 移来) ——
        y1 = Section(c1, y1, I18n.T("物品 & 装备", "Items & Gear"));
        bool stk   = GUI.Toggle(R(c1,            y1, TOG_W, ROW_H), s.StackingEnabled,    I18n.T(" UI 堆叠", " UI Stacking"));
        bool dur   = GUI.Toggle(R(c1 + TOG2_OFF, y1, TOG_W, ROW_H), s.InfiniteDurability, I18n.T(" 无限耐久", " No Degrade"));
        y1 += ROW_ADV;
        bool wet   = GUI.Toggle(R(c1,            y1, TOG_W, ROW_H), s.NoWetClothes,       I18n.T(" 衣物防潮", " Dry Clothes"));
        bool lamp  = GUI.Toggle(R(c1 + TOG2_OFF, y1, TOG_W, ROW_H), s.LampFuelNoDrain,    I18n.T(" 油灯不耗", " Lamp No Drain"));
        y1 += ROW_ADV;
        bool flsk1 = GUI.Toggle(R(c1,            y1, TOG_W, ROW_H), s.FlaskNoHeatLoss,    I18n.T(" 保温杯不降温", " Flask Keep Hot"));
        bool flsk2 = GUI.Toggle(R(c1 + TOG2_OFF, y1, TOG_W, ROW_H), s.FlaskInfiniteVol,   I18n.T(" 保温杯不耗量", " Flask No Drain"));
        y1 += ROW_ADV;
        bool flsk3 = GUI.Toggle(R(c1,            y1, TOG_W, ROW_H), s.FlaskAnyItem,            I18n.T(" 保温杯装任意液体", " Flask Any Liquid"));
        bool bnop  = GUI.Toggle(R(c1 + TOG2_OFF, y1, TOG_W, ROW_H), s.BlockAutoPickupOwnDrops, I18n.T(" 不捡丢物", " Skip Own Drops"));
        y1 += ROW_ADV + SECTION_END_ADV;
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


        // ═════════ 列 2:世界 & 环境 & 一次性 ═════════
        if (_cols < 2) { c2 = c1; y2 = y1; }
        y2 = Section(c2, y2, I18n.T("动物", "Animals"));
        bool kill    = GUI.Toggle(R(c2,            y2, TOG_W, ROW_H), s.InstantKillAnimals, I18n.T(" 一击必杀", " One Hit Kill"));
        bool frz     = GUI.Toggle(R(c2 + TOG2_OFF, y2, TOG_W, ROW_H), s.FreezeAnimals,      I18n.T(" 冻结动物", " Freeze Animals"));
        y2 += ROW_ADV;
        bool stealth = GUI.Toggle(R(c2,            y2, TOG_W, ROW_H), s.Stealth,            I18n.T(" 动物逃离", " Animals Flee"));
        bool tinv    = GUI.Toggle(R(c2 + TOG2_OFF, y2, TOG_W, ROW_H), s.TrueInvisible,      I18n.T(" 完全隐形", " Full Invisible"));
        y2 += ROW_ADV + SECTION_END_ADV;
        if (kill != s.InstantKillAnimals || frz != s.FreezeAnimals || stealth != s.Stealth || tinv != s.TrueInvisible)
        {
            s.InstantKillAnimals = kill; s.FreezeAnimals = frz; s.Stealth = stealth; s.TrueInvisible = tinv;
            CheatState.InstantKillAnimals = kill; CheatState.FreezeAnimals = frz;
            CheatState.Stealth = stealth; CheatState.TrueInvisible = tinv;
            s.Save();
        }

        string[] spawnCmds = { "spawn_wolf", "spawn_bear", "spawn_cougar", "spawn_moose", "spawn_doe", "spawn_stag", "spawn_rabbit", "spawn_ptarmigan" };
        string[] spawnLbls = I18n.IsEnglish
            ? new[] { "Wolf", "Bear", "Cougar", "Moose", "Doe", "Buck", "Rabbit", "Ptarmigan" }
            : new[] { "狼", "熊", "美洲狮", "驼鹿", "母鹿", "雄鹿", "兔", "松鸡" };
        float spW = (TOG_WIDE - 12f) / 4f;
        for (int i = 0; i < spawnCmds.Length; i++)
        {
            int col = i % 4, row = i / 4;
            if (GUI.Button(R(c2 + col * (spW + 4f), y2 + row * ROW_ADV, spW, ROW_H), spawnLbls[i]))
                ConsoleBridge.Run(spawnCmds[i]);
        }
        y2 += ROW_ADV * 2 + SECTION_END_ADV;

        y2 = Section(c2, y2, I18n.T("环境 / 篝火", "Environment / Fire"));
        bool ice  = GUI.Toggle(R(c2,            y2, TOG_W, ROW_H), s.ThinIceNoBreak, I18n.T(" 冰面不破", " Thin Ice Safe"));
        bool ftmp = GUI.Toggle(R(c2 + TOG2_OFF, y2, TOG_W, ROW_H), s.FireTemp300,    I18n.T(" 篝火 300℃", " Fire 300°C"));
        y2 += ROW_ADV;
        bool fnev = GUI.Toggle(R(c2,            y2, TOG_W, ROW_H), s.FireNeverDie,   I18n.T(" 篝火永不熄灭", " Fire Never Dies"));
        bool fany = GUI.Toggle(R(c2 + TOG2_OFF, y2, TOG_W, ROW_H), s.FireAnywhere,   I18n.T(" 随意生火", " Fire Anywhere"));
        y2 += ROW_ADV;
        bool ffue = GUI.Toggle(R(c2,            y2, TOG_W, ROW_H), s.FreeFireFuel,   I18n.T(" 材料不减", " Free Fuel"));
        bool ftrch= GUI.Toggle(R(c2 + TOG2_OFF, y2, TOG_W, ROW_H), s.TorchFullValue, I18n.T(" 火把满值", " Torch Full"));
        y2 += ROW_ADV;
        bool qf   = GUI.Toggle(R(c2,            y2, TOG_W, ROW_H), s.QuickFire,      I18n.T(" 生火必成功", " 100% Fire"));
        bool qfish= GUI.Toggle(R(c2 + TOG2_OFF, y2, TOG_W, ROW_H), s.QuickFishing,   I18n.T(" 钓鱼必成功", " 100% Fish"));
        y2 += ROW_ADV;
        GUI.Label(R(c2, y2, 130f, ROW_H), I18n.T("燃烧上限(h):", "Max Burn(h):"));
        float fmbh = GUI.HorizontalSlider(R(c2 + 130f, y2 + 8f, 200f, 12f), s.FireMaxBurnHours, 12f, 1000f);
        GUI.Label(R(c2 + 338f, y2, 60f, ROW_H), fmbh.ToString("F0"));
        if (fmbh != s.FireMaxBurnHours) { s.FireMaxBurnHours = fmbh; CheatState.FireMaxBurnHours = fmbh; s.Save(); }
        y2 += ROW_ADV + SECTION_END_ADV;
        if (ice != s.ThinIceNoBreak || ftmp != s.FireTemp300 || fnev != s.FireNeverDie
            || fany != s.FireAnywhere || ffue != s.FreeFireFuel || ftrch != s.TorchFullValue
            || qf != s.QuickFire || qfish != s.QuickFishing)
        {
            s.ThinIceNoBreak = ice; s.FireTemp300 = ftmp; s.FireNeverDie = fnev;
            s.FireAnywhere = fany; s.FreeFireFuel = ffue; s.TorchFullValue = ftrch;
            s.QuickFire = qf; s.QuickFishing = qfish;
            CheatState.ThinIceNoBreak = ice; CheatState.FireTemp300 = ftmp; CheatState.FireNeverDie = fnev;
            CheatState.FireAnywhere = fany; CheatState.FreeFireFuel = ffue; CheatState.TorchFullValue = ftrch;
            CheatState.QuickFire = qf; CheatState.QuickFishing = qfish;
            s.Save();
        }

        // 世界时钟 — 地图 + 天气(3行) + 时间(1行)合并一块
        y2 = Section(c2, y2, I18n.T("世界时钟 — 地图 / 天气 / 时间", "World Clock — Map / Weather / Time"));
        if (GUI.Button(R(c2, y2, TOG_W, ROW_H), I18n.T("全开地图", "Reveal Map"))) Cheats.RevealFullMap();
        bool mtp = GUI.Toggle(R(c2 + TOG2_OFF, y2, TOG_W, ROW_H), s.MapClickTP, I18n.T(" 地图双击传送", " Map Click TP"));
        if (mtp != s.MapClickTP) { s.MapClickTP = mtp; CheatState.MapClickTP = mtp; }
        y2 += ROW_ADV + 2f;
        // 天气 — 每行 5 个 + 最后一行剩余
        float bx2 = c2;
        for (int i = 0; i < WeatherStages.Length; i++)
        {
            if (GUI.Button(R(bx2, y2, 72f, ROW_H), WeatherLabels[i])) Cheats.SetWeatherStage(WeatherStages[i]);
            bx2 += 76f;
            if ((i + 1) % 5 == 0 && i < WeatherStages.Length - 1) { bx2 = c2; y2 += ROW_ADV; }
        }
        bool wLock = GUI.Toggle(R(bx2, y2, 72f, ROW_H), CheatState.WeatherLocked,
            I18n.T(" 锁定", " Lock"), MenuTweaks.ToggleStyle ?? GUI.skin.toggle);
        if (wLock != CheatState.WeatherLocked) CheatState.WeatherLocked = wLock;
        y2 += ROW_ADV + 2f;
        // 时间 — 4 个小时 preset
        bx2 = c2;
        for (int i = 0; i < HourPresets.Length; i++)
        {
            if (GUI.Button(R(bx2, y2, 92f, ROW_H), HourLabels[i])) Cheats.SetTimeOfDay(HourPresets[i]);
            bx2 += 96f;
        }
        y2 += ROW_ADV + SECTION_END_ADV;

        // 商人 & 美洲狮 — toggle + 控制台命令合并
        y2 = Section(c2, y2, I18n.T("商人 & 美洲狮", "Trader & Cougar"));
        bool tul = GUI.Toggle(R(c2,            y2, TOG_W, ROW_H), s.TraderUnlimitedList,   I18n.T(" 交易扩充", " More Trades"));
        bool tmt = GUI.Toggle(R(c2 + TOG2_OFF, y2, TOG_W, ROW_H), s.TraderMaxTrust,        I18n.T(" 信任满值", " Max Trust"));
        y2 += ROW_ADV;
        bool tix = GUI.Toggle(R(c2,            y2, TOG_W, ROW_H), s.TraderInstantExchange, I18n.T(" 即时交易", " Instant Trade"));
        bool taa = GUI.Toggle(R(c2 + TOG2_OFF, y2, TOG_W, ROW_H), s.TraderAlwaysAvailable, I18n.T(" 商人常驻", " Always Online"));
        y2 += ROW_ADV;
        bool cia = GUI.Toggle(R(c2,            y2, TOG_W, ROW_H), s.CougarInstantActivate, I18n.T(" 召唤美洲狮", " Spawn Cougar"));
        bool stt = GUI.Toggle(R(c2 + TOG2_OFF, y2, TOG_W, ROW_H), s.TT_ShowTraderTrust,   I18n.T(" 显示信任度", " Show Trust"));
        y2 += ROW_ADV;
        GUI.Label(R(c2, y2, 100f, ROW_H), I18n.T("到来(天)", "Arrival"), MenuTweaks.LabelStyle ?? GUI.skin.label);
        float td = GUI.HorizontalSlider(R(c2 + 100f, y2 + 8f, 200f, 12f), s.TraderArrivalDays, 0f, 35f);
        GUI.Label(R(c2 + 310f, y2, 50f, ROW_H), ((int)td).ToString(), MenuTweaks.MutedLabel ?? GUI.skin.label);
        if ((int)td != (int)s.TraderArrivalDays) { s.TraderArrivalDays = (int)td; s.Save(); }
        y2 += ROW_ADV;
        GUI.Label(R(c2, y2, 100f, ROW_H), I18n.T("信任值", "Trust"), MenuTweaks.LabelStyle ?? GUI.skin.label);
        int curTrust = 0;
        try { var tm2 = Il2Cpp.GameManager.GetTraderManager(); if (tm2?.m_CurrentState != null) curTrust = tm2.m_CurrentState.m_CurrentTrust; } catch { }
        float trustSlider = GUI.HorizontalSlider(R(c2 + 100f, y2 + 8f, 200f, 12f), curTrust, 0f, 500f);
        GUI.Label(R(c2 + 310f, y2, 50f, ROW_H), ((int)trustSlider).ToString(), MenuTweaks.MutedLabel ?? GUI.skin.label);
        if ((int)trustSlider != curTrust)
        {
            try { var tm2 = Il2Cpp.GameManager.GetTraderManager(); if (tm2?.m_CurrentState != null) { tm2.m_CurrentState.m_CurrentTrust = (int)trustSlider; tm2.m_CurrentState.m_HighestTrust = Math.Max(tm2.m_CurrentState.m_HighestTrust, (int)trustSlider); } } catch { }
        }
        y2 += ROW_ADV + SECTION_END_ADV;
        if (tul != s.TraderUnlimitedList || tmt != s.TraderMaxTrust
            || tix != s.TraderInstantExchange || taa != s.TraderAlwaysAvailable
            || cia != s.CougarInstantActivate || stt != s.TT_ShowTraderTrust)
        {
            s.TraderUnlimitedList = tul; s.TraderMaxTrust = tmt;
            s.TraderInstantExchange = tix; s.TraderAlwaysAvailable = taa;
            s.CougarInstantActivate = cia; s.TT_ShowTraderTrust = stt;
            CheatState.TraderUnlimitedList = tul; CheatState.TraderMaxTrust = tmt;
            CheatState.TraderInstantExchange = tix; CheatState.TraderAlwaysAvailable = taa;
            CheatState.CougarInstantActivate = cia; CheatState.TT_ShowTraderTrust = stt;
            s.Save();
        }

        // ═════════ 列 3:物品 & 武器 ═════════
        if (_cols < 3) { c3 = _cols == 2 ? c1 : c1; y3 = _cols == 2 ? y1 : y2; }
        y3 = Section(c3, y3, I18n.T("快速操作(CT 复刻)", "Quick Actions (CT)"));
        bool qcook = GUI.Toggle(R(c3,            y3, TOG_W, ROW_H), s.QuickCook,      I18n.T(" 秒烹饪", " Instant Cook"));
        bool nbrn  = GUI.Toggle(R(c3 + TOG2_OFF, y3, TOG_W, ROW_H), s.NoBurn,         I18n.T(" 防烤焦", " No Burn"));
        y3 += ROW_ADV;
        bool qsrch = GUI.Toggle(R(c3,            y3, TOG_W, ROW_H), s.QuickSearch,    I18n.T(" 秒搜刮", " Instant Loot"));
        bool qhv   = GUI.Toggle(R(c3 + TOG2_OFF, y3, TOG_W, ROW_H), s.QuickHarvest,   I18n.T(" 秒割肉", " Instant Harvest"));
        y3 += ROW_ADV;
        bool qbd   = GUI.Toggle(R(c3,            y3, TOG_W, ROW_H), s.QuickBreakDown, I18n.T(" 秒拆解", " Quick Break"));
        bool qev   = GUI.Toggle(R(c3 + TOG2_OFF, y3, TOG_W, ROW_H), s.QuickEvolve,    I18n.T(" 秒风干", " Quick Cure"));
        y3 += ROW_ADV;
        bool qcl   = GUI.Toggle(R(c3,            y3, TOG_W, ROW_H), s.QuickClimb,     I18n.T(" 秒爬绳", " Fast Climb"));
        bool qa    = GUI.Toggle(R(c3 + TOG2_OFF, y3, TOG_W, ROW_H), s.QuickAction,    I18n.T(" 秒采集", " Quick Action"));
        y3 += ROW_ADV + SECTION_END_ADV;
        if (qcook != s.QuickCook || nbrn != s.NoBurn || qsrch != s.QuickSearch || qhv != s.QuickHarvest
            || qbd != s.QuickBreakDown || qev != s.QuickEvolve || qcl != s.QuickClimb
            || qa != s.QuickAction)
        {
            s.QuickCook = qcook; s.NoBurn = nbrn; s.QuickSearch = qsrch; s.QuickHarvest = qhv;
            s.QuickBreakDown = qbd; s.QuickEvolve = qev; s.QuickClimb = qcl;
            s.QuickAction = qa;
            CheatState.QuickCook = qcook; CheatState.NoBurn = nbrn;
            CheatState.QuickSearch = qsrch; CheatState.QuickHarvest = qhv;
            CheatState.QuickBreakDown = qbd; CheatState.QuickEvolve = qev;
            CheatState.QuickClimb = qcl; CheatState.QuickAction = qa;
            s.Save();
        }

        // —— 制作 & 修理(操作类,与快速操作语义同源) ——
        y3 = Section(c3, y3, I18n.T("制作 & 修理", "Crafting & Repair"));
        bool fc = GUI.Toggle(R(c3,            y3, TOG_W, ROW_H), s.FreeCraft,  I18n.T(" 免费制作", " Free Craft"));
        bool qk = GUI.Toggle(R(c3 + TOG2_OFF, y3, TOG_W, ROW_H), s.QuickCraft, I18n.T(" 快速制作", " Quick Craft"));
        y3 += ROW_ADV;
        bool fr = GUI.Toggle(R(c3,            y3, TOG_W, ROW_H), s.FreeRepair, I18n.T(" 免费修理", " Free Repair"));
        y3 += ROW_ADV + SECTION_END_ADV;
        if (fc != s.FreeCraft || qk != s.QuickCraft || fr != s.FreeRepair)
        {
            s.FreeCraft = fc; s.QuickCraft = qk; s.FreeRepair = fr;
            CheatState.FreeCraft = fc; CheatState.QuickCraft = qk; CheatState.FreeRepair = fr;
            s.Save();
        }

        // —— 锁 & 容器 ——
        y3 = Section(c3, y3, I18n.T("锁 & 容器", "Locks & Containers"));
        bool lok  = GUI.Toggle(R(c3,            y3, TOG_W, ROW_H), s.IgnoreLock,         I18n.T(" 忽略上锁", " Ignore Locks"));
        bool qc   = GUI.Toggle(R(c3 + TOG2_OFF, y3, TOG_W, ROW_H), s.QuickOpenContainer, I18n.T(" 秒开容器", " Quick Open"));
        y3 += ROW_ADV;
        bool usaf = GUI.Toggle(R(c3,            y3, TOG_W, ROW_H), s.UnlockSafes,        I18n.T(" 解锁保箱", " Unlock Safes"));
        bool tbag = GUI.Toggle(R(c3 + TOG2_OFF, y3, TOG_W, ROW_H), s.TechBackpack,       I18n.T(" 背包负重", " Carry Cap"));
        y3 += ROW_ADV;
        float tbagKg = s.TechBackpackKg;
        if (tbag)
        {
            GUI.Label(R(c3, y3, 100f, ROW_H), I18n.T("  上限kg:", "  Max kg:"));
            tbagKg = GUI.HorizontalSlider(R(c3 + 100f, y3 + 8f, 200f, 12f), s.TechBackpackKg, 10f, 10000f);
            GUI.Label(R(c3 + 308f, y3, 60f, ROW_H), tbagKg.ToString("F0"));
            y3 += ROW_ADV;
        }
        bool infCon = GUI.Toggle(R(c3, y3, TOG_W, ROW_H), s.InfiniteContainer, I18n.T(" 容器无限", " Inf. Container"));
        y3 += ROW_ADV + SECTION_END_ADV;
        if (lok != s.IgnoreLock || qc != s.QuickOpenContainer || usaf != s.UnlockSafes
            || tbag != s.TechBackpack || infCon != s.InfiniteContainer || tbagKg != s.TechBackpackKg)
        {
            bool wasOn = s.TechBackpack;
            s.IgnoreLock = lok; s.QuickOpenContainer = qc; s.UnlockSafes = usaf;
            s.TechBackpack = tbag; s.InfiniteContainer = infCon; s.TechBackpackKg = tbagKg;
            CheatState.IgnoreLock = lok; CheatState.QuickOpenContainer = qc; CheatState.UnlockSafes = usaf;
            CheatState.TechBackpack = tbag; CheatState.InfiniteContainer = infCon; CheatState.TechBackpackKg = tbagKg;
            if (wasOn && !tbag) EncumberVanilla.Reset();
            s.Save();
        }

        // v2.7.91 "武器 & 瞄准"
        y3 = Section(c3, y3, I18n.T("武器 & 瞄准", "Weapons & Aiming"));
        bool amm  = GUI.Toggle(R(c3,            y3, TOG_W, ROW_H), s.InfiniteAmmo,   I18n.T(" 无限弹药", " Infinite Ammo"));
        bool jam  = GUI.Toggle(R(c3 + TOG2_OFF, y3, TOG_W, ROW_H), s.NoJam,          I18n.T(" 永不卡壳", " No Jam"));
        y3 += ROW_ADV;
        bool rec  = GUI.Toggle(R(c3,            y3, TOG_W, ROW_H), s.NoRecoil,       I18n.T(" 无后坐力", " No Recoil"));
        bool nstam= GUI.Toggle(R(c3 + TOG2_OFF, y3, TOG_W, ROW_H), s.NoAimStamina,   I18n.T(" 瞄准不耗体力", " No Aim Stamina"));
        y3 += ROW_ADV;
        bool sway = GUI.Toggle(R(c3,            y3, TOG_W, ROW_H), s.NoAimSway,      I18n.T(" 稳定瞄准", " Steady Aim"));
        y3 += ROW_ADV + SECTION_END_ADV;
        if (amm != s.InfiniteAmmo || jam != s.NoJam || rec != s.NoRecoil
            || sway != s.NoAimSway || nstam != s.NoAimStamina)
        {
            s.InfiniteAmmo = amm; s.NoJam = jam; s.NoRecoil = rec;
            s.NoAimSway = sway; s.NoAimStamina = nstam;
            CheatState.InfiniteAmmo = amm; CheatState.NoJam = jam; CheatState.NoRecoil = rec;
            CheatState.NoAimSway = sway; CheatState.NoAimStamina = nstam;
            s.Save();
        }

        // —— 一次性操作 ——
        y3 = Section(c3, y3, I18n.T("一次性操作", "One-shot Actions"));
        if (GUI.Button(R(c3,             y3, TOG_W, ROW_H), I18n.T("清除所有负面", "Clear Afflictions"))) Cheats.ClearAllAfflictions();
        if (GUI.Button(R(c3 + TOG2_OFF,  y3, TOG_W, ROW_H), I18n.T("一键解锁勋章", "Unlock Feats")))       Feats.UnlockAllFeats();
        y3 += ROW_ADV;
        if (GUI.Button(R(c3,             y3, TOG_W, ROW_H), I18n.T("恢复全部耐久", "Restore Durability"))) Cheats.RestoreAllSceneGear();
        if (GUI.Button(R(c3 + TOG2_OFF,  y3, TOG_W, ROW_H), I18n.T("修复背包", "Repair Inventory")))       QuickActions.RepairAllInventory();
        y3 += ROW_ADV;
        if (GUI.Button(R(c3,             y3, TOG_W, ROW_H), I18n.T("修复手持", "Repair Held")))            Cheats.RepairItemInHands();
        if (GUI.Button(R(c3 + TOG2_OFF,  y3, TOG_W, ROW_H), I18n.T("★ 全关并同步", "★ Disable All")))     Cheats.DisableAllCheats();
        y3 += ROW_ADV + SECTION_END_ADV;

        return Mathf.Max(y1, Mathf.Max(y2, y3));
    }

    // ————————————— Tab 2:物品刷出 + 传送 —————————————
    // v2.7.28 重排:15 个传送目的地改 5 列 × 3 行;Item Spawner 2 列 × 3 行(6/页)→ 3 列 × 6 行(18/页)
    private static float DrawSpawnerTab(TldHacksSettings s)
    {
        float y = 6f;

        // ========== 快速传送(5 列 × 3 行,紧凑) ==========
        GUI.Label(R(10f, y, 400f, ROW_H), I18n.IsEnglish
            ? $"—— Quick Teleport ({Teleport.Destinations.Count} targets) ——"
            : $"—— 快速传送({Teleport.Destinations.Count} 个目的地) ——");
        y += ROW_ADV;
        int teleCols = W >= 1000f ? 5 : W >= 700f ? 3 : 2;
        float teleW = (W - 30f) / teleCols;
        for (int i = 0; i < Teleport.Destinations.Count; i++)
        {
            var d = Teleport.Destinations[i];
            float tx = 10f + (i % teleCols) * teleW;
            float ty = y + (i / teleCols) * ROW_ADV;
            if (GUI.Button(R(tx, ty, teleW - 4f, ROW_H), $"→ {d.DisplayLabel}"))
                Teleport.TravelTo(d);
        }
        y += ROW_ADV * ((Teleport.Destinations.Count + teleCols - 1) / teleCols) + SECTION_END_ADV;

        // 位置控制行 —— v2.7.55 "打印位置到 log" 按钮:在 Latest.log 留 [POS-MARK],mod 作者可回读坐标
        if (GUI.Button(R(10f, y, 160f, ROW_H), "uConsole pos"))    ConsoleBridge.Run("pos");
        if (GUI.Button(R(180f, y, 160f, ROW_H), I18n.T("刷新位置显示", "Refresh Pos")))   Cheats.UpdatePlayerPosition();
        if (GUI.Button(R(350f, y, 200f, ROW_H), I18n.T("★ 打印坐标到 log", "★ Print Pos to Log"))) Cheats.PrintPositionToLog();
        GUI.Label(R(560f, y, W - 570f, ROW_H),
            I18n.IsEnglish
                ? $"Current: {(string.IsNullOrEmpty(CheatState.PositionText) ? "(none)" : CheatState.PositionText)}"
                : $"当前:{(string.IsNullOrEmpty(CheatState.PositionText) ? "(未获取)" : CheatState.PositionText)}");
        y += ROW_ADV + SECTION_END_ADV;

        GUI.Box(R(10f, y, W - 20f, 1f), "");
        y += 8f;

        // ========== 物品刷出 ==========
        GUI.Label(R(10f, y, 400f, ROW_H), I18n.IsEnglish
            ? $"Item Spawner ({ItemDatabase.All.Count + ItemDatabaseMod.All.Count} items)"
            : $"Item Spawner ({ItemDatabase.All.Count + ItemDatabaseMod.All.Count} 条)");
        y += ROW_ADV;

        // 类别 tabs (全 9 个平铺)
        float catW = (W - 20f) / ItemDatabase.Categories.Length;
        for (int i = 0; i < ItemDatabase.Categories.Length; i++)
        {
            string lbl = ItemDatabase.DisplayCategory(i);
            if (CBtn(10f + i * catW, y, catW - 2f, ROW_H, lbl, i == _selectedCategory))
            {
                if (_selectedCategory != i) { _selectedCategory = i; _page = 0; }
            }
        }
        y += ROW_ADV + SECTION_END_ADV;

        // 搜索栏已移除,直接按类别浏览

        // 数量预设 (v6.6: 预缓存标签,零运行时 alloc)
        GUI.Label(R(10f, y, 60f, ROW_H), I18n.T("数量:", "Qty:"));
        float bx = 70f;
        for (int i = 0; i < QuantityPresets.Length; i++)
        {
            if (CBtn(bx, y, 70f, ROW_H, QuantityPresetLabels[i], _quantity == QuantityPresets[i])) _quantity = QuantityPresets[i];
            bx += 75f;
        }
        // 数量标签:仅在 _quantity 变化时重建
        if (_quantity != _cachedQtyValue)
        {
            _cachedQtyValue    = _quantity;
            _cachedQtyLabel    = $"Now: {_quantity}";
            _cachedQtyLabelZh  = $"当前: {_quantity}";
            _cachedSpawnBtnLabel = $"+×{_quantity}";
        }
        GUI.Label(R(bx + 10f, y, 140f, ROW_H), I18n.IsEnglish ? _cachedQtyLabel : _cachedQtyLabelZh);
        y += ROW_ADV + SECTION_END_ADV;

        if (_lastCat != _selectedCategory) { RebuildFilter(); _lastCat = _selectedCategory; }
        else if (_filtered.Count == 0 && (ItemDatabase.All.Count + ItemDatabaseMod.All.Count) > 0) { RebuildFilter(); }

        const int cols = 4;
        float availH = (H - 130f) - y - ROW_ADV * 2f - SECTION_END_ADV - 10f;
        _pageRows = Mathf.Max(4, (int)(availH / ROW_ADV));
        _pageSize = _pageRows * cols;

        int totalPages = Mathf.Max(1, (_filtered.Count + _pageSize - 1) / _pageSize);
        _page = Mathf.Clamp(_page, 0, totalPages - 1);
        int start = _page * _pageSize;
        int end = Mathf.Min(start + _pageSize, _filtered.Count);

        float colW = (W - 40f) / cols;
        float btnW = 56f;
        var spawnBtnLabel = _cachedSpawnBtnLabel; // 局部引用,避免每次 static 读
        var rowStyle = _rowLabelStyle ?? GUI.skin.label;
        for (int idx = start; idx < end; idx++)
        {
            int i = idx - start;
            int col = i % cols, row = i / cols;
            float x = 10f + col * colW;
            float yy = y + row * ROW_ADV;
            var e = _filtered[idx];
            if (GUI.Button(R(x, yy, btnW, ROW_H), spawnBtnLabel))
                Cheats.SpawnItem(e.PrefabName, _quantity);
            // v6.6 GC: 利用 ItemEntry 上预缓存的显示名+卡路里标签(见 RebuildFilter)
            GUI.Label(R(x + btnW + 6f, yy, colW - btnW - 14f, ROW_H),
                I18n.IsEnglish ? (e.DisplayLabelEn ?? e.NameEn ?? e.Name) : (e.DisplayLabelZh ?? e.NameZh ?? e.Name),
                rowStyle);
        }
        y += ROW_ADV * _pageRows + SECTION_END_ADV;

        // 分页控制(右对齐,跟随窗口宽度) — v6.6: 仅在页/总页/总数变化时重建
        if (_page != _cachedPageNum || totalPages != _cachedTotalPages || _filtered.Count != _cachedFilteredCount)
        {
            _cachedPageNum      = _page;
            _cachedTotalPages   = totalPages;
            _cachedFilteredCount = _filtered.Count;
            _cachedPageInfo = I18n.IsEnglish
                ? $"Page {_page + 1}/{totalPages}   Total {_filtered.Count}   /page {_pageSize}"
                : $"页 {_page + 1}/{totalPages}   共 {_filtered.Count} 个   每页 {_pageSize}";
        }
        float pw = W - 20f;
        if (GUI.Button(R(10f, y, 100f, ROW_H), I18n.T("◀ 上一页", "◀ Prev"))) { if (_page > 0) { _page--; _cachedPageNum = -1; } }
        GUI.Label(R(120f, y, 400f, ROW_H), _cachedPageInfo);
        if (GUI.Button(R(pw - 280f, y, 100f, ROW_H), I18n.T("下一页 ▶", "Next ▶"))) { if (_page < totalPages - 1) { _page++; _cachedPageNum = -1; } }
        if (GUI.Button(R(pw - 170f, y, 80f, ROW_H), I18n.T("⏮ 首页", "⏮ First")))   { _page = 0; _cachedPageNum = -1; }
        if (GUI.Button(R(pw - 80f, y, 80f, ROW_H), I18n.T("末页 ⏭", "Last ⏭")))    { _page = totalPages - 1; _cachedPageNum = -1; }
        y += ROW_ADV;
        return y;
    }

    private static float Section(float x, float y, string title)
    {
        GUI.Box(R(x, y, BOX_W, SEC_H), "");
        GUI.Box(R(x, y, 3f, SEC_H), "", _buttonActiveStyle);
        GUI.Label(R(x + 10f, y + 4f, BOX_W - 20f, SEC_H - 8f), title, _sectionTitleStyle);
        return y + SEC_ADV;
    }

    private static bool HasChineseChar(string s)
    {
        if (string.IsNullOrEmpty(s)) return false;
        foreach (char c in s) if (c >= 0x4E00 && c <= 0x9FFF) return true;
        return false;
    }

    private static string PrefabToEn(string prefab)
    {
        if (string.IsNullOrEmpty(prefab)) return prefab;
        string s = prefab.StartsWith("GEAR_") ? prefab.Substring(5) : prefab;
        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < s.Length; i++)
        {
            char c = s[i];
            if (c == '_') { sb.Append(' '); continue; }
            if (i > 0 && char.IsUpper(c) && !char.IsUpper(s[i - 1]) && s[i - 1] != ' ') sb.Append(' ');
            sb.Append(c);
        }
        return sb.ToString();
    }

    private static void ResolveDisplayNames()
    {
        if (_namesResolved) return;
        _namesResolved = true;
        try
        {
            void Resolve(System.Collections.Generic.List<ItemEntry> list, bool isMod)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    var e = list[i];
                    e.IsMod = isMod;
                    e.NameZh = e.Name;                        // 硬编码默认中文
                    e.NameEn = PrefabToEn(e.PrefabName);      // 驼峰拆分作英文兜底
                    try
                    {
                        if (e.PrefabName == "GEAR_RandomCerealBox") { e.IsValid = false; continue; }
                        var gi = GearItem.LoadGearItemPrefab(e.PrefabName);
                        if (gi == null) { e.IsValid = false; continue; }
                        string dn = gi.GetDisplayNameWithoutConditionForInventoryInterfaces();
                        if (!string.IsNullOrEmpty(dn) && dn != e.PrefabName)
                        {
                            if (HasChineseChar(dn)) e.NameZh = dn;
                            else e.NameEn = dn;
                        }
                        if (isMod) { e.NameZh += " (mod)"; e.NameEn += " (mod)"; }
                    }
                    catch { e.IsValid = false; }
                }
            }
            Resolve(ItemDatabase.All, false);
            Resolve(ItemDatabaseMod.All, true);
            ModMain.Log?.Msg("[Spawner] Display names resolved from game localization");
        }
        catch (System.Exception ex) { ModMain.Log?.Warning($"[ResolveNames] {ex.Message}"); }
    }

    private static void RebuildFilter()
    {
        ResolveDisplayNames();
        _filtered.Clear();
        string cat = ItemDatabase.Categories[_selectedCategory];

        for (int i = 0; i < ItemDatabase.All.Count; i++)
        {
            var e = ItemDatabase.All[i];
            if (!e.IsValid) continue;
            if (_selectedCategory != 0 && e.Category != cat) continue;
            BuildDisplayLabels(e);
            _filtered.Add(e);
        }
        for (int i = 0; i < ItemDatabaseMod.All.Count; i++)
        {
            var e = ItemDatabaseMod.All[i];
            if (!e.IsValid) continue;
            if (_selectedCategory != 0 && e.Category != cat) continue;
            BuildDisplayLabels(e);
            _filtered.Add(e);
        }
        // 分页缓存失效
        _cachedPageNum = -1;
    }

    // v6.6 GC: 预计算带卡路里后缀的显示名,供 Spawner UI 每帧直接读取
    private static void BuildDisplayLabels(ItemEntry e)
    {
        string suffix = e.Calories > 0 ? $" {e.Calories}k" : "";
        string baseZh = e.NameZh ?? e.Name;
        string baseEn = e.NameEn ?? e.Name;
        e.DisplayLabelZh = suffix.Length > 0 ? baseZh + suffix : baseZh;
        e.DisplayLabelEn = suffix.Length > 0 ? baseEn + suffix : baseEn;
    }
}
