using System;
using UnityEngine;

namespace RapidGUI
{
    public class RapidGUIBehaviour : MonoBehaviour
    {
        #region static 

        static RapidGUIBehaviour s_instance;
        public static RapidGUIBehaviour Instance
        {
            get
            {
                if (s_instance == null)
                {
                    s_instance = FindObjectOfType<RapidGUIBehaviour>();
                    if (s_instance == null)
                    {
                        var ga = new GameObject("RapidGUI");
                        s_instance = ga.AddComponent<RapidGUIBehaviour>();
                    }

                    if (Application.isPlaying)
                    {
                        DontDestroyOnLoad(s_instance);
                    }
                }

                return s_instance;
            }
        }

        #endregion

        public KeyCode closeFocusedWindowKey = KeyCode.Q;
        public int prefixLabelSlideButton = 1;
        public Action onGUI;

        public void OnGUI()
        {
            onGUI?.Invoke();
        }
    }
}
