using System;
using System.Linq;
using UnityEngine;
using ValheimTooler.Utils;

namespace ValheimTooler.UI
{
    class InterfaceMaker
    {
        private static GUISkin s_customSkin;

        private static Texture2D s_winBackground;
        private static Texture2D s_winTitleBackground;
        private static Texture2D s_boxBackground;
        private static Texture2D s_toggleOffBackground;
        private static Texture2D s_toggleOnBackground;
        private static Texture2D s_buttonNormalBackground;
        private static Texture2D s_buttonHoverBackground;
        private static Texture2D s_buttonActiveBackground;
        private static Texture2D s_buttonActiveNormalBackground;
        private static Texture2D s_buttonActiveHoverBackground;
        private static Texture2D s_buttonActiveActiveBackground;
        private static Texture2D s_fieldBackground;
        private static Texture2D s_scrollBackground;
        private static Texture2D s_scrollThumbBackground;

        private static Texture2D s_flatButtonNormalBackground;
        private static Texture2D s_flatButtonHoverBackground;
        private static Texture2D s_flatButtonActiveBackground;

        private static Font s_font;

        private static GUISkin s_oldSkin;

        public static GUISkin CustomSkin
        {
            get
            {
                if (InterfaceMaker.s_customSkin == null)
                {
                    try
                    {
                        InterfaceMaker.s_customSkin = InterfaceMaker.CreateSkin();
                    }
                    catch (Exception ex)
                    {
                        ZLog.Log("Could not load custom GUISkin - " + ex.Message);
                        InterfaceMaker.s_customSkin = GUI.skin;
                    }
                }
                return InterfaceMaker.s_customSkin;
            }
        }

        public static GUISkin OldSkin
        {
            get { return s_oldSkin;  }
        }

