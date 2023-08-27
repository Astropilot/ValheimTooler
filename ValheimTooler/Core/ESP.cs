using System;
using System.Collections.Generic;
using UnityEngine;
using ValheimTooler.UI;
using ValheimTooler.Utils;

namespace ValheimTooler.Core
{
    public static class ESP
    {
        private static readonly Texture2D s_playersBoxTexture;
        private static readonly Texture2D s_monstersAndOthersBoxTexture;
        private static readonly Texture2D s_tamedMonstersBoxTexture;
        private static readonly Texture2D s_pickablesBoxTexture;
        private static readonly Texture2D s_dropsBoxTexture;
        private static readonly Texture2D s_depositsBoxTexture;

        private static readonly Color s_playersColor = Color.magenta;
        private static readonly Color s_monstersAndOthersColor = Color.red;
        private static readonly Color s_tamedMonstersColor = new Color(1, 0.3f, 0, 1); // Orange
        private static readonly Color s_pickablesColor = new Color(0.13f, 0.58f, 0.89f, 1); // Light blue
        private static readonly Color s_dropsColor = new Color(0.13f, 0.72f, 0.11f, 1); // Light green
        private static readonly Color s_depositsColor = Color.yellow;

        private static readonly List<Character> s_characters = new List<Character>();
        private static readonly List<Pickable> s_pickables = new List<Pickable>();
        private static readonly List<PickableItem> s_pickableItems = new List<PickableItem>();
        private static readonly List<ItemDrop> s_drops = new List<ItemDrop>();
        private static readonly List<Destructible> s_depositsDestructible = new List<Destructible>();
        private static readonly List<MineRock5> s_mineRock5s = new List<MineRock5>();

        private static float s_updateTimer = 0f;
        private static readonly float s_updateTimerInterval = 1.5f;

        public static bool s_showPlayerESP = false;
        public static bool s_showMonsterESP = false;
        public static bool s_showDroppedESP = false;
        public static bool s_showDepositESP = false;
        public static bool s_showPickableESP = false;

        static ESP()
        {
            ESP.s_playersBoxTexture = new Texture2D(2, 2, TextureFormat.ARGB32, false);
            ESP.s_playersBoxTexture.SetPixel(0, 0, s_playersColor);
            ESP.s_playersBoxTexture.SetPixel(1, 0, s_playersColor);
            ESP.s_playersBoxTexture.SetPixel(0, 1, s_playersColor);
            ESP.s_playersBoxTexture.SetPixel(1, 1, s_playersColor);
            ESP.s_playersBoxTexture.Apply();

            ESP.s_monstersAndOthersBoxTexture = new Texture2D(2, 2, TextureFormat.ARGB32, false);
            ESP.s_monstersAndOthersBoxTexture.SetPixel(0, 0, s_monstersAndOthersColor);
            ESP.s_monstersAndOthersBoxTexture.SetPixel(1, 0, s_monstersAndOthersColor);
            ESP.s_monstersAndOthersBoxTexture.SetPixel(0, 1, s_monstersAndOthersColor);
            ESP.s_monstersAndOthersBoxTexture.SetPixel(1, 1, s_monstersAndOthersColor);
            ESP.s_monstersAndOthersBoxTexture.Apply();

            ESP.s_tamedMonstersBoxTexture = new Texture2D(2, 2, TextureFormat.ARGB32, false);
            ESP.s_tamedMonstersBoxTexture.SetPixel(0, 0, s_tamedMonstersColor);
            ESP.s_tamedMonstersBoxTexture.SetPixel(1, 0, s_tamedMonstersColor);
            ESP.s_tamedMonstersBoxTexture.SetPixel(0, 1, s_tamedMonstersColor);
            ESP.s_tamedMonstersBoxTexture.SetPixel(1, 1, s_tamedMonstersColor);
            ESP.s_tamedMonstersBoxTexture.Apply();

            ESP.s_pickablesBoxTexture = new Texture2D(2, 2, TextureFormat.ARGB32, false);
            ESP.s_pickablesBoxTexture.SetPixel(0, 0, s_pickablesColor);
            ESP.s_pickablesBoxTexture.SetPixel(1, 0, s_pickablesColor);
            ESP.s_pickablesBoxTexture.SetPixel(0, 1, s_pickablesColor);
            ESP.s_pickablesBoxTexture.SetPixel(1, 1, s_pickablesColor);
            ESP.s_pickablesBoxTexture.Apply();

            ESP.s_dropsBoxTexture = new Texture2D(2, 2, TextureFormat.ARGB32, false);
            ESP.s_dropsBoxTexture.SetPixel(0, 0, s_dropsColor);
            ESP.s_dropsBoxTexture.SetPixel(1, 0, s_dropsColor);
            ESP.s_dropsBoxTexture.SetPixel(0, 1, s_dropsColor);
            ESP.s_dropsBoxTexture.SetPixel(1, 1, s_dropsColor);
            ESP.s_dropsBoxTexture.Apply();

            ESP.s_depositsBoxTexture = new Texture2D(2, 2, TextureFormat.ARGB32, false);
            ESP.s_depositsBoxTexture.SetPixel(0, 0, s_depositsColor);
            ESP.s_depositsBoxTexture.SetPixel(1, 0, s_depositsColor);
            ESP.s_depositsBoxTexture.SetPixel(0, 1, s_depositsColor);
            ESP.s_depositsBoxTexture.SetPixel(1, 1, s_depositsColor);
            ESP.s_depositsBoxTexture.Apply();

            UnityEngine.Object.DontDestroyOnLoad(ESP.s_playersBoxTexture);
            UnityEngine.Object.DontDestroyOnLoad(ESP.s_monstersAndOthersBoxTexture);
            UnityEngine.Object.DontDestroyOnLoad(ESP.s_tamedMonstersBoxTexture);
            UnityEngine.Object.DontDestroyOnLoad(ESP.s_pickablesBoxTexture);
            UnityEngine.Object.DontDestroyOnLoad(ESP.s_dropsBoxTexture);
            UnityEngine.Object.DontDestroyOnLoad(ESP.s_depositsBoxTexture);
        }

