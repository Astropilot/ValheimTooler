using System;
using HarmonyLib;
using ValheimTooler.Core.Extensions;

namespace ValheimTooler.Patches
{
    [HarmonyPatch(typeof(Player), "EdgeOfWorldKill", new Type[]
    {
            typeof(float)
    })]
    class EdgeMapKill
    {
        private static bool Prefix()
        {
            if (Player.m_localPlayer != null && Player.m_localPlayer.VTInGodMode())
            {
                return false;
            }
            return true;
        }
    }
}