        private static GUISkin CreateSkin()
        {
            s_oldSkin = GUI.skin;

            var guiskin = UnityEngine.Object.Instantiate<GUISkin>(GUI.skin);

            UnityEngine.Object.DontDestroyOnLoad(guiskin);

            LoadTextures();

            guiskin.font = s_font;

            guiskin.label.normal.textColor = Color.gray;

            guiskin.box.onNormal.background = null;
            guiskin.box.normal.background = InterfaceMaker.s_boxBackground;
            guiskin.box.normal.textColor = Color.gray;

            guiskin.window.border = new RectOffset(80, 80, 80, 20);
            guiskin.window.padding.top += 5;
            guiskin.window.onNormal.background = null;
            guiskin.window.normal.background = InterfaceMaker.s_winTitleBackground;
            guiskin.window.normal.textColor = Color.white;

            guiskin.button.normal.textColor = Color.gray;
            guiskin.button.normal.background = s_buttonNormalBackground;
            guiskin.button.hover.textColor = Color.gray;
            guiskin.button.hover.background = s_buttonHoverBackground;
            guiskin.button.active.textColor = Color.gray;
            guiskin.button.active.background = s_buttonActiveBackground;
            guiskin.button.onNormal.textColor = Color.white;
            guiskin.button.onNormal.background = s_buttonActiveNormalBackground;
            guiskin.button.onHover.textColor = Color.white;
            guiskin.button.onHover.background = s_buttonActiveHoverBackground;
            guiskin.button.onActive.textColor = Color.white;
            guiskin.button.onActive.background = s_buttonActiveActiveBackground;
            guiskin.button.font = s_font;
            guiskin.button.wordWrap = false;

            guiskin.toggle.normal.textColor = Color.gray;
            guiskin.toggle.normal.background = s_toggleOffBackground;
            guiskin.toggle.onNormal.textColor = Color.gray;
            guiskin.toggle.onNormal.background = s_toggleOnBackground;

            guiskin.toggle.hover.textColor = Color.gray;
            guiskin.toggle.hover.background = s_toggleOffBackground;
            guiskin.toggle.onHover.textColor = Color.gray;
            guiskin.toggle.onHover.background = s_toggleOnBackground;

            guiskin.toggle.active.textColor = Color.gray;
            guiskin.toggle.active.background = s_toggleOnBackground;
            guiskin.toggle.onActive.textColor = Color.gray;
            guiskin.toggle.onActive.background = s_toggleOffBackground;

            guiskin.toggle.border = new RectOffset(0, 0, 0, 0);
            guiskin.toggle.overflow = new RectOffset(0, 0, 0, 0);
            guiskin.toggle.imagePosition = ImagePosition.ImageOnly;
            guiskin.toggle.padding = new RectOffset(0, 0, 0, 0);
            guiskin.toggle.fixedWidth = 30;
            guiskin.toggle.fixedHeight = 30;

            guiskin.button.padding.top = guiskin.button.padding.bottom = 6;
            guiskin.button.border.bottom = guiskin.button.border.top = 0;
            guiskin.button.border.left = guiskin.button.border.right = 10;

            guiskin.textField.normal.background = s_fieldBackground;
            guiskin.textField.normal.textColor = Color.gray;
            guiskin.textField.onNormal.background = s_fieldBackground;
            guiskin.textField.onNormal.textColor = Color.gray;
            guiskin.textField.hover.background = s_fieldBackground;
            guiskin.textField.hover.textColor = Color.gray;
            guiskin.textField.onHover.background = s_fieldBackground;
            guiskin.textField.onHover.textColor = Color.gray;
            guiskin.textField.active.background = s_fieldBackground;
            guiskin.textField.active.textColor = Color.gray;
            guiskin.textField.onActive.background = s_fieldBackground;
            guiskin.textField.onActive.textColor = Color.gray;
            guiskin.textField.focused.background = s_fieldBackground;
            guiskin.textField.focused.textColor = Color.gray;
            guiskin.textField.onFocused.background = s_fieldBackground;
            guiskin.textField.onFocused.textColor = Color.gray;
            guiskin.textField.padding = new RectOffset(8, 8, 5, 5);

            guiskin.settings.cursorColor = Color.gray;

            guiskin.verticalScrollbar.normal.background = s_scrollBackground;
            guiskin.verticalScrollbarThumb.normal.background = s_scrollThumbBackground;

            var popupStyle = new GUIStyle(guiskin.box)
            {
                border = new RectOffset(),
                name = "popup"
            };

            popupStyle.normal.background =
            popupStyle.hover.background = InterfaceMaker.s_winBackground;
            popupStyle.border = new RectOffset(30, 30, 30, 30);

            var flatButtonStyle = new GUIStyle(InterfaceMaker.OldSkin.label)
            {
                wordWrap = false,
                alignment = TextAnchor.MiddleCenter,
                name = "flatButton"
            };

            flatButtonStyle.normal.background = s_flatButtonNormalBackground;
            flatButtonStyle.hover.background = s_flatButtonHoverBackground;
            flatButtonStyle.onNormal.background = s_flatButtonActiveBackground;

            guiskin.customStyles = new GUIStyle[] { popupStyle, flatButtonStyle };

            return guiskin;
        }

