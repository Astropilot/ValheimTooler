using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using ValheimTooler.UI;
using ValheimTooler.Utils;

namespace ValheimTooler.Core
{
    public static class ESP
    {
        private static readonly Color s_playersColor = Color.magenta;
        private static readonly Color s_monstersAndOthersColor = Color.red;
        private static readonly Color s_tamedMonstersColor = new Color(1, 0.3f, 0, 1); // Orange
        private static readonly Color s_pickablesColor = new Color(0.13f, 0.58f, 0.89f, 1); // Light blue
        private static readonly Color s_dropsColor = new Color(0.13f, 0.72f, 0.11f, 1); // Light green
        private static readonly Color s_depositsColor = Color.yellow;

        private static readonly List<Character> s_characters = new List<Character>();
        private static readonly List<Pickable> s_pickables = new List<Pickable>();
        private static readonly List<PickableItem> s_pickableItems = new List<PickableItem>();
        private static readonly List<ItemDrop> s_drops = new List<ItemDrop>();
        private static readonly List<Destructible> s_depositsDestructible = new List<Destructible>();
        private static readonly List<MineRock5> s_mineRock5s = new List<MineRock5>();

        private static float s_updateTimer = 0f;
        private static readonly float s_updateTimerInterval = 1.5f;

        public static bool s_showPlayerESP = false;
        public static bool s_showMonsterESP = false;
        public static bool s_showDroppedESP = false;
        public static bool s_showDepositESP = false;
        public static bool s_showPickableESP = false;
        public static bool s_showLabels = true;
        public static bool s_xray = false;
        public static bool s_show2DBoxes = false;
        public static bool s_show3DBoxes = false;
        public static bool s_fillVisible = true;

        private static readonly Dictionary<Renderer, GameObject> s_xrayOutlines = new Dictionary<Renderer, GameObject>();
        private static readonly Dictionary<Renderer, Renderer> s_xrayVisiblePass = new Dictionary<Renderer, Renderer>();
        private static readonly Dictionary<Transform, bool> s_visibility = new Dictionary<Transform, bool>();
        private static readonly HashSet<Transform> s_xrayTrackedRoots = new HashSet<Transform>();
        private static readonly List<Renderer> s_xrayRemovalBuffer = new List<Renderer>();
        private static float s_cleanupTimer = 0f;
        private static readonly float s_cleanupTimerInterval = 5f;
        private static readonly string s_bundlePath = Path.Combine(BepInEx.Paths.PluginPath, "Shader", "espbundle");
        private static Texture2D s_lineTexture;
        // NEU: Enum für ZTest
        private enum XRayDepthMode
        {
            VisibleLEqual = (int)UnityEngine.Rendering.CompareFunction.LessEqual,
            HiddenGreater = (int)UnityEngine.Rendering.CompareFunction.Greater
        }

        // ALT: private static Material s_xrayBaseMaterial;
        // NEU:
        private static Material s_xrayBaseVisible;
        private static Material s_xrayBaseHidden;

        // ALT: private static readonly Dictionary<Color, Material> s_xrayMaterials = new();
        // NEU: Cache keyed by (Color, DepthMode)
        private static readonly Dictionary<(Color col, XRayDepthMode mode), Material> s_xrayMaterials =
            new Dictionary<(Color col, XRayDepthMode mode), Material>();



        // Flag, ob wir die „gute“ Stencil-Variante nutzen
        private static bool s_useStencil = false;

        // Optional: Pfad zu einem Bundle mit den Shadern (anpassen/leer lassen)
        private static readonly string s_shaderBundlePath = null; // z.B. Path.Combine(BepInEx.Paths.PluginPath, "ValheimTooler", "espbundle");

        // ---- INITIALISIERUNG ----
        static ESP()
        {
            InitXRayMaterials();

            // GUI-Linien-Textur
            s_lineTexture = new Texture2D(1, 1);
            s_lineTexture.SetPixel(0, 0, Color.white);
            s_lineTexture.Apply();
            UnityEngine.Object.DontDestroyOnLoad(s_lineTexture);
        }

