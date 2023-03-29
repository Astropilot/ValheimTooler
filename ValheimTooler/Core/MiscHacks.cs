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

                        if (GUILayout.Button(VTLocalization.instance.Localize("$vt_misc_autopin : " + (s_enableAutopinMap ? VTLocalization.s_cheatOn : VTLocalization.s_cheatOff))))
                        {
                            s_enableAutopinMap = !s_enableAutopinMap;
                        }

                        GUILayout.BeginHorizontal();
                        {
                            ConfigManager.instance.s_permanentPins = GUILayout.Toggle(ConfigManager.instance.s_permanentPins, "");
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

                        if (GUILayout.Button(VTLocalization.instance.Localize("$vt_misc_player_esp_button : " + (ESP.s_showPlayerESP ? VTLocalization.s_cheatOn : VTLocalization.s_cheatOff))))
                        {
                            ESP.s_showPlayerESP = !ESP.s_showPlayerESP;
                        }

                        if (GUILayout.Button(VTLocalization.instance.Localize("$vt_misc_monster_esp_button : " + (ESP.s_showMonsterESP ? VTLocalization.s_cheatOn : VTLocalization.s_cheatOff))))
                        {
                            ESP.s_showMonsterESP = !ESP.s_showMonsterESP;
                        }

                        if (GUILayout.Button(VTLocalization.instance.Localize("$vt_misc_dropped_esp_button : " + (ESP.s_showDroppedESP ? VTLocalization.s_cheatOn : VTLocalization.s_cheatOff))))
                        {
                            ESP.s_showDroppedESP = !ESP.s_showDroppedESP;
                        }

                        if (GUILayout.Button(VTLocalization.instance.Localize("$vt_misc_deposit_esp_button : " + (ESP.s_showDepositESP ? VTLocalization.s_cheatOn : VTLocalization.s_cheatOff))))
                        {
                            ESP.s_showDepositESP = !ESP.s_showDepositESP;
                        }

                        if (GUILayout.Button(VTLocalization.instance.Localize("$vt_misc_pickable_esp_button : " + (ESP.s_showPickableESP ? VTLocalization.s_cheatOn : VTLocalization.s_cheatOff))))
                        {
                            ESP.s_showPickableESP = !ESP.s_showPickableESP;
                        }

                        GUILayout.Label("ESP Radius distance (" + ConfigManager.instance.s_espRadius.ToString("0.0") + "m)", GUILayout.MinWidth(200));
                        ConfigManager.instance.s_espRadius = GUILayout.HorizontalSlider(ConfigManager.instance.s_espRadius, 5f, 500f, GUILayout.ExpandWidth(true));

                        GUILayout.BeginHorizontal();
                        {
                            ConfigManager.instance.s_espRadiusEnabled = GUILayout.Toggle(ConfigManager.instance.s_espRadiusEnabled, "");
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

            UserInfo fakePlayerSender = new UserInfo
            {
                Name = username,
                Gamertag = (string)ReflectionExtensions.CallStaticMethod<UserInfo>("GetLocalPlayerGamertag"),
                NetworkUserId = PrivilegeManager.GetNetworkUserId()
            };

            if (playerSender)
            {
                if (type == Talker.Type.Shout)
                {
                    if (ZRoutedRpc.instance != null)
                    {
                        ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody, "ChatMessage", new object[]
                        {
                            playerSender.GetHeadPoint(),
                            2,
                            fakePlayerSender,
                            message,
                            PrivilegeManager.GetNetworkUserId()
                        });
                    }
                    return;
                }
                ZNetView nview = playerSender.GetComponent<Talker>().GetComponent<ZNetView>();

                if (nview)
                {
                    nview.InvokeRPC(ZNetView.Everybody, "Say", new object[]
                    {
                        (int)type,
                        fakePlayerSender,
                        message,
                        PrivilegeManager.GetNetworkUserId()
                    });
                }
            }
        }
    }
}
