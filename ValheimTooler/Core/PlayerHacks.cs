using System;
using System.Collections.Generic;
using System.Linq;
using RapidGUI;
using UnityEngine;
using ValheimTooler.Utils;

namespace ValheimTooler.Core
{
    public static class PlayerHacks
    {
        private static bool s_isInfiniteStaminaMe = false;
        private static bool s_isInfiniteStaminaOthers = false;
        private static bool s_isNoStaminaOthers = false;
        private static int s_teleportTargetIdx = -1;
        private static int s_healTargetIdx = -1;
        private static string s_guardianPowerIdx = "";
        private static readonly IDictionary<string, string> s_guardianPowers = new Dictionary<string, string>() {
            { "$se_eikthyr_name", "GP_Eikthyr" },
            { "$se_theelder_name", "GP_TheElder" },
            { "$se_bonemass_name", "GP_Bonemass" },
            { "$se_moder_name", "GP_Moder" },
            { "$se_yagluth_name", "GP_Yagluth" }
        };
        private static int s_skillNameIdx = -1;
        private static int s_skillLevelIdx = 0;

        private static float s_actionTimer = 0f;
        private static readonly float s_actionTimerInterval = 0.5f;

        private static float s_updateTimer = 0f;
        private static readonly float s_updateTimerInterval = 1.5f;

        private static List<ZNet.PlayerInfo> s_netPlayers = null;
        private static List<string> s_netPlayerNames = new List<string>();

        private static List<Player> s_players = new List<Player>();
        private static List<string> s_playerNames = new List<string>();

        private static readonly List<string> s_skills = new List<string>();
        private static readonly List<string> s_levels = new List<string>();

        public static void Start()
        {
            foreach (object obj in Enum.GetValues(typeof(Skills.SkillType)))
            {
                Skills.SkillType skillType = (Skills.SkillType)obj;

                s_skills.Add(skillType.ToString());
            }
            for (var i = 1; i <= 100; i++)
            {
                s_levels.Add(i.ToString());
            }
        }

        public static void Update()
        {
            if (Time.time >= s_actionTimer)
            {
                if (s_isInfiniteStaminaMe)
                {
                    UpdateInfiniteStamina(Player.m_localPlayer);
                }
                if (s_isInfiniteStaminaOthers)
                {
                    UpdateInfiniteStamina();
                }
                if (s_isNoStaminaOthers)
                {
                    UpdateNoStamina();
                }
                s_actionTimer = Time.time + s_actionTimerInterval;
            }

            if (Time.time >= s_updateTimer)
            {
                s_netPlayerNames.Clear();
                s_playerNames.Clear();

                if (ZNet.instance == null)
                {
                    s_netPlayers = null;
                    s_teleportTargetIdx = -1;
                }
                else
                {
                    s_netPlayers = ZNet.instance.GetPlayerList();

                    if (s_netPlayers != null)
                    {
                        s_netPlayerNames = s_netPlayers.Select(p => p.m_name).ToList();
                    }
                }

                s_players = Player.GetAllPlayers();
                s_playerNames = s_players.Select(p => p.GetPlayerName()).ToList();

                s_updateTimer = Time.time + s_updateTimerInterval;
            }
        }

        public static void DisplayGUI()
        {
            GUILayout.BeginVertical(Translator.Localize("$vt_player_general_title"), GUI.skin.box, GUILayout.ExpandWidth(false));
            {
                GUILayout.Space(EntryPoint.s_boxSpacing);

                if (GUILayout.Button(Translator.Localize("$vt_player_god_mode : " + (InGodMode() ? Translator.s_cheatOn : Translator.s_cheatOff))))
                {
                    SetGodMode(!InGodMode());
                }
                if (GUILayout.Button(Translator.Localize("$vt_player_inf_stamina_me : " + (s_isInfiniteStaminaMe ? Translator.s_cheatOn : Translator.s_cheatOff))))
                {
                    s_isInfiniteStaminaMe = !s_isInfiniteStaminaMe;
                }
                if (GUILayout.Button(Translator.Localize("$vt_player_inf_stamina_others : " + (s_isInfiniteStaminaOthers ? Translator.s_cheatOn : Translator.s_cheatOff))))
                {
                    s_isInfiniteStaminaOthers = !s_isInfiniteStaminaOthers;
                }
                if (GUILayout.Button(Translator.Localize("$vt_player_no_stamina : " + (s_isNoStaminaOthers ? Translator.s_cheatOn : Translator.s_cheatOff))))
                {
                    s_isNoStaminaOthers = !s_isNoStaminaOthers;
                }
                if (GUILayout.Button(Translator.Localize("$vt_player_fly_mode : " + (InFlyMode() ? Translator.s_cheatOn : Translator.s_cheatOff))))
                {
                    SetFlyMode(!InFlyMode());
                }
                if (GUILayout.Button(Translator.Localize("$vt_player_ghost_mode : " + (InGhostMode() ? Translator.s_cheatOn : Translator.s_cheatOff))))
                {
                    SetGhostMode(!InGhostMode());
                }
                if (GUILayout.Button(Translator.Localize("$vt_player_nop_lacement_cost : " + (IsNoPlacementCost() ? Translator.s_cheatOn : Translator.s_cheatOff))))
                {
                    SetNoPlacementCost(!IsNoPlacementCost());
                }
                if (GUILayout.Button(Translator.Localize("$vt_player_explore_minimap")))
                {
                    ExploreAllMinimap();
                }
            }
            GUILayout.EndVertical();

            GUILayout.BeginVertical(Translator.Localize("$vt_player_teleport_title"), GUI.skin.box, GUILayout.ExpandWidth(false));
            {
                GUILayout.Space(EntryPoint.s_boxSpacing);
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label(Translator.Localize("$vt_player_teleport_player :"));

                    s_teleportTargetIdx = RGUI.SelectionPopup(s_teleportTargetIdx, s_netPlayerNames.ToArray());
                }
                GUILayout.EndHorizontal();

                if (GUILayout.Button(Translator.Localize("$vt_player_teleport_button")))
                {
                    if (s_netPlayers != null && s_teleportTargetIdx < s_netPlayers.Count && s_teleportTargetIdx >= 0)
                    {
                        TeleportPlayerTo(s_netPlayers[s_teleportTargetIdx]);
                    }
                }
            }
            GUILayout.EndVertical();

