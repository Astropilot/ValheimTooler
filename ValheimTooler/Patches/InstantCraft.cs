using System;
using HarmonyLib;
using ValheimTooler.Core;

namespace ValheimTooler.Patches
{
    [HarmonyPatch(typeof(InventoryGui), "UpdateRecipe", new Type[]
    {
        typeof(Player),
        typeof(float)
    })]
    class InstantCraft
    {
        private static void Prefix(ref Player player, ref float dt)
        {
            if (PlayerHacks.s_instantCraft)
            {
                dt = 2f;
            }
        }
    }
}