        private static void LoadTextures()
        {
            InterfaceMaker.s_winBackground = ResourceUtils.LoadTexture(ResourceUtils.GetEmbeddedResource("Resources.window.png", null));
            UnityEngine.Object.DontDestroyOnLoad(InterfaceMaker.s_winBackground);

            InterfaceMaker.s_winTitleBackground = ResourceUtils.LoadTexture(ResourceUtils.GetEmbeddedResource("Resources.window_title.png", null));
            UnityEngine.Object.DontDestroyOnLoad(InterfaceMaker.s_winTitleBackground);

            InterfaceMaker.s_boxBackground = ResourceUtils.LoadTexture(ResourceUtils.GetEmbeddedResource("Resources.box.png", null));
            UnityEngine.Object.DontDestroyOnLoad(InterfaceMaker.s_boxBackground);

            InterfaceMaker.s_toggleOffBackground = ResourceUtils.LoadTexture(ResourceUtils.GetEmbeddedResource("Resources.toggle_off.png", null));
            UnityEngine.Object.DontDestroyOnLoad(InterfaceMaker.s_toggleOffBackground);

            InterfaceMaker.s_toggleOnBackground = ResourceUtils.LoadTexture(ResourceUtils.GetEmbeddedResource("Resources.toggle_on.png", null));
            UnityEngine.Object.DontDestroyOnLoad(InterfaceMaker.s_toggleOnBackground);

            InterfaceMaker.s_buttonNormalBackground = ResourceUtils.LoadTexture(ResourceUtils.GetEmbeddedResource("Resources.button_normal.png", null));
            UnityEngine.Object.DontDestroyOnLoad(InterfaceMaker.s_buttonNormalBackground);

            InterfaceMaker.s_buttonHoverBackground = ResourceUtils.LoadTexture(ResourceUtils.GetEmbeddedResource("Resources.button_hover.png", null));
            UnityEngine.Object.DontDestroyOnLoad(InterfaceMaker.s_buttonHoverBackground);

            InterfaceMaker.s_buttonActiveBackground = ResourceUtils.LoadTexture(ResourceUtils.GetEmbeddedResource("Resources.button_active.png", null));
            UnityEngine.Object.DontDestroyOnLoad(InterfaceMaker.s_buttonActiveBackground);

            InterfaceMaker.s_buttonActiveNormalBackground = ResourceUtils.LoadTexture(ResourceUtils.GetEmbeddedResource("Resources.button_active_normal.png", null));
            UnityEngine.Object.DontDestroyOnLoad(InterfaceMaker.s_buttonActiveNormalBackground);

            InterfaceMaker.s_buttonActiveHoverBackground = ResourceUtils.LoadTexture(ResourceUtils.GetEmbeddedResource("Resources.button_active_hover.png", null));
            UnityEngine.Object.DontDestroyOnLoad(InterfaceMaker.s_buttonActiveHoverBackground);

            InterfaceMaker.s_buttonActiveActiveBackground = ResourceUtils.LoadTexture(ResourceUtils.GetEmbeddedResource("Resources.button_active_active.png", null));
            UnityEngine.Object.DontDestroyOnLoad(InterfaceMaker.s_buttonActiveActiveBackground);

            InterfaceMaker.s_fieldBackground = ResourceUtils.LoadTexture(ResourceUtils.GetEmbeddedResource("Resources.field.png", null));
            UnityEngine.Object.DontDestroyOnLoad(InterfaceMaker.s_fieldBackground);

            InterfaceMaker.s_scrollBackground = ResourceUtils.LoadTexture(ResourceUtils.GetEmbeddedResource("Resources.scroll_background.png", null));
            UnityEngine.Object.DontDestroyOnLoad(InterfaceMaker.s_scrollBackground);

            InterfaceMaker.s_scrollThumbBackground = ResourceUtils.LoadTexture(ResourceUtils.GetEmbeddedResource("Resources.scroll_thumb.png", null));
            UnityEngine.Object.DontDestroyOnLoad(InterfaceMaker.s_scrollThumbBackground);

            InterfaceMaker.s_flatButtonNormalBackground = new Texture2D(1, 1);
            InterfaceMaker.s_flatButtonNormalBackground.SetPixels(new[] { new Color(0.5f, 0.5f, 0.5f, 0.5f) });
            InterfaceMaker.s_flatButtonNormalBackground.Apply();
            UnityEngine.Object.DontDestroyOnLoad(InterfaceMaker.s_flatButtonNormalBackground);

            InterfaceMaker.s_flatButtonHoverBackground = new Texture2D(1, 1);
            InterfaceMaker.s_flatButtonHoverBackground.SetPixels(new[] { new Color(0.5f, 0.5f, 0.5f, 0.2f) });
            InterfaceMaker.s_flatButtonHoverBackground.Apply();
            UnityEngine.Object.DontDestroyOnLoad(InterfaceMaker.s_flatButtonHoverBackground);

            InterfaceMaker.s_flatButtonActiveBackground = new Texture2D(1, 1);
            InterfaceMaker.s_flatButtonActiveBackground.SetPixels(new[] { new Color(0.9f, 0.5f, 0.1f, 0.5f) });
            InterfaceMaker.s_flatButtonActiveBackground.Apply();
            UnityEngine.Object.DontDestroyOnLoad(InterfaceMaker.s_flatButtonActiveBackground);

            InterfaceMaker.s_font = (Resources.FindObjectsOfTypeAll(typeof(Font)) as Font[]).ToList().First(f => f.name.Equals("Norsebold"));

            if (InterfaceMaker.s_font != null)
            {
                UnityEngine.Object.DontDestroyOnLoad(InterfaceMaker.s_font);
            } else
            {
                ZLog.Log("Error while loading font!");
            }
        }
    }
}
