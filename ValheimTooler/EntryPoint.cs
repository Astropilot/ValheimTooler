using System.Linq;
using System.Reflection;
using UnityEngine;
using ValheimTooler.Utils;
using ValheimTooler.Core;
using ValheimTooler.UI;

namespace ValheimTooler
{
    public class EntryPoint : MonoBehaviour
    {
        public static readonly int s_boxSpacing = 30;
        private Rect _valheimToolerRect;

        private bool _showMainWindow = true;
        private bool _wasMainWindowShowed = false;
        public static bool s_showItemGiver = false;

        private WindowToolbar _windowToolbar = WindowToolbar.PLAYER;
        private readonly string[] _toolbarChoices = {
            "$vt_toolbar_player",
            "$vt_toolbar_entities",
            "$vt_toolbar_terrain_shaper",
            "$vt_toolbar_misc"
        };

        private string _version;
        private ConfigManager _config;

        public void Start()
        {
            _config = ConfigManager.instance;
            _valheimToolerRect = new Rect(_config.s_mainWindowInitialPosition.x, _config.s_mainWindowInitialPosition.y, 800, 300);
            _showMainWindow = _config.s_showAtStartup;

            PlayerHacks.Start();
            EntitiesItemsHacks.Start();
            ItemGiver.Start();
            MiscHacks.Start();
            ESP.Start();
            TerrainShaper.Start();

            _version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }
        public void Update()
        {
            if (Input.GetKeyDown(_config.s_toggleInterfaceKey))
            {
                _showMainWindow = !_showMainWindow;
            }

            PlayerHacks.Update();
            EntitiesItemsHacks.Update();
            ItemGiver.Update();
            MiscHacks.Update();
            ESP.Update();
            TerrainShaper.Update();
        }

        public void OnGUI()
        {
            GUI.skin = InterfaceMaker.CustomSkin;

            if (_showMainWindow)
            {
                if (GameCamera.instance != null)
                {
                    GameCamera.instance.SetFieldValue<bool>("m_mouseCapture", false);
                    GameCamera.instance.CallMethod("UpdateMouseCapture");
                }

                _valheimToolerRect = GUILayout.Window(1001, _valheimToolerRect, ValheimToolerWindow, VTLocalization.instance.Localize($"$vt_main_title (v{_version})"), GUILayout.Height(10), GUILayout.Width(10));

                if (s_showItemGiver)
                {
                    ItemGiver.DisplayGUI();
                }
                _wasMainWindowShowed = true;

                _config.s_mainWindowInitialPosition = _valheimToolerRect.position;
            }
            else
            {
                if (_wasMainWindowShowed)
                {
                    if (GameCamera.instance != null)
                    {
                        GameCamera.instance.SetFieldValue<bool>("m_mouseCapture", true);
                        GameCamera.instance.CallMethod("UpdateMouseCapture");
                    }
                    _wasMainWindowShowed = false;
                }
            }

            ESP.DisplayGUI();
        }

        void ValheimToolerWindow(int windowID)
        {
            GUILayout.Space(10);

            _windowToolbar = (WindowToolbar)GUILayout.Toolbar((int)_windowToolbar, _toolbarChoices.Select(choice => VTLocalization.instance.Localize(choice)).ToArray());

            switch (_windowToolbar)
            {
                case WindowToolbar.PLAYER:
                    PlayerHacks.DisplayGUI();
                    break;
                case WindowToolbar.ENTITIES_ITEMS:
                    EntitiesItemsHacks.DisplayGUI();
                    break;
                case WindowToolbar.TERRAIN_SHAPER:
                    TerrainShaper.DisplayGUI();
                    break;
                case WindowToolbar.MISC:
                    MiscHacks.DisplayGUI();
                    break;
            }

            GUI.DragWindow();
        }

        public void OnDestroy()
        {
            ConfigManager.instance.SaveConfig();
        }
    }
}
