using System;
using System.Collections.Generic;
using UnityEngine;
using ValheimTooler.UI;

namespace ValheimTooler.Core
{
    public static class ESP
    {
        private static readonly Texture2D s_espTexture;

        private static readonly List<Player> s_players = new List<Player>();
        private static float s_updateTimer = 0f;
        private static readonly float s_updateTimerInterval = 1.5f;

        static ESP()
        {
            ESP.s_espTexture = new Texture2D(2, 2, TextureFormat.ARGB32, false);
            ESP.s_espTexture.SetPixel(0, 0, Color.red);
            ESP.s_espTexture.SetPixel(1, 0, Color.red);
            ESP.s_espTexture.SetPixel(0, 1, Color.red);
            ESP.s_espTexture.SetPixel(1, 1, Color.red);
            ESP.s_espTexture.Apply();
            UnityEngine.Object.DontDestroyOnLoad(ESP.s_espTexture);
        }

        public static void Start()
        {
            return;
        }

        public static void Update()
        {
            if (Time.time >= s_updateTimer)
            {
                s_players.Clear();

                List<Player> players = Player.GetAllPlayers();

                if (players != null && Camera.main != null && Player.m_localPlayer != null)
                {
                    foreach (Player player in players)
                    {
                        var distance = Vector3.Distance(Camera.main.transform.position, player.transform.position);

                        if (!player.GetPlayerName().Equals(Player.m_localPlayer.GetPlayerName()) && distance > 2)
                        {
                            s_players.Add(player);
                        }
                    }
                    s_updateTimer = Time.time + s_updateTimerInterval;
                }
            }
        }

        public static void DisplayGUI()
        {
            if (Camera.main != null && Player.m_localPlayer != null)
            {
                var main = Camera.main;
                foreach (Player player in s_players)
                {
                    Vector3 vector = main.WorldToScreenPoint(player.transform.position);

                    if (vector.z > -1)
                    {
                        float a = Math.Abs(main.WorldToScreenPoint(player.GetHeadPoint()).y - main.WorldToScreenPoint(player.transform.position).y);
                        var distance = Vector3.Distance(main.transform.position, player.transform.position);

                        Box(vector.x, Screen.height - vector.y, a * 0.65f, a, s_espTexture, 1f);

                        var labelSkin = new GUIStyle(InterfaceMaker.CustomSkin.label);

                        labelSkin.normal.textColor = Color.white;

                        GUI.Label(new Rect((int)vector.x - 5, Screen.height - vector.y - 10, 40, 40), ((int)(distance)).ToString(), labelSkin);
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
