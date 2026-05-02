using System;
using UnityEngine;
using Il2Cpp;

namespace TldHacks;

internal static class CheatStateESP
{
    public static bool ESP;
    public static bool AutoAim;
    public static bool MagicBullet;
    public static float AutoAimFOV = 30f;
    public static float AutoAimSpeed = 15f;
    public static int AimPart; // 0=body, 1=head, 2=legs
    public static float RecoilScale = 1f;
    public static float FireRateScale = 1f;
    public static float ReloadScale = 1f;
    public static bool ShowBoxes = true;
    public static bool ShowHealth = true;
    public static float ESPRange = 300f;
}

internal static class AiCache
{
    private static Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppArrayBase<BaseAi> _cached;
    private static int _frame = -1;

    public static Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppArrayBase<BaseAi> Get()
    {
        if (Time.frameCount != _frame)
        {
            _frame = Time.frameCount;
            _cached = UnityEngine.Object.FindObjectsOfType<BaseAi>();
        }
        return _cached;
    }
}

internal static class ESPOverlay
{
    private static GUIStyle _labelStyle;
    private static Texture2D _whiteTex;
    private static readonly Color AnimalColor = new Color(1f, 0.2f, 0.2f, 1f);
    private static readonly Color ContainerColor = new Color(0.3f, 0.8f, 1f, 0.9f);

    public static void OnGUI()
    {
        if (!CheatStateESP.ESP && !CheatStateESP.AutoAim && !CheatStateESP.MagicBullet) return;
        var cam = Camera.main;
        if (cam == null) return;
        EnsureResources();
        if (CheatStateESP.AutoAim) try { DrawFOVCircle(); } catch { }
        if (CheatStateESP.ESP)
        {
            try { DrawAnimals(cam); } catch { }
            try { DrawContainers(cam); } catch { }
        }
        if (CheatStateESP.MagicBullet) try { DrawMagicBulletPreview(cam); } catch { }
    }

    private static void EnsureResources()
    {
        if (_labelStyle == null)
        {
            _labelStyle = new GUIStyle(GUI.skin.label) { fontSize = 12 };
            _labelStyle.normal.textColor = Color.white;
        }
        if (_whiteTex == null)
        {
            _whiteTex = new Texture2D(1, 1);
            _whiteTex.SetPixel(0, 0, Color.white);
            _whiteTex.Apply();
        }
    }

    private static void DrawAnimals(Camera cam)
    {
        var ais = AiCache.Get();
        if (ais == null) return;
        var playerPos = GameManager.GetPlayerTransform()?.position ?? Vector3.zero;

        for (int i = 0; i < ais.Count; i++)
        {
            var ai = ais[i];
            if (ai == null) continue;
            var go = ai.gameObject;
            if (go == null || !go.activeSelf) continue;

            var pos = go.transform.position;
            float dist = Vector3.Distance(pos, playerPos);
            if (dist > CheatStateESP.ESPRange) continue;

            float hp = 100f, maxHp = 100f;
            try { hp = ai.m_CurrentHP; maxHp = ai.m_MaxHP; } catch { }
            if (hp <= 0) continue;

            string name = "Animal";
            try { name = ai.m_AiSubType.ToString(); } catch { }

            bool isTarget = (ai == AutoAimSystem.LockedTarget);
            bool visible = AutoAimSystem.HasLOS(playerPos + Vector3.up * 1.6f, pos + Vector3.up * 0.8f, go);

            if (CheatStateESP.ShowBoxes)
                DrawBBox(cam, go, dist, name, hp, maxHp, isTarget, visible);
            else
                DrawLabel(cam, pos, dist, name);

            if (isTarget)
                DrawTargetMarker(cam, go);
        }
        GUI.color = Color.white;
    }

    private static readonly Color TargetColor = new Color(1f, 0.85f, 0f, 1f);

    private static void DrawBBox(Camera cam, GameObject go, float dist, string name, float hp, float maxHp, bool isTarget, bool visible)
    {
        var pos = go.transform.position;
        float h = 1.2f;
        try { var r = go.GetComponentInChildren<Renderer>(); if (r != null) h = r.bounds.size.y; } catch { }
        var top = cam.WorldToScreenPoint(pos + Vector3.up * h);
        var bot = cam.WorldToScreenPoint(pos);
        if (top.z <= 0 && bot.z <= 0) return;
        float sy1 = Screen.height - top.y;
        float sy2 = Screen.height - bot.y;
        float bh = Mathf.Abs(sy2 - sy1);
        if (bh < 6f) bh = 6f;
        float bw = bh * 0.6f;
        float cx = (top.x + bot.x) * 0.5f;
        float ty = Mathf.Min(sy1, sy2);
        var boxColor = isTarget ? TargetColor : AnimalColor;
        if (!visible) boxColor.a = 0.35f;
        DrawRect(cx - bw / 2, ty, bw, bh, boxColor, isTarget ? 3f : 2f);
        if (CheatStateESP.ShowHealth && maxHp > 0)
        {
            float frac = Mathf.Clamp01(hp / maxHp);
            var hpC = Color.Lerp(Color.red, Color.green, frac);
            if (!visible) hpC.a = 0.35f;
            FillRect(cx - bw / 2, ty + bh + 2, bw * frac, 3f, hpC);
        }
        GUI.color = boxColor;
        string prefix = isTarget ? "[TARGET] " : (!visible ? "[WALL] " : "");
        GUI.Label(new Rect(cx - bw / 2, ty - 16, 200, 16), $"{prefix}{name} [{dist:F0}m]", _labelStyle);
    }

