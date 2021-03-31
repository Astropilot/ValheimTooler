using System;
using System.Collections.Generic;
using System.Linq;
using RapidGUI;
using UnityEngine;
using ValheimTooler.Core.Extensions;
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
        private static IDictionary<string, string> s_guardianPowers;
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

            s_guardianPowers = new Dictionary<string, string>() {
            { Translator.Localize("$se_eikthyr_name"), "GP_Eikthyr" },
            { Translator.Localize("$se_theelder_name"), "GP_TheElder" },
            { Translator.Localize("$se_bonemass_name"), "GP_Bonemass" },
            { Translator.Localize("$se_moder_name"), "GP_Moder" },
            { Translator.Localize("$se_yagluth_name"), "GP_Yagluth" }
        };
        }

        public static void Update()
        {
            if (Time.time >= s_actionTimer)
            {
                if (s_isInfiniteStaminaMe)
                {
                    Player.m_localPlayer.VTSetMaxStamina();
                }
                if (s_isInfiniteStaminaOthers)
                {
                    AllOtherPlayersMaxStamina();
                }
                if (s_isNoStaminaOthers)
                {
                    AllOtherPlayerNoStamina();
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
            GUILayout.BeginHorizontal();
            {
                GUILayout.BeginVertical(Translator.Localize("$vt_player_general_title"), GUI.skin.box, GUILayout.ExpandWidth(false));
                {
                    GUILayout.Space(EntryPoint.s_boxSpacing);

                    if (GUILayout.Button(Translator.Localize("$vt_player_god_mode : " + (Player.m_localPlayer.VTInGodMode() ? Translator.s_cheatOn : Translator.s_cheatOff))))
                    {
                        Player.m_localPlayer.VTSetGodMode(!Player.m_localPlayer.VTInGodMode());
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
                    if (GUILayout.Button(Translator.Localize("$vt_player_fly_mode : " + (Player.m_localPlayer.VTInFlyMode() ? Translator.s_cheatOn : Translator.s_cheatOff))))
                    {
                        Player.m_localPlayer.VTSetFlyMode(!Player.m_localPlayer.VTInFlyMode());
                    }
                    if (GUILayout.Button(Translator.Localize("$vt_player_ghost_mode : " + (Player.m_localPlayer.VTInGhostMode() ? Translator.s_cheatOn : Translator.s_cheatOff))))
                    {
                        Player.m_localPlayer.VTSetGhostMode(!Player.m_localPlayer.VTInGhostMode());
                    }
                    if (GUILayout.Button(Translator.Localize("$vt_player_nop_lacement_cost : " + (Player.m_localPlayer.VTIsNoPlacementCost() ? Translator.s_cheatOn : Translator.s_cheatOff))))
                    {
                        Player.m_localPlayer.VTSetNoPlacementCost(!Player.m_localPlayer.VTIsNoPlacementCost());
                    }
                    if (GUILayout.Button(Translator.Localize("$vt_player_explore_minimap")))
                    {
                        Minimap.instance.VTExploreAll();
                    }
                    if (GUILayout.Button(Translator.Localize("$vt_player_reset_minimap")))
                    {
                        Minimap.instance.VTReset();
                    }
                    if (GUILayout.Button(Translator.Localize("$vt_player_tame_creatures")))
                    {
                        Player.m_localPlayer.VTTameNearbyCreatures();
                    }
                    if (GUILayout.Button(Translator.Localize("$vt_player_infinite_weight : " + (Player.m_localPlayer.VTIsInventoryInfiniteWeight() ? Translator.s_cheatOn : Translator.s_cheatOff))))
                    {
                        Player.m_localPlayer.VTInventoryInfiniteWeight(!Player.m_localPlayer.VTIsInventoryInfiniteWeight());
                    }
                }
                GUILayout.EndVertical();

                GUILayout.BeginVertical();
                {
                    GUILayout.BeginVertical(Translator.Localize("$vt_player_teleport_title"), GUI.skin.box, GUILayout.ExpandWidth(false));
                    {
                        GUILayout.Space(EntryPoint.s_boxSpacing);
                        GUILayout.BeginHorizontal();
                        {
                            GUILayout.Label(Translator.Localize("$vt_player_teleport_player :"), GUILayout.ExpandWidth(false));

                            s_teleportTargetIdx = RGUI.SelectionPopup(s_teleportTargetIdx, s_netPlayerNames.ToArray());
                        }
                        GUILayout.EndHorizontal();

                        if (GUILayout.Button(Translator.Localize("$vt_player_teleport_button")))
                        {
                            if (s_netPlayers != null && s_teleportTargetIdx < s_netPlayers.Count && s_teleportTargetIdx >= 0)
                            {
                                Player.m_localPlayer.VTTeleportTo(s_netPlayers[s_teleportTargetIdx]);
                            }
                        }
                    }
                    GUILayout.EndVertical();

                    GUILayout.BeginVertical(Translator.Localize("$vt_player_heal_manager_title"), GUI.skin.box, GUILayout.ExpandWidth(false));
                    {
                        GUILayout.Space(EntryPoint.s_boxSpacing);
                        GUILayout.BeginHorizontal();
                        {
                            GUILayout.Label(Translator.Localize("$vt_player_heal_player :"), GUILayout.ExpandWidth(false));

                            s_healTargetIdx = RGUI.SelectionPopup(s_healTargetIdx, s_playerNames.ToArray());
                        }
                        GUILayout.EndHorizontal();

                        if (GUILayout.Button(Translator.Localize("$vt_player_heal_selected_player")))
                        {
                            if (s_healTargetIdx < s_players.Count && s_healTargetIdx >= 0)
                            {
                                s_players[s_healTargetIdx].VTHeal();
                            }
                        }
                        if (GUILayout.Button(Translator.Localize("$vt_player_heal_all_players")))
                        {
                            foreach (Player player in s_players)
                            {
                                player.VTHeal();
                            }
                        }
                    }
                    GUILayout.EndVertical();

                    GUILayout.BeginVertical(Translator.Localize("$vt_player_power_title"), GUI.skin.box, GUILayout.ExpandWidth(false));
                    {
                        GUILayout.Space(EntryPoint.s_boxSpacing);
                        GUILayout.BeginHorizontal();
                        {
                            GUILayout.Label(Translator.Localize("$vt_player_power_name :"), GUILayout.ExpandWidth(false));
                            s_guardianPowerIdx = RGUI.SelectionPopup(s_guardianPowerIdx, s_guardianPowers.Keys.Select(p => Translator.Localize(p)).ToArray());
                        }
                        GUILayout.EndHorizontal();


                        if (GUILayout.Button(Translator.Localize("$vt_player_power_active_me")))
                        {
                            if (Player.m_localPlayer != null)
                            {
                                Player.m_localPlayer.VTActiveGuardianPower(s_guardianPowers[s_guardianPowerIdx]);
                            }
                        }
                        if (GUILayout.Button(Translator.Localize("$vt_player_power_active_all")))
                        {
                            if (s_guardianPowers.ContainsKey(s_guardianPowerIdx))
                            {
                                AllPlayersActiveGuardianPower(s_guardianPowers[s_guardianPowerIdx]);
                            }
                        }
                    }
                    GUILayout.EndVertical();

                    GUILayout.BeginVertical(Translator.Localize("$vt_player_skill_title"), GUI.skin.box, GUILayout.ExpandWidth(false));
                    {
                        GUILayout.Space(EntryPoint.s_boxSpacing);
                        GUILayout.BeginHorizontal();
                        {
                            GUILayout.Label(Translator.Localize("$vt_player_skill_name :"), GUILayout.ExpandWidth(false));
                            s_skillNameIdx = RGUI.SelectionPopup(s_skillNameIdx, s_skills.ToArray());
                        }
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        {
                            GUILayout.Label(Translator.Localize("$vt_player_skill_level :"), GUILayout.ExpandWidth(false));
                            s_skillLevelIdx = RGUI.SelectionPopup(s_skillLevelIdx, s_levels.ToArray());
                        }
                        GUILayout.EndHorizontal();

                        if (GUILayout.Button(Translator.Localize("$vt_player_skill_button")))
                        {
                            if (s_skillNameIdx < s_skills.Count && s_skillNameIdx >= 0)
                            {
                                if (int.TryParse(s_levels[s_skillLevelIdx], out int levelInt))
                                {
                                    Player.m_localPlayer.VTUpdateSkillLevel(s_skills[s_skillNameIdx], levelInt);
                                }
                            }
                        }
                    }
                    GUILayout.EndVertical();
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();
        }

        private static void AllOtherPlayersMaxStamina()
        {
            List<Player> players = Player.GetAllPlayers();

            if (players != null && Player.m_localPlayer != null)
            {
                foreach (Player player in players)
                {
                    if (player.GetPlayerID() != Player.m_localPlayer.GetPlayerID())
                    {
                        player.VTSetMaxStamina();
                    }
                }
            }
        }

        private static void AllOtherPlayerNoStamina()
        {
            List<Player> players = Player.GetAllPlayers();

            if (players != null && Player.m_localPlayer != null)
            {
                foreach (Player player in players)
                {
                    if (player.GetPlayerID() != Player.m_localPlayer.GetPlayerID())
                    {
                        player.VTSetNoStamina();
                    }
                }
            }
        }

        private static void AllPlayersActiveGuardianPower(string guardianPower)
        {
            List<Player> players = Player.GetAllPlayers();

            if (players != null)
            {
                foreach (Player player in players)
                {
                    player.VTActiveGuardianPower(guardianPower);
                }
            }
        }
    }
}
