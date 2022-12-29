using System;
using HarmonyLib;
using ValheimTooler.Utils;

namespace ValheimTooler.Patches
{
    class NoPlacementCostRepair
    {
        [HarmonyPatch(typeof(InventoryGui), "CanRepair", new Type[] { typeof(ItemDrop.ItemData) })]
        public class CanRepairPatch
        {
            private static bool Prefix(ref InventoryGui __instance, ref bool __result, ItemDrop.ItemData item)
            {
                if (Player.m_localPlayer == null)
                {
                    return true;
                }
                var m_noPlacementCost = Player.m_localPlayer.GetFieldValue<bool>("m_noPlacementCost");
                if (m_noPlacementCost)
                {
                    __result = m_noPlacementCost;
                    return false;
                }
                return true;
            }
        }
    }
}