            GUILayout.BeginVertical(Translator.Localize("$vt_player_heal_manager_title"), GUI.skin.box, GUILayout.ExpandWidth(false));
            {
                GUILayout.Space(EntryPoint.s_boxSpacing);
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label(Translator.Localize("$vt_player_heal_player :"));

                    s_healTargetIdx = RGUI.SelectionPopup(s_healTargetIdx, s_playerNames.ToArray());
                }
                GUILayout.EndHorizontal();

                if (GUILayout.Button(Translator.Localize("$vt_player_heal_selected_player")))
                {
                    if (s_healTargetIdx < s_players.Count && s_healTargetIdx >= 0)
                    {
                        HealPlayer(s_players[s_healTargetIdx]);
                    }
                }
                if (GUILayout.Button(Translator.Localize("$vt_player_heal_all_players")))
                {
                    foreach (Player player in s_players)
                    {
                        HealPlayer(player);
                    }
                }
            }
            GUILayout.EndVertical();

            GUILayout.BeginVertical(Translator.Localize("$vt_player_power_title"), GUI.skin.box, GUILayout.ExpandWidth(false));
            {
                GUILayout.Space(EntryPoint.s_boxSpacing);
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label(Translator.Localize("$vt_player_power_name :"));
                    s_guardianPowerIdx = RGUI.SelectionPopup(s_guardianPowerIdx, s_guardianPowers.Keys.Select(p => Translator.Localize(p)).ToArray());
                }
                GUILayout.EndHorizontal();


