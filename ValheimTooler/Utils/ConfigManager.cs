using System.Diagnostics;
using System.IO;
using UnityEngine;
using ValheimTooler.Configuration;

namespace ValheimTooler.Utils
{
    public static class ConfigManager
    {
        private static readonly ConfigFile s_settingsFile = null;
        private static readonly ConfigFile s_internalFile = null;
        private static readonly string s_configurationPath = null;

        private const string SettingsFileName = "valheimtooler_settings.cfg";
        private const string InternalFileName = "valheimtooler_internal.cfg";

        public static ConfigEntry<KeyboardShortcut> s_toggleInterfaceKey;
        public static ConfigEntry<bool> s_showAtStartup;
        public static ConfigEntry<string> s_language;
        public static ConfigEntry<KeyboardShortcut> s_godModeShortCut;
        public static ConfigEntry<KeyboardShortcut> s_unlimitedStaminaShortcut;
        public static ConfigEntry<KeyboardShortcut> s_flyModeShortcut;
        public static ConfigEntry<KeyboardShortcut> s_ghostModeShortcut;
        public static ConfigEntry<KeyboardShortcut> s_noPlacementCostShortcut;
        public static ConfigEntry<KeyboardShortcut> s_inventoryInfiniteWeightShortcut;
        public static ConfigEntry<KeyboardShortcut> s_instantCraftShortcut;
        public static ConfigEntry<KeyboardShortcut> s_guardianPowerAllShortcut;
        public static ConfigEntry<KeyboardShortcut> s_healAllShortcut;
        public static ConfigEntry<KeyboardShortcut> s_removeAllDropShortcut;
        public static ConfigEntry<KeyboardShortcut> s_terrainShapeShortcut;
        public static ConfigEntry<KeyboardShortcut> s_terrainLevelShortcut;
        public static ConfigEntry<KeyboardShortcut> s_terrainLowerShortcut;
        public static ConfigEntry<KeyboardShortcut> s_terrainRaiseShortcut;
        public static ConfigEntry<KeyboardShortcut> s_terrainResetShortcut;
        public static ConfigEntry<KeyboardShortcut> s_terrainSmoothShortcut;
        public static ConfigEntry<KeyboardShortcut> s_terrainPaintShortcut;
        public static ConfigEntry<KeyboardShortcut> s_espPlayersShortcut;
        public static ConfigEntry<KeyboardShortcut> s_espMonstersShortcut;
        public static ConfigEntry<KeyboardShortcut> s_espDroppedItemsShortcut;
        public static ConfigEntry<KeyboardShortcut> s_espDepositsShortcut;
        public static ConfigEntry<KeyboardShortcut> s_espPickablesShortcut;

        public static ConfigEntry<Vector2> s_mainWindowPosition;
        public static ConfigEntry<Vector2> s_itemGiverWindowPosition;
        public static ConfigEntry<bool> s_permanentPins;
        public static ConfigEntry<bool> s_espRadiusEnabled;
        public static ConfigEntry<float> s_espRadius;

        static ConfigManager()
        {
            var valheimAssemblyFolder = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);

            var configPathFilePath = Path.Combine(valheimAssemblyFolder, "config_vt.path");
            var configPath = "";

            if (File.Exists(configPathFilePath))
            {
                configPath = Path.GetFullPath(File.ReadAllText(configPathFilePath));

                if (!Directory.Exists(configPath))
                {
                    ZLog.Log("[ValheimTooler - ConfigManager] Path given in config_vt.path file is incorrect.");
                    configPath = "";
                }
            }
            else
            {
                ZLog.Log("[ValheimTooler - ConfigManager] Failed to find config_vt.path file.");
            }

            s_configurationPath = configPath;

            var configFilePath = Path.Combine(s_configurationPath, SettingsFileName);
            var internalFilePath = Path.Combine(s_configurationPath, InternalFileName);

            s_settingsFile = new ConfigFile(configFilePath, true);
            s_internalFile = new ConfigFile(internalFilePath, true);

            s_toggleInterfaceKey = s_settingsFile.Bind("General", "ToggleInterfaceKey", new KeyboardShortcut(KeyCode.Delete), "Which key will show/hide the tool.");
            s_showAtStartup = s_settingsFile.Bind("General", "ShowAtStartup", true, "Choose whether or not to display the tool at startup.");
            s_language = s_settingsFile.Bind("General", "Language", "Auto", new ConfigDescription("Tool language, in auto the language is chosen according to the game language.", new AcceptableValueList<string>("Auto", "French", "English")));

