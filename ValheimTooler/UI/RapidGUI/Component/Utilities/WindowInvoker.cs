using System.Collections.Generic;
using UnityEngine;

namespace RapidGUI
{
    public static class WindowInvoker
    {
        static readonly HashSet<IDoGUIWindow> s_windows = new HashSet<IDoGUIWindow>();

        static WindowInvoker()
        {
            RapidGUIBehaviour.Instance.onGUI += DoGUI;
        }


        public static void Add(IDoGUIWindow window) => s_windows.Add(window);
        public static void Remove(IDoGUIWindow window) => s_windows.Remove(window);

        static IDoGUIWindow s_focusedWindow;

        public static void SetFocusedWindow(IDoGUIWindow window)
        {
            s_focusedWindow = window;
        }

        static void DoGUI()
        {
            foreach (IDoGUIWindow window in s_windows)
            {
                window?.DoGUIWindow();
            }

            var evt = Event.current;

            if ((evt.type == EventType.KeyUp) 
                && (evt.keyCode == RapidGUIBehaviour.Instance.closeFocusedWindowKey)
                && (GUIUtility.keyboardControl == 0)
                )
            {
                if (s_windows.Contains(s_focusedWindow))
                {
                    s_focusedWindow.CloseWindow();
                    s_focusedWindow = null;
                }
            }


            if (Event.current.type == EventType.Repaint)
            {
                s_windows.Clear();
            }
        }
    }
}