        public static void Start()
        {
            return;
        }

        public static void Update()
        {
            if (Time.time >= s_updateTimer)
            {
                s_characters.Clear();
                s_pickables.Clear();
                s_pickableItems.Clear();
                s_drops.Clear();
                s_depositsDestructible.Clear();
                s_mineRock5s.Clear();

                if (ESP.s_showPlayerESP || ESP.s_showMonsterESP)
                {
                    List<Character> characters = Character.GetAllCharacters();

                    if (characters != null && Camera.main != null && Player.m_localPlayer != null)
                    {
                        foreach (Character character in characters)
                        {
                            var distance = Vector3.Distance(Camera.main.transform.position, character.transform.position);

                            if (character.IsPlayer() && ((Player)character).GetPlayerID() == Player.m_localPlayer.GetPlayerID())
                            {
                                continue;
                            }

                            if (distance > 2 && (!ConfigManager.s_espRadiusEnabled.Value || distance < ConfigManager.s_espRadius.Value))
                            {
                                s_characters.Add(character);
                            }
                        }
                    }
                }

                if (ESP.s_showPickableESP)
                {
                    var pickables = UnityEngine.Object.FindObjectsOfType<Pickable>();

                    if (pickables != null && Camera.main != null)
                    {
                        foreach (Pickable pickable in pickables)
                        {
                            var distance = Vector3.Distance(Camera.main.transform.position, pickable.transform.position);

                            if (distance > 2 && (!ConfigManager.s_espRadiusEnabled.Value || distance < ConfigManager.s_espRadius.Value))
                            {
                                s_pickables.Add(pickable);
                            }
                        }
                    }

                    var pickableItems = UnityEngine.Object.FindObjectsOfType<PickableItem>();

                    if (pickableItems != null && Camera.main != null)
                    {
                        foreach (PickableItem pickableItem in pickableItems)
                        {
                            var distance = Vector3.Distance(Camera.main.transform.position, pickableItem.transform.position);

                            if (distance > 2 && (!ConfigManager.s_espRadiusEnabled.Value || distance < ConfigManager.s_espRadius.Value))
                            {
                                s_pickableItems.Add(pickableItem);
                            }
                        }
                    }
                }

                if (ESP.s_showDroppedESP)
                {
                    var itemDrops = UnityEngine.Object.FindObjectsOfType<ItemDrop>();

                    if (itemDrops != null && Camera.main != null)
                    {
                        foreach (ItemDrop itemDrop in itemDrops)
                        {
                            var distance = Vector3.Distance(Camera.main.transform.position, itemDrop.transform.position);

                            if (distance > 2 && (!ConfigManager.s_espRadiusEnabled.Value || distance < ConfigManager.s_espRadius.Value))
                            {
                                s_drops.Add(itemDrop);
                            }
                        }
                    }
                }

                if (ESP.s_showDepositESP)
                {
                    var mineRock5s = UnityEngine.Object.FindObjectsOfType<MineRock5>();

                    if (mineRock5s != null && Camera.main != null)
                    {
                        foreach (MineRock5 mineRock5 in mineRock5s)
                        {
                            string name = mineRock5.GetHoverText().ToLower();

                            if (name.Contains("rock") || name.Length == 0)
                                continue;

                            var distance = Vector3.Distance(Camera.main.transform.position, mineRock5.transform.position);

                            if (distance > 2 && (!ConfigManager.s_espRadiusEnabled.Value || distance < ConfigManager.s_espRadius.Value))
                            {
                                s_mineRock5s.Add(mineRock5);
                            }
                        }
                    }

                    var destructibles = UnityEngine.Object.FindObjectsOfType<Destructible>();

                    if (destructibles != null && Camera.main != null)
                    {
                        foreach (Destructible destructible in destructibles)
                        {
                            HoverText component = destructible.GetComponent<HoverText>();
                            if (component == null)
                                continue;
                            string text = component.m_text.ToLower();

                            if (!text.Contains("deposit") && !text.Contains("piece_mudpile"))
                            {
                                continue;
                            }

                            var distance = Vector3.Distance(Camera.main.transform.position, destructible.transform.position);

                            if (distance > 2 && (!ConfigManager.s_espRadiusEnabled.Value || distance < ConfigManager.s_espRadius.Value))
                            {
                                s_depositsDestructible.Add(destructible);
                            }
                        }
                    }
                }

                s_updateTimer = Time.time + s_updateTimerInterval;
            }
        }