            s_godModeShortCut = s_settingsFile.Bind("Shortcuts", "GodMode", new KeyboardShortcut(), "The shortcut to use the god mode feature");
            s_unlimitedStaminaShortcut = s_settingsFile.Bind("Shortcuts", "UnlimitedStamina", new KeyboardShortcut(), "The shortcut to use the unlimited stamina feature");
            s_flyModeShortcut = s_settingsFile.Bind("Shortcuts", "FlyMode", new KeyboardShortcut(), "The shortcut to use the fly mode feature");
            s_ghostModeShortcut = s_settingsFile.Bind("Shortcuts", "GhostMode", new KeyboardShortcut(), "The shortcut to use the ghost mode feature");
            s_noPlacementCostShortcut = s_settingsFile.Bind("Shortcuts", "NoPlacementCost", new KeyboardShortcut(), "The shortcut to use the no placement cost feature");
            s_inventoryInfiniteWeightShortcut = s_settingsFile.Bind("Shortcuts", "InventoryInfiniteWeight", new KeyboardShortcut(), "The shortcut to use the inventory with infinite weight feature");
            s_instantCraftShortcut = s_settingsFile.Bind("Shortcuts", "InstantCraft", new KeyboardShortcut(), "The shortcut to use the instant craft feature");
            s_guardianPowerAllShortcut = s_settingsFile.Bind("Shortcuts", "GuardianPowerAllPlayers", new KeyboardShortcut(), "The shortcut to give all players the selected guardian power");
            s_healAllShortcut = s_settingsFile.Bind("Shortcuts", "HealAllPlayers", new KeyboardShortcut(), "Shortcut to heal all the players");
            s_removeAllDropShortcut = s_settingsFile.Bind("Shortcuts", "RemoveAllDrops", new KeyboardShortcut(), "The shortcut to use the remove all drops feature");
            s_terrainShapeShortcut = s_settingsFile.Bind("Shortcuts", "TerrainChangeShape", new KeyboardShortcut(), "");
            s_terrainLevelShortcut = s_settingsFile.Bind("Shortcuts", "TerrainLevel", new KeyboardShortcut(), "");
            s_terrainLowerShortcut = s_settingsFile.Bind("Shortcuts", "TerrainLower", new KeyboardShortcut(), "");
            s_terrainRaiseShortcut = s_settingsFile.Bind("Shortcuts", "TerrainRaise", new KeyboardShortcut(), "");
            s_terrainResetShortcut = s_settingsFile.Bind("Shortcuts", "TerrainReset", new KeyboardShortcut(), "");
            s_terrainSmoothShortcut = s_settingsFile.Bind("Shortcuts", "TerrainSmooth", new KeyboardShortcut(), "");
            s_terrainPaintShortcut = s_settingsFile.Bind("Shortcuts", "TerrainPaint", new KeyboardShortcut(), "");
            s_espPlayersShortcut = s_settingsFile.Bind("Shortcuts", "ESPPlayers", new KeyboardShortcut(), "The shortcut to show/hide the ESP for players");
            s_espMonstersShortcut = s_settingsFile.Bind("Shortcuts", "ESPMonsters", new KeyboardShortcut(), "The shortcut to show/hide the ESP for monsters");
            s_espDroppedItemsShortcut = s_settingsFile.Bind("Shortcuts", "ESPDroppedItems", new KeyboardShortcut(), "The shortcut to show/hide the ESP for dropped items");
            s_espDepositsShortcut = s_settingsFile.Bind("Shortcuts", "ESPDeposits", new KeyboardShortcut(), "The shortcut to show/hide the ESP for deposits");
            s_espPickablesShortcut = s_settingsFile.Bind("Shortcuts", "ESPPickables", new KeyboardShortcut(), "The shortcut to show/hide the ESP for pickables");

            s_mainWindowPosition = s_internalFile.Bind("Internal", "MainWindowPosition", new Vector2(5, 5));
            s_itemGiverWindowPosition = s_internalFile.Bind("Internal", "ItemGiverPosition", new Vector2(Screen.width - 400, 5));
            s_permanentPins = s_internalFile.Bind("Internal", "PermanentPins", false);
            s_espRadiusEnabled = s_internalFile.Bind("Internal", "RadiusEnabled", false);
            s_espRadius = s_internalFile.Bind("Internal", "RadiusValue", 5f);

            s_settingsFile.OrphanedEntries.Clear();
            s_internalFile.OrphanedEntries.Clear();

            s_settingsFile.Save();
            s_internalFile.Save();
        }
    }
}
