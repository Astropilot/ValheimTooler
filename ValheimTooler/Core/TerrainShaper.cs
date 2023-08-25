using System;
using System.Collections.Generic;
using System.Linq;
using RapidGUI;
using UnityEngine;
using ValheimTooler.Utils;

namespace ValheimTooler.Core
{
    public static class TerrainShaper
    {
        private static float s_radius = 10f;
        private static float s_depth = 1f;
        private static float s_strength = 0.01f;
        private static TerrainModifier.PaintType s_paintType = TerrainModifier.PaintType.Dirt;

        public static void Start()
        {
            return;
        }

        public static void Update()
        {
            return;
        }

        public static void DisplayGUI()
        {
            GUILayout.BeginVertical(VTLocalization.instance.Localize("$vt_terrainshaper_settings"), GUI.skin.box, GUILayout.ExpandWidth(false));
            {
                GUILayout.Space(EntryPoint.s_boxSpacing);
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label(VTLocalization.instance.Localize("$vt_terrainshaper_radius (") + s_radius.ToString("F1") + ")", GUILayout.ExpandWidth(false));
                    s_radius = GUILayout.HorizontalSlider(s_radius, 1f, 50f, GUILayout.ExpandWidth(true));
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label(VTLocalization.instance.Localize("$vt_terrainshaper_depth (") + s_depth.ToString("F2") + ")", GUILayout.ExpandWidth(false));
                    s_depth = GUILayout.HorizontalSlider(s_depth, 1f, 10f, GUILayout.ExpandWidth(true));
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label(VTLocalization.instance.Localize("$vt_terrainshaper_strength (") + s_strength.ToString("F2") + ")", GUILayout.ExpandWidth(false));
                    s_strength = GUILayout.HorizontalSlider(s_strength, 0.001f, 0.1f, GUILayout.ExpandWidth(true));
                }
                GUILayout.EndHorizontal();

                if (GUILayout.Button(VTLocalization.instance.Localize("$vt_terrainshaper_shape : " + (Ground.square ? VTLocalization.instance.Localize("$vt_terrainshaper_shape_square") : VTLocalization.instance.Localize("$vt_terrainshaper_shape_circle")))))
                {
                    Ground.square = !Ground.square;
                }
            }
            GUILayout.EndVertical();

            GUILayout.BeginVertical(VTLocalization.instance.Localize("$vt_terrainshaper_actions"), GUI.skin.box, GUILayout.ExpandWidth(false));
            {
                GUILayout.Space(EntryPoint.s_boxSpacing);

                if (GUILayout.Button(VTLocalization.instance.Localize("$vt_terrainshaper_action_level")))
                {
                    if (Player.m_localPlayer != null)
                    {
                        Ground.Level(Player.m_localPlayer.transform.position, s_radius);
                    }
                }
                if (GUILayout.Button(VTLocalization.instance.Localize("$vt_terrainshaper_action_lower")))
                {
                    if (Player.m_localPlayer != null)
                    {
                        Ground.Lower(Player.m_localPlayer.transform.position, s_radius, s_depth, s_strength);
                    }
                }
                if (GUILayout.Button(VTLocalization.instance.Localize("$vt_terrainshaper_action_raise")))
                {
                    if (Player.m_localPlayer != null)
                    {
                        Ground.Raise(Player.m_localPlayer.transform.position, s_radius, s_depth, s_strength);
                    }
                }
                if (GUILayout.Button(VTLocalization.instance.Localize("$vt_terrainshaper_action_reset")))
                {
                    if (Player.m_localPlayer != null)
                    {
                        Ground.Reset(Player.m_localPlayer.transform.position, s_radius);
                    }
                }
                if (GUILayout.Button(VTLocalization.instance.Localize("$vt_terrainshaper_action_smooth")))
                {
                    if (Player.m_localPlayer != null)
                    {
                        Ground.Smooth(Player.m_localPlayer.transform.position, s_radius, s_strength);
                    }
                }
            }
            GUILayout.EndVertical();

