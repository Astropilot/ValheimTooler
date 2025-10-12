using System;
using UnityEngine;
using ValheimTooler.Utils;
using RapidGUI;

namespace ValheimTooler.Core
{
    public static class TeleportHacks
    {
        private static string s_radiusText = "10";
        private static readonly string[] s_entityTypes = { "", "Character", "Item" };
        private static int s_entityTypeIdx = 0;

        public static void Start()
        {
        }

        public static void Update()
        {
        }

        public static void DisplayGUI()
        {
            GUILayout.BeginVertical(VTLocalization.instance.Localize("$vt_teleport_title"), GUI.skin.box, GUILayout.ExpandWidth(false));
            {
                GUILayout.Space(EntryPoint.s_boxSpacing);

                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label(VTLocalization.instance.Localize("$vt_teleport_radius") + " :", GUILayout.ExpandWidth(false));
                    s_radiusText = GUILayout.TextField(s_radiusText, GUILayout.ExpandWidth(true));
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label(VTLocalization.instance.Localize("$vt_teleport_type") + " :", GUILayout.ExpandWidth(false));
                    s_entityTypeIdx = RGUI.SelectionPopup(s_entityTypeIdx, s_entityTypes);
                }
                GUILayout.EndHorizontal();

                if (GUILayout.Button(VTLocalization.instance.Localize("$vt_teleport_button")))
                {
                    if (float.TryParse(s_radiusText, out float radius) && radius > 0f)
                    {
                        TeleportEntities(radius, s_entityTypeIdx);
                    }
                }
            }
            GUILayout.EndVertical();
        }

        private static void TeleportEntities(float radius, int entityTypeIdx)
        {
            Player player = Player.m_localPlayer;
            if (player == null)
            {
                return;
            }

            Vector3 targetPosition = player.transform.position;

            bool teleportCharacters = entityTypeIdx == 0 || entityTypeIdx == 1;
            bool teleportItems = entityTypeIdx == 0 || entityTypeIdx == 2;

            if (teleportCharacters)
            {
                foreach (Character character in UnityEngine.Object.FindObjectsOfType<Character>())
                {
                    if (character == null || character.IsPlayer())
                    {
                        continue;
                    }

                    if (Vector3.Distance(character.transform.position, targetPosition) > radius)
                    {
                        continue;
                    }

                    character.transform.position = targetPosition + UnityEngine.Random.insideUnitSphere;
                }
            }

            if (teleportItems)
            {
                foreach (ItemDrop item in UnityEngine.Object.FindObjectsOfType<ItemDrop>())
                {
                    if (item == null)
                    {
                        continue;
                    }

                    if (Vector3.Distance(item.transform.position, targetPosition) > radius)
                    {
                        continue;
                    }

                    item.transform.position = targetPosition + UnityEngine.Random.insideUnitSphere;
                }
            }
        }
    }
}