    private static void DrawTargetMarker(Camera cam, GameObject go)
    {
        var sp = cam.WorldToScreenPoint(go.transform.position + Vector3.up * 0.8f);
        if (sp.z <= 0) return;
        float sx = sp.x, sy = Screen.height - sp.y;
        GUI.color = TargetColor;
        FillRect(sx - 8, sy - 1, 16, 2, TargetColor);
        FillRect(sx - 1, sy - 8, 2, 16, TargetColor);
    }

    private static void DrawLabel(Camera cam, Vector3 pos, float dist, string name)
    {
        var sp = cam.WorldToScreenPoint(pos + Vector3.up * 1f);
        if (sp.z <= 0) return;
        float sx = sp.x, sy = Screen.height - sp.y;
        GUI.color = AnimalColor;
        GUI.Label(new Rect(sx - 50, sy - 10, 100, 20), $"{name} {dist:F0}m", _labelStyle);
    }

    private static void DrawContainers(Camera cam)
    {
        var containers = UnityEngine.Object.FindObjectsOfType<Container>();
        if (containers == null) return;
        var playerPos = GameManager.GetPlayerTransform()?.position ?? Vector3.zero;
        for (int i = 0; i < containers.Count; i++)
        {
            var c = containers[i];
            if (c == null) continue;
            var go = c.gameObject;
            if (go == null || !go.activeSelf) continue;
            var pos = go.transform.position;
            float dist = Vector3.Distance(pos, playerPos);
            if (dist > 50f) continue;
            var sp = cam.WorldToScreenPoint(pos);
            if (sp.z <= 0) continue;
            GUI.color = ContainerColor;
            GUI.Label(new Rect(sp.x - 40, Screen.height - sp.y - 10, 80, 20), $"[Box] {dist:F0}m", _labelStyle);
        }
        GUI.color = Color.white;
    }

    private static void DrawRect(float x, float y, float w, float h, Color c, float t)
    {
        GUI.color = c;
        GUI.DrawTexture(new Rect(x, y, w, t), _whiteTex);
        GUI.DrawTexture(new Rect(x, y + h - t, w, t), _whiteTex);
        GUI.DrawTexture(new Rect(x, y, t, h), _whiteTex);
        GUI.DrawTexture(new Rect(x + w - t, y, t, h), _whiteTex);
    }

    private static void FillRect(float x, float y, float w, float h, Color c)
    {
        GUI.color = c;
        GUI.DrawTexture(new Rect(x, y, w, h), _whiteTex);
    }

    private static readonly Color FovColor = new Color(1f, 1f, 1f, 0.3f);
    private static readonly Color FovColorActive = new Color(1f, 0.85f, 0f, 0.5f);

    private static void DrawFOVCircle()
    {
        float cx = Screen.width * 0.5f;
        float cy = Screen.height * 0.5f;
        float fovRad = CheatStateESP.AutoAimFOV * Mathf.Deg2Rad;
        float radius = Mathf.Tan(fovRad) * (Screen.height * 0.5f) / Mathf.Tan(Camera.main.fieldOfView * 0.5f * Mathf.Deg2Rad);
        var col = AutoAimSystem.LockedTarget != null ? FovColorActive : FovColor;
        GUI.color = col;
        int segments = 48;
        float angleStep = 360f / segments;
        for (int i = 0; i < segments; i++)
        {
            float a = i * angleStep * Mathf.Deg2Rad;
            float px = cx + Mathf.Cos(a) * radius;
            float py = cy + Mathf.Sin(a) * radius;
            GUI.DrawTexture(new Rect(px - 1f, py - 1f, 2f, 2f), _whiteTex);
        }
    }

    private static readonly Color MagicColor = new Color(0.9f, 0.3f, 1f, 0.9f);

