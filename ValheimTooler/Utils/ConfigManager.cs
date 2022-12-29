using System;
using System.IO;
using System.Reflection;
using SharpConfig;
using UnityEngine;

namespace ValheimTooler.Utils
{
    public sealed class ConfigManager
    {
        private static ConfigManager s_instance;
        private Configuration _settingsConfiguration = null;
        private Configuration _internalConfiguration = null;
        private string _configurationPath = null;

        private const string SettingsFileName = "valheimtooler_settings.cfg";
        private const string InternalFileName = "valheimtooler_internal.cfg";

        public KeyCode s_toggleInterfaceKey { get; private set; } = KeyCode.Delete;
        public bool s_showAtStartup { get; private set; } = true;
        public string s_language { get; private set; } = "Auto";

        public Vector2 s_mainWindowInitialPosition = new Vector2(5, 5);
        public Vector2 s_itemGiverInitialPosition = new Vector2(Screen.width - 400, 5);
        public bool s_permanentPins = false;
        public bool s_espRadiusEnabled = false;
        public float s_espRadius = 5.0f;

        public static ConfigManager instance
        {
            get
            {
                if (s_instance == null)
                {
                    Initialize();
                }
                return s_instance;
            }
        }

        private ConfigManager()
        {
            var valheimAssemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            var configPathFilePath = Path.Combine(valheimAssemblyFolder, "config_vt.path");

            if (!File.Exists(configPathFilePath))
            {
                ZLog.Log("[ValheimTooler - ConfigManager] Failed to find config_vt.path file. Fallback to default configuration.");
                return;
            }

            var configPath = Path.GetFullPath(File.ReadAllText(configPathFilePath));

            if (!Directory.Exists(configPath))
            {
                ZLog.Log("[ValheimTooler - ConfigManager] Path given in config_vt.path file is incorrect. Fallback to default configuration.");
                return;
            }

            _configurationPath = configPath;

            var configFilePath = Path.Combine(_configurationPath, SettingsFileName);
            var internalFilePath = Path.Combine(_configurationPath, InternalFileName);
            
            if (File.Exists(configFilePath))
            {
                _settingsConfiguration = Configuration.LoadFromFile(configFilePath);

                if (_settingsConfiguration.Contains("Settings"))
                {
                    var settingsSection = _settingsConfiguration["Settings"];

                    if (settingsSection.Contains("ToggleInterfaceKey"))
                    {
                        try
                        {
                            var toggleInterfaceKey = settingsSection["ToggleInterfaceKey"].GetValue<KeyCode>();

                            s_toggleInterfaceKey = toggleInterfaceKey;
                        } catch (InvalidOperationException)
                        {
                            ZLog.Log("[ValheimTooler - ConfigManager] ToggleInterfaceKey value is not correct. Fallback to default value.");
                        }
                    }

                    if (settingsSection.Contains("ShowAtStartup"))
                    {
                        try
                        {
                            s_showAtStartup = settingsSection["ShowAtStartup"].BoolValue;
                        }
                        catch (InvalidOperationException)
                        {
                            ZLog.Log("[ValheimTooler - ConfigManager] ShowAtStartup value is not correct. Fallback to default value.");
                        }
                    }

                    if (settingsSection.Contains("Language"))
                    {
                        try
                        {
                            s_language = settingsSection["Language"].StringValue;
                        }
                        catch (InvalidOperationException)
                        {
                            ZLog.Log("[ValheimTooler - ConfigManager] Language value is not correct. Fallback to default value.");
                        }
                    }
                }
                else
                {
                    ZLog.Log("[ValheimTooler - ConfigManager] Failed to find settings section. Fallback to default values.");
                }
            }
            else
            {
                ZLog.Log($"[ValheimTooler - ConfigManager] Failed to find {SettingsFileName} file! Fallback to default values.");
                CreateDefaultSettingConfiguration();
            }

            if (File.Exists(internalFilePath))
            {
                _internalConfiguration = Configuration.LoadFromFile(internalFilePath);

                if (_internalConfiguration.Contains("Internal"))
                {
                    var internalSection = _internalConfiguration["Internal"];

                    if (internalSection.Contains("MainWindowLocationX") && internalSection.Contains("MainWindowLocationY"))
                    {
                        var mainWindowLocationX = internalSection["MainWindowLocationX"].FloatValue;
                        var mainWindowLocationY = internalSection["MainWindowLocationY"].FloatValue;
                        s_mainWindowInitialPosition = new Vector2(mainWindowLocationX, mainWindowLocationY);
                    }

                    if (internalSection.Contains("ItemGiverLocationX") && internalSection.Contains("ItemGiverLocationY"))
                    {
                        var itemGiverLocationX = internalSection["ItemGiverLocationX"].FloatValue;
                        var itemGiverLocationY = internalSection["ItemGiverLocationY"].FloatValue;
                        s_itemGiverInitialPosition = new Vector2(itemGiverLocationX, itemGiverLocationY);
                    }

                    if (internalSection.Contains("PermanentPins"))
                    {
                        s_permanentPins = internalSection["PermanentPins"].BoolValue;
                    }

                    if (internalSection.Contains("RadiusEnabled"))
                    {
                        s_espRadiusEnabled = internalSection["RadiusEnabled"].BoolValue;
                    }

                    if (internalSection.Contains("RadiusValue"))
                    {
                        s_espRadius = internalSection["RadiusValue"].FloatValue;
                    }
                }
                else
                {
                    ZLog.Log("[ValheimTooler - ConfigManager] Failed to find internal section. Fallback to default values.");
                }
            }
            else
            {
                ZLog.Log($"[ValheimTooler - ConfigManager] Failed to find {InternalFileName} file! Fallback to default values.");
                _internalConfiguration = new Configuration();
            }
        }

