using UnityEngine;

namespace RapidGUI
{
    public static partial class RGUIUtility
    {
        static readonly GUIContent s_tempContent = new GUIContent();
    

        public static GUIContent TempContent(string text)
        {
            s_tempContent.text = text;
            s_tempContent.tooltip = null;
            s_tempContent.image = null;

            return s_tempContent;
        }

        public static Vector2 GetMouseScreenPos(Vector2? screenInsideOffset = null)
        {
            //var windowPos = GUIUtility.GUIToScreenPoint(pos); // doesn't seem to work on the unity2020 Editor.

            var mousePos = Input.mousePosition;
            var ret = new Vector2(mousePos.x, Screen.height - mousePos.y);

            if (screenInsideOffset.HasValue)
            {
                var maxPos = new Vector2(Screen.width, Screen.height) - screenInsideOffset.Value;
                ret = Vector2.Min(ret, maxPos);
            }

            return ret;
        }
    }
}
