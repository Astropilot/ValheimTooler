using System;
using HarmonyLib;

namespace ValheimTooler.Patches
{
    class DisableInputWhenInterfaceIsShowed
    {
        [HarmonyPatch(typeof(Player), "TakeInput")]
        class PlayerTakeInput
        {
            private static bool Prefix(ref bool __result)
            {
                if (EntryPoint.s_showMainWindow)
                {
                    __result = false;
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(PlayerController), "InInventoryEtc")]
        class PlayerControllerInInventoryEtc
        {
            private static bool Prefix(ref bool __result)
            {
                if (EntryPoint.s_showMainWindow)
                {
                    __result = true;
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(InventoryGrid), "OnLeftClick", new Type[]
        {
                typeof(UIInputHandler)
        })]
        class InventoryGridOnLeftClick
        {
            private static bool Prefix()
            {
                if (EntryPoint.s_showMainWindow)
                {
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(InventoryGrid), "OnRightClick", new Type[]
        {
                typeof(UIInputHandler)
        })]
        class InventoryGridOnRightClick
        {
            private static bool Prefix()
            {
                if (EntryPoint.s_showMainWindow)
                {
                    return false;
                }
                return true;
            }
        }
    }
}
