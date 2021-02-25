using System;
using System.Collections.Generic;
using System.Reflection;
using RapidGUI;
using UnityEngine;
using ValheimTooler.Core;
using ValheimTooler.UI;

namespace ValheimTooler
{
    public class EntryPoint : MonoBehaviour
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
        private int playerTargetIdx = 0;

        private string entityPrefabText = "";
        private string entityQuantityText = "";
        private string entityLevelText = "";

        private int playerDamageIdx = 0;
        private string damageToDeal = "1";

        private bool showHax = true;

        private int selectedItem = 0;
        private string quantityItem = "1";
        private string qualityItem = "1";

        private string godModeButton = "God Mode: OFF";
        private bool isGodMode = false;

        private string flyModeButton = "Fly Mode: OFF";
        private bool isFlyMode = false;

        private string ghostModeButton = "Ghost Mode: OFF";
        private bool isGhostMode = false;

        private string noPlacementCostButton = "No Placement Cost: OFF";
        private bool noPlacementCost = false;

        private string freeFlyButton = "Free Fly Camera: OFF";
        private bool isFreeFly = false;

        private string windDebugButton = "Wind Debug: OFF";
        private bool isWindDebug = false;
        private float windAngle = 0f;
        private float windIntensity = 0f;

        private int guardianPowerIdx = 0;
        private string[] guardianPowers = {
            "GP_Eikthyr",
            "GP_TheElder",
            "GP_Bonemass",
            "GP_Moder",
            "GP_Yagluth"
        };

        private int playerHealIdx = 0;

        private int skillNameIdx = 0;
        private int skillLevelIdx = 0;

        private readonly List<Player> _players = new List<Player>();
        //private readonly List<ZNet.PlayerInfo> _znetPlayers = new List<ZNet.PlayerInfo>();

        private Shader DefaultShader;
        private Texture2D texture;
        private float timer;
        private float interval = 5;

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

