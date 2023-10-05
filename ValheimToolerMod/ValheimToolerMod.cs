using System;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using HarmonyLib;
using ValheimToolerMod.Patches;

namespace ValheimToolerMod
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInProcess("valheim.exe")]
    public class ValheimToolerMod : BaseUnityPlugin
    {
        const string PluginGUID = "com.github.Astropilot.ValheimTooler";
        const string PluginName = "ValheimTooler";
        const string PluginVersion = "1.9.1";

        private Harmony _harmony;

        private void Awake()
        {
            FejdStartupPatch.OnGameInitialized += LoadPlugin;
            _harmony = Harmony.CreateAndPatchAll(typeof(FejdStartupPatch), PluginGUID);
        }

        private void OnDestroy()
        {
            _harmony?.UnpatchSelf();
            CallLoaderUnload();
        }

        private void LoadPlugin()
        {
            FejdStartupPatch.OnGameInitialized -= LoadPlugin;
            _harmony?.UnpatchSelf();
            _harmony = null;

            string bepInExPluginsPath = Paths.PluginPath;
            var valheimToolerFiles = Directory.GetFiles(bepInExPluginsPath, "ValheimToolerMod.dll", SearchOption.AllDirectories).ToList();
            string configpathfilepath = null;

            while (configpathfilepath == null && valheimToolerFiles.Count > 0)
            {
                string valheimToolerPath = Path.GetDirectoryName(valheimToolerFiles[0]);

                valheimToolerFiles.RemoveAt(0);

                if (File.Exists(Path.Combine(valheimToolerPath, "ValheimTooler.dll")))
                {
                    configpathfilepath = Path.Combine(valheimToolerPath, "config_vt.path");
                }
            }

            if (!string.IsNullOrEmpty(configpathfilepath))
            {
                File.WriteAllText(configpathfilepath, Paths.ConfigPath);
            }

            CallLoaderInit();
        }

        private static Type GetLoaderType()
        {
            var typeEntryPoint = typeof(ValheimTooler.EntryPoint);
            var typeLoader = typeEntryPoint.Assembly.GetType("ValheimTooler.Loader");

            if (typeLoader is null)
            {
                throw new Exception("Can't find Type ValheimTooler.Loader");
            }

            return typeLoader;
        }

        private void CallLoader(string methodName)
        {
            var typeLoader = GetLoaderType();
            var methodInfo = typeLoader.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public);

            if (methodInfo is null)
            {
                throw new Exception("Can't find method " + methodName);
            }
            methodInfo.Invoke(obj: null, parameters: new object[] { });
            Logger.LogDebug($"Called ValheimTooler.Loader.{methodName}");
        }

        private void CallLoaderInit()
        {
            CallLoader("Init");
        }

        private void CallLoaderUnload()
        {
            CallLoader("Unload");
        }
    }
}
