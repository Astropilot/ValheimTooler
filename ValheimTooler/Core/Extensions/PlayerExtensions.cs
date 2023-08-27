using System;
using UnityEngine;
using ValheimTooler.Utils;

namespace ValheimTooler.Core.Extensions
{
    public static class PlayerExtensions
    {
        public static bool VTInGodMode(this Player player)
        {
            if (player != null)
            {
                return player.InGodMode();
            }
            return false;
        }

        public static void VTSetGodMode(this Player player, bool isGodMode)
        {
            if (player != null)
            {
                player.SetGodMode(isGodMode);
            }
        }

        public static bool VTInFlyMode(this Player player)
        {
            if (player != null)
            {
                return player.InDebugFlyMode();
            }
            return false;
        }

        public static void VTSetFlyMode(this Player player, bool isFlyMode)
        {
            if (player != null)
            {
                player.SetFieldValue<bool>("m_debugFly", isFlyMode);
                ZNetView m_nview = player.GetFieldValue<ZNetView>("m_nview");
                m_nview.GetZDO().Set(ZDOVars.s_debugFly, isFlyMode);
            }
        }

        public static bool VTInGhostMode(this Player player)
        {
            if (player != null)
            {
                return player.InGhostMode();
            }
            return false;
        }

        public static void VTSetGhostMode(this Player player, bool isGhostMode)
        {
            if (player != null)
            {
                player.SetGhostMode(isGhostMode);
            }
        }

        public static bool VTIsNoPlacementCost(this Player player)
        {
            if (player != null)
            {
                return player.NoCostCheat();
            }
            return false;
        }

        public static void VTSetNoPlacementCost(this Player player, bool isNoPlacementCost)
        {
            if (player != null)
            {
                player.SetFieldValue<bool>("m_noPlacementCost", isNoPlacementCost);
                player.CallMethod("UpdateAvailablePiecesList");
            }
        }

        public static void VTSetMaxStamina(this Player player)
        {
            if (player == null || !player.GetFieldValue<ZNetView>("m_nview").IsValid())
            {
                return;
            }

            player.UseStamina(- player.GetMaxStamina());
            player.GetFieldValue<ZNetView>("m_nview").GetZDO().Set(ZDOVars.s_stamina, player.GetMaxStamina());
        }

        public static void VTSetNoStamina(this Player player)
        {
            if (player == null || !player.GetFieldValue<ZNetView>("m_nview").IsValid())
            {
                return;
            }

            player.UseStamina(99999f);
            player.GetFieldValue<ZNetView>("m_nview").GetZDO().Set(ZDOVars.s_stamina, 0);
        }

        public static void VTTeleportTo(this Player player, ZNet.PlayerInfo target)
        {
            if (player != null)
            {
                ZDOID characterID = target.m_characterID;

                if (!characterID.IsNone())
                {
                    player.TeleportTo(target.m_position, player.transform.rotation, true);
                }
            }
        }

        public static void VTHeal(this Player player)
        {
            if (player != null)
            {
                player.Heal(player.GetMaxHealth(), true);
            }
        }

        public static void VTActiveGuardianPower(this Player player, string guardianPower)
        {
            if (player != null)
            {
                player.GetSEMan().AddStatusEffect(guardianPower.GetStableHashCode(), true);
            }
        }

        public static void VTUpdateSkillLevel(this Player player, Skills.SkillType skillType, int level)
        {
            if (player != null)
            {
                Skills.SkillDef skillDef = (Skills.SkillDef)player.GetSkills().CallMethod("GetSkillDef", skillType);

                if (skillDef != null)
                {
                    Skills.Skill skill = (Skills.Skill)player.GetSkills().CallMethod("GetSkill", skillType);

                    skill.m_level = Mathf.Clamp(level, 0f, 100f);
                }
            }
        }

        public static void VTAddItemToInventory(this Player player, string itemPrefab, int quantity, int quality, int variant)
        {
            if (player != null && Game.instance != null)
            {
                long playerID = player.GetPlayerID();
                string playerName = player.GetPlayerName();

                player.GetInventory().AddItem(itemPrefab, quantity, quality, variant, playerID, playerName);
            }
        }

        public static void VTTameNearbyCreatures(this Player player)
        {
            if (player != null)
            {
                Tameable.TameAllInArea(player.transform.position, 20f);
            }
        }

        public static void VTSendMessage(this Player player, string message)
        {
            if (player != null)
            {
                player.Message(MessageHud.MessageType.Center, message);
            }
        }
    }
}