            texture = new Texture2D(2, 2, TextureFormat.ARGB32, false);
            texture.SetPixel(0, 0, Color.red);
            texture.SetPixel(1, 0, Color.red);
            texture.SetPixel(0, 1, Color.red);
            texture.SetPixel(1, 1, Color.red);
            texture.Apply();
        }
        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.Delete))
            {
                showHax = !showHax;
            }

            if (Time.time >= this.timer)
            {
                //foreach (Player player in _players)
                //{
                //    if (player != null)
                //    {
                //        var rend = player.GetComponentsInChildren<Renderer>();
                //        foreach (Renderer renderer in rend)
                //            renderer.material.shader = DefaultShader;
                //    }
                //}
                _players.Clear();

                List<Player> players = Player.GetAllPlayers();

                if (players != null && Camera.main != null && Player.m_localPlayer != null)
                {
                    foreach (Player player in players)
                    {
                        var distance = Vector3.Distance(Camera.main.transform.position, player.transform.position);

                        if (!player.GetPlayerName().Equals(Player.m_localPlayer.GetPlayerName()) && distance > 2)
                        {
                            _players.Add(player);
                            //var rend = player.GetComponentInChildren<Renderer>();
                            //if (rend.material.name.Contains("S") || rend.material.name.Contains("B"))
                            //{
                            //    _players.Add(player);
                            //    var rend2 = player.GetComponentsInChildren<Renderer>();

                            //    foreach (Renderer renderer in rend2)
                            //    {
                            //        renderer.material.mainTexture = texture;
                            //        DefaultShader = renderer.material.shader;
                            //        renderer.material.shader = Shader.Find("Hidden/Internal-GUITexture");
                            //    }
                            //}
                            //else if (distance > 12)
                            //{
                            //    _players.Add(player);
                            //}
                        }
                    }

                    this.timer = Time.time + this.interval;
                }
            }
        }

        public void OnGUI()
        {
            if (showHax)
            {
                valheimToolerRect = GUILayout.Window(0, valheimToolerRect, ValheimToolerWindow, "ValheimTooler", RGUIStyle.darkWindow);

                itemGiverRect = GUILayout.Window(1, itemGiverRect, ItemGiverWindow, "Item Giver", RGUIStyle.darkWindow);

                if (Camera.main != null && Player.m_localPlayer != null)
                {
                    var main = Camera.main;
                    foreach (Player player in _players)
                    {
                        Vector3 vector = main.WorldToScreenPoint(player.transform.position);

                        if (!player.GetPlayerName().Equals(Player.m_localPlayer.GetPlayerName()) && vector.z > -1)
                        {
                            float a = Math.Abs(main.WorldToScreenPoint(player.GetHeadPoint()).y - main.WorldToScreenPoint(player.transform.position).y);
                            var distance = Vector3.Distance(main.transform.position, player.transform.position);

                            Box(vector.x, Screen.height - vector.y, a * 0.65f, a, texture, 1f);
                            GUI.Label(new Rect((int)vector.x - 5, Screen.height - vector.y - 10, 40, 40), ((int)(distance)).ToString());
                        }
                    }
                }
            }
        }

        public static void Box(float x, float y, float width, float height, Texture2D text, float thickness = 1f)
        {
            RectOutlined(x - width / 2f, y - height, width, height, text, thickness);
        }

        public static void RectOutlined(float x, float y, float width, float height, Texture2D text, float thickness = 1f)
        {
            RectFilled(x, y, thickness, height, text);
            RectFilled(x + width - thickness, y, thickness, height, text);
            RectFilled(x + thickness, y, width - thickness * 2f, thickness, text);
            RectFilled(x + thickness, y + height - thickness, width - thickness * 2f, thickness, text);
        }

        public static void RectFilled(float x, float y, float width, float height, Texture2D text)
        {
            GUI.DrawTexture(new Rect(x, y, width, height), text);
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
                    EntryPoint.MessageAllInRange(MessageHud.MessageType.Center, worldMessageText);
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
                    EntryPoint.ChatMessage(isShoutMessage ? Talker.Type.Shout : Talker.Type.Normal, chatUsernameText, chatMessageText);
                }
            }
            GUILayout.EndVertical();

            GUILayout.BeginVertical("Teleport to Player", RGUIStyle.alignLeftBox, GUILayout.ExpandWidth(false));
            {
                List<ZNet.PlayerInfo> players;
                if (ZNet.instance == null)
                {
                    players = new List<ZNet.PlayerInfo>();
                }
                else
                {
                    players = ZNet.instance.GetPlayerList();
                }

                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("Player: ");

                    List<string> znetPlayers = new List<string>();

                    foreach (ZNet.PlayerInfo playerInfo in players)
                    {
                        znetPlayers.Add(playerInfo.m_name);
                    }

                    playerTargetIdx = RGUI.SelectionPopup(playerTargetIdx, znetPlayers.ToArray());
                }
                GUILayout.EndHorizontal();

                if (GUILayout.Button("Teleport", RGUIStyle.flatButton))
                {
                    TeleportPlayerTo(players[playerTargetIdx]);
                }
            }
            GUILayout.EndVertical();

            GUILayout.BeginVertical("Spawn Entity", RGUIStyle.alignLeftBox, GUILayout.ExpandWidth(false));
            {
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("Entity Prefab: ");
                    entityPrefabText = GUILayout.TextField(entityPrefabText, GUILayout.ExpandWidth(true));
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("Quantity: ");
                    entityQuantityText = GUILayout.TextField(entityQuantityText, GUILayout.ExpandWidth(true));
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("Entity Level: ");
                    entityLevelText = GUILayout.TextField(entityLevelText, GUILayout.ExpandWidth(true));
                }
                GUILayout.EndHorizontal();

                if (GUILayout.Button("Spawn", RGUIStyle.flatButton))
                {
                    int entityLevel;
                    int entityQuantity;
                    GameObject prefab = ZNetScene.instance.GetPrefab(entityPrefabText);

                    if (int.TryParse(entityQuantityText, out entityQuantity) && int.TryParse(entityLevelText, out entityLevel) && prefab != null)
                    {
                        for (var j = 0; j < entityQuantity; j++)
                        {
                            Vector3 b = UnityEngine.Random.insideUnitSphere * 0.5f;
                            Character component2 = UnityEngine.Object.Instantiate<GameObject>(prefab, Player.m_localPlayer.transform.position + Player.m_localPlayer.transform.forward * 2f + Vector3.up + b, Quaternion.identity).GetComponent<Character>();
                            component2.SetLevel(entityLevel);
                        }
                    }
                }
            }
            GUILayout.EndVertical();

            GUILayout.BeginVertical("Deal Damage", RGUIStyle.alignLeftBox, GUILayout.ExpandWidth(false));
            {
                List<Player> players = Player.GetAllPlayers();

                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("Player: ");

                    List<string> playerUsernames = new List<string>();

                    if (players != null)
                    {
                        foreach (Player player in players)
                        {
                            playerUsernames.Add(player.GetPlayerName());
                        }
                    }
                    playerDamageIdx = RGUI.SelectionPopup(playerDamageIdx, playerUsernames.ToArray());
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("Damage value: ");
                    damageToDeal = GUILayout.TextField(damageToDeal, GUILayout.ExpandWidth(true));
                }
                GUILayout.EndHorizontal();

                if (GUILayout.Button("Damage player", RGUIStyle.flatButton))
                {
                    HitData hitData = new HitData();
                    int damage;
                    if (int.TryParse(damageToDeal, out damage))
                    {
                        hitData.m_damage.m_damage = damage;
                        players[playerDamageIdx].Damage(hitData);
                    }
                }
                if (GUILayout.Button("Kill All (no players)", RGUIStyle.flatButton))
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
            }
            GUILayout.EndVertical();

            GUILayout.BeginVertical("Remove Drop", RGUIStyle.alignLeftBox, GUILayout.ExpandWidth(false));
            {
                if (GUILayout.Button("Remove All Drop", RGUIStyle.flatButton))
                {
                    ItemDrop[] array2 = UnityEngine.Object.FindObjectsOfType<ItemDrop>();
                    for (int i = 0; i < array2.Length; i++)
                    {
                        ZNetView component = array2[i].GetComponent<ZNetView>();
                        if (component)
                        {
                            component.Destroy();
                        }
                    }
                }
            }
            GUILayout.EndVertical();

            GUILayout.BeginVertical("Player", RGUIStyle.alignLeftBox, GUILayout.ExpandWidth(false));
            {
                List<Player> players = Player.GetAllPlayers();
                List<string> playerUsernames = new List<string>();

                if (GUILayout.Button(godModeButton, RGUIStyle.flatButton))
                {
                    if (isGodMode)
                    {
                        isGodMode = false;
                        godModeButton = "God Mode: OFF";
                    }
                    else
                    {
                        isGodMode = true;
                        godModeButton = "God Mode: ON";
                    }
                    Player.m_localPlayer.SetGodMode(isGodMode);
                }

                if (GUILayout.Button(flyModeButton, RGUIStyle.flatButton))
                {
                    if (isFlyMode)
                    {
                        isFlyMode = false;
                        flyModeButton = "Fly Mode: OFF";
                    }
                    else
                    {
                        isFlyMode = true;
                        flyModeButton = "Fly Mode: ON";
                    }

                    if (Player.m_localPlayer != null)
                    {
                        Player.m_localPlayer.SetFieldValue<bool>("m_debugFly", isFlyMode);
                        ZNetView m_nview = Player.m_localPlayer.GetFieldValue<ZNetView>("m_nview");
                        m_nview.GetZDO().Set("DebugFly", isFlyMode);

                    }
                }

                if (GUILayout.Button(ghostModeButton, RGUIStyle.flatButton))
                {
                    if (isGhostMode)
                    {
                        isGhostMode = false;
                        ghostModeButton = "Ghost Mode: OFF";
                    }
                    else
                    {
                        isGhostMode = true;
                        ghostModeButton = "Ghost Mode: ON";
                    }

                    if (Player.m_localPlayer != null)
                    {
                        Player.m_localPlayer.SetGhostMode(isGhostMode);

                    }
                }

                if (GUILayout.Button("Explore Minimap", RGUIStyle.flatButton))
                {
                    Minimap.instance.ExploreAll();
                }

                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("Player: ");

                    if (players != null)
                    {
                        foreach (Player player in players)
                        {
                            playerUsernames.Add(player.GetPlayerName());
                        }
                    }
                    playerHealIdx = RGUI.SelectionPopup(playerHealIdx, playerUsernames.ToArray());

                    if (GUILayout.Button("Heal", RGUIStyle.flatButton))
                    {
                        players[playerHealIdx].Heal(players[playerHealIdx].GetMaxHealth(), true);
                    }
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("Skill: ");

                    List<string> skills = new List<string>();
                    List<string> levels = new List<string>();

                    foreach (object obj in Enum.GetValues(typeof(Skills.SkillType)))
                    {
                        Skills.SkillType skillType = (Skills.SkillType)obj;

                        skills.Add(skillType.ToString());
                    }
                    for (var i = 1; i <= 100; i++)
                    {
                        levels.Add(i.ToString());
                    }

                    skillNameIdx = RGUI.SelectionPopup(skillNameIdx, skills.ToArray());
                    skillLevelIdx = RGUI.SelectionPopup(skillLevelIdx, levels.ToArray());

                    if (GUILayout.Button("Set Level", RGUIStyle.flatButton))
                    {
                        int level;
                        if (int.TryParse(levels[skillLevelIdx], out level))
                        {
                            Skills.SkillType skillType = (Skills.SkillType)Enum.Parse(typeof(Skills.SkillType), skills[skillNameIdx]);
                            Skills.Skill skill = (Skills.Skill)Player.m_localPlayer.GetSkills().CallMethod("GetSkill", skillType);

                            int offset = (int)(level - skill.m_level);

                            Player.m_localPlayer.GetSkills().CheatRaiseSkill(skills[skillNameIdx].ToLower(), offset);
                        }
                    }
                }
                GUILayout.EndHorizontal();

                if (GUILayout.Button(noPlacementCostButton, RGUIStyle.flatButton))
                {
                    if (noPlacementCost)
                    {
                        noPlacementCost = false;
                        noPlacementCostButton = "No Placement Cost: OFF";
                    }
                    else
                    {
                        noPlacementCost = true;
                        noPlacementCostButton = "No Placement Cost: ON";
                    }
                    Player.m_localPlayer.SetFieldValue<bool>("m_noPlacementCost", noPlacementCost);
                    Player.m_localPlayer.CallMethod("UpdateAvailablePiecesList");
                }

                if (GUILayout.Button(freeFlyButton, RGUIStyle.flatButton))
                {
                    if (isFreeFly)
                    {
                        isFreeFly = false;
                        freeFlyButton = "Free Fly Camera: OFF";
                    }
                    else
                    {
                        isFreeFly = true;
                        freeFlyButton = "Free Fly Camera: ON";
                    }
                    GameCamera.instance.ToggleFreeFly();
                }

                GUILayout.BeginVertical("Wind Manager", RGUIStyle.alignLeftBox, GUILayout.ExpandWidth(false));
                {
                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.Label("Wind Angle: ");
                        windAngle = GUILayout.HorizontalSlider(windAngle, 0f, 360f, GUILayout.ExpandWidth(true));
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.Label("Wind Intensity: ");
                        windIntensity = GUILayout.HorizontalSlider(windIntensity, 0f, 1f, GUILayout.ExpandWidth(true));
                    }
                    GUILayout.EndHorizontal();

                    if (GUILayout.Button(windDebugButton, RGUIStyle.flatButton))
                    {
                        if (isWindDebug)
                        {
                            isWindDebug = false;
                            windDebugButton = "Wind Debug: OFF";
                            EnvMan.instance.ResetDebugWind();
                        }
                        else
                        {
                            isWindDebug = true;
                            windDebugButton = "Wind Debug: ON";
                            EnvMan.instance.SetDebugWind(windAngle, windIntensity);
                        }
                    }
                }
                GUILayout.EndVertical();

                GUILayout.BeginVertical("Guardian Power", RGUIStyle.alignLeftBox, GUILayout.ExpandWidth(false));
                {
                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.Label("Power name: ");
                        guardianPowerIdx = RGUI.SelectionPopup(guardianPowerIdx, guardianPowers);
                    }
                    GUILayout.EndHorizontal();


                    if (GUILayout.Button("Activate Power", RGUIStyle.flatButton))
                    {
                        ZLog.Log("Guardian Power: " + Player.m_localPlayer.GetFieldValue<string>("m_guardianPower"));
                        List<Player> list = new List<Player>();
                        Player.GetPlayersInRange(Player.m_localPlayer.transform.position, 10f, list);
                        foreach (Player player in list)
                        {
                            player.GetSEMan().AddStatusEffect(guardianPowers[guardianPowerIdx], true);
                        }
                    }
                }
                GUILayout.EndVertical();
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

        private static void TeleportPlayerTo(ZNet.PlayerInfo target)
        {
            Player localPlayer = Player.m_localPlayer;
            List<ZNet.PlayerInfo> players = ZNet.instance.GetPlayerList();

            if (localPlayer == null) return;

            ZDOID characterID = target.m_characterID;

            if (!characterID.IsNone())
            {
                localPlayer.TeleportTo(target.m_position, localPlayer.transform.rotation, true);
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
