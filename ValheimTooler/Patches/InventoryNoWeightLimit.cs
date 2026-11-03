using HarmonyLib;
using ValheimTooler.Core;

namespace ValheimTooler.Patches
{
    [HarmonyPatch(typeof(Inventory), nameof(Inventory.GetTotalWeight))]
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

    [HarmonyPatch(typeof(Player), nameof(Player.IsEncumbered))]
    class PlayerIsEncumbered
    {
        private static bool Prefix(ref bool __result)
        {
            if (PlayerHacks.s_inventoryNoWeightLimit)
            {
                __result = false;
                return false;
            }
            return true;
        }
    }
}