            GUILayout.BeginVertical(VTLocalization.instance.Localize("$vt_terrainshaper_painter"), GUI.skin.box, GUILayout.ExpandWidth(false));
            {
                GUILayout.Space(EntryPoint.s_boxSpacing);

                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label(VTLocalization.instance.Localize("$vt_terrainshaper_paint_type :"), GUILayout.ExpandWidth(false));

                    var enumValues = Enum.GetValues(typeof(TerrainModifier.PaintType)).Cast<object>().ToList();
                    var idx = enumValues.IndexOf(s_paintType);
                    var valueNames = enumValues.Select(value => value.ToString()).ToArray();

                    idx = RGUI.SelectionPopup(idx, valueNames);

                    s_paintType = (TerrainModifier.PaintType)enumValues.ElementAtOrDefault(idx);
                }
                GUILayout.EndHorizontal();

                if (GUILayout.Button(VTLocalization.instance.Localize("$vt_terrainshaper_action_paint")))
                {
                    if (Player.m_localPlayer != null)
                    {
                        Ground.Paint(Player.m_localPlayer.transform.position, s_radius, s_paintType);
                    }
                }
            }
            GUILayout.EndVertical();
        }
    }

    // Class originally created by Gungnir mod: https://github.com/zambony/Gungnir
    internal static class Ground
    {
        internal enum Operation
        {
            None,
            Level,
            Raise,
            Lower,
            Smooth,
            Paint
        }

        public static bool square = false;

        public static void Level(Vector3 position, float radius)
        {
            GameObject prefab = ResourceUtils.GetHiddenPrefab("mud_road_v2");

            Make(prefab, position, radius, Operation.Level);
        }

        public static void Raise(Vector3 position, float radius, float height, float strength = 0.01f)
        {
            GameObject prefab = ResourceUtils.GetHiddenPrefab("raise_v2");

            Make(prefab, position, radius, Operation.Raise, height, strength);
        }

        public static void Lower(Vector3 position, float radius, float depth, float strength = 0.01f)
        {
            GameObject prefab = ResourceUtils.GetHiddenPrefab("digg_v3");

            Make(prefab, position, radius, Operation.Lower, depth, strength);
        }

        public static void Smooth(Vector3 position, float radius, float strength = 0.5f)
        {
            GameObject prefab = ResourceUtils.GetHiddenPrefab("mud_road_v2");

            Make(prefab, position, radius, Operation.Smooth, strength);
        }

        public static void Paint(Vector3 position, float radius, TerrainModifier.PaintType type)
        {
            GameObject prefab = null;

            switch (type)
            {
                case TerrainModifier.PaintType.Dirt:
                    prefab = ResourceUtils.GetHiddenPrefab("mud_road_v2");
                    break;
                case TerrainModifier.PaintType.Paved:
                    prefab = ResourceUtils.GetHiddenPrefab("paved_road_v2");
                    break;
                case TerrainModifier.PaintType.Reset:
                    prefab = ResourceUtils.GetHiddenPrefab("replant_v2");
                    break;
                case TerrainModifier.PaintType.Cultivate:
                    prefab = ResourceUtils.GetHiddenPrefab("cultivate_v2");
                    break;
                default:
                    return;
            }

            Make(prefab, position, radius, Operation.Paint);
        }

        public static void Reset(Vector3 position, float radius)
        {
            // Remove all Terrain V2 edits. These are typically done by mods,
            // or are present in very old save files that haven't run the "optterrain" console command.
            foreach (var obj in TerrainModifier.GetAllInstances())
            {
                if (global::Utils.DistanceXZ(position, obj.transform.position) <= radius)
                {
                    ZNetView netView = obj.GetComponent<ZNetView>();

                    if (netView == null)
                        continue;

                    netView.ClaimOwnership();
                    netView.Destroy();
                }
            }

            List<Heightmap> heightmaps = new List<Heightmap>();
            Heightmap.FindHeightmap(position, radius + 50f, heightmaps);

            bool resetGrass = false;

            foreach (Heightmap heightmap in heightmaps)
            {
                bool modified = false;
                TerrainComp compiler = TerrainComp.FindTerrainCompiler(heightmap.transform.position);

                if (compiler == null)
                    continue;

                if (!compiler.GetFieldValue<bool>("m_initialized"))
                    continue;

                heightmap.WorldToVertex(position, out int x, out int y);

                int width = compiler.GetFieldValue<int>("m_width");
                float[] levelDelta = compiler.GetFieldValue<float[]>("m_levelDelta");
                float[] smoothDelta = compiler.GetFieldValue<float[]>("m_smoothDelta");
                bool[] modifiedHeight = compiler.GetFieldValue<bool[]>("m_modifiedHeight");
                Color[] paintMask = compiler.GetFieldValue<Color[]>("m_paintMask");
                bool[] modifiedPaint = compiler.GetFieldValue<bool[]>("m_modifiedPaint");

                for (int h = 0; h <= width; ++h)
                {
                    for (int w = 0; w <= width; ++w)
                    {
                        if (Distance2D(x, y, w, h) > radius)
                            continue;

                        int heightIndex = w + (h * (width + 1));

                        if (modifiedHeight[heightIndex])
                        {
                            modifiedHeight[heightIndex] = false;
                            levelDelta[heightIndex] = 0;
                            smoothDelta[heightIndex] = 0;
                            modified = true;
                            resetGrass = true;
                        }

                        if (h < width && w < width)
                        {
                            int paintIndex = w + (h * width);

                            if (modifiedPaint[paintIndex])
                            {
                                modifiedPaint[paintIndex] = false;
                                paintMask[paintIndex] = Color.clear;
                                modified = true;
                                resetGrass = true;
                            }
                        }
                    }
                }

                if (!modified)
                    continue;

                compiler.SetFieldValue("m_levelDelta", levelDelta);
                compiler.SetFieldValue("m_smoothDelta", smoothDelta);
                compiler.SetFieldValue("m_modifiedHeight", modifiedHeight);
                compiler.SetFieldValue("m_paintMask", paintMask);
                compiler.SetFieldValue("m_modifiedPaint", modifiedPaint);

                compiler.CallMethod("Save");
                heightmap.Poke(true);

                // Push new terrain data to all clients.
                var zdo = compiler.GetComponent<ZNetView>().GetZDO();
                ZDOMan.instance.ForceSendZDO(zdo.m_uid);
            }

            if (ClutterSystem.instance != null && resetGrass)
                ClutterSystem.instance.ResetGrass(position, radius);
        }

        private static void Make(GameObject prefab, Vector3 position, float radius, Operation op, params object[] args)
        {
            // This might stop some of the particle spam when making terrain mods.
            TerrainModifier.SetTriggerOnPlaced(false);
            bool wasActive = prefab.activeSelf || prefab.activeInHierarchy;
            // All the terrain prefabs we're using apply terrain modifications the moment they spawn.
            // We want to modify the operation before it's applied, so disabling the object before instantiating it
            // allows us to delay the Awake() method.
            prefab.SetActive(false);
            TerrainOp mod = prefab.GetComponentInChildren<TerrainOp>();

            // Some ground pieces spawn with an offset. Account for it.
            float levelOffset = mod.m_settings.m_levelOffset;

            GameObject spawned = UnityEngine.Object.Instantiate(prefab, position - Vector3.up * levelOffset, Quaternion.identity);
            // Restore the original prefab to its active state since we disabled it before making a clone.
            prefab.SetActive(wasActive);

            mod = spawned.GetComponentInChildren<TerrainOp>();

            // Delete all the smoke/rock/dirt particle effects on spawn.
            mod.m_onPlacedEffect = new EffectList();

            // Modify terrain settings.
            mod.m_settings.m_square = square;

            if (op == Operation.Level)
            {
                mod.m_settings.m_level = true;
                mod.m_settings.m_levelRadius = radius;
                mod.m_settings.m_levelOffset = 0f;
                mod.m_settings.m_smooth = false;
            }
            else if (op == Operation.Raise || op == Operation.Lower)
            {
                mod.m_settings.m_raise = true;
                mod.m_settings.m_raiseRadius = radius;
                mod.m_settings.m_raiseDelta = (float)args[0];
                mod.m_settings.m_raisePower = (float)args[1];

                if (op == Operation.Lower)
                    mod.m_settings.m_raiseDelta *= -1f;
            }
            else if (op == Operation.Smooth)
            {
                mod.m_settings.m_level = false;
                mod.m_settings.m_smooth = true;
                mod.m_settings.m_smoothPower = (float)args[0];
            }
            else if (op == Operation.Paint)
            {
                mod.m_settings.m_level = false;
                mod.m_settings.m_smooth = false;
                mod.m_settings.m_raise = false;
            }

            mod.m_settings.m_paintRadius = radius;
            mod.m_settings.m_smoothRadius = radius;

            // All done, let Unity call Awake() and stuff.
            spawned.SetActive(true);
        }

        public static float Distance2D(float x1, float y1, float x2, float y2)
        {
            float a = x2 - x1;
            float b = y2 - y1;
            return Mathf.Sqrt(a * a + b * b);
        }
    }
}
