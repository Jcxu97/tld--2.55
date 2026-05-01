using System;
using System.Globalization;

namespace TldHacks;

internal static class I18n
{
    public const int MODE_AUTO = 0;
    public const int MODE_ZH   = 1;
    public const int MODE_EN   = 2;

    private static bool _autoIsEn;
    private static bool _autoResolved;

    public static bool IsEnglish
    {
        get
        {
            int mode = ModMain.Settings != null ? ModMain.Settings.LanguageMode : MODE_AUTO;
            if (mode == MODE_ZH) return false;
            if (mode == MODE_EN) return true;
            if (!_autoResolved) { _autoIsEn = DetectSystemIsEnglish(); _autoResolved = true; }
            return _autoIsEn;
        }
    }

    private static bool DetectSystemIsEnglish()
    {
        try
        {
            string name = CultureInfo.CurrentUICulture?.Name;
            if (string.IsNullOrEmpty(name)) return false;
            return !name.StartsWith("zh", StringComparison.OrdinalIgnoreCase);
        }
        catch { return false; }
    }

    public static string T(string zh, string en) => IsEnglish ? en : zh;
}
