using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using RapidGUI;
using UnityEngine;
using ValheimTooler.Core.Extensions;
using ValheimTooler.Utils;

namespace ValheimTooler.Core
{
    public static class MiscHacks
    {
        public static bool s_enableAutopinMap = false;
        public static bool s_fullBrightness = false;
        private static bool s_fullBrightnessApplied = false;
        private static Color s_originalAmbientLight;

        public static bool s_disableFog = false;
        private static bool s_disableFogApplied = false;
        private static bool s_originalFog;
        private static ParticleSystem[] s_smokeParticles;
        private static int s_playerDamageIdx = 0;
        private static string s_damageToDeal = "1";

        private static string s_worldMessageText = "";

        private static string s_chatUsernameText = "";
        private static string s_chatMessageText = "";
        private static bool s_isShoutMessage = false;

        private static List<Player> s_players = null;

        private static float s_updateTimer = 0f;
        private static readonly float s_updateTimerInterval = 1.5f;

        public static void Start()
        {
            return;
        }

        public static void Update()
        {
            if (Time.time >= s_updateTimer)
            {
                s_players = Player.GetAllPlayers();

                s_updateTimer = Time.time + s_updateTimerInterval;
            }

            if (ConfigManager.s_espPlayersShortcut.Value.IsDown())
            {
                ActionToggleESPPlayers(true);
            }
            if (ConfigManager.s_espMonstersShortcut.Value.IsDown())
            {
                ActionToggleESPMonsters(true);
            }
            if (ConfigManager.s_espDroppedItemsShortcut.Value.IsDown())
            {
                ActionToggleESPDroppedItems(true);
            }
            if (ConfigManager.s_espDepositsShortcut.Value.IsDown())
            {
                ActionToggleESPDeposits(true);
            }
            if (ConfigManager.s_espPickablesShortcut.Value.IsDown())
            {
                ActionToggleESPPickables(true);
            }

            if (s_fullBrightness)
            {
                ApplyFullBrightness();
            }
            else if (s_fullBrightnessApplied)
            {
                RestoreBrightness();
            }

            if (s_disableFog)
            {
                ApplyNoFog();
            }
            else if (s_disableFogApplied)
            {
                RestoreFog();
            }
        }