        private void CreateDefaultSettingConfiguration()
        {
            var configFilePath = Path.Combine(_configurationPath, SettingsFileName);
            _settingsConfiguration = new Configuration();

            _settingsConfiguration["Settings"]["ToggleInterfaceKey"].SetValue(s_toggleInterfaceKey);
            _settingsConfiguration["Settings"]["ToggleInterfaceKey"].PreComment = "Which key will show/hide the tool. See the possible list here: https://gist.github.com/Extremelyd1/4bcd495e21453ed9e1dffa27f6ba5f69";

            _settingsConfiguration["Settings"]["ShowAtStartup"].BoolValue = s_showAtStartup;
            _settingsConfiguration["Settings"]["ShowAtStartup"].PreComment = "Set True for showing the tool window when started, False otherwise";

            _settingsConfiguration["Settings"]["Language"].StringValue = s_language;
            _settingsConfiguration["Settings"]["Language"].PreComment = "The language to use. Auto will use the game language. Either French or English available";

            _settingsConfiguration.SaveToFile(configFilePath);
        }

        public void SaveConfig()
        {
            if (string.IsNullOrEmpty(_configurationPath))
            {
                return;
            }

            var internalFilePath = Path.Combine(_configurationPath, InternalFileName);

            _internalConfiguration["Internal"]["MainWindowLocationX"].FloatValue = s_mainWindowInitialPosition.x;
            _internalConfiguration["Internal"]["MainWindowLocationY"].FloatValue = s_mainWindowInitialPosition.y;

            _internalConfiguration["Internal"]["ItemGiverLocationX"].FloatValue = s_itemGiverInitialPosition.x;
            _internalConfiguration["Internal"]["ItemGiverLocationY"].FloatValue = s_itemGiverInitialPosition.y;

            _internalConfiguration["Internal"]["PermanentPins"].BoolValue = s_permanentPins;
            _internalConfiguration["Internal"]["RadiusEnabled"].BoolValue = s_espRadiusEnabled;
            _internalConfiguration["Internal"]["RadiusValue"].FloatValue = s_espRadius;

            _internalConfiguration.SaveToFile(internalFilePath);
        }

        private static void Initialize()
        {
            if (s_instance == null)
            {
                s_instance = new ConfigManager();
            }
        }
    }
}
