using System;
using HarmonyLib;

namespace ValheimToolerMod.Patches
{
    class FejdStartupPatch
    {
        public static event Action OnGameInitialized;

        [HarmonyPatch(typeof(FejdStartup), "Start")]
        [HarmonyPostfix]
        private static void OnFejdStartup(FejdStartup __instance)
        {
            OnGameInitialized?.Invoke();
        }
    }
}