        public static void DisplayGUI()
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.BeginVertical();
                {
                    GUILayout.BeginVertical(VTLocalization.instance.Localize("$vt_misc_damage_title"), GUI.skin.box, GUILayout.ExpandWidth(false));
                    {
                        GUILayout.Space(EntryPoint.s_boxSpacing);
                        GUILayout.BeginHorizontal();
                        {
                            GUILayout.Label(VTLocalization.instance.Localize("$vt_misc_damage_player :"), GUILayout.ExpandWidth(false));
                            s_playerDamageIdx = RGUI.SelectionPopup(s_playerDamageIdx, s_players?.Select(p => p.GetPlayerName()).ToArray());
                        }
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        {
                            GUILayout.Label(VTLocalization.instance.Localize("$vt_misc_damage_value :"), GUILayout.ExpandWidth(false));
                            s_damageToDeal = GUILayout.TextField(s_damageToDeal, GUILayout.ExpandWidth(true));
                        }
                        GUILayout.EndHorizontal();

                        if (GUILayout.Button(VTLocalization.instance.Localize("$vt_misc_damage_button_player")))
                        {

                            if (int.TryParse(s_damageToDeal, out int damage))
                            {
                                s_players[s_playerDamageIdx].VTDamage(damage);
                            }
                        }
                        if (GUILayout.Button(VTLocalization.instance.Localize("$vt_misc_damage_button_entities")))
                        {
                            DamageAllCharacters();
                        }
                        if (GUILayout.Button(VTLocalization.instance.Localize("$vt_misc_damage_button_players")))
                        {
                            DamageAllOtherPlayers();
                        }
                    }
                    GUILayout.EndVertical();

                    GUILayout.BeginVertical(VTLocalization.instance.Localize("$vt_misc_map_title"), GUI.skin.box, GUILayout.ExpandWidth(false));
                    {
                        GUILayout.Space(EntryPoint.s_boxSpacing);

                        if (GUILayout.Button(UI.Utils.ToggleButtonLabel("$vt_misc_autopin", s_enableAutopinMap)))
                        {
                            s_enableAutopinMap = !s_enableAutopinMap;
                        }

                        GUILayout.BeginHorizontal();
                        {
                            ConfigManager.s_permanentPins.Value = GUILayout.Toggle(ConfigManager.s_permanentPins.Value, "");
                            GUILayout.Label(VTLocalization.instance.Localize("$vt_misc_autopin_permanent"));
                        }
                        GUILayout.EndHorizontal();

                        if (GUILayout.Button(VTLocalization.instance.Localize("$vt_misc_autopin_clear")))
                        {
                            if (Minimap.instance != null)
                            {
                                var pinListCopy = new List<Minimap.PinData>(Minimap.instance.GetFieldValue<List<Minimap.PinData>>("m_pins"));
                                foreach (var pin in pinListCopy)
                                {
                                    if (Regex.IsMatch(pin.m_name, @"^.+\[VT[0-9]{5}]$"))
                                    {
                                        Minimap.instance.RemovePin(pin);
                                    }
                                }
                            }
                        }
                    }
                    GUILayout.EndVertical();

                    GUILayout.BeginVertical(VTLocalization.instance.Localize("$vt_misc_visual_title"), GUI.skin.box, GUILayout.ExpandWidth(false));
                    {
                        GUILayout.Space(EntryPoint.s_boxSpacing);
                        GUILayout.BeginHorizontal();
                        {
                            s_fullBrightness = GUILayout.Toggle(s_fullBrightness, "");
                            GUILayout.Label(VTLocalization.instance.Localize("$vt_misc_full_brightness"));
                        }
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        {
                            s_disableFog = GUILayout.Toggle(s_disableFog, "");
                            GUILayout.Label(VTLocalization.instance.Localize("$vt_misc_disable_fog"));
                        }
                        GUILayout.EndHorizontal();
                    }
                    GUILayout.EndVertical();

                    GUILayout.BeginVertical(VTLocalization.instance.Localize("$vt_misc_event_title"), GUI.skin.box, GUILayout.ExpandWidth(false));
                    {
                        GUILayout.Space(EntryPoint.s_boxSpacing);
                        GUILayout.BeginHorizontal();
                        {
                            GUILayout.Label(VTLocalization.instance.Localize("$vt_misc_event_message :"), GUILayout.ExpandWidth(false));
                            s_worldMessageText = GUILayout.TextField(s_worldMessageText, GUILayout.ExpandWidth(true));
                        }
                        GUILayout.EndHorizontal();

                        if (GUILayout.Button(VTLocalization.instance.Localize("$vt_misc_event_button")))
                        {
                            MessageAllInRange(MessageHud.MessageType.Center, s_worldMessageText);
                        }
                    }
                    GUILayout.EndVertical();
                }
                GUILayout.EndVertical();