        // Fix for "CS0103: Der Name "bundlePath" ist im aktuellen Kontext nicht vorhanden."
        private static void InitXRayMaterials()
        {
            Shader shVisible = null, shHidden = null;

            // 1) Aus AssetBundle laden (wenn vorhanden)
            if (File.Exists(s_bundlePath))
            {
                try
                {
                    var bundle = AssetBundle.LoadFromFile(s_bundlePath);
                    if (bundle == null)
                        Debug.LogWarning("[ESP] Bundle konnte nicht geladen werden: " + s_bundlePath);
                    else
                    {
                        // Assetnamen im Bundle sind i.d.R. Pfade in lower-case.
                        // Wir suchen robust per EndsWith.
                        string visName = bundle.GetAllAssetNames()
                            .FirstOrDefault(n => n.EndsWith("espfillvisible.shader"));
                        string hidName = bundle.GetAllAssetNames()
                            .FirstOrDefault(n => n.EndsWith("espfillhidden.shader"));

                        if (!string.IsNullOrEmpty(visName))
                            shVisible = bundle.LoadAsset<Shader>(visName);
                        if (!string.IsNullOrEmpty(hidName))
                            shHidden = bundle.LoadAsset<Shader>(hidName);

                        // Alternativ (wenn du die genauen Pfade kennst):
                        // shVisible = bundle.LoadAsset<Shader>("assets/shaders/espfillvisible.shader");
                        // shHidden  = bundle.LoadAsset<Shader>("assets/shaders/espfillhidden.shader");

                        bundle.Unload(false); // Shader bleiben resident
                        Debug.Log("[ESP] Shader aus Bundle geladen: vis=" + (shVisible != null) + ", hid=" + (shHidden != null));
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning("[ESP] Bundle-Load Fehler: " + e.Message);
                }
            }
            if (!File.Exists(s_bundlePath))
            {
                Debug.LogError("[ESP] Shader-Bundle fehlt: " + s_bundlePath);
                return;
            }

            // 2) Falls noch null: versuchen, per Shader.Find zu bekommen (wenn sie zufällig schon im Build sind)
            if (shVisible == null)
                shVisible = Shader.Find("Custom/ESPFillVisible");
            if (shHidden == null)
                shHidden = Shader.Find("Custom/ESPFillHidden");

            if (shVisible != null && shHidden != null)
            {
                s_xrayBaseVisible = new Material(shVisible) { enableInstancing = true };
                s_xrayBaseHidden = new Material(shHidden) { enableInstancing = true };
                UnityEngine.Object.DontDestroyOnLoad(s_xrayBaseVisible);
                UnityEngine.Object.DontDestroyOnLoad(s_xrayBaseHidden);
                Debug.Log("[ESP] Verwende Stencil-Shader.");
                return;
            }

            // 3) Letzter Fallback: Unlit/Color
            Shader fb = Shader.Find("Unlit/Color");
            if (fb == null)
                fb = Shader.Find("Hidden/Internal-Colored");
            if (fb == null)
            {
                Debug.LogError("[ESP] Kein geeigneter Shader gefunden – XRay wird deaktiviert.");
                s_xrayBaseVisible = null;
                s_xrayBaseHidden = null;
                s_xray = false;
                return;
            }

            s_xrayBaseVisible = new Material(fb);
            s_xrayBaseVisible.SetInt("_ZWrite", 0);
            s_xrayBaseVisible.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Back);
            s_xrayBaseVisible.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.LessEqual);
            s_xrayBaseVisible.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            s_xrayBaseVisible.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            s_xrayBaseVisible.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

            s_xrayBaseHidden = new Material(s_xrayBaseVisible);
            s_xrayBaseHidden.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Greater);

