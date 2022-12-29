using HarmonyLib;
using ValheimTooler.Core;

namespace ValheimTooler.Patches
{
    [HarmonyPatch(typeof(Inventory), "GetTotalWeight")]
    class InventoryNoWeightLimit
    {
        private static bool Prefix(ref float __result)
        {
            if (PlayerHacks.s_inventoryNoWeightLimit)
            {
                __result = 0f;
                return false;
            }
            return true;
        }
    }
}