    private static void DrawMagicBulletPreview(Camera cam)
    {
        var target = MagicBulletSystem.PreviewTarget;
        if (target == null) return;
        try
        {
            var go = target.gameObject;
            if (go == null || !go.activeSelf) return;
            var pos = go.transform.position + Vector3.up * 0.8f;
            var sp = cam.WorldToScreenPoint(pos);
            if (sp.z <= 0) return;
            float sx = sp.x, sy = Screen.height - sp.y;
            GUI.color = MagicColor;
            float sz = 12f;
            FillRect(sx - sz, sy - 1f, sz * 2, 2f, MagicColor);
            FillRect(sx - 1f, sy - sz, 2f, sz * 2, MagicColor);
            float d = sz * 0.7f;
            FillRect(sx - d, sy - d, 2f, d * 0.6f, MagicColor);
            FillRect(sx - d, sy - d, d * 0.6f, 2f, MagicColor);
            FillRect(sx + d - 2f, sy - d, 2f, d * 0.6f, MagicColor);
            FillRect(sx + d - d * 0.6f, sy - d, d * 0.6f, 2f, MagicColor);
            FillRect(sx - d, sy + d - 2f, d * 0.6f, 2f, MagicColor);
            FillRect(sx - d, sy + d - d * 0.6f, 2f, d * 0.6f, MagicColor);
            FillRect(sx + d - d * 0.6f, sy + d - 2f, d * 0.6f, 2f, MagicColor);
            FillRect(sx + d - 2f, sy + d - d * 0.6f, 2f, d * 0.6f, MagicColor);
            GUI.Label(new Rect(sx + sz + 4, sy - 8, 120, 16), "[MAGIC]", _labelStyle);
        }
        catch { }
    }
}

internal static class AutoAimSystem
{
    private static BaseAi _lockedTarget;
    private static float _lastScanTime;
    private static float _releaseTime;
    private static bool _wasHolding;
    private const float ReleaseSmooth = 0.2f;
    private const float ArrowSpeed = 40f;
    private const float Gravity = 9.81f;
    public static BaseAi LockedTarget => _lockedTarget;

    private static bool IsHoldingBow()
    {
        try
        {
            var pm = GameManager.GetPlayerManagerComponent();
            if (pm == null) return false;
            var gi = pm.m_ItemInHands;
            if (gi == null) return false;
            return gi.GetComponent<BowItem>() != null;
        }
        catch { return false; }
    }

    public static void Tick()
    {
        if (!CheatStateESP.AutoAim) { _lockedTarget = null; _wasHolding = false; return; }
        var cam = GameManager.GetVpFPSCamera();
        if (cam == null) return;

        bool isBow = IsHoldingBow();
        bool holding = isBow ? Input.GetMouseButton(0) : Input.GetMouseButton(1);
        if (!holding && _wasHolding)
            _releaseTime = Time.time;
        _wasHolding = holding;

        float speedMul = 1f;
        if (!holding)
        {
            float elapsed = Time.time - _releaseTime;
            if (elapsed > ReleaseSmooth) { _lockedTarget = null; return; }
            speedMul = 1f - (elapsed / ReleaseSmooth);
        }

        if (_lockedTarget != null)
        {
            try
            {
                var tgo = _lockedTarget.gameObject;
                if (tgo == null || !tgo.activeSelf || _lockedTarget.m_CurrentHP <= 0)
                    _lockedTarget = null;
            }
            catch { _lockedTarget = null; }
        }

        if (holding && Time.time - _lastScanTime > 0.1f)
        {
            _lastScanTime = Time.time;
            _lockedTarget = FindBestTarget(cam);
        }
        if (_lockedTarget == null) return;

        try
        {
            var go = _lockedTarget.gameObject;
            var targetPos = GetAimPoint(go);
            var camT = cam.transform;

            if (isBow)
                targetPos = CompensateArrowDrop(camT.position, targetPos);

            var dir = (targetPos - camT.position).normalized;
            var targetRot = Quaternion.LookRotation(dir);
            float speed = CheatStateESP.AutoAimSpeed * speedMul;
            camT.rotation = Quaternion.Slerp(camT.rotation, targetRot, Time.deltaTime * speed);
        }
        catch { _lockedTarget = null; }
    }

    private static Vector3 CompensateArrowDrop(Vector3 from, Vector3 to)
    {
        float dist = Vector3.Distance(from, to);
        if (dist < 5f) return to;
        float flightTime = dist / ArrowSpeed;
        float drop = 0.5f * Gravity * flightTime * flightTime;
        return to + Vector3.up * drop;
    }

    private static Vector3 GetAimPoint(GameObject go)
    {
        var pos = go.transform.position;
        return CheatStateESP.AimPart switch
        {
            1 => pos + Vector3.up * 1.4f,
            2 => pos + Vector3.up * 0.3f,
            _ => pos + Vector3.up * 0.8f,
        };
    }

