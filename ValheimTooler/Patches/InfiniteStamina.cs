using System;
using HarmonyLib;
using ValheimTooler.Core;

namespace ValheimTooler.Patches
{
    [HarmonyPatch(typeof(Player), nameof(Player.UseStamina), new Type[]
    {
        typeof(float), typeof(bool)
    })]
    class InfiniteStamina
    {
        private static void Prefix(ref float v)
        {
            if (PlayerHacks.s_isInfiniteStaminaMe)
            {
                v = 0f;
            }
        }
    }
}
