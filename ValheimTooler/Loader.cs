using RapidGUI;
using UnityEngine;

namespace ValheimTooler
{
    class Loader
    {
        public static void Init()
        {
            Loader.s_entryPoint = new GameObject();
            Loader.s_entryPoint.AddComponent<EntryPoint>();
            Loader.s_entryPoint.AddComponent<RapidGUIBehaviour>();
            Object.DontDestroyOnLoad(Loader.s_entryPoint);
        }

        public static void Unload()
        {
            _Unload();
        }
        private static void _Unload()
        {
            GameObject.Destroy(Loader.s_entryPoint);
        }

        private static GameObject s_entryPoint;
    }

}