    private static BaseAi FindBestTarget(vp_FPSCamera cam)
    {
        var camT = cam.transform;
        var camPos = camT.position;
        var camFwd = camT.forward;
        float fovCos = Mathf.Cos(CheatStateESP.AutoAimFOV * Mathf.Deg2Rad);

        var ais = AiCache.Get();
        if (ais == null) return null;

        BaseAi bestVisible = null, bestAny = null;
        float bestVisScore = float.MaxValue, bestAnyScore = float.MaxValue;
        for (int i = 0; i < ais.Count; i++)
        {
            var ai = ais[i];
            if (ai == null) continue;
            var go = ai.gameObject;
            if (go == null || !go.activeSelf) continue;
            try { if (ai.m_CurrentHP <= 0) continue; } catch { }

            var pos = go.transform.position + Vector3.up * 0.8f;
            var toTarget = pos - camPos;
            float dist = toTarget.magnitude;
            if (dist < 1f || dist > 200f) continue;
            float dot = Vector3.Dot(camFwd, toTarget / dist);
            if (dot < fovCos) continue;
            float angle = Mathf.Acos(Mathf.Clamp(dot, -1f, 1f)) * Mathf.Rad2Deg;
            float score = angle + dist * 0.05f;

            if (score < bestAnyScore) { bestAnyScore = score; bestAny = ai; }
            if (score < bestVisScore && HasLOS(camPos, pos, go))
            { bestVisScore = score; bestVisible = ai; }
        }
        return bestVisible ?? bestAny;
    }

    internal static bool HasLOS(Vector3 from, Vector3 to, GameObject target)
    {
        try
        {
            var dir = to - from;
            float dist = dir.magnitude;
            if (UnityEngine.Physics.Raycast(from, dir / dist, out var hit, dist))
            {
                if (hit.collider != null && hit.collider.gameObject != null)
                    return hit.collider.gameObject == target
                        || hit.collider.transform.IsChildOf(target.transform);
            }
            return true;
        }
        catch { return true; }
    }
}

internal static class WeaponTuning
{
    public static void Tick()
    {
        if (CheatStateESP.RecoilScale == 1f && CheatStateESP.FireRateScale == 1f
            && CheatStateESP.ReloadScale == 1f) return;
        try
        {
            var pm = GameManager.GetPlayerManagerComponent();
            if (pm == null) return;
            var gi = pm.m_ItemInHands;
            if (gi == null) return;
            var gun = gi.GetComponent<GunItem>();
            if (gun == null) return;

            if (CheatStateESP.RecoilScale != 1f)
            {
                try { gun.m_MultiplierAiming = CheatStateESP.RecoilScale; } catch { }
                try { gun.m_MultiplierFire = CheatStateESP.RecoilScale; } catch { }
            }
            if (CheatStateESP.FireRateScale != 1f)
            {
                // FireRate: scale MultiplierFire which affects fire animation speed
                try { gun.m_MultiplierFire = CheatStateESP.FireRateScale; } catch { }
            }
            if (CheatStateESP.ReloadScale != 1f)
            {
                try { gun.m_MultiplierReload = CheatStateESP.ReloadScale; } catch { }
            }
        }
        catch { }
    }
}

internal static class MagicBulletSystem
{
    private static BaseAi _previewTarget;
    public static BaseAi PreviewTarget => _previewTarget;

    public static void UpdatePreview()
    {
        if (!CheatStateESP.MagicBullet) { _previewTarget = null; return; }
        _previewTarget = AutoAimSystem.LockedTarget ?? FindNearest();
    }

    public static void OnFired()
    {
        if (!CheatStateESP.MagicBullet) return;
        var target = AutoAimSystem.LockedTarget ?? FindNearest();
        if (target == null) return;
        try
        {
            float dmg = CheatState.InstantKillAnimals ? target.m_MaxHP + 100f : 80f;
            target.ApplyDamage(dmg, 0f, DamageSource.Player, "");
        }
        catch { }
    }

    private static BaseAi FindNearest()
    {
        var cam = GameManager.GetVpFPSCamera();
        if (cam == null) return null;
        var camPos = cam.transform.position;
        var camFwd = cam.transform.forward;
        var ais = AiCache.Get();
        if (ais == null) return null;
        BaseAi best = null;
        float bestDist = 200f;
        for (int i = 0; i < ais.Count; i++)
        {
            var ai = ais[i];
            if (ai == null) continue;
            var go = ai.gameObject;
            if (go == null || !go.activeSelf) continue;
            try { if (ai.m_CurrentHP <= 0) continue; } catch { }
            var toTarget = go.transform.position - camPos;
            float dist = toTarget.magnitude;
            if (dist > bestDist) continue;
            if (Vector3.Dot(camFwd, toTarget.normalized) < 0.5f) continue;
            bestDist = dist;
            best = ai;
        }
        return best;
    }
}