                if (GUILayout.Button(Translator.Localize("$vt_player_power_active_me")))
                {
                    if (Player.m_localPlayer != null)
                    {
                        AddGuardianPower(Player.m_localPlayer, s_guardianPowers[s_guardianPowerIdx]);
                    }
                }
                if (GUILayout.Button(Translator.Localize("$vt_player_power_active_all")))
                {
                    if (s_guardianPowers.ContainsKey(s_guardianPowerIdx))
                    {
                        AddGuardianPower(s_guardianPowers[s_guardianPowerIdx]);
                    }
                }
            }
            GUILayout.EndVertical();

            GUILayout.BeginVertical(Translator.Localize("$vt_player_skill_title"), GUI.skin.box, GUILayout.ExpandWidth(false));
            {
                GUILayout.Space(EntryPoint.s_boxSpacing);
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label(Translator.Localize("$vt_player_skill_name :"));
                    s_skillNameIdx = RGUI.SelectionPopup(s_skillNameIdx, s_skills.ToArray());
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label(Translator.Localize("$vt_player_skill_level :"));
                    s_skillLevelIdx = RGUI.SelectionPopup(s_skillLevelIdx, s_levels.ToArray());
                }
                GUILayout.EndHorizontal();

                if (GUILayout.Button(Translator.Localize("$vt_player_skill_button")))
                {
                    if (s_skillNameIdx < s_skills.Count && s_skillNameIdx >= 0)
                    {
                        UpdateSkillLevel(s_skills[s_skillNameIdx], s_levels[s_skillLevelIdx]);
                    }
                }
            }
            GUILayout.EndVertical();
        }

        private static bool InGodMode()
        {
            if (Player.m_localPlayer)
            {
                return Player.m_localPlayer.InGodMode();
            }
            return false;
        }

        private static void SetGodMode(bool isGodMode)
        {
            if (Player.m_localPlayer)
            {
                Player.m_localPlayer.SetGodMode(isGodMode);
            }
        }

        private static void UpdateInfiniteStamina()
        {
            List<Player> players = Player.GetAllPlayers();

            if (players != null && Player.m_localPlayer != null)
            {
                foreach (Player player in players)
                {
                    if (player.GetPlayerID() != Player.m_localPlayer.GetPlayerID())
                    {
                        UpdateInfiniteStamina(player);
                    }
                }
            }
        }

        private static void UpdateInfiniteStamina(Player player)
        {
            if (player == null || !player.GetFieldValue<ZNetView>("m_nview").IsValid())
            {
                return;
            }

            player.UseStamina(- player.GetMaxStamina());
            player.GetFieldValue<ZNetView>("m_nview").GetZDO().Set("stamina", player.GetMaxStamina());
        }

        private static void UpdateNoStamina()
        {
            List<Player> players = Player.GetAllPlayers();

            if (players != null && Player.m_localPlayer != null)
            {
                foreach (Player player in players)
                {
                    if (player.GetPlayerID() != Player.m_localPlayer.GetPlayerID())
                    {
                        if (player == null || !player.GetFieldValue<ZNetView>("m_nview").IsValid())
                        {
                            return;
                        }

                        player.UseStamina(99999f);
                        player.GetFieldValue<ZNetView>("m_nview").GetZDO().Set("stamina", 0);
                    }
                }
            }
        }

        private static bool InFlyMode()
        {
            if (Player.m_localPlayer)
            {
                return Player.m_localPlayer.InDebugFlyMode();
            }
            return false;
        }

        private static void SetFlyMode(bool isFlyMode)
        {
            if (Player.m_localPlayer)
            {
                Player.m_localPlayer.SetFieldValue<bool>("m_debugFly", isFlyMode);
                ZNetView m_nview = Player.m_localPlayer.GetFieldValue<ZNetView>("m_nview");
                m_nview.GetZDO().Set("DebugFly", isFlyMode);
            }
        }

        private static bool InGhostMode()
        {
            if (Player.m_localPlayer)
            {
                return Player.m_localPlayer.InGhostMode();
            }
            return false;
        }

        private static void SetGhostMode(bool isGhostMode)
        {
            if (Player.m_localPlayer)
            {
                Player.m_localPlayer.SetGhostMode(isGhostMode);
            }
        }

        private static bool IsNoPlacementCost()
        {
            if (Player.m_localPlayer)
            {
                return Player.m_localPlayer.NoCostCheat();
            }
            return false;
        }

        private static void SetNoPlacementCost(bool isNoPlacementCost)
        {
            if (Player.m_localPlayer)
            {
                Player.m_localPlayer.SetFieldValue<bool>("m_noPlacementCost", isNoPlacementCost);
                Player.m_localPlayer.CallMethod("UpdateAvailablePiecesList");
            }
        }

        private static void ExploreAllMinimap()
        {
            if (Minimap.instance)
            {
                Minimap.instance.ExploreAll();
            }
        }

        private static void TeleportPlayerTo(ZNet.PlayerInfo target)
        {
            Player localPlayer = Player.m_localPlayer;

            if (localPlayer == null)
            {
                return;
            }

            ZDOID characterID = target.m_characterID;

            if (!characterID.IsNone())
            {
                localPlayer.TeleportTo(target.m_position, localPlayer.transform.rotation, true);
            }
        }

        private static void HealPlayer(Player player)
        {
            if (player != null)
            {
                player.Heal(player.GetMaxHealth(), true);
            }
        }

        private static void AddGuardianPower(Player player, string guardianPower)
        {
            if (player != null)
            {
                player.GetSEMan().AddStatusEffect(guardianPower, true);
            }
        }

        private static void AddGuardianPower(string guardianPower)
        {
            List<Player> players = Player.GetAllPlayers();

            if (players != null)
            {
                foreach (Player player in players)
                {
                    AddGuardianPower(player, guardianPower);
                }
            }
        }

        private static void UpdateSkillLevel(string skillName, string level)
        {
            if (int.TryParse(level, out int levelInt))
            {
                UpdateSkillLevel(skillName, levelInt);
            }
        }

        private static void UpdateSkillLevel(string skillName, int level)
        {
            if (Player.m_localPlayer == null)
            {
                return;
            }

            Skills.SkillType skillType = (Skills.SkillType)Enum.Parse(typeof(Skills.SkillType), skillName);
            Skills.Skill skill = (Skills.Skill)Player.m_localPlayer.GetSkills().CallMethod("GetSkill", skillType);

            int offset = (int)Math.Ceiling(level - skill.m_level);

            Player.m_localPlayer.GetSkills().CheatRaiseSkill(skillName.ToLower(), offset);
        }
    }
}
