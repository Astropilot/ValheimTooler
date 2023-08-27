using ValheimTooler.Configuration;
using ValheimTooler.Utils;

namespace ValheimTooler.UI
{
    public static class Utils
    {
        public static string ToggleButtonLabel(string labelCode, bool condition, KeyboardShortcut? shortcut)
        {
            return ToggleButtonLabelCustom(labelCode, condition, VTLocalization.s_cheatOn, VTLocalization.s_cheatOff, shortcut);
        }

        public static string ToggleButtonLabel(string labelCode, bool condition)
        {
            return ToggleButtonLabel(labelCode, condition, null);
        }

        public static string ToggleButtonLabelCustom(string labelCode, bool condition, string activeLabelCode, string normalLabelCode)
        {
            return ToggleButtonLabelCustom(labelCode, condition, activeLabelCode, normalLabelCode, null);
        }

        public static string ToggleButtonLabelCustom(string labelCode, bool condition, string activeLabelCode, string normalLabelCode, KeyboardShortcut? shortcut)
        {
            string label = VTLocalization.instance.Localize(labelCode + " : " + (condition ? activeLabelCode : normalLabelCode));

            if (shortcut != null && shortcut.Value.MainKey != UnityEngine.KeyCode.None)
            {
                label += " [" + shortcut.Value.ToString() + "]";
            }

            return label;
        }

        public static string LabelWithShortcut(string labelCode, KeyboardShortcut? shortcut)
        {
            string label = VTLocalization.instance.Localize(labelCode);

            if (shortcut != null && shortcut.Value.MainKey != UnityEngine.KeyCode.None)
            {
                label += " [" + shortcut.Value.ToString() + "]";
            }

            return label;
        }
    }
}