                GUILayout.BeginVertical();
                {
                    GUILayout.BeginVertical(VTLocalization.instance.Localize("$vt_misc_chat_title"), GUI.skin.box, GUILayout.ExpandWidth(false));
                    {
                        GUILayout.Space(EntryPoint.s_boxSpacing);
                        GUILayout.BeginHorizontal();
                        {
                            GUILayout.Label(VTLocalization.instance.Localize("$vt_misc_chat_username :"), GUILayout.ExpandWidth(false));
                            s_chatUsernameText = GUILayout.TextField(s_chatUsernameText, GUILayout.ExpandWidth(true));
                        }
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        {
                            GUILayout.Label(VTLocalization.instance.Localize("$vt_misc_chat_message :"), GUILayout.ExpandWidth(false));
                            s_chatMessageText = GUILayout.TextField(s_chatMessageText, GUILayout.ExpandWidth(true));
                        }
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        {
                            s_isShoutMessage = GUILayout.Toggle(s_isShoutMessage, "");
                            GUILayout.Label(VTLocalization.instance.Localize("$vt_misc_chat_shout"));
                        }
                        GUILayout.EndHorizontal();

                        if (GUILayout.Button(VTLocalization.instance.Localize("$vt_misc_chat_button")))
                        {
                            ChatMessage(s_isShoutMessage ? Talker.Type.Shout : Talker.Type.Normal, s_chatUsernameText, s_chatMessageText);
                        }
                    }
                    GUILayout.EndVertical();

                    GUILayout.BeginVertical(VTLocalization.instance.Localize("$vt_misc_esp_title"), GUI.skin.box, GUILayout.ExpandWidth(false));
                    {
                        GUILayout.Space(EntryPoint.s_boxSpacing);

                        if (GUILayout.Button(UI.Utils.ToggleButtonLabel("$vt_misc_player_esp_button", ESP.s_showPlayerESP, ConfigManager.s_espPlayersShortcut.Value)))
                        {
                            ActionToggleESPPlayers();
                        }

                        if (GUILayout.Button(UI.Utils.ToggleButtonLabel("$vt_misc_monster_esp_button", ESP.s_showMonsterESP, ConfigManager.s_espMonstersShortcut.Value)))
                        {
                            ActionToggleESPMonsters();
                        }

                        if (GUILayout.Button(UI.Utils.ToggleButtonLabel("$vt_misc_dropped_esp_button", ESP.s_showDroppedESP, ConfigManager.s_espDroppedItemsShortcut.Value)))
                        {
                            ActionToggleESPDroppedItems();
                        }

                        if (GUILayout.Button(UI.Utils.ToggleButtonLabel("$vt_misc_deposit_esp_button", ESP.s_showDepositESP, ConfigManager.s_espDepositsShortcut.Value)))
                        {
                            ActionToggleESPDeposits();
                        }

                        if (GUILayout.Button(UI.Utils.ToggleButtonLabel("$vt_misc_pickable_esp_button", ESP.s_showPickableESP, ConfigManager.s_espPickablesShortcut.Value)))
                        {
                            ActionToggleESPPickables();
                        }

                        if (GUILayout.Button(UI.Utils.ToggleButtonLabel("XRAY", ESP.s_xray)))
                        {
                            ActionToggleESPXRay();
                        }

                        if (GUILayout.Button(UI.Utils.ToggleButtonLabel("LABELS", ESP.s_showLabels)))
                        {
                            ActionToggleESPLabels();
                        }

                        GUILayout.BeginHorizontal();
                        {
                            ESP.s_show2DBoxes = GUILayout.Toggle(ESP.s_show2DBoxes, "");
                            GUILayout.Label(VTLocalization.instance.Localize("$vt_misc_esp_2d_boxes"));
                        }
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        {
                            ESP.s_show3DBoxes = GUILayout.Toggle(ESP.s_show3DBoxes, "");
                            GUILayout.Label(VTLocalization.instance.Localize("$vt_misc_esp_3d_boxes"));
                        }
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        {
                            bool newFill = GUILayout.Toggle(ESP.s_fillVisible, "");
                            if (newFill != ESP.s_fillVisible)
                            {
                                ESP.SetFillVisible(newFill);
                            }
                            GUILayout.Label(VTLocalization.instance.Localize("$vt_misc_esp_fill_visible"));
                        }
                        GUILayout.EndHorizontal();

                        GUILayout.Label("ESP Radius distance (" + ConfigManager.s_espRadius.Value.ToString("0.0") + "m)", GUILayout.MinWidth(200));
                        ConfigManager.s_espRadius.Value = GUILayout.HorizontalSlider(ConfigManager.s_espRadius.Value, 5f, 500f, GUILayout.ExpandWidth(true));

                        GUILayout.BeginHorizontal();
                        {
                            ConfigManager.s_espRadiusEnabled.Value = GUILayout.Toggle(ConfigManager.s_espRadiusEnabled.Value, "");
                            GUILayout.Label(VTLocalization.instance.Localize("$vt_misc_radius_enable"));
                        }
                        GUILayout.EndHorizontal();
                    }
                    GUILayout.EndVertical();
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();
        }

        private static void ActionToggleESPPlayers(bool sendNotification = false)
        {
            ESP.s_showPlayerESP = !ESP.s_showPlayerESP;

            if (sendNotification)
            {
                Player.m_localPlayer.VTSendMessage(UI.Utils.ToggleButtonLabel("$vt_misc_player_esp_button", ESP.s_showPlayerESP));
            }
        }

        private static void ActionToggleESPMonsters(bool sendNotification = false)
        {
            ESP.s_showMonsterESP = !ESP.s_showMonsterESP;

            if (sendNotification)
            {
                Player.m_localPlayer.VTSendMessage(UI.Utils.ToggleButtonLabel("$vt_misc_monster_esp_button", ESP.s_showMonsterESP));
            }
        }

