using System;
using System.Linq;
using HarmonyLib;
using ValheimTooler.Core;
using ValheimTooler.Models.Mono;

namespace ValheimTooler.Patches
{
    [HarmonyPatch(typeof(Destructible), "Start")]
    class AutoPinResources
    {
        private static Random s_random = new Random();
        private const string Chars = "0123456789";
        private static void Postfix(ref Destructible __instance)
        {
            if (!MiscHacks.s_enableAutopinMap)
                return;
            HoverText component = __instance.GetComponent<HoverText>();
            if (component)
            {
                if (__instance.gameObject.GetComponent<PinnedObject>() != null)
                    return;

                string text = component.m_text.ToLower();

                if (!text.Contains("deposit"))
                    return;

                string random_nounce = new string(Enumerable.Repeat(Chars, 5).Select(s => s[s_random.Next(s.Length)]).ToArray());
                string name = component.GetHoverName() + " [VT" + random_nounce + "]";

                __instance.gameObject.AddComponent<PinnedObject>().Init(name);
                ZLog.Log($"Pin candidate: {name}");
            }
        }
    }
}
