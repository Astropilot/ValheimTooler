using System;
using HarmonyLib;
using ValheimTooler.Core;
using ValheimTooler.Utils;

namespace ValheimTooler.Patches
{
    [HarmonyPatch(typeof(Player), "UpdateAttackBowDraw")]
    class InstantBow
    {
        private static void Postfix(Player __instance)
        {
            if (!PlayerHacks.s_instantBow || __instance != Player.m_localPlayer)
            {
                return;
            }

            bool attackHold = __instance.GetFieldValue<bool>("m_attackHold");

            Attack currentAttack = __instance.GetFieldValue<Attack>("m_currentAttack");
            if (attackHold && currentAttack != null && currentAttack.m_bowDraw)
            {
                __instance.SetFieldValue("m_attackDrawTime", currentAttack.m_drawDurationMin);
                __instance.SetFieldValue("m_attackDrawPercentage", 1f);
            }
        }
    }
}

