using System;

namespace ValheimTooler.Configuration
{
    // Class originally created by BepInEx: https://github.com/BepInEx/BepInEx
    /// <summary>
	/// Arguments for events concerning a change of a setting.
	/// </summary>
	/// <inheritdoc />
	public sealed class SettingChangedEventArgs : EventArgs
    {
        /// <inheritdoc />
        public SettingChangedEventArgs(ConfigEntryBase changedSetting)
        {
            ChangedSetting = changedSetting;
        }

        /// <summary>
        /// Setting that was changed
        /// </summary>
        public ConfigEntryBase ChangedSetting { get; }
    }
}
