using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;
using ValheimTooler.Core;

namespace ValheimTooler.Patches
{
    [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.GetAmmoItem))]
    class ArrowOverrideGetAmmo
    {
        private static void Postfix(Humanoid __instance, ref ItemDrop.ItemData __result)
        {
            if (__instance != Player.m_localPlayer)
            {
                return;
            }

            if (PlayerHacks.s_arrowTypeIdx <= 0)
            {
                return;
            }

            if (__result != null)
            {
                return;
            }

            var arrowItem = PlayerHacks.GetSelectedArrowItem();
            if (arrowItem != null)
            {
                __result = arrowItem;
            }
        }
    }

    [HarmonyPatch(typeof(Attack), nameof(Attack.Start))]
    class ArrowOverrideAttackStart
    {
        private static readonly MethodInfo HaveAmmoMethod = AccessTools.DeclaredMethod(
            typeof(Attack), "HaveAmmo", new System.Type[] { typeof(Humanoid), typeof(ItemDrop.ItemData) });

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var replacement = AccessTools.Method(typeof(ArrowOverrideAttackStart), nameof(CheckAmmo));

            foreach (var instruction in instructions)
            {
                if (instruction.Calls(HaveAmmoMethod))
                {
                    yield return new CodeInstruction(OpCodes.Call, replacement);
                }
                else
                {
                    yield return instruction;
                }
            }
        }

        private static bool CheckAmmo(Humanoid character, ItemDrop.ItemData weapon)
        {
            if (character == Player.m_localPlayer && PlayerHacks.s_arrowTypeIdx > 0 && weapon != null && weapon.m_shared.m_ammoType == "arrow")
            {
                return true;
            }

            return (bool)HaveAmmoMethod.Invoke(null, new object[] { character, weapon });
        }
    }
}