            s_xrayBaseVisible.enableInstancing = true;
            s_xrayBaseHidden.enableInstancing = true;
            UnityEngine.Object.DontDestroyOnLoad(s_xrayBaseVisible);
            UnityEngine.Object.DontDestroyOnLoad(s_xrayBaseHidden);
            Debug.LogWarning("[ESP] Stencil-Shader nicht gefunden – nutze Fallback ohne Stencil.");
        }







        public static void Start()
        {
            return;
        }

        public static void Update()
        {
            if (Time.time >= s_updateTimer)
            {
                s_characters.Clear();
                s_pickables.Clear();
                s_pickableItems.Clear();
                s_drops.Clear();
                s_depositsDestructible.Clear();
                s_mineRock5s.Clear();
                s_visibility.Clear();

                Camera mainCamera = global::Utils.GetMainCamera();

                if (ESP.s_showPlayerESP || ESP.s_showMonsterESP)
                {
                    List<Character> characters = Character.GetAllCharacters();

                    if (characters != null && mainCamera != null && Player.m_localPlayer != null)
                    {
                        foreach (Character character in characters)
                        {
                            var distance = Vector3.Distance(mainCamera.transform.position, character.transform.position);

                            if (character.IsPlayer() && ((Player)character).GetPlayerID() == Player.m_localPlayer.GetPlayerID())
                            {
                                continue;
                            }

                            bool needVis = !s_xray; // XRay braucht die Sichtbarkeitsinfo nicht

                            // Beispiel:
                            if (distance > 2 && (!ConfigManager.s_espRadiusEnabled.Value || distance < ConfigManager.s_espRadius.Value))
                            {
                                s_characters.Add(character);
                                if (needVis)
                                    s_visibility[character.transform] = HasLineOfSight(character.gameObject);
                            }
                        }
                    }
                }

                if (ESP.s_showPickableESP)
                {
                    var pickables = UnityEngine.Object.FindObjectsOfType<Pickable>();

                    if (pickables != null && mainCamera != null)
                    {
                        foreach (Pickable pickable in pickables)
                        {
                            var distance = Vector3.Distance(mainCamera.transform.position, pickable.transform.position);

                            bool needVis = !s_xray; // XRay braucht die Sichtbarkeitsinfo nicht

                            // Beispiel:
                            if (distance > 2 && (!ConfigManager.s_espRadiusEnabled.Value || distance < ConfigManager.s_espRadius.Value))
                            {
                                s_pickables.Add(pickable);
                                if (needVis)
                                    s_visibility[pickable.transform] = HasLineOfSight(pickable.gameObject);
                            }
                        }
                    }

                    var pickableItems = UnityEngine.Object.FindObjectsOfType<PickableItem>();

                    if (pickableItems != null && mainCamera != null)
                    {
                        foreach (PickableItem pickableItem in pickableItems)
                        {
                            var distance = Vector3.Distance(mainCamera.transform.position, pickableItem.transform.position);

                            // beim Einsammeln von Objekten:
                            bool needVis = !s_xray; // XRay braucht die Sichtbarkeitsinfo nicht

                            // Beispiel:
                            if (distance > 2 && (!ConfigManager.s_espRadiusEnabled.Value || distance < ConfigManager.s_espRadius.Value))
                            {
                                s_pickableItems.Add(pickableItem);
                                if (needVis)
                                    s_visibility[pickableItem.transform] = HasLineOfSight(pickableItem.gameObject);
                            }

                        }
                    }
                }

                if (ESP.s_showDroppedESP)
                {
                    var itemDrops = UnityEngine.Object.FindObjectsOfType<ItemDrop>();

                    if (itemDrops != null && mainCamera != null)
                    {
                        foreach (ItemDrop itemDrop in itemDrops)
                        {
                            var distance = Vector3.Distance(mainCamera.transform.position, itemDrop.transform.position);

                            // beim Einsammeln von Objekten:
                            bool needVis = !s_xray; // XRay braucht die Sichtbarkeitsinfo nicht

                            // Beispiel:
                            if (distance > 2 && (!ConfigManager.s_espRadiusEnabled.Value || distance < ConfigManager.s_espRadius.Value))
                            {
                                s_drops.Add(itemDrop);
                                if (needVis)
                                    s_visibility[itemDrop.transform] = HasLineOfSight(itemDrop.gameObject);
                            }

                        }
                    }
                }

                if (ESP.s_showDepositESP)
                {
                    var mineRock5s = UnityEngine.Object.FindObjectsOfType<MineRock5>();

                    if (mineRock5s != null && mainCamera != null)
                    {
                        foreach (MineRock5 mineRock5 in mineRock5s)
                        {
                            string name = mineRock5.GetHoverText().ToLower();

                            if (name.Contains("rock") || name.Length == 0)
                                continue;

                            var distance = Vector3.Distance(mainCamera.transform.position, mineRock5.transform.position);

                            // beim Einsammeln von Objekten:
                            bool needVis = !s_xray; // XRay braucht die Sichtbarkeitsinfo nicht

                            // Beispiel:
                            if (distance > 2 && (!ConfigManager.s_espRadiusEnabled.Value || distance < ConfigManager.s_espRadius.Value))
                            {
                                s_mineRock5s.Add(mineRock5);
                                if (needVis)
                                    s_visibility[mineRock5.transform] = HasLineOfSight(mineRock5.gameObject);
                            }

                        }
                    }


                    var destructibles = UnityEngine.Object.FindObjectsOfType<Destructible>();

                    if (destructibles != null && mainCamera != null)
                    {
                        foreach (Destructible destructible in destructibles)
                        {
                            HoverText component = destructible.GetComponent<HoverText>();
                            if (component == null)
                                continue;
                            string text = component.m_text.ToLower();

                            if (!text.Contains("deposit") && !text.Contains("piece_mudpile"))
                            {
                                continue;
                            }

                            var distance = Vector3.Distance(mainCamera.transform.position, destructible.transform.position);

                            // beim Einsammeln von Objekten:
                            bool needVis = !s_xray; // XRay braucht die Sichtbarkeitsinfo nicht

                            // Beispiel:
                            if (distance > 2 && (!ConfigManager.s_espRadiusEnabled.Value || distance < ConfigManager.s_espRadius.Value))
                            {
                                s_depositsDestructible.Add(destructible);
                                if (needVis)
                                    s_visibility[destructible.transform] = HasLineOfSight(destructible.gameObject);
                            }

                        }
                    }
                }

                UpdateXRay();
                if (Time.time >= s_cleanupTimer)
                {
                    CleanupXRayOutlines();
                    s_cleanupTimer = Time.time + s_cleanupTimerInterval;
                }
                s_updateTimer = Time.time + s_updateTimerInterval;
            }
        }

        public static void SetXRay(bool enabled)
        {
            s_xray = enabled;

            if (s_xray)
            {
                UpdateXRay();
            }
            else
            {
                ClearAllXRay();
            }
        }

        public static void SetFillVisible(bool enabled)
        {
            if (s_fillVisible == enabled)
                return;
            s_fillVisible = enabled;

            if (s_xray)
            {
                ClearAllXRay();
                UpdateXRay();
            }
        }

        private static bool HasLineOfSight(GameObject obj)
        {
            Camera cam = global::Utils.GetMainCamera();
            if (cam == null || obj == null)
                return false;

            Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
                return true;


            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; ++i)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            Vector3 origin = cam.transform.position;
            Vector3[] samples = new Vector3[5];
            samples[0] = bounds.center;
            samples[1] = bounds.center + new Vector3(bounds.extents.x, bounds.extents.y, bounds.extents.z);
            samples[2] = bounds.center + new Vector3(-bounds.extents.x, bounds.extents.y, bounds.extents.z);
            samples[3] = bounds.center + new Vector3(bounds.extents.x, -bounds.extents.y, bounds.extents.z);
            samples[4] = bounds.center + new Vector3(-bounds.extents.x, -bounds.extents.y, bounds.extents.z);

            foreach (var s in samples)
            {
                Vector3 dir = s - origin;
                if (!Physics.Raycast(origin, dir, out RaycastHit hit, dir.magnitude, ~0, QueryTriggerInteraction.Ignore) || hit.transform.IsChildOf(obj.transform))
                {
                    return true;
                }
            }

            return false;
        }

        private static void UpdateXRay()
        {
            if (!s_xray)
            {
                return;
            }

            foreach (Character c in s_characters)
            {
                if (c == null) continue;

                Color color = c.IsPlayer() ? s_playersColor : (c.IsTamed() ? s_tamedMonstersColor : s_monstersAndOthersColor);
                ApplyXRayOutline(c.gameObject, color);
            }
            foreach (Pickable p in s_pickables)
            {
                if (p == null) continue;
                ApplyXRayOutline(p.gameObject, s_pickablesColor);
            }
            foreach (PickableItem p in s_pickableItems)
            {
                if (p == null) continue;
                ApplyXRayOutline(p.gameObject, s_pickablesColor);
            }
            foreach (ItemDrop d in s_drops)
            {
                if (d == null) continue;
                ApplyXRayOutline(d.gameObject, s_dropsColor);
            }
            foreach (Destructible d in s_depositsDestructible)
            {
                if (d == null) continue;
                ApplyXRayOutline(d.gameObject, s_depositsColor);
            }
            foreach (MineRock5 m in s_mineRock5s)
            {
                if (m == null) continue;
                ApplyXRayOutline(m.gameObject, s_depositsColor);
            }

            if (s_fillVisible && s_xrayVisiblePass.Count > 0)
            {
                foreach (var kvp in s_xrayVisiblePass)
                {
                    if (kvp.Key != null && kvp.Value != null)
                        kvp.Value.enabled = kvp.Key.isVisible;
                }
            }

        }

        private static Material GetXRayMaterial(Color color, XRayDepthMode mode)
        {
            Material material;
            if (!s_xrayMaterials.TryGetValue((color, mode), out material))
            {
                Material baseMat = (mode == XRayDepthMode.VisibleLEqual) ? s_xrayBaseVisible : s_xrayBaseHidden;
                material = new Material(baseMat);
                material.color = color;
                material.renderQueue = baseMat.renderQueue;
                UnityEngine.Object.DontDestroyOnLoad(material);
                s_xrayMaterials[(color, mode)] = material;
            }
            return material;
        }


        private static void ApplyXRayOutline(GameObject obj, Color baseColor)
        {
            if (obj == null || s_xrayBaseVisible == null || s_xrayBaseHidden == null)
                return;

            // Sichtbar nur leicht einfärben
            Color colorVisible = new Color(baseColor.r, baseColor.g, baseColor.b, 0.1f);

            // Verdeckt aufhellen
            float brightenFactor = 0.3f;
            Color colorHidden = Color.Lerp(baseColor, Color.white, brightenFactor);
            colorHidden.a = 0.5f;

            foreach (Renderer renderer in obj.GetComponentsInChildren<Renderer>())
            {
                if (renderer.gameObject.name == "ESP_XRAY")
                    continue;
                if (s_xrayOutlines.ContainsKey(renderer))
                    continue;

                GameObject parent = new GameObject("ESP_XRAY");
                parent.transform.SetParent(renderer.transform, false);
                parent.transform.localPosition = Vector3.zero;
                parent.transform.localRotation = Quaternion.identity;
                parent.transform.localScale = Vector3.one;

                Renderer visChild = null;
                if (s_fillVisible && s_useStencil)
                {
                    // Sichtbar-Pass (leichter Tint)
                    visChild = CreateOverlayChild(parent.transform, renderer,
                        GetXRayMaterial(colorVisible, XRayDepthMode.VisibleLEqual),
                        "ESP_XRAY_VISIBLE");
                    if (visChild != null)
                    {
                        visChild.enabled = renderer.isVisible;
                        s_xrayVisiblePass[renderer] = visChild;
                    }
                }
                if (s_useStencil)
                {
                    // Verdeckt-Pass funktioniert nur mit Stencil korrekt
                    CreateOverlayChild(parent.transform, renderer,
                        GetXRayMaterial(colorHidden, XRayDepthMode.HiddenGreater),
                        "ESP_XRAY_HIDDEN");
                }
                // Verdeckt-Pass (aufgehellt)
                CreateOverlayChild(parent.transform, renderer,
                    GetXRayMaterial(colorHidden, XRayDepthMode.HiddenGreater),
                    "ESP_XRAY_HIDDEN");

                s_xrayOutlines[renderer] = parent;
            }
        }

        private static Renderer CreateOverlayChild(Transform parent, Renderer src, Material mat, string name)
        {
            GameObject g = new GameObject(name);
            g.transform.SetParent(parent, false);

            MeshFilter mf = src.GetComponent<MeshFilter>();
            MeshRenderer mr = src as MeshRenderer;
            SkinnedMeshRenderer smr = src as SkinnedMeshRenderer;

            if (mf != null && mr != null)
            {
                var cFilter = g.AddComponent<MeshFilter>();
                cFilter.sharedMesh = mf.sharedMesh;
                var cRenderer = g.AddComponent<MeshRenderer>();
                cRenderer.material = mat;

                cRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                cRenderer.receiveShadows = false;
                cRenderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
                cRenderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
                cRenderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
                cRenderer.allowOcclusionWhenDynamic = false;
                return cRenderer;
            }
            else if (smr != null)
            {
                var cRenderer = g.AddComponent<SkinnedMeshRenderer>();
                cRenderer.sharedMesh = smr.sharedMesh;
                cRenderer.bones = smr.bones;
                cRenderer.rootBone = smr.rootBone;
                cRenderer.updateWhenOffscreen = false; // **wichtig**
                cRenderer.material = mat;

                cRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                cRenderer.receiveShadows = false;
                cRenderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
                cRenderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
                cRenderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
                cRenderer.allowOcclusionWhenDynamic = false;
                return cRenderer;
            }
            else
            {
                UnityEngine.Object.Destroy(g);
                return null;
            }
        }

        private static void ClearAllXRay()
        {
            foreach (var kvp in s_xrayOutlines)
            {
                if (kvp.Value != null)
                {
                    UnityEngine.Object.Destroy(kvp.Value);
                }
            }
            s_xrayOutlines.Clear();
            s_xrayVisiblePass.Clear();
        }

        private static void CleanupXRayOutlines()
        {
            if (s_xrayOutlines.Count == 0)
                return;

            s_xrayTrackedRoots.Clear();
            foreach (var c in s_characters) if (c != null) s_xrayTrackedRoots.Add(c.transform.root);
            foreach (var p in s_pickables) if (p != null) s_xrayTrackedRoots.Add(p.transform.root);
            foreach (var p in s_pickableItems) if (p != null) s_xrayTrackedRoots.Add(p.transform.root);
            foreach (var d in s_drops) if (d != null) s_xrayTrackedRoots.Add(d.transform.root);
            foreach (var d in s_depositsDestructible) if (d != null) s_xrayTrackedRoots.Add(d.transform.root);
            foreach (var m in s_mineRock5s) if (m != null) s_xrayTrackedRoots.Add(m.transform.root);

            s_xrayRemovalBuffer.Clear();
            foreach (var kvp in s_xrayOutlines)
            {
                if (kvp.Key == null || !s_xrayTrackedRoots.Contains(kvp.Key.transform.root))
                    s_xrayRemovalBuffer.Add(kvp.Key);
            }

            foreach (var renderer in s_xrayRemovalBuffer)
            {
                if (s_xrayOutlines[renderer] != null)
                    UnityEngine.Object.Destroy(s_xrayOutlines[renderer]);
                s_xrayOutlines.Remove(renderer);
                s_xrayVisiblePass.Remove(renderer);
            }
        }

        public static void DisplayGUI()
        {
            if (!s_showLabels && !s_show2DBoxes && !s_show3DBoxes)
            {
                return;
            }

            Camera mainCamera = global::Utils.GetMainCamera();

            if (mainCamera != null && Player.m_localPlayer != null)
            {
                var main = mainCamera;
                var labelSkin = new GUIStyle(InterfaceMaker.CustomSkin.label)
                {
                    alignment = TextAnchor.UpperCenter
                };

                if (ESP.s_showPlayerESP || ESP.s_showMonsterESP)
                {
                    foreach (Character character in s_characters)
                    {
                        if (character == null || (!ESP.s_showMonsterESP && !character.IsPlayer()))
                        {
                            continue;
                        }
                        if (!s_xray && s_visibility.TryGetValue(character.transform, out bool visChar) && !visChar)
                        {
                            continue;
                        }
                        Vector3 vector = main.WorldToScreenPointScaled(character.transform.position);

                        if (vector.z > -1)
                        {
                            if (character.IsPlayer() && ESP.s_showPlayerESP)
                            {
                                string espLabel = ((Player)character).GetPlayerName() + $" [{(int)vector.z}]";
                                Color color = s_playersColor;
                                if (s_showLabels)
                                {
                                    labelSkin.normal.textColor = color;
                                    DrawCenteredLabel(vector, espLabel, labelSkin);
                                }
                                DrawESPBoxes(character.gameObject, main, color);
                            }
                            else if (!character.IsPlayer() && ESP.s_showMonsterESP)
                            {
                                string espLabel = character.GetHoverName() + $" [{(int)vector.z}]";
                                Color color = character.IsTamed() ? s_tamedMonstersColor : s_monstersAndOthersColor;
                                if (s_showLabels)
                                {
                                    labelSkin.normal.textColor = color;
                                    DrawCenteredLabel(vector, espLabel, labelSkin);
                                }
                                DrawESPBoxes(character.gameObject, main, color);
                            }
                        }
                    }
                }

                if (ESP.s_showPickableESP)
                {
                    Color color = s_pickablesColor;
                    foreach (Pickable pickable in s_pickables)
                    {
                        if (pickable == null)
                        {
                            continue;
                        }
                        if (!s_xray && s_visibility.TryGetValue(pickable.transform, out bool visPickable) && !visPickable)
                        {
                            continue;
                        }
                        Vector3 vector = main.WorldToScreenPointScaled(pickable.transform.position);

                        if (vector.z > -1)
                        {
                            string espLabel = $"{Localization.instance.Localize(pickable.GetHoverName())} [{(int)vector.z}]";

                            if (s_showLabels)
                            {
                                labelSkin.normal.textColor = color;
                                DrawCenteredLabel(vector, espLabel, labelSkin);
                            }
                            DrawESPBoxes(pickable.gameObject, main, color);
                        }
                    }
                    foreach (PickableItem pickableItem in s_pickableItems)
                    {
                        if (pickableItem == null)
                        {
                            continue;
                        }
                        if (!s_xray && s_visibility.TryGetValue(pickableItem.transform, out bool visPickableItem) && !visPickableItem)
                        {
                            continue;
                        }
                        Vector3 vector = main.WorldToScreenPointScaled(pickableItem.transform.position);

                        if (vector.z > -1)
                        {
                            string espLabel = $"{Localization.instance.Localize(pickableItem.GetHoverName())} [{(int)vector.z}]";

                            if (s_showLabels)
                            {
                                labelSkin.normal.textColor = color;
                                DrawCenteredLabel(vector, espLabel, labelSkin);
                            }
                            DrawESPBoxes(pickableItem.gameObject, main, color);
                        }
                    }
                }

                if (ESP.s_showDroppedESP)
                {
                    Color color = s_dropsColor;
                    foreach (ItemDrop itemDrop in s_drops)
                    {
                        if (itemDrop == null)
                        {
                            continue;
                        }
                        if (!s_xray && s_visibility.TryGetValue(itemDrop.transform, out bool visDrop) && !visDrop)
                        {
                            continue;
                        }
                        Vector3 vector = main.WorldToScreenPointScaled(itemDrop.transform.position);

                        if (vector.z > -1)
                        {
                            string espLabel = $"{Localization.instance.Localize(itemDrop.GetHoverName())} [{(int)vector.z}]";

                            if (s_showLabels)
                            {
                                labelSkin.normal.textColor = color;
                                DrawCenteredLabel(vector, espLabel, labelSkin);
                            }
                            DrawESPBoxes(itemDrop.gameObject, main, color);
                        }
                    }
                }

                if (ESP.s_showDepositESP)
                {
                    Color color = s_depositsColor;

                    foreach (Destructible depositDestructible in s_depositsDestructible)
                    {
                        if (depositDestructible == null)
                        {
                            continue;
                        }
                        if (!s_xray && s_visibility.TryGetValue(depositDestructible.transform, out bool visDeposit) && !visDeposit)
                        {
                            continue;
                        }
                        Vector3 vector = main.WorldToScreenPointScaled(depositDestructible.transform.position);

                        if (vector.z > -1)
                        {
                            string name = depositDestructible.GetComponent<HoverText>().GetHoverName();
                            string espLabel = $"{name} [{(int)vector.z}]";

                            if (s_showLabels)
                            {
                                labelSkin.normal.textColor = color;
                                DrawCenteredLabel(vector, espLabel, labelSkin);
                            }
                            DrawESPBoxes(depositDestructible.gameObject, main, color);
                        }
                    }

                    foreach (MineRock5 mineRock5 in s_mineRock5s)
                    {
                        if (mineRock5 == null)
                        {
                            continue;
                        }
                        if (!s_xray && s_visibility.TryGetValue(mineRock5.transform, out bool visMine) && !visMine)
                        {
                            continue;
                        }
                        Vector3 vector = main.WorldToScreenPointScaled(mineRock5.transform.position);

                        if (vector.z > -1)
                        {
                            string espLabel = $"{mineRock5.GetHoverText()} [{(int)vector.z}]";

                            if (s_showLabels)
                            {
                                labelSkin.normal.textColor = color;
                                DrawCenteredLabel(vector, espLabel, labelSkin);
                            }
                            DrawESPBoxes(mineRock5.gameObject, main, color);
                        }
                    }
                }
            }
        }

        private static void DrawCenteredLabel(Vector3 screenPos, string text, GUIStyle style)
        {
            Vector2 size = style.CalcSize(new GUIContent(text));
            GUI.Label(new Rect(screenPos.x - size.x / 2f, Screen.height - screenPos.y - 5f, size.x, size.y), text, style);
        }

        private static void DrawESPBoxes(GameObject obj, Camera cam, Color color)
        {
            if (!s_show2DBoxes && !s_show3DBoxes)
            {
                return;
            }

            if (s_show2DBoxes)
            {
                Draw2DBox(obj, cam, color);
            }
            if (s_show3DBoxes)
            {
                Draw3DBox(obj, cam, color);
            }
        }
        
        private static Bounds? GetObjectBounds(GameObject obj)
        {
            Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
            if (renderers.Length > 0)
            {
                Bounds bounds = renderers[0].bounds;
                for (int i = 1; i < renderers.Length; i++)
                {
                    bounds.Encapsulate(renderers[i].bounds);
                }
                return bounds;
            }
            Collider[] colliders = obj.GetComponentsInChildren<Collider>();
            if (colliders.Length > 0)
            {
                Bounds bounds = colliders[0].bounds;
                for (int i = 1; i < colliders.Length; i++)
                {
                    bounds.Encapsulate(colliders[i].bounds);
                }
                return bounds;
            }
            return null;
        }

        private static void Draw2DBox(GameObject obj, Camera cam, Color color)
        {
            Bounds? maybeBounds = GetObjectBounds(obj);
            if (maybeBounds == null)
            {
                return;
            }
            Bounds bounds = maybeBounds.Value;
            Vector3 center = bounds.center;
            Vector3 ext = bounds.extents;
            Vector3[] world = new Vector3[8];
            world[0] = center + new Vector3(-ext.x, -ext.y, -ext.z);
            world[1] = center + new Vector3(ext.x, -ext.y, -ext.z);
            world[2] = center + new Vector3(ext.x, -ext.y, ext.z);
            world[3] = center + new Vector3(-ext.x, -ext.y, ext.z);
            world[4] = center + new Vector3(-ext.x, ext.y, -ext.z);
            world[5] = center + new Vector3(ext.x, ext.y, -ext.z);
            world[6] = center + new Vector3(ext.x, ext.y, ext.z);
            world[7] = center + new Vector3(-ext.x, ext.y, ext.z);

            Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 max = new Vector2(float.MinValue, float.MinValue);
            for (int i = 0; i < 8; i++)
            {
                Vector3 sp = cam.WorldToScreenPoint(world[i]);
                if (sp.z <= 0f)
                {
                    return;
                }
                Vector2 pt = new Vector2(sp.x, Screen.height - sp.y);
                min = Vector2.Min(min, pt);
                max = Vector2.Max(max, pt);
            }

            Rect rect = Rect.MinMaxRect(min.x, min.y, max.x, max.y);
            DrawRectangle(rect, color);
        }

        private static void Draw3DBox(GameObject obj, Camera cam, Color color)
        {
            Bounds? maybeBounds = GetObjectBounds(obj);
            if (maybeBounds == null)
            {
                return;
            }
            Bounds bounds = maybeBounds.Value;
            Vector3 center = bounds.center;
            Vector3 ext = bounds.extents;
            Quaternion rot = obj.transform.rotation;
            Vector3[] world = new Vector3[8];
            world[0] = center + rot * new Vector3(-ext.x, -ext.y, -ext.z);
            world[1] = center + rot * new Vector3(ext.x, -ext.y, -ext.z);
            world[2] = center + rot * new Vector3(ext.x, -ext.y, ext.z);
            world[3] = center + rot * new Vector3(-ext.x, -ext.y, ext.z);
            world[4] = center + rot * new Vector3(-ext.x, ext.y, -ext.z);
            world[5] = center + rot * new Vector3(ext.x, ext.y, -ext.z);
            world[6] = center + rot * new Vector3(ext.x, ext.y, ext.z);
            world[7] = center + rot * new Vector3(-ext.x, ext.y, ext.z);

            Vector2[] pts = new Vector2[8];
            for (int i = 0; i < 8; i++)
            {
                Vector3 sp = cam.WorldToScreenPoint(world[i]);
                if (sp.z <= 0f)
                {
                    return;
                }
                pts[i] = new Vector2(sp.x, Screen.height - sp.y);
            }

            int[,] edges = new int[,]
            {
                {0,1},{1,2},{2,3},{3,0},
                {4,5},{5,6},{6,7},{7,4},
                {0,4},{1,5},{2,6},{3,7}
            };

            for (int i = 0; i < edges.GetLength(0); i++)
            {
                DrawLine(pts[edges[i,0]], pts[edges[i,1]], color);
            }
        }

        private static void DrawRectangle(Rect rect, Color color)
        {
            DrawLine(new Vector2(rect.xMin, rect.yMin), new Vector2(rect.xMax, rect.yMin), color);
            DrawLine(new Vector2(rect.xMax, rect.yMin), new Vector2(rect.xMax, rect.yMax), color);
            DrawLine(new Vector2(rect.xMax, rect.yMax), new Vector2(rect.xMin, rect.yMax), color);
            DrawLine(new Vector2(rect.xMin, rect.yMax), new Vector2(rect.xMin, rect.yMin), color);
        }

        private static void DrawLine(Vector2 a, Vector2 b, Color color, float width = 1f)
        {
            Matrix4x4 matrix = GUI.matrix;
            Color oldColor = GUI.color;
            GUI.color = color;

            float angle = Mathf.Atan2(b.y - a.y, b.x - a.x) * Mathf.Rad2Deg;
            float length = Vector2.Distance(a, b);
            GUIUtility.RotateAroundPivot(angle, a);
            GUI.DrawTexture(new Rect(a.x, a.y, length, width), s_lineTexture);
            GUI.matrix = matrix;
            GUI.color = oldColor;
        }
        


    }
}
