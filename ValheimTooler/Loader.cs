using HarmonyLib;
using RapidGUI;
using UnityEngine;

namespace ValheimTooler
{
    class Loader
    {
        public static void Init()
        {
            if (Loader.s_entryPoint == null)
            {
                RunPatches();
                Loader.s_entryPoint = new GameObject();
                Loader.s_entryPoint.AddComponent<EntryPoint>();
                Loader.s_entryPoint.AddComponent<RapidGUIBehaviour>();
                Object.DontDestroyOnLoad(Loader.s_entryPoint);
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
