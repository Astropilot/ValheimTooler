using System;
using System.Collections.Generic;
using UnityEngine;
using ValheimTooler.UI;
using ValheimTooler.Utils;

namespace ValheimTooler.Core
{
    public static class ESP
    {
        private static readonly Texture2D s_playerBoxTexture;
        private static readonly Texture2D s_monsterAndOthersBoxTexture;
        private static readonly Texture2D s_tamedMonsterBoxTexture;

        private static readonly List<Character> s_characters = new List<Character>();
        private static readonly List<Pickable> s_pickables = new List<Pickable>();
        private static readonly List<PickableItem> s_pickableItems = new List<PickableItem>();
        private static readonly List<ItemDrop> s_drops = new List<ItemDrop>();
        private static readonly List<MineRock5> s_mineRock5s = new List<MineRock5>();

        private static float s_updateTimer = 0f;
        private static readonly float s_updateTimerInterval = 1.5f;

        static ESP()
        {
            ESP.s_playerBoxTexture = new Texture2D(2, 2, TextureFormat.ARGB32, false);
            ESP.s_playerBoxTexture.SetPixel(0, 0, Color.red);
            ESP.s_playerBoxTexture.SetPixel(1, 0, Color.red);
            ESP.s_playerBoxTexture.SetPixel(0, 1, Color.red);
            ESP.s_playerBoxTexture.SetPixel(1, 1, Color.red);
            ESP.s_playerBoxTexture.Apply();

            ESP.s_monsterAndOthersBoxTexture = new Texture2D(2, 2, TextureFormat.ARGB32, false);
            ESP.s_monsterAndOthersBoxTexture.SetPixel(0, 0, Color.magenta);
            ESP.s_monsterAndOthersBoxTexture.SetPixel(1, 0, Color.magenta);
            ESP.s_monsterAndOthersBoxTexture.SetPixel(0, 1, Color.magenta);
            ESP.s_monsterAndOthersBoxTexture.SetPixel(1, 1, Color.magenta);
            ESP.s_monsterAndOthersBoxTexture.Apply();

            ESP.s_tamedMonsterBoxTexture = new Texture2D(2, 2, TextureFormat.ARGB32, false);
            ESP.s_tamedMonsterBoxTexture.SetPixel(0, 0, Color.yellow);
            ESP.s_tamedMonsterBoxTexture.SetPixel(1, 0, Color.yellow);
            ESP.s_tamedMonsterBoxTexture.SetPixel(0, 1, Color.yellow);
            ESP.s_tamedMonsterBoxTexture.SetPixel(1, 1, Color.yellow);
            ESP.s_tamedMonsterBoxTexture.Apply();

            UnityEngine.Object.DontDestroyOnLoad(ESP.s_playerBoxTexture);
            UnityEngine.Object.DontDestroyOnLoad(ESP.s_monsterAndOthersBoxTexture);
            UnityEngine.Object.DontDestroyOnLoad(ESP.s_tamedMonsterBoxTexture);
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
                s_mineRock5s.Clear();

                if (EntryPoint.s_showPlayerESP || EntryPoint.s_showMonsterESP)
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

                            if (distance > 2)
                            {
                                s_characters.Add(character);
                            }
                        }
                        s_updateTimer = Time.time + s_updateTimerInterval;
                    }
                }

                if (EntryPoint.s_showPickableESP)
                {
                    var pickables = UnityEngine.Object.FindObjectsOfType<Pickable>();

                    if (pickables != null && Camera.main != null)
                    {
                        foreach (Pickable pickable in pickables)
                        {
                            var distance = Vector3.Distance(Camera.main.transform.position, pickable.transform.position);

                            if (distance > 2)
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

                            if (distance > 2)
                            {
                                s_pickableItems.Add(pickableItem);
                            }
                        }
                    }
                }

                if (EntryPoint.s_showDroppedESP)
                {
                    var itemDrops = UnityEngine.Object.FindObjectsOfType<ItemDrop>();

                    if (itemDrops != null && Camera.main != null)
                    {
                        foreach (ItemDrop itemDrop in itemDrops)
                        {
                            var distance = Vector3.Distance(Camera.main.transform.position, itemDrop.transform.position);

                            if (distance > 2)
                            {
                                s_drops.Add(itemDrop);
                            }
                        }
                    }
                }

                if (EntryPoint.s_showDepositESP)
                {
                    var mineRock5s = UnityEngine.Object.FindObjectsOfType<MineRock5>();

                    if (mineRock5s != null && Camera.main != null)
                    {
                        foreach (MineRock5 mineRock5 in mineRock5s)
                        {
                            var distance = Vector3.Distance(Camera.main.transform.position, mineRock5.transform.position);

                            if (distance > 2)
                            {
                                s_mineRock5s.Add(mineRock5);
                            }
                        }
                    }
                }
            }
        }

        public static void DisplayGUI()
        {
            if (Camera.main != null && Player.m_localPlayer != null)
            {
                var main = Camera.main;
                var labelSkin = new GUIStyle(InterfaceMaker.CustomSkin.label);

                if (EntryPoint.s_showPlayerESP || EntryPoint.s_showMonsterESP)
                {
                    foreach (Character character in s_characters)
                    {
                        if (character == null)
                        {
                            continue;
                        }
                        Vector3 vector = main.WorldToScreenPoint(character.transform.position);

                        if (vector.z > -1)
                        {
                            float a = Math.Abs(main.WorldToScreenPoint(character.GetEyePoint()).y - vector.y);
                            var distance = (int)Vector3.Distance(main.transform.position, character.transform.position);

                            if (character.IsPlayer() && EntryPoint.s_showPlayerESP)
                            {
                                string espLabel = ((Player)character).GetPlayerName() + $" [{distance}]";
                                Box(vector.x, Screen.height - vector.y, a * 0.65f, a, s_playerBoxTexture, 1f);
                                labelSkin.normal.textColor = Color.red;
                                GUI.Label(new Rect((int)vector.x - 10, Screen.height - vector.y - 5, 150, 40), espLabel, labelSkin);
                            }
                            else if (!character.IsPlayer() && !character.IsTamed() && EntryPoint.s_showMonsterESP)
                            {
                                string espLabel = character.GetHoverName() + $" [{distance}]";
                                Box(vector.x, Screen.height - vector.y, a * 0.65f, a, s_monsterAndOthersBoxTexture, 1f);
                                labelSkin.normal.textColor = Color.magenta;
                                GUI.Label(new Rect((int)vector.x - 10, Screen.height - vector.y - 5, 150, 40), espLabel, labelSkin);
                            }
                            else if (!character.IsPlayer() && character.IsTamed() && EntryPoint.s_showMonsterESP)
                            {
                                string espLabel = character.GetHoverName() + $" [{distance}]";
                                Box(vector.x, Screen.height - vector.y, a * 0.65f, a, s_tamedMonsterBoxTexture, 1f);
                                labelSkin.normal.textColor = Color.yellow;
                                GUI.Label(new Rect((int)vector.x - 10, Screen.height - vector.y - 5, 150, 40), espLabel, labelSkin);
                            }
                        }
                    }
                }

                if (EntryPoint.s_showPickableESP)
                {
                    labelSkin.normal.textColor = Color.magenta;
                    foreach (Pickable pickable in s_pickables)
                    {
                        if (pickable == null)
                        {
                            continue;
                        }
                        Vector3 vector = main.WorldToScreenPoint(pickable.transform.position);

                        if (vector.z > -1)
                        {
                            var distance = (int)Vector3.Distance(main.transform.position, pickable.transform.position);
                            string espLabel = $"{Localization.instance.Localize(pickable.GetHoverName())} [{distance}]";
                            
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
                            var distance = (int)Vector3.Distance(main.transform.position, pickableItem.transform.position);
                            string espLabel = $"{Localization.instance.Localize(pickableItem.GetHoverName())} [{distance}]";

                            GUI.Label(new Rect((int)vector.x - 5, Screen.height - vector.y - 5, 150, 40), espLabel, labelSkin);
                        }
                    }
                }

                if (EntryPoint.s_showDroppedESP)
                {
                    labelSkin.normal.textColor = Color.magenta;
                    foreach (ItemDrop itemDrop in s_drops)
                    {
                        if (itemDrop == null)
                        {
                            continue;
                        }
                        Vector3 vector = main.WorldToScreenPoint(itemDrop.transform.position);

                        if (vector.z > -1)
                        {
                            var distance = (int)Vector3.Distance(main.transform.position, itemDrop.transform.position);
                            string espLabel = $"{Localization.instance.Localize(itemDrop.GetHoverName())} [{distance}]";

                            GUI.Label(new Rect((int)vector.x - 5, Screen.height - vector.y - 5, 150, 40), espLabel, labelSkin);
                        }
                    }
                }

                if (EntryPoint.s_showDepositESP)
                {
                    labelSkin.normal.textColor = Color.magenta;
                    foreach (MineRock5 mineRock5 in s_mineRock5s)
                    {
                        if (mineRock5 == null)
                        {
                            continue;
                        }
                        Vector3 vector = main.WorldToScreenPoint(mineRock5.transform.position);

                        if (vector.z > -1)
                        {
                            var distance = (int)Vector3.Distance(main.transform.position, mineRock5.transform.position);
                            string espLabel = $"{mineRock5.GetHoverText()} [{distance}]";

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
