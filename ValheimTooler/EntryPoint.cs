using System.Linq;
using System.Reflection;
using UnityEngine;
using ValheimTooler.Core;
using ValheimTooler.UI;
using ValheimTooler.Utils;

namespace ValheimTooler
{
    public class EntryPoint : MonoBehaviour
    {
        public static readonly int s_boxSpacing = 30;
        private Rect _valheimToolerRect;

        private bool _showMainWindow = true;
        public static bool s_showItemGiver = false;
        public static bool s_showESP = false;

        private WindowToolbar _windowToolbar = WindowToolbar.PLAYER;
        private readonly string[] _toolbarChoices = {
            "$vt_toolbar_player",
            "$vt_toolbar_entities",
            "$vt_toolbar_misc"
        };

        private string _version;

        public void Start()
        {
            _valheimToolerRect = new Rect(5, 5, 350, 300);

            PlayerHacks.Start();
            EntitiesItemsHacks.Start();
            ItemGiver.Start();
            MiscHacks.Start();
            ESP.Start();

            _version = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            Translator.InitLocalization("English");
            string prefLanguage = PlayerPrefs.GetString("language", "");
            if (prefLanguage.Length > 0)
            {
                Translator.InitLocalization(prefLanguage);
            }
        }
        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.Delete))
            {
                _showMainWindow = !_showMainWindow;
            }

            PlayerHacks.Update();
            EntitiesItemsHacks.Update();
            ItemGiver.Update();
            MiscHacks.Update();
            ESP.Update();
        }

        public void OnGUI()
        {
            GUI.skin = InterfaceMaker.CustomSkin;

            if (_showMainWindow)
            {
                _valheimToolerRect = GUILayout.Window(1001, _valheimToolerRect, ValheimToolerWindow, Translator.Localize($"$vt_main_title (v{_version})"), GUILayout.Height(10));
            }

            if (s_showItemGiver)
            {
                ItemGiver.DisplayGUI();
            }

            if (s_showESP)
            {
                ESP.DisplayGUI();
            }
        }

        void ValheimToolerWindow(int windowID)
        {
            GUILayout.Space(10);

            _windowToolbar = (WindowToolbar)GUILayout.Toolbar((int)_windowToolbar, _toolbarChoices.Select(choice => Translator.Localize(choice)).ToArray());

            switch (_windowToolbar)
            {
                case WindowToolbar.PLAYER:
                    PlayerHacks.DisplayGUI();
                    break;
                case WindowToolbar.ENTITIES_ITEMS:
                    EntitiesItemsHacks.DisplayGUI();
                    break;
                case WindowToolbar.MISC:
                    MiscHacks.DisplayGUI();
                    break;
            }

            GUI.DragWindow();
        }
    }
}
