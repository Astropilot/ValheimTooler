using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