        public static void DisplayGUI()
        {
            if (Camera.main != null && Player.m_localPlayer != null)
            {
                var main = Camera.main;
                var labelSkin = new GUIStyle(InterfaceMaker.CustomSkin.label);

                if (ESP.s_showPlayerESP || ESP.s_showMonsterESP)
                {
                    foreach (Character character in s_characters)
                    {
                        if (character == null || (!ESP.s_showMonsterESP && !character.IsPlayer()))
                        {
                            continue;
                        }
                        Vector3 vector = main.WorldToScreenPoint(character.transform.position);

                        if (vector.z > -1)
                        {
                            float a = Math.Abs(main.WorldToScreenPoint(character.GetEyePoint()).y - vector.y);

                            if (character.IsPlayer() && ESP.s_showPlayerESP)
                            {
                                string espLabel = ((Player)character).GetPlayerName() + $" [{(int)vector.z}]";
                                Box(vector.x, Screen.height - vector.y, a * 0.65f, a, s_playersBoxTexture, 1f);
                                labelSkin.normal.textColor = s_playersColor;
                                GUI.Label(new Rect((int)vector.x - 10, Screen.height - vector.y - 5, 150, 40), espLabel, labelSkin);
                            }
                            else if (!character.IsPlayer() && ESP.s_showMonsterESP)
                            {
                                string espLabel = character.GetHoverName() + $" [{(int)vector.z}]";
                                Box(vector.x, Screen.height - vector.y, a * 0.65f, a, character.IsTamed() ? s_tamedMonstersBoxTexture : s_monstersAndOthersBoxTexture, 1f);
                                labelSkin.normal.textColor = character.IsTamed() ? s_tamedMonstersColor : s_monstersAndOthersColor;
                                GUI.Label(new Rect((int)vector.x - 10, Screen.height - vector.y - 5, 150, 40), espLabel, labelSkin);
                            }
                        }
                    }
                }

                if (ESP.s_showPickableESP)
                {
                    labelSkin.normal.textColor = s_pickablesColor;
                    foreach (Pickable pickable in s_pickables)
                    {
                        if (pickable == null)
                        {
                            continue;
                        }
                        Vector3 vector = main.WorldToScreenPoint(pickable.transform.position);

                        if (vector.z > -1)
                        {
                            string espLabel = $"{Localization.instance.Localize(pickable.GetHoverName())} [{(int)vector.z}]";
                            
                            GUI.Label(new Rect((int)vector.x - 5, Screen.height - vector.y - 5, 150, 40), espLabel, labelSkin);
                        }
                    }
                    foreach (PickableItem pickableItem in s_pickableItems)
                    {
                        if (pickableItem == null)
                        {
                            continue;
                        }
                        Vector3 vector = main.WorldToScreenPoint(pickableItem.transform.position);

                        if (vector.z > -1)
                        {
                            string espLabel = $"{Localization.instance.Localize(pickableItem.GetHoverName())} [{(int)vector.z}]";

                            GUI.Label(new Rect((int)vector.x - 5, Screen.height - vector.y - 5, 150, 40), espLabel, labelSkin);
                        }
                    }
                }

                if (ESP.s_showDroppedESP)
                {
                    labelSkin.normal.textColor = s_dropsColor;
                    foreach (ItemDrop itemDrop in s_drops)
                    {
                        if (itemDrop == null)
                        {
                            continue;
                        }
                        Vector3 vector = main.WorldToScreenPoint(itemDrop.transform.position);

                        if (vector.z > -1)
                        {
                            string espLabel = $"{Localization.instance.Localize(itemDrop.GetHoverName())} [{(int)vector.z}]";

                            GUI.Label(new Rect((int)vector.x - 5, Screen.height - vector.y - 5, 150, 40), espLabel, labelSkin);
                        }
                    }
                }

                if (ESP.s_showDepositESP)
                {
                    labelSkin.normal.textColor = s_depositsColor;

                    foreach (Destructible depositDestructible in s_depositsDestructible)
                    {
                        if (depositDestructible == null)
                        {
                            continue;
                        }
                        Vector3 vector = main.WorldToScreenPoint(depositDestructible.transform.position);

                        if (vector.z > -1)
                        {
                            string name = depositDestructible.GetComponent<HoverText>().GetHoverName();
                            string espLabel = $"{name} [{(int)vector.z}]";

                            GUI.Label(new Rect((int)vector.x - 5, Screen.height - vector.y - 5, 150, 40), espLabel, labelSkin);
                        }
                    }

                    foreach (MineRock5 mineRock5 in s_mineRock5s)
                    {
                        if (mineRock5 == null)
                        {
                            continue;
                        }
                        Vector3 vector = main.WorldToScreenPoint(mineRock5.transform.position);

                        if (vector.z > -1)
                        {
                            string espLabel = $"{mineRock5.GetHoverText()} [{(int)vector.z}]";

                            GUI.Label(new Rect((int)vector.x - 5, Screen.height - vector.y - 5, 150, 40), espLabel, labelSkin);
                        }
                    }
                }
            }
        }

        private static void Box(float x, float y, float width, float height, Texture2D text, float thickness = 1f)
        {
            RectOutlined(x - width / 2f, y - height, width, height, text, thickness);
        }

        private static void RectOutlined(float x, float y, float width, float height, Texture2D text, float thickness = 1f)
        {
            RectFilled(x, y, thickness, height, text);
            RectFilled(x + width - thickness, y, thickness, height, text);
            RectFilled(x + thickness, y, width - thickness * 2f, thickness, text);
            RectFilled(x + thickness, y + height - thickness, width - thickness * 2f, thickness, text);
        }

        private static void RectFilled(float x, float y, float width, float height, Texture2D text)
        {
            GUI.DrawTexture(new Rect(x, y, width, height), text);
        }
    }
}