        private static void ActionToggleESPDroppedItems(bool sendNotification = false)
        {
            ESP.s_showDroppedESP = !ESP.s_showDroppedESP;

            if (sendNotification)
            {
                Player.m_localPlayer.VTSendMessage(UI.Utils.ToggleButtonLabel("$vt_misc_dropped_esp_button", ESP.s_showDroppedESP));
            }
        }

        private static void ActionToggleESPDeposits(bool sendNotification = false)
        {
            ESP.s_showDepositESP = !ESP.s_showDepositESP;

            if (sendNotification)
            {
                Player.m_localPlayer.VTSendMessage(UI.Utils.ToggleButtonLabel("$vt_misc_deposit_esp_button", ESP.s_showDepositESP));
            }
        }


        private static void ActionToggleESPPickables(bool sendNotification = false)
        {
            ESP.s_showPickableESP = !ESP.s_showPickableESP;

            if (sendNotification)
            {
                Player.m_localPlayer.VTSendMessage(UI.Utils.ToggleButtonLabel("$vt_misc_pickable_esp_button", ESP.s_showPickableESP));
            }
        }

        private static void ActionToggleESPXRay(bool sendNotification = false)
        {
            ESP.SetXRay(!ESP.s_xray);

            if (sendNotification)
            {
                Player.m_localPlayer.VTSendMessage(UI.Utils.ToggleButtonLabel("XRAY", ESP.s_xray));
            }
        }

        private static void ActionToggleESPLabels(bool sendNotification = false)
        {
            ESP.s_showLabels = !ESP.s_showLabels;

            if (sendNotification)
            {
                Player.m_localPlayer.VTSendMessage(UI.Utils.ToggleButtonLabel("LABELS", ESP.s_showLabels));
            }
        }

        private static void ApplyFullBrightness()
        {
            if (!s_fullBrightnessApplied)
            {
                s_originalAmbientLight = RenderSettings.ambientLight;
                s_fullBrightnessApplied = true;
            }

            RenderSettings.ambientLight = Color.white;

            foreach (Light light in Object.FindObjectsOfType<Light>())
            {
                light.shadows = LightShadows.None;
            }
        }

        private static void RestoreBrightness()
        {
            RenderSettings.ambientLight = s_originalAmbientLight;

            foreach (Light light in Object.FindObjectsOfType<Light>())
            {
                light.shadows = LightShadows.Soft;
            }

            s_fullBrightnessApplied = false;
        }

        private static void ApplyNoFog()
        {
            if (!s_disableFogApplied)
            {
                s_originalFog = RenderSettings.fog;
                s_smokeParticles = Object.FindObjectsOfType<ParticleSystem>()
                    .Where(ps => ps.name.ToLower().Contains("smoke")).ToArray();
                foreach (var ps in s_smokeParticles)
                {
                    // ParticleSystemStopBehavior is not available in all Unity versions
                    ps.Stop(true);
                    ps.Clear();
                }
                s_disableFogApplied = true;
            }

            RenderSettings.fog = false;
        }

        private static void RestoreFog()
        {
            RenderSettings.fog = s_originalFog;
            if (s_smokeParticles != null)
            {
                foreach (var ps in s_smokeParticles)
                {
                    if (ps != null)
                    {
                        ps.Play();
                    }
                }
            }
            s_smokeParticles = null;
            s_disableFogApplied = false;
        }

        private static void DamageAllCharacters()
        {
            foreach (Character character in Character.GetAllCharacters())
            {
                if (!character.IsPlayer())
                {
                    character.VTDamage(1E+10f);
                }
            }
        }

        private static void DamageAllOtherPlayers()
        {
            if (Player.m_localPlayer == null)
            {
                return;
            }

            foreach (Character character in Character.GetAllCharacters())
            {
                if (character.IsPlayer() && (character as Player).GetPlayerID() != Player.m_localPlayer.GetPlayerID())
                {
                    character.VTDamage(1E+10f);
                }
            }
        }

        private static void MessageAllInRange(MessageHud.MessageType type, string msg)
        {
            foreach (Player player in Player.GetAllPlayers())
            {
                player.Message(type, msg, 0, null);
            }
        }

        private static void ChatMessage(Talker.Type type, string username, string message)
        {
            Player playerSender = Player.m_localPlayer;

            foreach (Player player in Player.GetAllPlayers())
            {
                if (player.GetPlayerName().ToLower().Equals(username.ToLower()))
                {
                    playerSender = player;
                    break;
                }
            }

            
        }
    }
}
