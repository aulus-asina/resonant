using Dalamud.Plugin;
using System;

namespace Resonant
{
    [Serializable]
    public class ConfigurationManager
    {
        private DalamudPluginInterface DalamudPlugin;

        public ConfigurationManager(DalamudPluginInterface dalamudPlugin)
        {
            DalamudPlugin = dalamudPlugin;
        }

        public ConfigurationProfile GetSavedConfig()
        {
            return DalamudPlugin.GetPluginConfig() as ConfigurationProfile ?? new ConfigurationProfile();
        }

        public void Save(ConfigurationProfile configuration)
        {
            DalamudPlugin.SavePluginConfig(configuration);
        }
    }
}
