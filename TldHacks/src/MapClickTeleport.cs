using System;
using Il2Cpp;
using UnityEngine;
using SceneManager = UnityEngine.SceneManagement.SceneManager;

namespace TldHacks;

// 双击地图图标 → 传送到该位置(同区内)
// v2.7.95 重写:MapPositionToWorldPosition 在 IL2CPP 下返回错误坐标,改用 MapDetail 匹配方案
//   策略:
//   1) 遍历场景中所有 MapDetail, 用 Panel_Map.WorldPositionToMapPosition 算出各自的 map 坐标
//   2) 找出与 hovered icon 的 m_PositionOnMap 最近的 MapDetail
//   3) 用该 MapDetail.transform.position 作为传送目标
//   4) Fallback: 匹配 Teleport.Destinations 中已知坐标
internal static class MapClickTeleport
{
    private static float _lastClickTime;
    private const float DoubleClickInterval = 0.4f;
    private static Panel_Map _cachedPanel;

    // 缓存当前场景的 MapDetail 位置映射(避免每次双击都全量扫描)
    private static string _cachedScene;
    private static (Vector2 mapPos, Vector3 worldPos)[] _mapDetailCache;

    // 仿射变换参数: worldX = ax*mapX + bx*mapY + tx, worldZ = az*mapX + bz*mapY + tz
    private static bool _affineValid;
    private static double _ax, _bx, _tx;
    private static double _az, _bz, _tz;

    public static void Tick()
    {
        try
        {
            if (!CheatState.MapClickTP) return;
            if (!Input.GetMouseButtonDown(0)) return;

            var panel = GetPanel();
            if (panel == null || !panel.isActiveAndEnabled) return;

            float now = Time.unscaledTime;
            if (now - _lastClickTime > DoubleClickInterval)
            {
                _lastClickTime = now;
                return;
            }

            _lastClickTime = 0f;
            TryTeleportToHoveredIcon(panel);
        }
        catch (Exception ex) { ModMain.Log?.Warning($"[MapTP] {ex.Message}"); }
    }

    private static Panel_Map GetPanel()
    {
        if (_cachedPanel != null && _cachedPanel.isActiveAndEnabled) return _cachedPanel;
        _cachedPanel = UnityEngine.Object.FindObjectOfType<Panel_Map>();
        return _cachedPanel;
    }

    public static void OnSceneChange()
    {
        _cachedPanel = null;
        _cachedScene = null;
        _mapDetailCache = null;
        _affineValid = false;
    }

