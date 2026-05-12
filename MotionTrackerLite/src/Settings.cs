using ModSettings;
using UnityEngine;

namespace MotionTrackerLite;

internal class Settings : JsonModSettings
{
    [Section("雷达设置")]

    [Name("热键")]
    [Description("按此键切换雷达开关")]
    public KeyCode ToggleKey = KeyCode.U;

    [Name("仅室外生效")]
    [Description("只有在室外时才显示雷达")]
    public bool OnlyOutdoors = true;

    [Name("探测范围(米)")]
    [Description("探测实体的最大距离")]
    [Slider(50, 500, 10)]
    public int DetectionRange = 100;

    [Name("雷达大小")]
    [Description("右上角雷达界面的缩放")]
    [Slider(0.5f, 3.0f)]
    public float Scale = 1.0f;

    [Name("雷达透明度")]
    [Description("雷达背景的透明程度")]
    [Slider(0.1f, 1.0f)]
    public float Opacity = 0.7f;

    [Section("野生动物")]

    [Name("动物图标大小")]
    [Description("动物图标的缩放倍率")]
    [Slider(0.5f, 3.0f)]
    public float AnimalIconScale = 1.0f;

    [Name("显示狼")]
    [Description("雷达上追踪普通狼")]
    public bool ShowWolves = true;

    [Name("显示森林狼")]
    [Description("雷达上追踪森林狼")]
    public bool ShowTimberwolves = true;

    [Name("显示熊")]
    [Description("雷达上追踪灰熊")]
    public bool ShowBears = true;

    [Name("显示驼鹿")]
    [Description("雷达上追踪驼鹿")]
    public bool ShowMoose = true;

    [Name("显示美洲狮")]
    [Description("雷达上追踪美洲狮")]
    public bool ShowCougars = true;

    [Name("显示公鹿")]
    [Description("雷达上追踪公鹿")]
    public bool ShowStags = true;

    [Name("显示母鹿")]
    [Description("雷达上追踪母鹿")]
    public bool ShowDoes = true;

    [Name("显示兔子")]
    [Description("雷达上追踪兔子")]
    public bool ShowRabbits = true;

    [Name("显示松鸡")]
    [Description("雷达上追踪松鸡")]
    public bool ShowPtarmigan = true;

    [Section("物品与结构")]

    [Name("玩家圆点透明度")]
    [Description("雷达中心代表玩家的白色圆点的透明度")]
    [Slider(0.3f, 1.0f)]
    public float PlayerDotOpacity = 0.85f;

    [Name("物品图标大小")]
    [Description("物品/结构图标的缩放倍率")]
    [Slider(0.5f, 3.0f)]
    public float GearIconScale = 1.0f;

    [Name("显示箭矢")]
    [Description("雷达上显示地上的箭矢")]
    public bool ShowArrows = true;

    [Name("显示煤炭")]
    [Description("雷达上显示地上的煤")]
    public bool ShowCoal = true;

    [Name("显示生鱼")]
    [Description("雷达上显示地上的生鱼")]
    public bool ShowRawFish = true;

    [Name("显示失物招领箱")]
    [Description("雷达上显示失物招领箱位置")]
    public bool ShowLostAndFound = true;

    [Name("显示盐矿")]
    [Description("雷达上显示盐矿位置")]
    public bool ShowSaltDeposit = true;

    [Name("显示海滩拾取物")]
    [Description("雷达上显示赶海物资位置")]
    public bool ShowBeachLoot = true;

    [Name("显示喷漆标记")]
    [Description("雷达上显示喷漆标记")]
    public bool ShowSpraypaint = true;

    protected override void OnConfirm()
    {
        base.OnConfirm();
        ApplyToConfig();
        TrackerConfig.Save();
    }

    internal void ApplyToConfig()
    {
        TrackerConfig.ToggleKey = ToggleKey;
        TrackerConfig.OnlyOutdoors = OnlyOutdoors;
        TrackerConfig.DetectionRange = DetectionRange;
        TrackerConfig.Scale = Scale;
        TrackerConfig.Opacity = Opacity;
        TrackerConfig.AnimalIconScale = AnimalIconScale;
        TrackerConfig.PlayerDotOpacity = PlayerDotOpacity;
        TrackerConfig.GearIconScale = GearIconScale;
        TrackerConfig.ShowWolves = ShowWolves;
        TrackerConfig.ShowTimberwolves = ShowTimberwolves;
        TrackerConfig.ShowBears = ShowBears;
        TrackerConfig.ShowMoose = ShowMoose;
        TrackerConfig.ShowCougars = ShowCougars;
        TrackerConfig.ShowStags = ShowStags;
        TrackerConfig.ShowDoes = ShowDoes;
        TrackerConfig.ShowRabbits = ShowRabbits;
        TrackerConfig.ShowPtarmigan = ShowPtarmigan;
        TrackerConfig.ShowArrows = ShowArrows;
        TrackerConfig.ShowCoal = ShowCoal;
        TrackerConfig.ShowRawFish = ShowRawFish;
        TrackerConfig.ShowLostAndFound = ShowLostAndFound;
        TrackerConfig.ShowSaltDeposit = ShowSaltDeposit;
        TrackerConfig.ShowBeachLoot = ShowBeachLoot;
        TrackerConfig.ShowSpraypaint = ShowSpraypaint;
    }

    internal void LoadFromConfig()
    {
        ToggleKey = TrackerConfig.ToggleKey;
        OnlyOutdoors = TrackerConfig.OnlyOutdoors;
        DetectionRange = TrackerConfig.DetectionRange;
        Scale = TrackerConfig.Scale;
        Opacity = TrackerConfig.Opacity;
        AnimalIconScale = TrackerConfig.AnimalIconScale;
        PlayerDotOpacity = TrackerConfig.PlayerDotOpacity;
        GearIconScale = TrackerConfig.GearIconScale;
        ShowWolves = TrackerConfig.ShowWolves;
        ShowTimberwolves = TrackerConfig.ShowTimberwolves;
        ShowBears = TrackerConfig.ShowBears;
        ShowMoose = TrackerConfig.ShowMoose;
        ShowCougars = TrackerConfig.ShowCougars;
        ShowStags = TrackerConfig.ShowStags;
        ShowDoes = TrackerConfig.ShowDoes;
        ShowRabbits = TrackerConfig.ShowRabbits;
        ShowPtarmigan = TrackerConfig.ShowPtarmigan;
        ShowArrows = TrackerConfig.ShowArrows;
        ShowCoal = TrackerConfig.ShowCoal;
        ShowRawFish = TrackerConfig.ShowRawFish;
        ShowLostAndFound = TrackerConfig.ShowLostAndFound;
        ShowSaltDeposit = TrackerConfig.ShowSaltDeposit;
        ShowBeachLoot = TrackerConfig.ShowBeachLoot;
        ShowSpraypaint = TrackerConfig.ShowSpraypaint;
    }
}
