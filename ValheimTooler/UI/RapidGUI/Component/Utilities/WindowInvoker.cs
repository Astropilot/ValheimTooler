using System.Collections.Generic;
using UnityEngine;

namespace RapidGUI
{
    public static class WindowInvoker
    {
        static readonly HashSet<IDoGUIWindow> Windows = new HashSet<IDoGUIWindow>();

        static WindowInvoker()
        {
            RapidGUIBehaviour.Instance.onGUI += DoGUI;
        }


        public static void Add(IDoGUIWindow window) => Windows.Add(window);
        public static void Remove(IDoGUIWindow window) => Windows.Remove(window);

        static IDoGUIWindow focusedWindow;

        public static void SetFocusedWindow(IDoGUIWindow window)
        {
            focusedWindow = window;
        }

        static void DoGUI()
        {
            foreach (IDoGUIWindow window in Windows)
            {
                window?.DoGUIWindow();
            }

            var evt = Event.current;

            if ((evt.type == EventType.KeyUp) 
                && (evt.keyCode == RapidGUIBehaviour.Instance.closeFocusedWindowKey)
                && (GUIUtility.keyboardControl == 0)
                )
            {
                if (Windows.Contains(focusedWindow))
                {
                    focusedWindow.CloseWindow();
                    focusedWindow = null;
                }
            }


            if (Event.current.type == EventType.Repaint)
            {
                Windows.Clear();
            }
        }
    }
}