    private static Vector3 FindGroundPosition(Vector3 pos)
    {
        Vector3 origin = new Vector3(pos.x, pos.y + 200f, pos.z);
        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 500f))
            return hit.point + Vector3.up * 0.5f;
        return new Vector3(pos.x, pos.y + 0.5f, pos.z);
    }

    /// <summary>
    /// 构建/刷新当前场景所有 MapDetail 的 (mapPos, worldPos) 缓存。
    /// MapDetail 是场景中标注地图兴趣点的 MonoBehaviour, 它的 transform.position 就是真实世界坐标。
    /// </summary>
    private static void EnsureMapDetailCache(Panel_Map panel, string region)
    {
        if (_cachedScene == region && _mapDetailCache != null) return;

        _cachedScene = region;
        var allDetails = UnityEngine.Object.FindObjectsOfType<MapDetail>();
        if (allDetails == null || allDetails.Count == 0)
        {
            _mapDetailCache = Array.Empty<(Vector2, Vector3)>();
            ModMain.Log?.Msg($"[MapTP] scene {region}: 0 MapDetails found");
            return;
        }

        var list = new System.Collections.Generic.List<(Vector2, Vector3)>(allDetails.Count);
        bool worldPosToMapBroken = false;
        for (int i = 0; i < allDetails.Count; i++)
        {
            try
            {
                var md = allDetails[i];
                if (md == null || md.gameObject == null) continue;
                Vector3 wp = md.transform.position;
                // 用游戏的正向投影 WorldPositionToMapPosition 得到 map 坐标
                Vector3 mp3 = panel.WorldPositionToMapPosition(region, wp);
                // 验证: map 坐标应该在合理范围内(±50),如果超出说明 interop 也坏了
                if (Mathf.Abs(mp3.x) > 100f || Mathf.Abs(mp3.y) > 100f)
                {
                    if (!worldPosToMapBroken)
                    {
                        ModMain.Log?.Warning($"[MapTP] WorldPositionToMapPosition returns out-of-range: wp={wp} → mp={mp3}");
                        worldPosToMapBroken = true;
                    }
                    continue;
                }
                Vector2 mp = new Vector2(mp3.x, mp3.y);
                list.Add((mp, wp));
            }
            catch { }
        }

        if (worldPosToMapBroken && list.Count == 0)
        {
            // WorldPositionToMapPosition 也不可用 → 降级: 直接按 m_PositionOnMap 的比例关系暴力反推
            // 用 Panel_Map.m_MapElementData 中已有的 (mapPos, locID) 对 Teleport.Destinations 做映射
            ModMain.Log?.Warning($"[MapTP] WorldPositionToMapPosition broken, MapDetail strategy disabled");
            _mapDetailCache = Array.Empty<(Vector2, Vector3)>();
            return;
        }

        _mapDetailCache = list.ToArray();
        ComputeAffineTransform();
        ModMain.Log?.Msg($"[MapTP] scene {region}: cached {_mapDetailCache.Length} MapDetail positions, affine={_affineValid}");
    }

    /// <summary>
    /// 从 MapDetail 缓存计算 2D 仿射变换 (最小二乘法)。
    /// map(2D) → world(XZ): worldX = ax*mapX + bx*mapY + tx, worldZ = az*mapX + bz*mapY + tz
    /// </summary>
    private static void ComputeAffineTransform()
    {
        _affineValid = false;
        if (_mapDetailCache == null || _mapDetailCache.Length < 3) return;

        int n = _mapDetailCache.Length;
        // 构建正规方程 A^T*A * params = A^T*b (3x3 系统, 分别求解 X 和 Z)
        double s_mx = 0, s_my = 0, s_mxmx = 0, s_mymy = 0, s_mxmy = 0;
        double s_wx = 0, s_wz = 0;
        double s_mx_wx = 0, s_my_wx = 0, s_mx_wz = 0, s_my_wz = 0;

        for (int i = 0; i < n; i++)
        {
            double mx = _mapDetailCache[i].mapPos.x;
            double my = _mapDetailCache[i].mapPos.y;
            double wx = _mapDetailCache[i].worldPos.x;
            double wz = _mapDetailCache[i].worldPos.z;

            s_mx += mx; s_my += my;
            s_mxmx += mx * mx; s_mymy += my * my; s_mxmy += mx * my;
            s_wx += wx; s_wz += wz;
            s_mx_wx += mx * wx; s_my_wx += my * wx;
            s_mx_wz += mx * wz; s_my_wz += my * wz;
        }

        // 正规方程 (A^T A): [[sum(mx^2), sum(mx*my), sum(mx)],
        //                     [sum(mx*my), sum(my^2), sum(my)],
        //                     [sum(mx),    sum(my),   n      ]]
        // 用克拉默法则解 3x3 线性方程
        double a00 = s_mxmx, a01 = s_mxmy, a02 = s_mx;
        double a10 = s_mxmy, a11 = s_mymy, a12 = s_my;
        double a20 = s_mx,   a21 = s_my,   a22 = n;

        double det = a00 * (a11 * a22 - a12 * a21)
                   - a01 * (a10 * a22 - a12 * a20)
                   + a02 * (a10 * a21 - a11 * a20);

        if (Math.Abs(det) < 1e-10)
        {
            ModMain.Log?.Warning("[MapTP] affine: singular matrix, skipping");
            return;
        }

        double invDet = 1.0 / det;

        // 逆矩阵 (伴随矩阵转置 / det)
        double i00 = (a11 * a22 - a12 * a21) * invDet;
        double i01 = (a02 * a21 - a01 * a22) * invDet;
        double i02 = (a01 * a12 - a02 * a11) * invDet;
        double i10 = (a12 * a20 - a10 * a22) * invDet;
        double i11 = (a00 * a22 - a02 * a20) * invDet;
        double i12 = (a02 * a10 - a00 * a12) * invDet;
        double i20 = (a10 * a21 - a11 * a20) * invDet;
        double i21 = (a01 * a20 - a00 * a21) * invDet;
        double i22 = (a00 * a11 - a01 * a10) * invDet;

        // 解 world.x = ax*mapX + bx*mapY + tx
        double bx_vec0 = s_mx_wx, bx_vec1 = s_my_wx, bx_vec2 = s_wx;
        _ax = i00 * bx_vec0 + i01 * bx_vec1 + i02 * bx_vec2;
        _bx = i10 * bx_vec0 + i11 * bx_vec1 + i12 * bx_vec2;
        _tx = i20 * bx_vec0 + i21 * bx_vec1 + i22 * bx_vec2;

        // 解 world.z = az*mapX + bz*mapY + tz
        double bz_vec0 = s_mx_wz, bz_vec1 = s_my_wz, bz_vec2 = s_wz;
        _az = i00 * bz_vec0 + i01 * bz_vec1 + i02 * bz_vec2;
        _bz = i10 * bz_vec0 + i11 * bz_vec1 + i12 * bz_vec2;
        _tz = i20 * bz_vec0 + i21 * bz_vec1 + i22 * bz_vec2;

        // 验证: 计算最大残差
        double maxResidual = 0;
        for (int i = 0; i < n; i++)
        {
            double mx = _mapDetailCache[i].mapPos.x;
            double my = _mapDetailCache[i].mapPos.y;
            double predX = _ax * mx + _bx * my + _tx;
            double predZ = _az * mx + _bz * my + _tz;
            double dx = predX - _mapDetailCache[i].worldPos.x;
            double dz = predZ - _mapDetailCache[i].worldPos.z;
            double r = Math.Sqrt(dx * dx + dz * dz);
            if (r > maxResidual) maxResidual = r;
        }

        if (maxResidual > 100.0)
        {
            ModMain.Log?.Warning($"[MapTP] affine transform residual too large: {maxResidual:F1}m, disabled");
            return;
        }

        _affineValid = true;
        ModMain.Log?.Msg($"[MapTP] affine transform computed: maxResidual={maxResidual:F1}m, n={n}");
    }

    /// <summary>
    /// 用仿射变换将 map 坐标直接转换为 world XZ,
    /// Y 坐标从最近 MapDetail 的已知地面高度插值得出（比 raycast 更可靠，不会落在屋顶上）。
    /// </summary>
    private static bool TryAffineTransform(Vector2 targetMapPos, out Vector3 worldPos)
    {
        worldPos = Vector3.zero;
        if (!_affineValid) return false;

        double wx = _ax * targetMapPos.x + _bx * targetMapPos.y + _tx;
        double wz = _az * targetMapPos.x + _bz * targetMapPos.y + _tz;

        float worldX = (float)wx;
        float worldZ = (float)wz;

        // 用最近的 MapDetail 的 Y 坐标作为地面高度参考（MapDetail 都是游戏放在地面的）
        float groundY = EstimateGroundY(worldX, worldZ);
        worldPos = new Vector3(worldX, groundY + 1.5f, worldZ);
        ModMain.Log?.Msg($"[MapTP] affine: mapPos={targetMapPos} → world=({worldX:F1},{groundY:F1},{worldZ:F1}) +2m");
        return true;
    }

    private static float EstimateGroundY(float worldX, float worldZ)
    {
        return RaycastGroundY(worldX, worldZ);
    }

    private static float RaycastGroundY(float x, float z)
    {
        Vector3 origin = new Vector3(x, 1000f, z);
        var hits = Physics.RaycastAll(origin, Vector3.down, 2000f);
        if (hits == null || hits.Length == 0) return 0f;

        // 取最高的非 trigger 命中面（= 从天空向下的第一个实体表面 = 地面/屋顶）
        float bestY = float.MinValue;
        for (int i = 0; i < hits.Length; i++)
        {
            var col = hits[i].collider;
            if (col != null && col.isTrigger) continue;
            float hy = hits[i].point.y;
            if (hy > bestY) bestY = hy;
        }
        if (bestY > float.MinValue) return bestY;

        // 全是 trigger 的 fallback：取最高的
        for (int i = 0; i < hits.Length; i++)
        {
            float hy = hits[i].point.y;
            if (hy > bestY) bestY = hy;
        }
        return bestY > float.MinValue ? bestY : 0f;
    }

    /// <summary>
    /// 在缓存中找与目标 mapPos 最近的 MapDetail 世界坐标(仿射变换不可用时的 fallback)。
    /// </summary>
    private static bool FindClosestMapDetail(Vector2 targetMapPos, out Vector3 worldPos)
    {
        worldPos = Vector3.zero;
        if (_mapDetailCache == null || _mapDetailCache.Length == 0) return false;

        float bestDist = float.MaxValue;
        int bestIdx = -1;
        for (int i = 0; i < _mapDetailCache.Length; i++)
        {
            float d = Vector2.Distance(_mapDetailCache[i].mapPos, targetMapPos);
            if (d < bestDist)
            {
                bestDist = d;
                bestIdx = i;
            }
        }

        if (bestIdx >= 0 && bestDist < 1.0f)
        {
            worldPos = _mapDetailCache[bestIdx].worldPos;
            ModMain.Log?.Msg($"[MapTP] closest MapDetail dist={bestDist:F3} → world={worldPos}");
            return true;
        }

        if (bestIdx >= 0)
            ModMain.Log?.Msg($"[MapTP] closest MapDetail dist={bestDist:F3} (too far, threshold=1.0)");
        return false;
    }

    /// <summary>
    /// Fallback: 在 Teleport.Destinations 中按 locID 关键词匹配。
    /// m_LocationNameLocID 通常是 "GAMEPLAY_Location_XXX" 格式。
    /// </summary>
    private static bool FindInWaypoints(string locNameLocID, string region, out Vector3 worldPos)
    {
        worldPos = Vector3.zero;
        if (string.IsNullOrEmpty(locNameLocID)) return false;

        // 优先精确匹配同 region 的已知坐标
        foreach (var wp in Teleport.Destinations)
        {
            if (wp.Scene != region || wp.Pos == Vector3.zero) continue;
            // locID 可能包含 waypoint label 的部分(比如 "GAMEPLAY_Location_MysteryLake_Bunker")
            // 或者 waypoint label 包含 locID 的一部分
            if (locNameLocID.IndexOf(wp.Label, StringComparison.OrdinalIgnoreCase) >= 0 ||
                wp.Label.IndexOf(locNameLocID, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                worldPos = wp.Pos;
                return true;
            }
        }
        return false;
    }

    private static void TryTeleportToHoveredIcon(Panel_Map panel)
    {
        try
        {
            var icon = panel.m_MapIconBeingHovered;
            if (icon == null) return;

            // 通过 m_TransformToMapData 字典拿到 MapElementSaveData
            var dict = panel.m_TransformToMapData;
            if (dict == null) { ModMain.Log?.Warning("[MapTP] m_TransformToMapData is null"); return; }

            MapElementSaveData mesd = null;
            try { dict.TryGetValue(icon.transform, out mesd); } catch { }
            if (mesd == null)
            {
                ModMain.Log?.Msg("[MapTP] no MapElementSaveData for hovered icon");
                return;
            }

            Vector2 mapPos = mesd.m_PositionOnMap;
            if (mapPos == Vector2.zero)
            {
                ModMain.Log?.Msg("[MapTP] m_PositionOnMap is zero");
                return;
            }

            string region = SceneManager.GetActiveScene().name;
            string locName = mesd.m_LocationNameLocID ?? "?";
            Vector3 worldPos;

            EnsureMapDetailCache(panel, region);

            // 策略 1: 仿射变换(精确数学计算,不依赖最近匹配)
            if (TryAffineTransform(mapPos, out worldPos))
            {
                panel.Enable(false);
                Teleport.MovePlayerTo(worldPos);
                CheatState.LastActionLog = $"MapTP → {locName} ({worldPos.x:F0},{worldPos.y:F0},{worldPos.z:F0})";
                ModMain.Log?.Msg($"[MapTP] (affine) → {locName} mapPos={mapPos} world={worldPos}");
                return;
            }

            // 策略 2: 名称精确匹配
            if (TryFindMapDetailByName(locName, out worldPos))
            {
                Vector3 finalPos = FindGroundPosition(worldPos);
                panel.Enable(false);
                Teleport.MovePlayerTo(finalPos);
                CheatState.LastActionLog = $"MapTP → {locName} ({finalPos.x:F0},{finalPos.y:F0},{finalPos.z:F0})";
                ModMain.Log?.Msg($"[MapTP] (name match) → {locName} world={finalPos}");
                return;
            }

            // 策略 3: MapDetail 最近匹配(fallback)
            if (FindClosestMapDetail(mapPos, out worldPos))
            {
                Vector3 finalPos = FindGroundPosition(worldPos);
                panel.Enable(false);
                Teleport.MovePlayerTo(finalPos);
                CheatState.LastActionLog = $"MapTP → {locName} ({finalPos.x:F0},{finalPos.y:F0},{finalPos.z:F0})";
                ModMain.Log?.Msg($"[MapTP] (closest) → {locName} mapPos={mapPos} world={finalPos}");
                return;
            }

            // 策略 4: 已知 waypoint 匹配
            if (FindInWaypoints(locName, region, out worldPos))
            {
                panel.Enable(false);
                Teleport.MovePlayerTo(worldPos);
                CheatState.LastActionLog = $"MapTP → {locName} ({worldPos.x:F0},{worldPos.y:F0},{worldPos.z:F0})";
                ModMain.Log?.Msg($"[MapTP] (waypoint match) → {locName} world={worldPos}");
                return;
            }

            ModMain.Log?.Warning($"[MapTP] no world position found for '{locName}' mapPos={mapPos} in {region}");
            CheatState.LastActionLog = $"MapTP: 无法定位 {locName}";
        }
        catch (Exception ex) { ModMain.Log?.Warning($"[MapTP] teleport: {ex.Message}"); }
    }

    /// <summary>
    /// 通过 MapDetail 的 GameObject.name 做子串匹配。
    /// TLD 中 MapDetail GO 通常命名为 "MapDetail_TrappersHomestead" 等。
    /// locID 通常是 "GAMEPLAY_Location_TrappersHomestead" 或 "GAMEPLAY_TrappersHomestead"。
    /// </summary>
    private static bool TryFindMapDetailByName(string locNameLocID, out Vector3 worldPos)
    {
        worldPos = Vector3.zero;
        if (string.IsNullOrEmpty(locNameLocID) || locNameLocID == "?") return false;

        var allDetails = UnityEngine.Object.FindObjectsOfType<MapDetail>();
        if (allDetails == null) return false;

        // 从 locID 中提取有意义的部分(去掉 "GAMEPLAY_Location_" 等前缀)
        string key = locNameLocID;
        if (key.StartsWith("GAMEPLAY_Location_")) key = key.Substring(18);
        else if (key.StartsWith("GAMEPLAY_")) key = key.Substring(9);
        if (string.IsNullOrEmpty(key)) return false;

        for (int i = 0; i < allDetails.Count; i++)
        {
            try
            {
                var md = allDetails[i];
                if (md == null || md.gameObject == null) continue;
                string goName = md.gameObject.name;
                if (string.IsNullOrEmpty(goName)) continue;

                // 宽松匹配: GO 名包含 locID 关键部分,或反过来
                if (goName.IndexOf(key, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    key.IndexOf(goName, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    worldPos = md.transform.position;
                    return true;
                }
            }
            catch { }
        }
        return false;
    }
}
