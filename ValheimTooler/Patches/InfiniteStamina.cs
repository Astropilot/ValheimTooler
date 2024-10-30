using System;
using HarmonyLib;
using ValheimTooler.Core;

namespace ValheimTooler.Patches
{
    [HarmonyPatch(typeof(Player), nameof(Player.UseStamina), new Type[]
    {
        typeof(float)
    })]
    class InfiniteStamina
    {
        private static void Prefix(ref Player __instance, ref float v)
        {
            if (PlayerHacks.s_isInfiniteStaminaMe && Player.m_localPlayer != null && __instance.GetPlayerID() == Player.m_localPlayer.GetPlayerID())
            {
                v = 0f;
            }
        }
    }
}
