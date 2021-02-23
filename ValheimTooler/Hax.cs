using System;
using System.Collections.Generic;
using UnityEngine;
using ValheimTooler.UI;

namespace ValheimTooler
{
    public class InventoryItem
    {
        public GameObject itemPrefab;
        public ItemDrop itemDrop;

        public InventoryItem(GameObject itemPrefab, ItemDrop itemDrop)
        {
            this.itemPrefab = itemPrefab;
            this.itemDrop = itemDrop;
        }
    }

    public class Hax : MonoBehaviour
    {
        // GUI
        private Rect valheimToolerRect;
        private Rect itemGiverRect;
        private Vector2 itemGiverScrollPosition;
        List<InventoryItem> items = new List<InventoryItem>();
        List<GUIContent> itemsGUI = new List<GUIContent>();

        // For World Message functionnality
        private string worldMessageText = "";

        // For Chat Message functionnality
        private string chatUsernameText = "";
        private string chatMessageText = "";
        private bool isShoutMessage = false;

        // Player Teleport
        private string playerTeleportTargetText = "";

        private bool showHax = true;

        private int selectedItem = 0;
        private string quantityItem = "1";
        private string qualityItem = "1";

        static Hax instance;

        public static Hax Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<Hax>();
                }

                return instance;
            }
        }

        public KeyCode closeFocusedWindowKey = KeyCode.Q;
        public int prefixLabelSlideButton = 1;
        public Action onGUI;

        public void Start()
        {
            valheimToolerRect = new Rect(5, Screen.height / 2, 350, 300);
            itemGiverRect = new Rect(Screen.width - 400, Screen.height / 2, 400, 400);

            items.Clear();
            itemsGUI.Clear();
            foreach (GameObject gameObject in ObjectDB.instance.m_items)
            {
                ItemDrop component = gameObject.GetComponent<ItemDrop>();

                if (component.m_itemData?.m_shared?.m_icons?.Length <= 0) continue;

                Texture texture = SpriteManager.TextureFromSprite(component.m_itemData.m_shared.m_icons[0]);
                itemsGUI.Add(new GUIContent(texture, component.m_itemData.m_shared.m_name));
                items.Add(new InventoryItem(gameObject, component));
            }
        }
        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.Delete))
            {
                showHax = !showHax;
            }
        }

        public void OnGUI()
        {
            onGUI?.Invoke();

            if (showHax)
            {
                valheimToolerRect = GUILayout.Window(0, valheimToolerRect, ValheimToolerWindow, "ValheimTooler", RGUIStyle.darkWindow);

                itemGiverRect = GUILayout.Window(1, itemGiverRect, ItemGiverWindow, "Item Giver", RGUIStyle.darkWindow);
            }
        }

        void ValheimToolerWindow(int windowID)
        {
            GUILayout.BeginVertical("World Message", RGUIStyle.alignLeftBox, GUILayout.ExpandWidth(false));
            {
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("Message: ");
                    worldMessageText = GUILayout.TextField(worldMessageText, GUILayout.ExpandWidth(true));
                }
                GUILayout.EndHorizontal();


                if (GUILayout.Button("Send world message", RGUIStyle.flatButton))
                {
                    Hax.MessageAllInRange(MessageHud.MessageType.Center, worldMessageText);
                }
            }
            GUILayout.EndVertical();

            GUILayout.BeginVertical("Chat Message", RGUIStyle.alignLeftBox, GUILayout.ExpandWidth(false));
            {
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("Username: ");
                    chatUsernameText = GUILayout.TextField(chatUsernameText, GUILayout.ExpandWidth(true));
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("Message: ");
                    chatMessageText = GUILayout.TextField(chatMessageText, GUILayout.ExpandWidth(true));
                }
                GUILayout.EndHorizontal();

                isShoutMessage = GUILayout.Toggle(isShoutMessage, "Shout message");

                if (GUILayout.Button("Send chat message", RGUIStyle.flatButton))
                {
                    Hax.ChatMessage(isShoutMessage ? Talker.Type.Shout : Talker.Type.Normal, chatUsernameText, chatMessageText);
                }
            }
            GUILayout.EndVertical();

            GUILayout.BeginVertical("Teleport to Player", RGUIStyle.alignLeftBox, GUILayout.ExpandWidth(false));
            {
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("Player name: ");
                    playerTeleportTargetText = GUILayout.TextField(playerTeleportTargetText, GUILayout.ExpandWidth(true));
                }
                GUILayout.EndHorizontal();

                if (GUILayout.Button("Teleport", RGUIStyle.flatButton))
                {
                    TeleportPlayerTo(playerTeleportTargetText);
                }
            }
            GUILayout.EndVertical();

            GUI.DragWindow();
        }

        void ItemGiverWindow(int windowID)
        {
            if (ObjectDB.instance == null || ObjectDB.instance.m_items.Count == 0) return;

            itemGiverScrollPosition = GUILayout.BeginScrollView(itemGiverScrollPosition, GUILayout.Height(350));
            {
                selectedItem = GUILayout.SelectionGrid(selectedItem, itemsGUI.ToArray(), 4, RGUIStyle.flatButton);
            }
            GUILayout.EndScrollView();

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Quantity :");
                quantityItem = GUILayout.TextField(quantityItem, GUILayout.ExpandWidth(true));
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Quality :");
                qualityItem = GUILayout.TextField(qualityItem, GUILayout.ExpandWidth(true));
            }
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Add item", RGUIStyle.flatButton))
            {
                int quantity;
                int quality;

                if (int.TryParse(quantityItem, out quantity) && int.TryParse(qualityItem, out quality))
                    AddItemToPlayer(items[selectedItem].itemPrefab.name, quantity, quality);
            }

            if (GUI.tooltip != "")
            {
                GUIContent tooltip = new GUIContent(Localization.instance.Localize(GUI.tooltip));
                Vector2 tooltip_size = new GUIStyle(GUI.skin.box).CalcSize(tooltip);
                Vector2 tooltip_pos = new Vector2();

                if (Event.current.mousePosition.x + tooltip_size.x > itemGiverRect.width)
                    tooltip_pos.x = Event.current.mousePosition.x - ((Event.current.mousePosition.x + tooltip_size.x) - (itemGiverRect.width));
                else
                    tooltip_pos.x = Event.current.mousePosition.x;
                if (Event.current.mousePosition.y + 30 + tooltip_size.y > itemGiverRect.height)
                    tooltip_pos.y = Event.current.mousePosition.y + 30 - ((Event.current.mousePosition.y + 30 + tooltip_size.y) - (itemGiverRect.height));
                else
                    tooltip_pos.y = Event.current.mousePosition.y + 30;

                GUI.Box(new Rect(tooltip_pos.x, tooltip_pos.y, tooltip_size.x, tooltip_size.y), tooltip, RGUIStyle.alignLeftBox);
            }

            GUI.DragWindow();
        }

        private static void TeleportPlayerTo(string username)
        {
            Player localPlayer = Player.m_localPlayer;
            List<ZNet.PlayerInfo> players = ZNet.instance.GetPlayerList();

            if (localPlayer == null) return;

            foreach (ZNet.PlayerInfo player in players)
            {
                ZDOID characterID = player.m_characterID;
                if (!characterID.IsNone() && player.m_name.ToLower().Equals(username.ToLower()))
                {
                    localPlayer.TeleportTo(player.m_position, localPlayer.transform.rotation, true);
                    break;
                }
            }
        }

        private static void AddItemToPlayer(string itemPrefab, int quantity, int quality)
        {
            long playerID = Player.m_localPlayer.GetPlayerID();
            string playerName = Player.m_localPlayer.GetPlayerName();

            Player.m_localPlayer.GetInventory().AddItem(itemPrefab, quantity, quality, 0, playerID, playerName);
            Game.instance.GetPlayerProfile().m_playerStats.m_crafts++;
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
                    ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody, "ChatMessage", new object[]
                    {
                        playerSender.GetHeadPoint(),
                        2,
                        username,
                        message
                    });
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
