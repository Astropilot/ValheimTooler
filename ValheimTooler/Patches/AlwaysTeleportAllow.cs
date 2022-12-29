using HarmonyLib;
using ValheimTooler.Core;

namespace ValheimTooler.Patches
{
    [HarmonyPatch(typeof(Inventory), "IsTeleportable")]
    class AlwaysTeleportAllow
    {
        private static bool Prefix(ref bool __result)
        {
            if (PlayerHacks.s_bypassRestrictedTeleportable)
            {
                __result = true;
                return false;
            }
            return true;
        }
    }
}
