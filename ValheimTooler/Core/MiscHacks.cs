using System.Collections.Generic;
using System.Linq;
using RapidGUI;
using UnityEngine;
using ValheimTooler.Utils;

namespace ValheimTooler.Core
{
    public static class MiscHacks
    {
        private static int s_playerDamageIdx = 0;
        private static string s_damageToDeal = "1";

        private static string s_worldMessageText = "";

        private static string s_chatUsernameText = "";
        private static string s_chatMessageText = "";
        private static bool s_isShoutMessage = false;

        private static List<Player> s_players = null;
        private static List<string> s_playerNames = new List<string>();

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
                s_playerNames.Clear();

                s_players = Player.GetAllPlayers();
                s_playerNames = s_players.Select(p => p.GetPlayerName()).ToList();

                s_updateTimer = Time.time + s_updateTimerInterval;
            }
        }

        public static void DisplayGUI()
        {
            GUILayout.BeginVertical(Translator.Localize("$vt_misc_damage_title"), GUI.skin.box, GUILayout.ExpandWidth(false));
            {
                GUILayout.Space(EntryPoint.s_boxSpacing);
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label(Translator.Localize("$vt_misc_damage_player :"));
                    s_playerDamageIdx = RGUI.SelectionPopup(s_playerDamageIdx, s_playerNames.ToArray());
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label(Translator.Localize("$vt_misc_damage_value :"));
                    s_damageToDeal = GUILayout.TextField(s_damageToDeal, GUILayout.ExpandWidth(true));
                }
                GUILayout.EndHorizontal();

                if (GUILayout.Button(Translator.Localize("$vt_misc_damage_button_player")))
                {
                    
                    if (int.TryParse(s_damageToDeal, out int damage))
                    {
                        DamageCharacter(s_players[s_playerDamageIdx], damage);
                    }
                }
                if (GUILayout.Button(Translator.Localize("$vt_misc_damage_button_entities")))
                {
                    DamageAllCharacters();
                }
                if (GUILayout.Button(Translator.Localize("$vt_misc_damage_button_players")))
                {
                    DamageAllOtherPlayers();
                }
            }
            GUILayout.EndVertical();

            GUILayout.BeginVertical(Translator.Localize("$vt_misc_event_title"), GUI.skin.box, GUILayout.ExpandWidth(false));
            {
                GUILayout.Space(EntryPoint.s_boxSpacing);
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label(Translator.Localize("$vt_misc_event_message :"));
                    s_worldMessageText = GUILayout.TextField(s_worldMessageText, GUILayout.ExpandWidth(true));
                }
                GUILayout.EndHorizontal();

                if (GUILayout.Button(Translator.Localize("$vt_misc_event_button")))
                {
                    MessageAllInRange(MessageHud.MessageType.Center, s_worldMessageText);
                }
            }
            GUILayout.EndVertical();

            GUILayout.BeginVertical(Translator.Localize("$vt_misc_chat_title"), GUI.skin.box, GUILayout.ExpandWidth(false));
            {
                GUILayout.Space(EntryPoint.s_boxSpacing);
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label(Translator.Localize("$vt_misc_chat_username :"));
                    s_chatUsernameText = GUILayout.TextField(s_chatUsernameText, GUILayout.ExpandWidth(true));
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label(Translator.Localize("$vt_misc_chat_message :"));
                    s_chatMessageText = GUILayout.TextField(s_chatMessageText, GUILayout.ExpandWidth(true));
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                {
                    s_isShoutMessage = GUILayout.Toggle(s_isShoutMessage, "");
                    GUILayout.Label(Translator.Localize("$vt_misc_chat_shout"));
                }
                GUILayout.EndHorizontal();

                if (GUILayout.Button(Translator.Localize("$vt_misc_chat_button")))
                {
                    ChatMessage(s_isShoutMessage ? Talker.Type.Shout : Talker.Type.Normal, s_chatUsernameText, s_chatMessageText);
                }
            }
            GUILayout.EndVertical();

            GUILayout.BeginVertical(Translator.Localize("$vt_misc_esp_title"), GUI.skin.box, GUILayout.ExpandWidth(false));
            {
                GUILayout.Space(EntryPoint.s_boxSpacing);

                if (GUILayout.Button(Translator.Localize("$vt_misc_esp_button : " + (EntryPoint.s_showESP ? Translator.s_cheatOn : Translator.s_cheatOff))))
                {
                    EntryPoint.s_showESP = !EntryPoint.s_showESP;
                }
            }
            GUILayout.EndVertical();
        }

        private static void DamageCharacter(Character character, int damage)
        {
            if (character != null)
            {
                HitData hitData = new HitData();

                hitData.m_damage.m_damage = damage;
                character.Damage(hitData);
            }
        }

        private static void DamageAllCharacters()
        {
            foreach (Character character in Character.GetAllCharacters())
            {
                if (!character.IsPlayer())
                {
                    HitData hitData = new HitData();

                    hitData.m_damage.m_damage = 1E+10f;
                    character.Damage(hitData);
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
                if (character.IsPlayer() && ((Player)character).GetPlayerID() != Player.m_localPlayer.GetPlayerID())
                {
                    HitData hitData = new HitData();

                    hitData.m_damage.m_damage = 1E+10f;
                    character.Damage(hitData);
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
                            username,
                            message
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
                        username,
                        message
                    });
                }
            }
        }
    }
}
