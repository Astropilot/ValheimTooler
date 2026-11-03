using HarmonyLib;
using RapidGUI;
using UnityEngine;

namespace ValheimTooler
{
    public class Loader
    {
        public static void Init()
        {
            if (Loader.s_entryPoint == null)
            {
                Debug.Log("[ValheimTooler] Initializing ValheimTooler...");
                try
                {
                    RunPatches();
                    Debug.Log("[ValheimTooler] Harmony patches applied successfully.");
                }
                catch (System.Exception ex)
                {
                    Debug.LogError("[ValheimTooler] Error applying Harmony patches: " + ex);
                }

                Loader.s_entryPoint = new GameObject("ValheimTooler_EntryPoint");
                Loader.s_entryPoint.AddComponent<EntryPoint>();
                Loader.s_entryPoint.AddComponent<RapidGUIBehaviour>();
                Object.DontDestroyOnLoad(Loader.s_entryPoint);
                Debug.Log("[ValheimTooler] EntryPoint GameObject created and set to DontDestroyOnLoad.");
            }
        }

        private static void RunPatches()
        {
            s_harmony.PatchAll();
        }

        public static void Unload()
        {
            _Unload();
        }
        private static void _Unload()
        {
            GameObject.Destroy(Loader.s_entryPoint);

            s_harmony.UnpatchSelf();
        }

        private static GameObject s_entryPoint = null;
        private static readonly Harmony s_harmony = new Harmony("ValheimTooler");
    }

}
